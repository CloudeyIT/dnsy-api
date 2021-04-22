using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using DnsClient;
using DnsClient.Protocol;
using Dnsy.Api.Types;
using Microsoft.AspNetCore.Mvc;
using MoreLinq.Experimental;

namespace Dnsy.Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class DnsController : ControllerBase
    {
        private readonly ILookupClient _dns;

        public DnsController (ILookupClient dns)
        {
            _dns = dns;
        }

        private readonly List<QueryType> _queryTypes = new()
        {
            QueryType.A,
            QueryType.AAAA,
            QueryType.CNAME,
            QueryType.MX,
            QueryType.PTR,
            QueryType.SOA,
            QueryType.NS,
            QueryType.SRV,
            QueryType.TXT,
            QueryType.DNSKEY,
            QueryType.DS,
            QueryType.CAA,
        };

        [HttpGet("{query}")]
        public async Task<LookupResponse> Lookup ([FromRoute] string query, [FromQuery] bool extended = false)
        {
            var hostEntry = await _dns.GetHostEntryAsync(query);

            if (hostEntry is null)
            {
                return new LookupResponse
                {
                    Query = query,
                    HostName = null,
                    NameServer = _dns.NameServers.First().Address,
                };
            }
            
            var response = new LookupResponse
            {
                Query = query,
                HostName = hostEntry.HostName,
                Addresses = hostEntry.AddressList.Select(address => address.ToString()),
                Aliases = hostEntry.Aliases,
                NameServer = _dns.NameServers.First().Address,
            };
            
            if (!extended)
            {
                return response;
            }
            
            var questions = _queryTypes.Select(type => new DnsQuestion(hostEntry.HostName, type)).ToArray();
            var resultBag = new ConcurrentBag<IDnsQueryResponse>();
            var tasks = questions.Select(async question =>
            {
                var result = await _dns.QueryAsync(question);
                resultBag.Add(result);
            });
            await Task.WhenAll(tasks);
            var results = resultBag.ToArray();
            
            var extendedResponse = response with
            {
                A = results.First(result => result.Questions[0].QuestionType == QueryType.A)
                    .Answers.ARecords().Select(record => new ARecordResponse
                    {
                        DomainName = record.DomainName.Value,
                        Address = record.Address.ToString(),
                        TTL = record.InitialTimeToLive,
                        Raw = record.ToString(),
                    }),
                Aaaa = results.First(result => result.Questions[0].QuestionType == QueryType.AAAA)
                    .Answers.AaaaRecords().Select(record => new AaaaRecordResponse
                    {
                        DomainName = record.DomainName.Value,
                        Address = record.Address.ToString(),
                        TTL = record.InitialTimeToLive,
                        Raw = record.ToString(),
                    }),
                Cname = results.First(result => result.Questions[0].QuestionType == QueryType.CNAME)
                    .Answers.CnameRecords().Select(record => new CnameRecordResponse
                    {
                        DomainName = record.DomainName.Value,
                        CanonicalName = record.CanonicalName.Value,
                        TTL = record.InitialTimeToLive,
                        Raw = record.ToString(),
                    }),
                Mx = results.First(result => result.Questions[0].QuestionType == QueryType.MX)
                    .Answers.MxRecords().Select(record => new MxRecordResponse
                    {
                        DomainName = record.DomainName.Value,
                        TTL = record.InitialTimeToLive,
                        Exchange = record.Exchange,
                        Priority = record.Preference,
                        Raw = record.ToString(),
                    }),
                Txt = results.First(result => result.Questions[0].QuestionType == QueryType.TXT)
                    .Answers.TxtRecords().Select(record => new TxtRecordResponse
                    {
                        DomainName = record.DomainName.Value,
                        TTL = record.InitialTimeToLive,
                        Raw = record.ToString(),
                        Text = record.Text.First(),
                        EscapedText = record.EscapedText.First(),
                    }),
                Soa = results.First(result => result.Questions[0].QuestionType == QueryType.SOA)
                    .Answers.SoaRecords().Select(record => new SoaRecordResponse
                    {
                        DomainName = record.DomainName.Value,
                        TTL = record.InitialTimeToLive,
                        Raw = record.ToString(),
                        Minimum = record.Minimum,
                        Expire = record.Expire,
                        MName = record.MName,
                        Retry = record.Retry,
                        Refresh = record.Refresh,
                        RName = record.RName,
                        Serial = record.Serial,
                    }),
                Ptr = results.First(result => result.Questions[0].QuestionType == QueryType.PTR)
                    .Answers.PtrRecords().Select(record => new PtrRecordResponse
                    {
                        DomainName = record.DomainName.Value,
                        TTL = record.InitialTimeToLive,
                        Raw = record.ToString(),
                        PtrDomainName = record.PtrDomainName,
                    }),
                Ns = results.First(result => result.Questions[0].QuestionType == QueryType.NS)
                    .Answers.NsRecords().Select(record => new NsRecordResponse
                    {
                        DomainName = record.DomainName.Value,
                        TTL = record.InitialTimeToLive,
                        Raw = record.ToString(),
                        NameServer = record.NSDName,
                    }),
                Srv = results.First(result => result.Questions[0].QuestionType == QueryType.SRV)
                    .Answers.SrvRecords().Select(record => new SrvRecordResponse
                    {
                        DomainName = record.DomainName.Value,
                        TTL = record.InitialTimeToLive,
                        Raw = record.ToString(),
                        Priority = record.Priority,
                        Port = record.Port,
                        Weight = record.Weight,
                        Target = record.Target,
                    }),
                Dnskey = results.First(result => result.Questions[0].QuestionType == QueryType.DNSKEY)
                    .Answers.OfType<DnsKeyRecord>().Select(record => new DnskeyRecordResponse
                    {
                        DomainName = record.DomainName.Value,
                        TTL = record.InitialTimeToLive,
                        Raw = record.ToString(),
                        Algorithm = record.Algorithm,
                        Flags = record.Flags,
                        Protocol = record.Protocol,
                        PublicKey = record.PublicKeyAsString,
                    }),
                Ds = results.First(result => result.Questions[0].QuestionType == QueryType.DS)
                    .Answers.OfType<DsRecord>().Select(record => new DsRecordResponse
                    {
                        DomainName = record.DomainName.Value,
                        TTL = record.InitialTimeToLive,
                        Raw = record.ToString(),
                        Algorithm = record.Algorithm,
                        KeyTag = record.KeyTag,
                        Digest = record.DigestAsString,
                        DigestType = record.DigestType,
                    }),
                Caa = results.First(result => result.Questions[0].QuestionType == QueryType.CAA)
                    .Answers.CaaRecords().Select(record => new CaaRecordResponse
                    {
                        DomainName = record.DomainName.Value,
                        TTL = record.InitialTimeToLive,
                        Raw = record.ToString(),
                        Flags = record.Flags,
                        Value = record.Value,
                        Tag = record.Tag,
                    }),
            };

            return extendedResponse;
        }
    }
}