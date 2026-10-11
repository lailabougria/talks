using System.Data;
using System.Data.Common;
using Polly;
using Polly.Retry;

namespace Sales.Resilience;

public record RetryScenario(string Name, string Title, string Description);

public static class RetryScenarios
{
    public const string AnyException = "any-exception";
    public const string DbConcurrency = "db-concurrency";
    public const string CombinedExceptions = "combined-exceptions";
    public const string LogOnRetry = "log-on-retry";
    public const string CustomWaitTime = "custom-wait-time";
    public const string ConstantBackOff = "constant-back-off";
    public const string ExponentialBackOffWithJitter = "exponential-back-off-with-jitter";

    public static readonly IReadOnlyList<RetryScenario> All =
    [
        new(AnyException, "Retry on any exception",
            "Handles every exception. Uses the defaults: 3 retries with a constant 2 second delay."),
        new(DbConcurrency, "Retry on DBConcurrencyException",
            "Only DBConcurrencyException is retried; any other exception fails immediately."),
        new(CombinedExceptions, "Retry on DBConcurrencyException or DbException",
            "Two exception types are handled by the same retry strategy."),
        new(LogOnRetry, "Log on every retry",
            "Uses the OnRetry callback to log the exception, attempt number and delay before each retry."),
        new(CustomWaitTime, "Custom wait time",
            "Configures the number of retries and a linear back-off: 500ms, 1s, 1.5s, 2s."),
        new(ConstantBackOff, "Constant back-off",
            "Waits the same amount of time (1 second) between every retry."),
        new(ExponentialBackOffWithJitter, "Exponential back-off with jitter",
            "Doubles the delay on every retry and adds randomness, so clients don't retry in lockstep."),
    ];

    public static IServiceCollection AddRetryScenarios(this IServiceCollection services)
    {
        // 1. Retry on any exception
        // The defaults are 3 retries, with a constant delay of 2 seconds between them.
        services.AddResiliencePipeline(AnyException, pipeline => pipeline
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>()
            }));

        // 2. Retry on a specific exception
        services.AddResiliencePipeline(DbConcurrency, pipeline => pipeline
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<DBConcurrencyException>()
            }));

        // 3. Handle multiple exception types the same way
        services.AddResiliencePipeline(CombinedExceptions, pipeline => pipeline
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<DBConcurrencyException>()
                    .Handle<DbException>()
            }));

        // 4. Log something before every retry
        // The overload with a context gives access to the service provider, so we can resolve a logger.
        services.AddResiliencePipeline(LogOnRetry, (pipeline, context) =>
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<RetryScenario>>();

            pipeline.AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                Delay = TimeSpan.FromSeconds(1),
                OnRetry = args =>
                {
                    logger.LogWarning(args.Outcome.Exception,
                        "Attempt {AttemptNumber} failed. Retrying in {RetryDelay}",
                        args.AttemptNumber + 1,
                        args.RetryDelay);
                    return ValueTask.CompletedTask;
                }
            });
        });

        // 5. Configure the number of retries and the wait time between them
        services.AddResiliencePipeline(CustomWaitTime, pipeline => pipeline
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                MaxRetryAttempts = 4,
                Delay = TimeSpan.FromMilliseconds(500),
                BackoffType = DelayBackoffType.Linear
            }));

        // 6. Constant back-off: the same delay between every retry
        services.AddResiliencePipeline(ConstantBackOff, pipeline => pipeline
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Constant
            }));

        // 7. Exponential back-off with jitter
        // With an exponential back-off, UseJitter uses the "decorrelated jitter" algorithm,
        // which originated in Polly.Contrib.WaitAndRetry and is now built into Polly.
        services.AddResiliencePipeline(ExponentialBackOffWithJitter, pipeline => pipeline
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                MaxRetryAttempts = 4,
                Delay = TimeSpan.FromMilliseconds(500),
                MaxDelay = TimeSpan.FromSeconds(5),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            }));

        return services;
    }
}
