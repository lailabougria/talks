using Azure.Monitor.OpenTelemetry.Exporter;
using Commands;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace RootCauseExample.Client;

public class Program
{
    private const string EndpointName = "ClientAPI";

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

                       var appInsightsConnString = Environment.GetEnvironmentVariable("AppInsights_ConnectionString");
                       services.AddOpenTelemetry()
                               .ConfigureResource(resourceBuilder => resourceBuilder.AddService(EndpointName))
                               .WithTracing(tracingBuilder => tracingBuilder
                                                              .AddSource("NServiceBus.*")
                                                              .AddSource("Azure.*")
                                                              .AddAspNetCoreInstrumentation()
                                                              .AddAzureMonitorTraceExporter(options =>
                                                              {
                                                                  options.ConnectionString = appInsightsConnString;
                                                              })
                                                              .AddJaegerExporter(c =>
                                                              {
                                                                  c.AgentHost = "localhost";
                                                                  c.AgentPort = 6831;
                                                              }))
                               .WithMetrics(metricsBuilder => metricsBuilder
                                                              .AddAspNetCoreInstrumentation()
                                                              .AddMeter("NServiceBus.Core")
                                                              .AddAzureMonitorMetricExporter(options =>
                                                              {
                                                                  options.ConnectionString = appInsightsConnString;
                                                              }));

                       // correlate traces with logs
                       services.AddLogging(loggingBuilder =>
                           loggingBuilder.AddOpenTelemetry(otelLoggerOptions =>
                           {
                               otelLoggerOptions.IncludeFormattedMessage = true;
                               otelLoggerOptions.IncludeScopes = true;
                               otelLoggerOptions.ParseStateValues = true;
                               otelLoggerOptions.AddAzureMonitorLogExporter(options =>
                                   options.ConnectionString = appInsightsConnString
                               );
                               otelLoggerOptions.AddConsoleExporter();
                           }).AddConsole()
                       );
                   })
                   .UseNServiceBus(_ =>
                   {
                       var endpointConfiguration = new EndpointConfiguration(EndpointName);
                       endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();
                       endpointConfiguration.UsePersistence<LearningPersistence>();

                       var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
                       var connectionString = Environment.GetEnvironmentVariable("ASB_ConnectionString");
                       if (string.IsNullOrEmpty(connectionString))
                           throw new InvalidOperationException("Please specify a connection string.");
                       transport.ConnectionString(connectionString);
                       transport.Routing().RouteToEndpoint(typeof(PlaceOrder), "Sales");

                       endpointConfiguration.EnableInstallers();
                       endpointConfiguration.EnableOpenTelemetry();

                       endpointConfiguration.Recoverability().Immediate(immediate => immediate.NumberOfRetries(0));
                       endpointConfiguration.Recoverability().Delayed(delayed => delayed.NumberOfRetries(3));

                       return endpointConfiguration;
                   });
    }
}