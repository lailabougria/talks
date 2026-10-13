using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClientUI.Models;

namespace ClientUI;

public class SalesClient(HttpClient httpClient)
{
    static readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<IReadOnlyList<RetryScenario>> GetScenarios(CancellationToken cancellationToken) =>
        await httpClient.GetFromJsonAsync<List<RetryScenario>>("scenarios", jsonOptions, cancellationToken) ?? [];

    public async Task<OrderResult> PlaceOrder(string scenario, ExceptionType exception, int failures, CancellationToken cancellationToken)
    {
        var request = new { Scenario = scenario, Failure = new { Exception = exception, Times = failures } };
        var response = await httpClient.PostAsJsonAsync("orders", request, jsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<OrderResult>(jsonOptions, cancellationToken))!;
    }

    // Returns false when Sales still had a concurrency conflict after all its retries (409 Conflict)
    public async Task<bool> CancelOrder(string orderId, int conflicts, CancellationToken cancellationToken)
    {
        var request = new { Conflicts = conflicts };
        var response = await httpClient.PostAsJsonAsync($"orders/{Uri.EscapeDataString(orderId)}/cancel", request, jsonOptions, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }

    // Sales returns a 503 for the first few attempts; the resilience handler on this HttpClient retries them
    public async Task<FlakyOrderResponse> PlaceFlakyOrder(int failures, CancellationToken cancellationToken)
    {
        var request = new { RequestId = Guid.NewGuid().ToString(), Failures = failures };
        var response = await httpClient.PostAsJsonAsync("orders/flaky", request, jsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<FlakyOrderResponse>(jsonOptions, cancellationToken))!;
    }
}
