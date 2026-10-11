using System.Data;
using System.Data.Common;

namespace Sales.Orders;

/// <summary>
/// Simulates storing an order in a database that fails a given number of times before succeeding.
/// </summary>
public class OrderRepository(ILogger<OrderRepository> logger)
{
    public Task Save(string orderId, SimulatedFailure failure, int attempt, CancellationToken cancellationToken)
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
}

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
