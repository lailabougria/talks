using System.Data;
using System.Text.Json.Serialization;
using Sales;
using Sales.Resilience;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRetryScenarios();
builder.Services.AddRetryLogLevels();
builder.Services.AddSingleton<Orders>();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.MapGet("/scenarios", () => RetryScenarios.All);

// Stores an order using the selected retry scenario, and reports every attempt that was made
app.MapPost("/orders", (PlaceOrderRequest request, Orders orders, CancellationToken cancellationToken) =>
    orders.PlaceOrder(request.Scenario, request.Failure, cancellationToken));

// Cancels an order, retrying concurrency conflicts
app.MapPost("/orders/{orderId}/cancel", async (string orderId, CancelOrderRequest request, Orders orders, CancellationToken cancellationToken) =>
{
    try
    {
        await orders.Cancel(orderId, request.Conflicts, cancellationToken);
        return Results.NoContent();
    }
    catch (DBConcurrencyException)
    {
        // All retries were exhausted. A 409 isn't a transient HTTP error, so ClientUI's
        // resilience handler won't retry it, which would start a new round of retries here.
        return Results.Conflict();
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

record CancelOrderRequest(int Conflicts);

record FlakyOrderRequest(string RequestId, int Failures);

record FlakyOrderResponse(int AttemptsReceived);
