# Sample

This sample is based on the use case that was discussed during the session, a small online retail store. The sample uses [NServiceBus](https://docs.particular.net/) on top of [Azure Service Bus](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview) to send/publish messages/events between components. The sample makes use of the [Microsoft Generic Host](https://docs.microsoft.com/en-us/dotnet/core/extensions/generic-host).

The sample demonstrates how to:

- Collect telemetry information from the Azure SDK (experimental at the time of writing) and NServiceBus framework
- Forward telemetry to an Azure Monitor exporter
- Define and use an ActivitySource to emit tracing information
- Connect traces and logs

## Setting up OpenTelemetry

In the sample, I'm making use of the Microsoft.Extensions.Hosting package. By pulling in the [`OpenTelemetry.Extensions.Hosting`-package](https://www.nuget.org/packages/OpenTelemetry.Extensions.Hosting), a method becomes available to add OpenTelemetry to the component:

``` c#
 services.AddOpenTelemetryTracing(config => config
      .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("component-name"))
      .AddSource("Azure.*")
      .AddSource("NServiceBus.Core")
      .AddAzureMonitorTraceExporter(options =>
      {
          options.ConnectionString = "insert-connection-string-here";
      })
);
```

The `component-name`-placeholder should reflect the name of the component, as this will be visible in the exported information.

Next, relevant sources from which to collect tracing information should be added. In this example, I'm collecting tracing from NServiceBus and the Azure SDK.
At this time, [OpenTelemetry support in the Azure SDK](https://devblogs.microsoft.com/azure-sdk/introducing-experimental-opentelemetry-support-in-the-azure-sdk-for-net/) is experimental. To enable it, ensure to enable the experimental telemetry as described in the ["Get started"-section](https://devblogs.microsoft.com/azure-sdk/introducing-experimental-opentelemetry-support-in-the-azure-sdk-for-net/#get-started).
OpenTelemetry support is [available in NServiceBus](https://docs.particular.net/nservicebus/operations/opentelemetry?version=core_8) starting from v8. For OpenTelemetry support in NServiceBus v7, there's a [community-supported package](https://github.com/jbogard/NServiceBus.Extensions.Diagnostics) available maintained by Jimmy Bogard.

In this setup, the collected tracing information is being exported to [Azure Monitor](https://docs.microsoft.com/en-us/azure/azure-monitor/overview). The `AddAzureMonitorTraceExporter`-method becomes available through the [`Azure.Monitor.OpenTelemetry.Exporter`-package](https://www.nuget.org/packages/Azure.Monitor.OpenTelemetry.Exporter).
For more information about using OpenTelemetry with Azure Monitor, visit the [Microsoft docs](https://docs.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-overview).

## Emitting trace information

As shown in the Inventory component, in order to add custom tracing to an application, first an `ActivitySource` needs to be defined. 

``` c#
private static readonly ActivitySource source = new("Inventory", "1.0.0");
````

When handling the message, information can be traced as follows:

``` c#
public Task Handle(UpdateProductStock message, IMessageHandlerContext context)
{
    using Activity? activity = source.StartActivity("Inventory_UpdateProductStock");

    try 
    {
        var product = ProductStore.Products.Single(x => x.ProductId == message.ProductId);

        activity?.SetTag("ProductId", product.ProductId);
        activity?.AddEvent(new ActivityEvent("Stock_Recalculation_Starting"));

        // update stock

        activity?.AddEvent(new ActivityEvent("Stock_Recalculation_Completed"));
        return Task.CompletedTask;
    }
    catch (Exception e)
    {
        activity?.SetTag("otel.status_code", "ERROR");
        activity?.SetTag("otel.status_description", e.Message);
        throw;
    }
}
```

The usage of the `using`-keyword, ensures that the activity will be stopped automatically.
The exception is being caught to set specific tags on the activity. These tags are propagated to the exporter, and any failures will be marked as failed traces in most exporter tools. 

## Connecting traces and logs

By connecting the traces and logs, each log message will have a reference to a trace id when one exists. This allows users to easily switch back and forth between log telemetry and trace telemetry.

To connect traces and logs, logging needs to be configured with OpenTelemetry. When using the Microsoft.Extensions.Logging framework, OpenTelemetry can be enabled as part of the logging configuration:

``` c#
 services.AddLogging(loggingBuilder =>
                       loggingBuilder.AddOpenTelemetry(otelLoggerOptions =>
                       {
                           otelLoggerOptions.IncludeFormattedMessage = true;
                           otelLoggerOptions.IncludeScopes = true;
                           otelLoggerOptions.ParseStateValues = true;
                           otelLoggerOptions.AddConsoleExporter();
                       }).AddConsole()
                   );
```
