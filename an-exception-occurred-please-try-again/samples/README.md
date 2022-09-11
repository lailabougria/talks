# Samples

This sample showcases how to build a retry policy with a randomized back-off time. The sample uses [NServiceBus](https://docs.particular.net/) on top of the [NServiceBus Learning transport](https://docs.particular.net/transports/learning/) which simulates queuing infrastructure by storing all message actions in the local file system.
For more information on how to use NServiceBus, check out the [quickstart](https://docs.particular.net/tutorials/quickstart/).

This sample demonstrates how to:

- Define a custom recoverability policy in NServiceBus
- A jittered exponential back-off retry mechanism
- A simple implementation of a randomized exponential back-off retry mechanism

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
static Random Jitterer = new Random();

public static RecoverabilityAction JitteredIncrementalBackOff(RecoverabilityConfig recoverabilityConfig, ErrorContext errorContext)
{
    var action = DefaultRecoverabilityPolicy.Invoke(recoverabilityConfig, errorContext);
    if (!(action is DelayedRetry))
    {
        // This policy only customizes DelayedRetries, so falling back to the default policy here
        return action;
    }

    var delay = TimeSpan.FromTicks(recoverabilityConfig.Delayed.TimeIncrease.Ticks * (errorContext.DelayedDeliveriesPerformed + 1));
    var jitteredDelay = delay + TimeSpan.FromMilliseconds(Jitterer.Next(0, 1000));
    return RecoverabilityAction.DelayedRetry(jitteredDelay);
}
```

## A randomized exponential back-off retry mechanism

The following example showcases a simple implementation that allows for full customization of the back-off time based on the amount of retries performed.

> **_NOTE:_**  This example showcases an overly simplified solution for sample-purposes. 

First, it defines a dictionary that stored the range in seconds to apply per retry attempt:

```c#
static Dictionary<int, Tuple<int, int>> BackOffPolicy = new Dictionary<int, Tuple<int, int>>
    {
        { 1, new Tuple<int, int>(1, 5) },
        { 2, new Tuple<int, int>(6, 10) },
        { 3, new Tuple<int, int>(11, 30) },
    };
```

The `RandomizedIncrementalBackOff`-method will first validate whether we're handling delayed retries; if not, it will return the default policy. 
Based on the delayed deliveries performed, the time range in seconds to wait before the next attempt is resolved.
The time range is used to generate a randomized nr of seconds to wait, which is then returned.
If no time range can be resolved, we fall back to the default policy.

```c#
public static RecoverabilityAction RandomizedIncrementalBackOff(RecoverabilityConfig recoverabilityConfig, ErrorContext errorContext)
{
    var action = DefaultRecoverabilityPolicy.Invoke(recoverabilityConfig, errorContext);
    if (!(action is DelayedRetry))
    {
        // This policy only customizes DelayedRetries, so falling back to the default policy here
        return action;
    }

    var nextDelayedRetryAttempt = errorContext.DelayedDeliveriesPerformed + 1;
    if (BackOffPolicy.ContainsKey(nextDelayedRetryAttempt))
    {
        var timeRangeToBackOff = BackOffPolicy[nextDelayedRetryAttempt];
        var timeToWait = Randomizer.Next(timeRangeToBackOff.Item1, timeRangeToBackOff.Item2);

        Logger.Info($"Retry will be scheduled in {timeToWait} seconds.");

        return RecoverabilityAction.DelayedRetry(TimeSpan.FromSeconds(timeToWait));
    }

    // Fallback to the default configured policy, in this sample, that would be 2 seconds * nrOfRetryAttempts
    Logger.Info($"Fallback to default delayed retries policy.");
    return action;
}
```

## Considerations when applying custom retry strategies

#### Don't retry for too long

There's no use in retrying forever. Instead, the total elapsed time when executing the maximum amount of retries should remain within the limits of the service's SLA.

#### Don't retry too many times

In high throughput systems, an overly aggressive retry strategy may incur further load on an already stressed system which may adversely affect the system. In such scenario's, the system is not allowed to recover from its stressed state, resulting in a vicious  circle.

#### Use the Exception to optimize the retry strategy

The `ErrorContext` exposes the Exception that occured when processing the message. If the type of Exception provides clear insights on the 'transientness' of an exception, e.g. HTTP 503 Service Unavailable or DBConcurrencyException, this information can be used to further optimize the retry policy.
