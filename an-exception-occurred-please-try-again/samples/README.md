# Samples

This sample showcases how to build a retry policy with a randomized back-off time. The sample uses [NServiceBus](https://docs.particular.net/) on top of the [NServiceBus Learning transport](https://docs.particular.net/transports/learning/) which simulates queuing infrastructure by storing all message actions in the local file system.
For more information on how to use NServiceBus, check out the [quickstart](https://docs.particular.net/tutorials/quickstart/).

This sample demonstrates how to:

- Define a custom recoverability policy in NServiceBus
- A jittered exponential back-off retry mechanism

## Defining a custom recoverability policy

The snippet code shows how to configure the default recoverability settings for retries in NServiceBus as well as a custom policy.
These are both being set because this implementation of the custom policy falls back to the default policy when it can't resolve the action to apply.

```c#
var recoverablility = endpointConfiguration.Recoverability();
recoverablility.Immediate(immediateRetries => immediateRetries.NumberOfRetries(0));
recoverablility.Delayed(
   delayed =>
   {
       delayed.NumberOfRetries(4);
       delayed.TimeIncrease(TimeSpan.FromSeconds(2));
   }
);
recoverablility.CustomPolicy(CustomRecoverability.RandomizedIncrementalBackOff);
```

## Jittered exponential back-off retry mechanism

This sample applies the default recoverability policy, in this case, an exponential time increase of 2 seconds based on the number of retries performed. Before returning, it will add some jitter to the delay, to avoid retries to be scheduled at the same time.

> **_NOTE:_** This sample illustrates a simplified way to add Jitter. There are [sophisticated Jitter algorithms](https://aws.amazon.com/blogs/architecture/exponential-backoff-and-jitter/) available that are better fit for production environments with high throughput.

```c#
public static RecoverabilityAction JitteredIncrementalBackOff(RecoverabilityConfig recoverabilityConfig, ErrorContext errorContext)
{
    var action = DefaultRecoverabilityPolicy.Invoke(recoverabilityConfig, errorContext);
    if (!(action is DelayedRetry))
    {
        // This policy only customizes DelayedRetries, so falling back to the default policy here
        return action;
    }

    var delay = TimeSpan.FromTicks(recoverabilityConfig.Delayed.TimeIncrease.Ticks * (errorContext.DelayedDeliveriesPerformed + 1));
    var jitteredDelay = delay + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));
    return RecoverabilityAction.DelayedRetry(jitteredDelay);
}
```

PS: There's an [issue on the NServiceBus repo for out-of-the-box Jitter support](https://github.com/Particular/NServiceBus/issues/6534). Feel free to go give it a thumbs up :wink:

## Considerations when applying custom retry strategies

#### Don't retry for too long

There's no use in retrying forever. Instead, the total elapsed time when executing the maximum amount of retries should remain within the limits of the service's SLA.

#### Don't retry too many times

In high throughput systems, an overly aggressive retry strategy may incur further load on an already stressed system which may adversely affect the system. In such scenario's, the system is not allowed to recover from its stressed state, resulting in a vicious  circle.

#### Use the Exception to optimize the retry strategy

The `ErrorContext` exposes the Exception that occured when processing the message. If the type of Exception provides clear insights on the 'transientness' of an exception, e.g. HTTP 503 Service Unavailable or DBConcurrencyException, this information can be used to further optimize the retry policy.
