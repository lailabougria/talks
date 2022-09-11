using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Transport;
using System;
using System.Collections.Generic;

namespace Sales
{
    public class CustomRecoverability
    {
        static readonly ILog Logger = LogManager.GetLogger<CustomRecoverability>();
        static Random Randomizer = new Random();

        static Dictionary<int, Tuple<int, int>> BackOffPolicy = new Dictionary<int, Tuple<int, int>>
        {
            { 1, new Tuple<int, int>(1, 5) },
            { 2, new Tuple<int, int>(6, 10) },
            { 3, new Tuple<int, int>(11, 30) },
        };

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
    }
}