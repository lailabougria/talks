namespace ClientUI.Models;

public record RetryScenario(string Name, string Title, string Description);

public enum ExceptionType
{
    Generic,
    Concurrency,
    Database,
    Timeout
}

public record OrderResult(string OrderId, string Scenario, bool Succeeded, List<AttemptResult> Attempts);

public record AttemptResult(int Attempt, TimeSpan StartedAt, string? Error);

public record FlakyOrderResponse(int AttemptsReceived);

public class HomeViewModel
{
    public IReadOnlyList<RetryScenario> Scenarios { get; init; } = [];

    public string? Scenario { get; init; }
    public ExceptionType Exception { get; init; }
    public int Failures { get; init; } = 2;
    public OrderResult? Result { get; init; }

    public int Conflicts { get; init; } = 2;
    public string? CancelledOrderId { get; init; }
    public bool Cancelled { get; init; }

    public int HttpFailures { get; init; } = 2;
    public FlakyOrderResponse? HttpResult { get; init; }
    public string? HttpError { get; init; }
}
