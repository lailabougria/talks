# Resources

You can find a list of resources to further your understanding of these topics. I try to keep this list up to date with new resources I come across, if you can think of valuable additions, feel free to contribute!

## Books

I haven't found any dedicated reading on this topic, but the following books do touch upon the topic.

- [Building data-intensive applications](https://www.amazon.com/Designing-Data-Intensive-Applications-Reliable-Maintainable/dp/1449373321), by Martin Kleppman
- [Patterns for API design](https://www.amazon.com/Patterns-API-Design-Simplifying-Addison-Wesley/dp/0137670109), by Olaf Zimmermann, Mirko Stocker, Daniel Lubke, Uwe Zdun, Cesare Pautasso

## Articles

The following articles are good further reading material connected to the session.

### Public versus private events

- [Data on the Outside vs. Data on the Inside](https://queue.acm.org/detail.cfm?id=3415014), by Pat Helland
- [Internal and external events, or how to design event-driven API](https://event-driven.io/en/internal_external_events/), by Oskar Dudycz
- [Events should be as small as possible, right?](https://event-driven.io/en/events_should_be_as_small_as_possible/), by Oskar Dudycz
- [Patterns for Decoupling in Distributed Systems: Summary Event](https://verraes.net/2019/05/patterns-for-decoupling-distsys-summary-event/), by Mathias Verraes
- [Patterns for Decoupling in Distributed Systems: Completeness Guarantee](https://verraes.net/2019/05/patterns-for-decoupling-distsys-completeness-guarantee/), by Mathias Verraes
- [Patterns for Decoupling in Distributed Systems: Explicit Public Events](https://verraes.net/2019/05/patterns-for-decoupling-distsys-explicit-public-events/), by Mathias Verraes
- [Event-Driven Architecture Gotcha! Inside or Outside Events](https://www.youtube.com/watch?v=qf-BSAhbrWw), by Derek Comartin

### Schema options

- [JSON Schema](https://json-schema.org/)
- [Avro Schema](https://avro.apache.org/docs/1.12.0/specification/)
- [Protobuf](https://protobuf.dev/overview/)

### Versioning strategies

- [Backward compatibility](https://docs.confluent.io/platform/current/schema-registry/fundamentals/schema-evolution.html#backward-compatibility)
- [Forward compatibility](https://docs.confluent.io/platform/current/schema-registry/fundamentals/schema-evolution.html#forward-compatibility)
- [Full compatibility](https://docs.confluent.io/platform/current/schema-registry/fundamentals/schema-evolution.html#full-compatibility)
- [Transitivity](https://docs.confluent.io/platform/current/schema-registry/fundamentals/schema-evolution.html#transitive-property)
- [Semantic Versioning for APIs](https://semver.org/)

### Schema registries

- [Apache Apicurio](https://www.apicur.io/registry/)
- [Schema Registry for the Confluent Platform](https://docs.confluent.io/platform/current/schema-registry/index.html)
- [Schema Registry feature in EventHubs](https://learn.microsoft.com/en-us/azure/event-hubs/schema-registry-overview)
- [AWS Glue Schema Registry](https://docs.aws.amazon.com/glue/latest/dg/schema-registry.html)

### xRegistry

- [xRegistry value proposition](https://github.com/xregistry/spec/blob/main/core/primer.md#value-proposition)
- xRegistry's [Core](https://github.com/xregistry/spec/blob/main/core), [Schema](https://github.com/xregistry/spec/blob/main/schema), [Message](https://github.com/xregistry/spec/blob/main/message) and [Endpoint](https://github.com/xregistry/spec/blob/main/endpoint) specifications
- xRegistry's [compatibility attribute](https://github.com/xregistry/spec/blob/main/core/spec.md#compatibility-attribute)
- xRegistry's [deprecation attribute](https://github.com/xregistry/spec/blob/main/core/spec.md#deprecated)
- xRegistry [server implementation](https://github.com/xregistry/server)
