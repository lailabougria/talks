using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Polly;
using Polly.Registry;
using Sales.Resilience;

namespace Sales;

/// <summary>
/// Places and cancels orders in a simulated database that fails a given number of times before succeeding.
/// It shows the two ways to get hold of a resilience pipeline:
/// <list type="bullet">
/// <item><see cref="PlaceOrder"/> resolves a pipeline by name, because the UI picks the retry scenario at runtime.</item>
/// <item><see cref="Cancel"/> uses a pipeline that's injected directly, which is what you'll usually do.</item>
/// </list>
/// </summary>
public class Orders(
    ResiliencePipelineProvider<string> pipelines,
    // AddResiliencePipeline also registers every pipeline as a keyed service, using its name as the key
    [FromKeyedServices(RetryScenarios.DbConcurrency)] ResiliencePipeline cancelPipeline,
    ILogger<Orders> logger)
{
    // Records every attempt and returns them instead of throwing, so the UI can show what happened
    public async Task<OrderResult> PlaceOrder(string scenario, SimulatedFailure failure, CancellationToken cancellationToken)
    {
        var pipeline = pipelines.GetPipeline(scenario);
        var orderId = Guid.NewGuid().ToString()[..8];
        var attempts = new List<AttemptResult>();
        var stopwatch = Stopwatch.StartNew();

        // A ResilienceContext carries data through the pipeline, so strategy callbacks like OnRetry can read it
        // Adding the order id allows us to propagate important context
        var resilienceContext = ResilienceContextPool.Shared.Get(cancellationToken);
        resilienceContext.Properties.Set(RetryScenarios.OrderId, orderId);

        try
        {
            await pipeline.ExecuteAsync(async context =>
            {
                var attempt = attempts.Count + 1;
                var startedAt = stopwatch.Elapsed;
                try
                {
                    await Save(orderId, failure, attempt, context.CancellationToken);
                    attempts.Add(new AttemptResult(attempt, startedAt, null));
                }
                catch (Exception exception)
                {
                    attempts.Add(new AttemptResult(attempt, startedAt, $"{exception.GetType().Name}: {exception.Message}"));
                    throw;
                }
            }, resilienceContext);

            return new OrderResult(orderId, scenario, Succeeded: true, attempts);
        }
        catch (Exception)
        {
            // Either the exception isn't handled by the scenario, or all retries were exhausted
            return new OrderResult(orderId, scenario, Succeeded: false, attempts);
        }
        finally
        {
            ResilienceContextPool.Shared.Return(resilienceContext);
        }
    }

    // All it takes to retry an operation: execute it through the pipeline.
    // When all retries are exhausted, the last exception is thrown to the caller.
    public async Task Cancel(string orderId, int conflicts, CancellationToken cancellationToken)
    {
        var attempt = 0; // Only needed to simulate the conflicts

        await cancelPipeline.ExecuteAsync(
            async token => await MarkAsCancelled(orderId, conflicts, ++attempt, token),
            cancellationToken);
    }

    // Simulates storing the order, failing with the given exception the first few attempts
    Task Save(string orderId, SimulatedFailure failure, int attempt, CancellationToken cancellationToken)
    {
        if (attempt <= failure.Times)
        {
            throw failure.Exception switch
            {
                ExceptionType.Concurrency => new DBConcurrencyException($"Order {orderId} was modified by another process."),
                ExceptionType.Database => new SimulatedDbException("The connection to the database was lost."),
                ExceptionType.Timeout => new TimeoutException("The database didn't respond in time."),
                _ => new InvalidOperationException("Something unexpected went wrong.")
            };
        }

        logger.LogInformation("Order {OrderId} stored on attempt {Attempt}", orderId, attempt);
        return Task.CompletedTask;
    }

    // Simulates updating the order's status, which conflicts with another process the first few attempts
    Task MarkAsCancelled(string orderId, int conflicts, int attempt, CancellationToken cancellationToken)
    {
        if (attempt <= conflicts)
        {
            throw new DBConcurrencyException($"Order {orderId} was modified by another process.");
        }

        logger.LogInformation("Order {OrderId} cancelled on attempt {Attempt}", orderId, attempt);
        return Task.CompletedTask;
    }
}

public record OrderResult(string OrderId, string Scenario, bool Succeeded, List<AttemptResult> Attempts);

public record AttemptResult(int Attempt, TimeSpan StartedAt, string? Error);

public enum ExceptionType
{
    Generic,
    Concurrency,
    Database,
    Timeout
}

public record SimulatedFailure(ExceptionType Exception, int Times);

// DbException is abstract; database providers each throw their own subclass (e.g. SqlException)
public class SimulatedDbException(string message) : DbException(message);
