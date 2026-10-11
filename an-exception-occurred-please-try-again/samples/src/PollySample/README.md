# Polly sample

This sample showcases how to build retry strategies with [Polly](https://www.pollydocs.org/) v8, and how to plug them into ASP.NET Core using [Microsoft.Extensions.Resilience](https://www.nuget.org/packages/Microsoft.Extensions.Resilience) and [Microsoft.Extensions.Http.Resilience](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience).

- **ClientUI** (http://localhost:5090): an ASP.NET Core MVC website that calls Sales over HTTP.
- **Sales** (http://localhost:5091): an ASP.NET Core backend that stores orders in a simulated database, which fails a configurable number of times with a configurable exception.

Start both projects, browse to http://localhost:5090, pick a retry scenario and the failure to simulate, and place an order. 
The page will show each attempt, how long Polly waited before it, and the outcome.

## Retry scenarios in Sales

| Scenario | What it shows |
| --- | --- |
| `any-exception` | Retry on any exception, using the defaults (3 retries, constant 2 second delay) |
| `db-concurrency` | Retry only on `DBConcurrencyException`; other exceptions fail immediately |
| `combined-exceptions` | Handle `DBConcurrencyException` and `DbException` with the same retry strategy |
| `log-on-retry` | Log the exception, attempt number and delay using the `OnRetry` callback |
| `custom-wait-time` | Configure the number of retries and the delay, using a linear back-off |
| `constant-back-off` | The same delay between every retry |
| `exponential-back-off-with-jitter` | An exponential back-off with jitter |

For example, retrying on two exception types:

```c#
services.AddResiliencePipeline("combined-exceptions", pipeline => pipeline
    .AddRetry(new RetryStrategyOptions
    {
        ShouldHandle = new PredicateBuilder()
            .Handle<DBConcurrencyException>()
            .Handle<DbException>()
    }));
```

And using it:

```c#
var pipeline = pipelineProvider.GetPipeline("combined-exceptions");
await pipeline.ExecuteAsync(async token => await repository.Save(order, token), cancellationToken);
```

> **_NOTE:_** Pipelines registered through `AddResiliencePipeline` automatically emit telemetry. That's why the Sales console also logs a Polly warning for every failed attempt, regardless of the scenario. The `log-on-retry` scenario shows how to add your own logging on top of that.

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
