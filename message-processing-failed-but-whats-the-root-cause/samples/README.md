# Samples

This sample showcases the use case presented during the session, a small online retail store. The sample the sample uses [NServiceBus](https://docs.particular.net/) to send/publish messages/events between components. The sample makes use of the [Microsoft Generic Host](https://docs.microsoft.com/en-us/dotnet/core/extensions/generic-host).

There are two versions of the sample available:
- [Azure-stack](azure/README.md): One sample uses [NServiceBus](https://docs.particular.net/) on top of [Azure Service Bus](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview), and exports traces, metrics and logs to [Azure Monitor](https://learn.microsoft.com/en-us/azure/azure-monitor/overview) and, in addition, sends traces to [Jaeger](https://www.jaegertracing.io/), both through a direct export without the use of the [OpenTelemetry Collector](https://opentelemetry.io/docs/collector/).
- [AWS-stack](aws/README.md): One sample uses [NServiceBus](https://docs.particular.net/) on top of [Amazon SQS](https://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSDeveloperGuide/welcome.html) and [Amazon SNS](https://docs.aws.amazon.com/sns/latest/dg/welcome.html), and exports traces, metrics and logs to [AWS X-Ray](https://docs.aws.amazon.com/xray/latest/devguide/aws-xray.html) and [CloudWatch](https://docs.aws.amazon.com/cloudwatch/). Each individual service sends telemetry to the [OpenTelemetry Collector](https://opentelemetry.io/docs/collector/), as there is no direct exporter package available for AWS X-Ray.

Check out each individual sample's README-file for more info.