# Message processing failed... But what's the root cause?

![root cause](root-cause-banner.jpg) 

## Abstract

How do you investigate failures in a distributed system? If your first thought is to look at the call stack, then good luck... In a distributed system, there is no such thing as a single call stack! Instead, it is scattered across multiple services that tackle a specific concern and communicate through a continuous stream of messages that flow through the system. That call stack becomes a haystack, so how do you find the proverbial needle?

Luckily, there are techniques and tools to regain the overview we lost. In this session, we'll look at modeling techniques, integration testing, and a deep dive into instrumentation with OpenTelemetry to help create visibility into your entire distributed system. And even if you're not (yet) using messaging in your architecture, you'll walk away with concrete takeaways around system observability that you can use in other architectures as well.

## Is there a recording?

A recording of this talk from NDC Oslo is available on [their YouTube channel](https://youtu.be/U8Aame0akl4).

## Additional information

Make sure to check the additional [resources](resources) for this topic as well as the [sample](sample).
