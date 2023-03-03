using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sentry.Serilog;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace Dnsy.Api
{
    public class Program
    {
        public static void Main (string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder (string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(
                    (_, builder) =>
                    {
                        builder.AddEnvironmentVariables("APP__");
                    })
                .UseSerilog(
                    (_, configuration) => configuration
                        .Enrich.FromLogContext()
                        .Enrich.WithExceptionDetails()
                        .Enrich.WithEnvironmentName()
                        .Enrich.WithMachineName()
                        .WriteTo.Console(_.HostingEnvironment.IsProduction() ? LogEventLevel.Information : LogEventLevel.Debug)
                        .WriteTo.Debug(_.HostingEnvironment.IsProduction() ? LogEventLevel.Information : LogEventLevel.Debug)
                        .WriteTo.Sentry(
                            sentry =>
                            {
                                var sentryConfig = _.Configuration.GetSection("Sentry").Get<SentrySerilogOptions>();
                                if (sentryConfig?.Dsn is null) return;

                                sentry.Dsn = sentryConfig.Dsn;
                                sentry.TracesSampleRate = sentryConfig.TracesSampleRate;
                                sentry.AttachStacktrace = true;
                                sentry.InitializeSdk = true;
                                sentry.AutoSessionTracking = true;
                            }
                        )
                )
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}