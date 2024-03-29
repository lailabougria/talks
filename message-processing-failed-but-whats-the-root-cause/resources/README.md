# Resources

Here you can find a list of resources to further your understanding of these topics. I try to keep this list up to date with new resources I come across, if you can think of valuable additions, feel free to contribute!

## Testing message-driven systems

If you're using [NServiceBus](https://docs.particular.net), to unit test message handlers and sagas, you can make use of the [testing framework](https://docs.particular.net/nservicebus/testing/) provided by the platform.

To perform integration- or end-to-end testing to ensure things like the overall business process behavior and orchestration of your messages works correctly, you can use the [NServiceBus integration testing framework](https://github.com/mauroservienti/NServiceBus.IntegrationTesting) by [Mauro Servienti](https://twitter.com/mauroservienti). This framework also allows you to test concerns like endpoint configuration, message routing, subscriptions, and more. Check out [Mauro's post](https://milestone.topics.it/2019/07/04/exploring-nservicebus-integration-testing-options.html) on the topic too.

## OpenTelemetry

To fully understand how OpenTelemetry works and to best implement it in your systems, you will also need to gain a broader understanding of observability in general. This list of resources is therefore not limited to OpenTelemetry alone.

### CNCF OpenTelemetry project

- [The OpenTelemetry specification](https://opentelemetry.io/docs/reference/specification/)
- [OpenTelemetry concepts](https://opentelemetry.io/docs/concepts/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)

### OpenTelemetry in .NET

- [Distributed tracing concepts](https://docs.microsoft.com/en-us/dotnet/core/diagnostics/distributed-tracing-concepts)
- [OpenTelemetry .NET tracing - Getting started](https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/docs/trace/getting-started)
- [Customizing OpenTelemetry .NET SDK](https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/docs/trace/customizing-the-sdk)
- [How to report exceptions](https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/docs/trace/reporting-exceptions)
- [GitHub repo for the OpenTelemetry.NET project](https://github.com/open-telemetry/opentelemetry-dotnet)

#### OpenTelemetry with Azure

- [OpenTelemetry with Azure Monitor](https://docs.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-overview)


#### OpenTelemetry with AWS

- [AWS Distro for OpenTelemetry](https://aws.amazon.com/otel/)
- [AWS Distro for OpenTelemetry in .NET GitHub repository](https://github.com/aws-observability/aws-otel-dotnet)
- [AWS Distro for OpenTelemetry (ADOT) Collector](https://github.com/aws-observability/aws-otel-collector)
- [Tracing with the AWS Distro for OpenTelemetry .NET SDK and X-Ray](https://aws-otel.github.io/docs/getting-started/dotnet-sdk/trace-manual-instr)
- [OpenTelemetry with XRay](https://timbussmann.github.io/2022/07/14/opentelemetry-with-xray.html), by Tim Bussmann
- [How to Instrument AWS Services with OpenTelemetry](https://www.aspecto.io/blog/getting-started-with-opentelemetry-aws), by Yoav Danieli

### Books

- [Cloud-native observability with OpenTelemetry, by Alex Boten](https://www.amazon.com/dp/1801077703)
- [Observability Engineering: Achieving Production Excellence, by Charity Majors, Liz Fong-Jones, George Miranda](https://www.amazon.com/dp/1492076449)
- [Practical OpenTelemetry: Adopting Open Observability Standards Across Your Organization](https://www.amazon.com/Practical-OpenTelemetry-Observability-Standards-Organization/dp/1484290747)

### Blogs

There are also multiple vendors out there sharing knowledge about observability. One that stood out for me is Honeycomb. They take knowledge sharing very seriously. Their [blog](https://www.honeycomb.io/blog/) is filled with interesting articles (make sure to check out categories[ `instrumentation`](https://www.honeycomb.io/category/instrumentation/) and [`observability`](https://www.honeycomb.io/category/observability)) that are very well explained and they conduct [webinars](https://www.honeycomb.io/type/webinar/) regularly. They've recently also released a book on Observability with O'Reilly, which is available as a [free digital download on their website](https://info.honeycomb.io/observability-engineering-oreilly-book-2022).

A few noteworthy blog posts:

- [Series on end-to-end-diagnostics, by Jimmy Bogard](https://jimmybogard.com/building-end-to-end-diagnostics-and-tracing-a-primer/)
- [An Overview of Distributed Tracing with OpenTelemetry in .NET 6, by Aaron Stannard](https://aaronstannard.com/opentelemetry-dotnet6/)
- [The Story of How I Wrote Another Instrumentation, by Nikolay Sokolik](https://www.oxeye.io/blog/diving-into-opentelemetrys-specs)

If you're interested in keeping up-to-date with the latest o11y news, Michael Hausenblas runs a [newsletter focussed on observability news](https://o11y.news/).