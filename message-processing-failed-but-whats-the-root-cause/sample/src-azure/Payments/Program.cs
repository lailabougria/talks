using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

const string EndpointName = "Payments";

var host = Host.CreateDefaultBuilder(args)
               .ConfigureServices((_, services) =>
               {
                   // Enables capturing OpenTelemetry from the Azure SDK
                   AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);

                   var appInsightsConnString = Environment.GetEnvironmentVariable("AppInsights_ConnectionString");
                   services.AddOpenTelemetry()
                           .ConfigureResource(resourceBuilder => resourceBuilder.AddService(EndpointName))
                           .WithTracing(tracingBuilder => tracingBuilder
                                                          .AddSource("NServiceBus.Core")
                                                          .AddSource("Azure.*")
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
                                                          .AddMeter("NServiceBus.Core")
                                                          .AddAzureMonitorMetricExporter(options =>
                                                          {
                                                              options.ConnectionString = appInsightsConnString;
                                                          }));

                   // connect traces with logs
                   services.AddLogging(loggingBuilder =>
                       loggingBuilder.AddOpenTelemetry(otelLoggerOptions =>
                       {
                           otelLoggerOptions.IncludeFormattedMessage = true;
                           otelLoggerOptions.IncludeScopes = true;
                           otelLoggerOptions.ParseStateValues = true;
                           otelLoggerOptions.AddConsoleExporter();
                           otelLoggerOptions.AddAzureMonitorLogExporter(options =>
                               options.ConnectionString = appInsightsConnString
                           );
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

                   endpointConfiguration.EnableInstallers();
                   endpointConfiguration.EnableOpenTelemetry();

                   endpointConfiguration.Recoverability().Immediate(immediate => immediate.NumberOfRetries(0));
                   endpointConfiguration.Recoverability().Delayed(delayed => delayed.NumberOfRetries(3));

                   return endpointConfiguration;
               }).Build();

var hostEnvironment = host.Services.GetRequiredService<IHostEnvironment>();
Console.Title = hostEnvironment.ApplicationName;
host.Run();