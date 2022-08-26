# Resources

Here you can find a list of resources to further your understanding of these topics (I keep this list up to date with new resources I come across).

## Testing message-driven systems

If you're using NServiceBus, to unit test message handlers and sagas, you can make use of the [testing framework](https://docs.particular.net/nservicebus/testing/) provided by the platform.

To perform integration- or end-to-end testing to ensure things like the overall business process behavior and orchestration of your messages works correctly, you can use the [NServiceBus integration testing framework by Mauro Servienti](https://github.com/mauroservienti/NServiceBus.IntegrationTesting). This framework also allows you to test concerns like endpoint configuration, message routing, subscriptions, and more.

## OpenTelemetry

To fully understand how OpenTelemetry works and to best implement it in your systems, you will also need to gain a broader understanding of observability in general. This list of resources is therefore not limited to OpenTelemetry alone.

### CNCF OpenTelemetry project

- [The specification](https://opentelemetry.io/docs/reference/specification/)
- [OpenTelemetry concepts](https://opentelemetry.io/docs/concepts/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)

### .NET OpenTelemetry project

- [Distributed tracing concepts](https://docs.microsoft.com/en-us/dotnet/core/diagnostics/distributed-tracing-concepts)
- [OpenTelemetry with Azure Monitor](https://docs.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-overview)
- [GitHub repo for the OpenTelemetry.NET project](https://github.com/open-telemetry/opentelemetry-dotnet)

### Books

- [Cloud-native observability with OpenTelemetry](https://www.amazon.com/dp/1801077703)
- [Observability Engineering: Achieving Production Excellence](https://www.amazon.com/dp/1492076449)

### Blogs

There are also multiple vendors out there sharing knowledge about observability. One that stood out for me is Honeycomb. They take knowledge sharing very seriously. Their [blog](https://www.honeycomb.io/blog/) is filled with interesting articles (make sure to check out categories[ `instrumentation`](https://www.honeycomb.io/category/instrumentation/) and [`observability`](https://www.honeycomb.io/category/observability)) that are very well explained and they conduct [webinars](https://www.honeycomb.io/type/webinar/) regularly. They've recently also released a book on Observability with O'Reilly, which is available as a [free digital download on their website](https://info.honeycomb.io/observability-engineering-oreilly-book-2022).

A few noteworthy blog posts:

- [Jimmy's series on end-to-end-diagnostics](https://jimmybogard.com/building-end-to-end-diagnostics-and-tracing-a-primer/)
- [An Overview of Distributed Tracing with OpenTelemetry in .NET 6 by Aaron Stannard](https://aaronstannard.com/opentelemetry-dotnet6/)
- [The Story of How I Wrote Another Instrumentation by Nikolay Sokolik](https://www.oxeye.io/blog/diving-into-opentelemetrys-specs)
