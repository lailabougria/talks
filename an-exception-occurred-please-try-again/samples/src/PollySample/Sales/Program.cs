using System.Diagnostics;
using System.Text.Json.Serialization;
using Polly;
using Polly.Registry;
using Sales.Orders;
using Sales.Resilience;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRetryScenarios();
builder.Services.AddRetryLogLevels();
builder.Services.AddSingleton<OrderRepository>();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.MapGet("/scenarios", () => RetryScenarios.All);

// Stores an order using the selected retry scenario, and reports every attempt that was made
app.MapPost("/orders", async (
    PlaceOrderRequest request,
    ResiliencePipelineProvider<string> pipelines,
    OrderRepository repository,
    CancellationToken cancellationToken) =>
{
    var pipeline = pipelines.GetPipeline(request.Scenario);
    var orderId = Guid.NewGuid().ToString()[..8];
    var attempts = new List<AttemptResult>();
    var stopwatch = Stopwatch.StartNew();

    // A ResilienceContext carries data through the pipeline, so strategy callbacks like OnRetry can read it
    // Adding the orderid allows us to propragate important context
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
                await repository.Save(orderId, request.Failure, attempt, context.CancellationToken);
                attempts.Add(new AttemptResult(attempt, startedAt, null));
            }
            catch (Exception exception)
            {
                attempts.Add(new AttemptResult(attempt, startedAt, $"{exception.GetType().Name}: {exception.Message}"));
                throw;
            }
        }, resilienceContext);

        return new PlaceOrderResponse(orderId, request.Scenario, Succeeded: true, attempts);
    }
    catch (Exception)
    {
        // Either the exception isn't handled by the scenario, or all retries were exhausted.
        // We return the attempts instead of an error, so the UI can show what happened.
        return new PlaceOrderResponse(orderId, request.Scenario, Succeeded: false, attempts);
    }
    finally
    {
        ResilienceContextPool.Shared.Return(resilienceContext);
    }
});

// Fails with a 503 Service Unavailable the first few times a request comes in,
// to show the retries configured on the HttpClient in ClientUI.
var flakyRequests = new Dictionary<string, int>();
app.MapPost("/orders/flaky", (FlakyOrderRequest request) =>
{
    int attempt;
    lock (flakyRequests)
    {
        flakyRequests[request.RequestId] = attempt = flakyRequests.GetValueOrDefault(request.RequestId) + 1;
    }

    app.Logger.LogInformation("Flaky request {RequestId}, attempt {Attempt}", request.RequestId, attempt);

    return attempt <= request.Failures
        ? Results.StatusCode(StatusCodes.Status503ServiceUnavailable)
        : Results.Ok(new FlakyOrderResponse(attempt));
});

app.Run();

record PlaceOrderRequest(string Scenario, SimulatedFailure Failure);

record PlaceOrderResponse(string OrderId, string Scenario, bool Succeeded, List<AttemptResult> Attempts);

record AttemptResult(int Attempt, TimeSpan StartedAt, string? Error);

record FlakyOrderRequest(string RequestId, int Failures);

record FlakyOrderResponse(int AttemptsReceived);
