using Events;
using NServiceBus;
using NServiceBus.Logging;

namespace Sales;

public class OrderChargedHandler : IHandleMessages<IOrderCharged>
{
    private static readonly ILog log = LogManager.GetLogger<OrderChargedHandler>();

    public async Task Handle(IOrderCharged message, IMessageHandlerContext context)
    {
        log.Info($"Order '{message.Order.OrderId}' was placed.");

        await context.Publish<IOrderPlaced>(orderPlaced =>
        {
            orderPlaced.CustomerId = message.CustomerId;
            orderPlaced.Order = message.Order;
        });
    }
}