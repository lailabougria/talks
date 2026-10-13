using System.Threading.Tasks;
using Messages;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace ClientUI.Handlers
{
    public class SendOrderConfirmationHandler(ILogger<SendOrderConfirmationHandler> logger) : IHandleMessages<SendOrderConfirmation>
    {
        public Task Handle(SendOrderConfirmation message, IMessageHandlerContext context)
        {
            logger.LogInformation("Sending order confirmation, OrderId = {OrderId}", message.OrderId);
            return Task.CompletedTask;
        }
    }
}
