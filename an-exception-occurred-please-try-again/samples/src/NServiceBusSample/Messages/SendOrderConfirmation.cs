using NServiceBus;

namespace Messages
{
    public class SendOrderConfirmation : ICommand
    {
        public string OrderId { get; set; }
    }
}
