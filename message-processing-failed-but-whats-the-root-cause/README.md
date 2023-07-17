# Message processing failed... But what's the root cause?

![root cause](root-cause-banner.jpg) 

## Abstract

How do you investigate failures in a distributed system? If your first thought is to look at the call stack, then good luck... In a distributed system, there is no such thing as a single call stack! Instead, it is scattered across multiple services that tackle a specific concern and communicate through a continuous stream of messages that flow through the system. That call stack becomes a haystack, so how do you find the proverbial needle?

Luckily, there are techniques and tools to regain the overview we lost. In this session, we'll look at modeling techniques, integration testing, and a deep dive into instrumentation with OpenTelemetry to help create visibility into your entire distributed system. And even if you're not (yet) using messaging in your architecture, you'll walk away with concrete takeaways around system observability that you can use in other architectures as well.

## Where?

This session was presented at the following conferences and/or user groups:

- [Techorama](https://techorama.be/), May 23-25, 2022, Antwerp, Belgium
- [.NET Day Switzerland](https://dotnetday.ch/), August 30th, 2022, Zürich, Switzerland
- [Full Stack Europe](https://fullstackeurope.com/2022/speakers/laila-bougria), October 6-7, 2022, Antwerp, Belgium
- [Techorama](https://techorama.nl/), October 10-12, 2022, Utrecht, Netherlands
- [.NET DeveloperDays](https://net.developerdays.pl/), October 18-19, 2022, Warsaw, Poland
- [JetBrains .NET Days Online](https://pages.jetbrains.com/dotnet-days-2022), October 26-27, Online
- [NDC London](https://ndclondon.com/agenda/message-processing-failed-but-whats-the-root-cause-0sf6/0tqekbn4me8), January 23-27, 2023, London, United Kingdom
- [Queaso GetTogether](https://www.queaso.be/news/MessageProcessing), March 30th, 2023, Destelbergen, Belgium
- [NDC Oslo](https://ndcoslo.com/agenda/message-processing-failed-but-whats-the-root-cause-05sa/0y2g4ftymzi), May 22-26, 2023, Oslo, Norway
- [.NET Enterprise Developer Day](https://www.developerweek.com/cloudx/net-enterprise-developer-day/), August 22, 2023, Online

A recording of this talk from NDC Oslo is available on [their YouTube channel](https://youtu.be/U8Aame0akl4).

## Additional information

Make sure to check the additional [resources](resources) for this topic as well as the [sample](sample).
