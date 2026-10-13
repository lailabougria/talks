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
    public const string LogWithContext = "log-with-context";
    public const string ConstantBackOff = "constant-back-off";
    public const string ExponentialBackOffWithJitter = "exponential-back-off-with-jitter";

    // The order being saved, which the /orders endpoint puts in the ResilienceContext
    public static readonly ResiliencePropertyKey<string> OrderId = new("OrderId");

    public static readonly IReadOnlyList<RetryScenario> All =
    [
        new(AnyException, "Retry on any exception",
            "Handles every exception, with 3 retries and a constant 2 second delay."),
        new(DbConcurrency, "Retry on DBConcurrencyException",
            "Only DBConcurrencyException is retried; any other exception fails immediately."),
        new(CombinedExceptions, "Retry on DBConcurrencyException or DbException",
            "Two exception types are handled by the same retry strategy."),
        new(LogWithContext, "Log with business context",
            "Polly already logs every retry, but not which order failed. The order id is passed in the ResilienceContext, so OnRetry can log it."),
        new(ConstantBackOff, "Constant back-off",
            "Waits the same amount of time (1 second) between every retry."),
        new(ExponentialBackOffWithJitter, "Exponential back-off with jitter",
            "Doubles the delay on every retry and adds randomness, so clients don't retry in lockstep."),
    ];

    public static IServiceCollection AddRetryScenarios(this IServiceCollection services)
    {
        // 1. Retry on any exception
        // MaxRetryAttempts and Delay are set to Polly's defaults, to show what you get when you leave them out
        services.AddResiliencePipeline(AnyException, pipeline => pipeline
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2)
            }));

        // 2. Retry on a specific exception
        services.AddResiliencePipeline(DbConcurrency, pipeline => pipeline
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<DBConcurrencyException>(),
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2)
            }));

        // 3. Handle multiple exception types the same way
        services.AddResiliencePipeline(CombinedExceptions, pipeline => pipeline
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<DBConcurrencyException>()
                    .Handle<DbException>(),
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2)
            }));

        // 4. Add business context to the retry logs
        // Polly's telemetry already logs every retry, but it doesn't know which order is being saved.
        // The /orders endpoint stores the order id in the ResilienceContext, and OnRetry reads it back.
        // The overload with a context gives access to the service provider, so we can resolve a logger.
        services.AddResiliencePipeline(LogWithContext, (pipeline, context) =>
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<RetryScenario>>();

            pipeline.AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                Delay = TimeSpan.FromSeconds(1),
                OnRetry = args =>
                {
                    logger.LogInformation(args.Outcome.Exception,
                        "Saving order {OrderId} failed on attempt {AttemptNumber}. Retrying in {RetryDelay}",
                        args.Context.Properties.GetValue(OrderId, "unknown"),
                        args.AttemptNumber + 1,
                        args.RetryDelay);
                    return ValueTask.CompletedTask;
                }
            });
        });

        // 5. Constant back-off: the same delay between every retry
        services.AddResiliencePipeline(ConstantBackOff, pipeline => pipeline
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Constant
            }));

        // 6. Exponential back-off with jitter
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
