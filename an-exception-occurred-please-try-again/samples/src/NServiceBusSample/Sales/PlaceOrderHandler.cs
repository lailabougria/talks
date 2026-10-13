using System;
using System.Threading.Tasks;
using Messages;
using NServiceBus;
using NServiceBus.Logging;
using Sales.Data;

namespace Sales
{
    public class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        static readonly ILog log = LogManager.GetLogger<PlaceOrderHandler>();
        static readonly Random random = new Random();

        private readonly SalesDataContext _dataContext;

        public PlaceOrderHandler(SalesDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            log.Info($"Received PlaceOrder, OrderId = {message.OrderId}");

            // Saved in the same transaction as the outgoing OrderPlaced event, when the handler completes
            _dataContext.Orders.Add(new Order { OrderId = message.OrderId, ReceivedAt = DateTimeOffset.UtcNow });

            var orderPlaced = new OrderPlaced
            {
                OrderId = message.OrderId
            };

            log.Info($"Publishing OrderPlaced, OrderId = {message.OrderId}");

            await context.Publish(orderPlaced);
        }
    }
}
