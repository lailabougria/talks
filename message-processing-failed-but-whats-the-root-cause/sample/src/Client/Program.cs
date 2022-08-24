using Azure.Monitor.OpenTelemetry.Exporter;
using Commands;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;

namespace RootCauseExample.Client
{
    public class Program
    {
        const string EndpointName = "ClientAPI";
        
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                       .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                       .ConfigureServices((_, services) =>
                       {
                           // Enables capturing OpenTelemetry from the Azure SDK
                           AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);

                           services.AddOpenTelemetryTracing(config => config
                                                                      .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(EndpointName))
                                                                      // add sources to collect telemetry from
                                                                      .AddSource("NServiceBus.Core")
                                                                      .AddSource("Azure.*")
                                                                      .AddAspNetCoreInstrumentation()
                                                                      // add exporters
                                                                      .AddAzureMonitorTraceExporter(options =>
                                                                      {
                                                                          options.ConnectionString = "insert-connection-string-here";
                                                                      })
                                                                      .AddJaegerExporter(c =>
                                                                      {
                                                                          c.AgentHost = "localhost";
                                                                          c.AgentPort = 6831;
                                                                      })
                           );

                           // connect traces with logs
                           services.AddLogging(loggingBuilder =>
                               loggingBuilder.AddOpenTelemetry(otelLoggerOptions =>
                               {
                                   otelLoggerOptions.IncludeFormattedMessage = true;
                                   otelLoggerOptions.IncludeScopes = true;
                                   otelLoggerOptions.ParseStateValues = true;
                                   otelLoggerOptions.AddConsoleExporter();
                               }).AddConsole()
                           );
                       })
                       .UseNServiceBus(_ =>
                       {
                           var endpointConfiguration = new EndpointConfiguration(EndpointName);
                           endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
                           endpointConfiguration.UsePersistence<LearningPersistence>();

                           var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
                           var connectionString = Environment.GetEnvironmentVariable("ASB_ConnectionString");
                           if (string.IsNullOrEmpty(connectionString))
                           {
                               throw new InvalidOperationException("Please specify a connection string.");
                           }
                           transport.ConnectionString(connectionString);
                           transport.Routing().RouteToEndpoint(typeof(PlaceOrder), "Sales");

                           endpointConfiguration.EnableInstallers();

                           endpointConfiguration.Recoverability().Immediate(immediate => immediate.NumberOfRetries(0));
                           endpointConfiguration.Recoverability().Delayed(delayed => delayed.NumberOfRetries(3));

                           return endpointConfiguration;
                       });
        }
    }
}