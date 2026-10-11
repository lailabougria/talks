using ClientUI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClientUI.Controllers;

[Route("/")]
public class HomeController(SalesClient sales) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(new HomeViewModel { Scenarios = await sales.GetScenarios(cancellationToken) });

    [HttpPost("place-order")]
    public async Task<IActionResult> PlaceOrder(string scenario, ExceptionType exception, int failures, CancellationToken cancellationToken)
    {
        var result = await sales.PlaceOrder(scenario, exception, failures, cancellationToken);

        return View(nameof(Index), new HomeViewModel
        {
            Scenarios = await sales.GetScenarios(cancellationToken),
            Scenario = scenario,
            Exception = exception,
            Failures = failures,
            Result = result
        });
    }

    [HttpPost("place-flaky-order")]
    public async Task<IActionResult> PlaceFlakyOrder(int httpFailures, CancellationToken cancellationToken)
    {
        FlakyOrderResponse? result = null;
        string? error = null;
        try
        {
            result = await sales.PlaceFlakyOrder(httpFailures, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            // All HTTP retries were exhausted
            error = exception.Message;
        }

        return View(nameof(Index), new HomeViewModel
        {
            Scenarios = await sales.GetScenarios(cancellationToken),
            HttpFailures = httpFailures,
            HttpResult = result,
            HttpError = error
        });
    }
}
