using NServiceBus;

namespace Events;

public interface IOrderCharged : IEvent
{
    Guid CustomerId { get; set; }
    OrderDetails Order { get; set; }
}