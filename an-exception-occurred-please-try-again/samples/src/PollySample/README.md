# Polly sample

This sample showcases how to build retry strategies with [Polly](https://www.pollydocs.org/) v8, and how to plug them into ASP.NET Core using [Microsoft.Extensions.Resilience](https://www.nuget.org/packages/Microsoft.Extensions.Resilience) and [Microsoft.Extensions.Http.Resilience](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience).

- **ClientUI** (http://localhost:5090): an ASP.NET Core MVC website that calls Sales over HTTP.
- **Sales** (http://localhost:5091): an ASP.NET Core backend that stores orders in a simulated database, which fails a configurable number of times with a configurable exception.

Start both projects, browse to http://localhost:5090, pick a retry scenario and the failure to simulate, and place an order. 
The page will show each attempt, how long Polly waited before it, and the outcome.

## Retry scenarios in Sales

| Scenario | What it shows |
| --- | --- |
| `any-exception` | Retry on any exception, with 3 retries and a constant 2 second delay (Polly's defaults) |
| `db-concurrency` | Retry only on `DBConcurrencyException`; other exceptions fail immediately |
| `combined-exceptions` | Handle `DBConcurrencyException` and `DbException` with the same retry strategy |
| `log-with-context` | Pass the order id through a `ResilienceContext`, so the `OnRetry` callback can log which order is being retried |
| `constant-back-off` | The same delay between every retry |
| `exponential-back-off-with-jitter` | An exponential back-off with jitter |

For example, retrying on two exception types:

```c#
services.AddResiliencePipeline("combined-exceptions", pipeline => pipeline
    .AddRetry(new RetryStrategyOptions
    {
        ShouldHandle = new PredicateBuilder()
            .Handle<DBConcurrencyException>()
            .Handle<DbException>(),
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(2)
    }));
```

And using it:

```c#
var pipeline = pipelineProvider.GetPipeline("combined-exceptions");
await pipeline.ExecuteAsync(async token => await repository.Save(order, token), cancellationToken);
```

### Logging and telemetry

Pipelines registered through `AddResiliencePipeline` automatically emit telemetry, so Polly already logs every failed attempt and every retry, regardless of the scenario. There's no need to log that yourself. Two things are worth adding, though.

**Business context.** Polly doesn't know which order it's retrying. A `ResilienceContext` carries data through the pipeline, so callbacks such as `OnRetry` can read it:

```c#
var context = ResilienceContextPool.Shared.Get(cancellationToken);
context.Properties.Set(RetryScenarios.OrderId, orderId);
try
{
    await pipeline.ExecuteAsync(async context => await repository.Save(order, context.CancellationToken), context);
}
finally
{
    ResilienceContextPool.Shared.Return(context);
}
```

And in the `log-with-context` scenario:

```c#
OnRetry = args =>
{
    logger.LogInformation(args.Outcome.Exception, "Saving order {OrderId} failed on attempt {AttemptNumber}. Retrying in {RetryDelay}",
        args.Context.Properties.GetValue(OrderId, "unknown"), args.AttemptNumber + 1, args.RetryDelay);
    return ValueTask.CompletedTask;
}
```

Use the `ResilienceContext` that Polly passes to the callback, including its `CancellationToken`, rather than capturing the outer one. That way, cancellation from strategies such as timeouts reaches your code.

**Log levels.** By default, Polly logs every failed attempt and retry as a warning, and the attempt that fails after all retries are exhausted as an error. A few failures that get retried are expected, so Sales logs the first two as information and only warns from the third failed attempt onwards. With 4 retries that all fail, you'll see `info`, `info`, `warn`, `warn` and then `error`.

Polly lets you change the severity of its telemetry events through a `SeverityProvider`, but it doesn't pass the attempt number. Sales therefore counts the failed attempts of each execution in the `ResilienceContext` (see `Sales/Resilience/RetryLogLevels.cs`):

```c#
builder.Services.Configure<TelemetryOptions>(options => options.SeverityProvider = args =>
{
    if (args.Event.EventName is not ("ExecutionAttempt" or "OnRetry")
        || args.Event.Severity != ResilienceEventSeverity.Warning)
    {
        return args.Event.Severity;
    }

    var failedAttempts = args.Context.Properties.GetValue(FailedAttempts, 0);
    if (args.Event.EventName == "ExecutionAttempt")
    {
        failedAttempts++;
        args.Context.Properties.Set(FailedAttempts, failedAttempts);
    }

    return failedAttempts < WarnFromAttempt ? ResilienceEventSeverity.Information : ResilienceEventSeverity.Warning;
});
```

### Jitter

Setting `UseJitter = true` with `BackoffType = DelayBackoffType.Exponential` uses the decorrelated jitter algorithm that originated in [Polly.Contrib.WaitAndRetry](https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry). It's built into Polly v8, so the contrib package is no longer needed. Place the same order a few times to see the delays change.

## HTTP retries in ClientUI

ClientUI registers a typed `HttpClient` for Sales with the standard resilience handler:

```c#
builder.Services.AddHttpClient<SalesClient>(client => client.BaseAddress = salesUrl)
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.Delay = TimeSpan.FromMilliseconds(500);
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(90);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(60);
    });
```

The standard handler combines five strategies, from outer to inner: a rate limiter, a total request timeout, retries, a circuit breaker, and a timeout per attempt. Its retries handle transient HTTP failures by default: `HttpRequestException`, and `408`, `429` and `5xx` responses. The second form on the page calls a Sales endpoint that responds with `503 Service Unavailable` a configurable number of times, so you can see the retries succeed, or get exhausted.

The defaults are sensible for most HTTP clients, but two of them don't fit this sample:

- The default retry delay of 2 seconds makes the demo slow.
- Sales already retries the database itself, and the `exponential-back-off-with-jitter` scenario can take up to ~20 seconds. The default attempt timeout of 10 seconds would cancel that request and send a new one, which starts a whole new round of retries in Sales. When you retry at more than one layer, the outer timeout must be longer than the inner layer's worst-case retry time.

The circuit breaker's sampling duration must be at least twice the attempt timeout, so it's raised as well. The options are validated when the `HttpClient` is first created.

To configure each strategy yourself, use `AddResilienceHandler("sales", pipeline => pipeline.AddRetry(new HttpRetryStrategyOptions { ... }))` instead.
