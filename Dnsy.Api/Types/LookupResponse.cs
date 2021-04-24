using System;
using System.Collections;
using System.Collections.Generic;
using DnsClient.Protocol;

namespace Dnsy.Api.Types
{
    public record LookupResponse
    {
        public string Query { get; set; }
        public string HostName { get; set; }
        public string NameServer { get; set; }
        public string Time { get; set; }
        public long Timestamp { get; set; }
        public IEnumerable<string> Addresses { get; set; }
        public IEnumerable<string> Aliases { get; set; }
        public IEnumerable<ARecordResponse> A { get; set; }
        public IEnumerable<AaaaRecordResponse> Aaaa { get; set; }
        public IEnumerable<CnameRecordResponse> Cname { get; set; }
        public IEnumerable<MxRecordResponse> Mx { get; set; }
        public IEnumerable<TxtRecordResponse> Txt { get; set; }
        public IEnumerable<SoaRecordResponse> Soa { get; set; }
        public IEnumerable<PtrRecordResponse> Ptr { get; set; }
        public IEnumerable<NsRecordResponse> Ns { get; set; }
        public IEnumerable<SrvRecordResponse> Srv { get; set; }
        public IEnumerable<DnskeyRecordResponse> Dnskey { get; set; }
        public IEnumerable<DsRecordResponse> Ds { get; set; }
        public IEnumerable<CaaRecordResponse> Caa { get; set; }
        
    }
    public abstract record RecordResponse
    {
        public string DomainName { get; set; }
        public int TTL { get; set; }
        public string Raw { get; set; }
    }

    public record ARecordResponse : RecordResponse
    {
        public string Address { get; set; }
    }

    public record AaaaRecordResponse : RecordResponse
    {
        public string Address { get; set; }
    }

    public record CnameRecordResponse : RecordResponse
    {
        public string CanonicalName { get; set; }
    }

    public record MxRecordResponse : RecordResponse
    {
        public ushort Priority { get; set; }
        public string Exchange { get; set; }
    }

    public record TxtRecordResponse : RecordResponse
    {
        public string Text { get; set; }
        public string EscapedText { get; set; }
    }

    public record SoaRecordResponse : RecordResponse
    {
        public uint Expire { get; set; }
        public uint Minimum { get; set; }
        public uint Refresh { get; set; }
        public uint Retry { get; set; }
        public uint Serial { get; set; }
        public string MName { get; set; }
        public string RName { get; set; }
    }

    public record PtrRecordResponse : RecordResponse
    {
        public string PtrDomainName { get; set; }
    }

    public record NsRecordResponse : RecordResponse
    {
        public string NameServer { get; set; }
    }

    public record SrvRecordResponse : RecordResponse
    {
        public uint Priority { get; set; }
        public uint Port { get; set; }
        public uint Weight { get; set; }
        public string Target { get; set; }
    }

    public record DnskeyRecordResponse : RecordResponse
    {
        public uint Protocol { get; set; }
        public DnsSecurityAlgorithm Algorithm { get; set; }
        public string AlgorithmName => Algorithm.ToString();
        public int Flags { get; set; }
        public string PublicKey { get; set; }
    }

    public record DsRecordResponse : RecordResponse
    {
        public DnsSecurityAlgorithm Algorithm { get; set; }
        public string AlgorithmName => Algorithm.ToString();
        public string Digest { get; set; }
        public uint DigestType { get; set; }
        public int KeyTag { get; set; }
    }

    public record CaaRecordResponse : RecordResponse
    {
        public int Flags { get; set; }
        public string Value { get; set; }
        public string Tag { get; set; }
    }
}