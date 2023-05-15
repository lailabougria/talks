# Resources

Here you can find a list of resources to further your understanding of these topics. I try to keep this list up to date with new resources I come across, if you can think of valuable additions, feel free to contribute!

## Fallacies of distributed computing

- [Fallacies of distributed computing on Wikipedia](https://en.wikipedia.org/wiki/Fallacies_of_distributed_computing)
- [Distributed Systems Design Fundamentals FREE course, by Udi Dahan](https://go.particular.net/Techoramabe-23)

## Patterns

- [Asynchronous request-response pattern, Microsoft docs](https://learn.microsoft.com/en-us/azure/architecture/patterns/async-request-reply)
- [Publish-subscribe pattern, Microsoft docs](https://learn.microsoft.com/en-us/azure/architecture/patterns/publisher-subscriber)
- [Saga distributed transactions pattern, Microsoft docs](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/saga/saga)
- [Sagas, Clemens Vasters](https://vasters.com/archive/Sagas.html)
- [Choreography](https://learn.microsoft.com/en-us/azure/architecture/patterns/choreography)
- [Idempotency patterns](https://blog.jonathanoliver.com/idempotency-patterns/)

## Additional messaging features

### Delayed delivery

- [Message deferral in Azure Service Bus](https://learn.microsoft.com/en-us/azure/service-bus-messaging/message-deferral)
- [Delay queues in Amazon SQS](https://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSDeveloperGuide/sqs-delay-queues.html)
- [Delayed delivery support in NServiceBus](https://docs.particular.net/nservicebus/messaging/delayed-delivery)

### Message deduplication & idempotency

- [Message deduplication in Azure Service Bus](https://learn.microsoft.com/en-us/azure/service-bus-messaging/duplicate-detection)
- [Message deduplication in Amazon SQS](https://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSDeveloperGuide/using-messagededuplicationid-property.html)
- [Message deduplication in NServiceBus with Outbox](https://docs.particular.net/nservicebus/outbox/)
- [Idempotent message processing, Azure Architecture Center](https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks-mission-critical/mission-critical-data-platform#idempotent-message-processing)

## Observability

I have two dedicated talks about observability and OpenTelemetry, including additional resources:
- [Message processing failed, but what's the root cause?](../../message-processing-failed-but-whats-the-root-cause)
- [How to effectively spy on your systems](../../how-to-effectively-spy-on-your-systems)

## Further reading

### Sessions

- [Finding your service boundaries, by Adam Ralph](https://www.youtube.com/watch?v=tVnIUZbsxWI)
- [Got the time?, by Mauro Servienti](https://particular.net/webinars/got-the-time)

### Blog posts

- [The pitfalls of request/response over messaging, Mauro Servienti](https://milestone.topics.it/2023/01/19/pitfalls-of-request-response-over-messaging.html)
- [Let's get logical! On logical and physical architectural views, Mauro Servienti](https://milestone.topics.it/2022/01/25/lets-get-logical.html)
- [You don't need ordered delivery, Particular blog](https://particular.net/blog/you-dont-need-ordered-delivery)
- [Outbox: what and why, Mauro Servienti](https://milestone.topics.it/2023/02/07/outbox-what-and-why.html)

### Books

- [Software Architecture: the hard parts, by Neal Ford, Mark Richards, Pramod Sadalage, Zhamak Dehghani](https://www.oreilly.com/library/view/software-architecture-the/9781492086888/)
