# Resources

Here you can find a list of resources to further your understanding of these topics. I try to keep this list up to date with new resources I come across, if you can think of valuable additions, feel free to contribute!

## Fallacies of distributed computing

- [Fallacies of distributed computing on Wikipedia](https://en.wikipedia.org/wiki/Fallacies_of_distributed_computing)
- [The network is reliable, by Udi Dahan](https://www.youtube.com/watch?v=8fRzZtJ_SLk)
- [Distributed Systems Design Fundementals course, by Udi Dahan](https://learn.particular.net/courses/distributed-systems-design-fundamentals-online)

## About exceptions

- [But, all my errors are severe, by David Boike, Particular Software](https://particular.net/blog/but-all-my-errors-are-severe)
- [Transient fault guidance, Microsoft docs](https://docs.microsoft.com/en-us/azure/architecture/best-practices/transient-faults)
- [Rate limiting pattern, Microsoft docs](https://docs.microsoft.com/en-us/azure/architecture/patterns/rate-limiting-pattern)
- [Throttling, Microsoft docs](https://docs.microsoft.com/en-us/azure/architecture/patterns/throttling)

## Resilience patterns

The following resources document several resilience patterns that can be applied in software. Each provides its own value, and ideally, multiple resilience patterns are applied to software simultaneously. 

- [Async request-reply, Microsoft docs](https://docs.microsoft.com/en-us/azure/architecture/patterns/async-request-reply)
- [Publish-subscribe, Microsoft docs](https://docs.microsoft.com/en-us/azure/architecture/patterns/publisher-subscriber)
- [Retry pattern, Microsoft docs](https://docs.microsoft.com/en-us/azure/architecture/patterns/retry)
- [Circuit breaker pattern, Microsoft docs](https://docs.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker)
- [Avoiding fallback in distributed systems, AWS](https://aws.amazon.com/builders-library/avoiding-fallback-in-distributed-systems/)
- [Timeouts, retries and backoff with Jitter, AWS](https://aws.amazon.com/builders-library/timeouts-retries-and-backoff-with-jitter/)
- [Exponential backoff and jitter, AWS](https://aws.amazon.com/blogs/architecture/exponential-backoff-and-jitter/)

## Frameworks and services

### Polly

- [Polly](http://www.thepollyproject.org/)
- [Polly GitHub repo](https://github.com/App-vNext/Polly#resilience-policies)

### NServiceBus

- [NServiceBus quickstart](https://docs.particular.net/tutorials/quickstart)
- [NServiceBus recoverability](https://docs.particular.net/nservicebus/recoverability/)

### Messaging services in the Azure Stack

- [Overview of available messaging services on Azure, Microsoft docs](https://azure.microsoft.com/en-us/solutions/messaging-services/#products)
- [Azure Storage Queues vs Azure Service Bus, Microsoft docs](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-azure-and-service-bus-queues-compared-contrasted)
- [Event Grid vs Event Hubs vs and Service Bus, Microsoft docs](https://docs.microsoft.com/en-us/azure/event-grid/compare-messaging-services)
- [List of retry capabilities per Azure service, Microsoft docs](https://docs.microsoft.com/en-us/azure/architecture/best-practices/retry-service-specific)
