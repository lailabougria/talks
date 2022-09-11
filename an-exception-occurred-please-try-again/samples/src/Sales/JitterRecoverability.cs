using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Transport;
using System;
using System.Collections.Generic;

namespace Sales
{
    public class JitterRecoverability
    {
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
    }
}