using Polly;
using Polly.Telemetry;

namespace Sales.Resilience;

public static class RetryLogLevels
{
    // The severity provider doesn't get the attempt number, so we count the failed attempts
    // of each execution in its ResilienceContext ourselves
    static readonly ResiliencePropertyKey<int> FailedAttempts = new("FailedAttempts");

    // From which failed attempt onwards we log a warning
    const int WarnFromAttempt = 3;

    // By default, Polly logs every failed attempt and retry as a warning, and the attempt that fails
    // after all retries are exhausted as an error. A few failures that get retried are expected,
    // so we log those as information and only warn when it keeps failing.
    public static IServiceCollection AddRetryLogLevels(this IServiceCollection services) =>
        services.Configure<TelemetryOptions>(options => options.SeverityProvider = args =>
        {
            // Leave errors (retries exhausted), successful attempts and other strategies' events as they are
            if (args.Event.EventName is not ("ExecutionAttempt" or "OnRetry")
                || args.Event.Severity != ResilienceEventSeverity.Warning)
            {
                return args.Event.Severity;
            }

            // Polly reports ExecutionAttempt and then OnRetry for every failed attempt that will be retried
            var failedAttempts = args.Context.Properties.GetValue(FailedAttempts, 0);
            if (args.Event.EventName == "ExecutionAttempt")
            {
                failedAttempts++;
                args.Context.Properties.Set(FailedAttempts, failedAttempts);
            }

            return failedAttempts < WarnFromAttempt ? ResilienceEventSeverity.Information : ResilienceEventSeverity.Warning;
        });
}
