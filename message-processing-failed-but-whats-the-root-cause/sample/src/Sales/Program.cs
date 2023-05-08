﻿using Azure.Monitor.OpenTelemetry.Exporter;
using Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

const string EndpointName = "Sales";

var host = Host.CreateDefaultBuilder((string[])args)
               .ConfigureServices((builder, services) =>
               {
                   // Enables capturing OpenTelemetry from the Azure SDK
                   AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);

                   var otelBuilder = services.AddOpenTelemetry();
                   otelBuilder.ConfigureResource(_ => ResourceBuilder.CreateDefault().AddService(EndpointName));
                   otelBuilder.WithTracing(builder => builder
                                                      .AddSource("NServiceBus.*")
                                                      .AddSource("Azure.*")
                                                      .AddAzureMonitorTraceExporter(options =>
                                                      {
                                                          options.ConnectionString = "insert-connection-string-here";
                                                      })
                                                      .AddJaegerExporter(c =>
                                                      {
                                                          c.AgentHost = "localhost";
                                                          c.AgentPort = 6831;
                                                      }));
                   otelBuilder.WithMetrics(builder => builder
                                                      .AddMeter("NServiceBus.Core")
                                                      .AddAzureMonitorMetricExporter(options =>
                                                      {
                                                          options.ConnectionString = "insert-connection-string-here";
                                                      }));

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
               .UseNServiceBus(context =>
               {
                   var endpointConfiguration = new EndpointConfiguration(EndpointName);
                   endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();
                   endpointConfiguration.UsePersistence<LearningPersistence>();

                   var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
                   var connectionString = Environment.GetEnvironmentVariable("ASB_ConnectionString");
                   if (string.IsNullOrEmpty(connectionString))
                   {
                       throw new InvalidOperationException("Please specify a connection string.");
                   }
                   transport.ConnectionString(connectionString);

                   transport.Routing().RouteToEndpoint(typeof(ChargeOrder), "Finance");
                   endpointConfiguration.EnableInstallers();
                   endpointConfiguration.EnableOpenTelemetry();

                   endpointConfiguration.Recoverability().Immediate(immediate => immediate.NumberOfRetries(0));
                   endpointConfiguration.Recoverability().Delayed(delayed => delayed.NumberOfRetries(3));

                   return endpointConfiguration;
               }).Build();

var hostEnvironment = host.Services.GetRequiredService<IHostEnvironment>();
Console.Title = hostEnvironment.ApplicationName;
host.Run();
