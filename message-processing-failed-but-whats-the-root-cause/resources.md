# Resources

I have received multiple requests for additional resources on the topics I touched on during the talk, especially regarding testing and OpenTelemetry.
Here you can find a list of resources to further your understanding in these topics.

## Testing message-driven systems

### Unit testing

If you're using NServiceBus, in order to unit test messsage handlers and sagas, you can make use of the [testing framework](https://docs.particular.net/nservicebus/testing/) provided by the platform.

To perform integration- or end-to-end testing to ensure things like your the overall business process behavior and orchestration of your messages works correctly, you can use the [NServiceBus integration testing framework by Mauro Servienti](https://github.com/mauroservienti/NServiceBus.IntegrationTesting). This framework also allows you to test concerns like endpoint configuration, message routing, subscriptions, and more.

## OpenTelemetry

OpenTelemetry is a very wide and interesting topic, and I have quite a few resources to share in this area.

