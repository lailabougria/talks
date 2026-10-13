using System;

namespace Sales.Data
{
    public class Order
    {
        public string OrderId { get; set; }
        public DateTimeOffset ReceivedAt { get; set; }
    }
}
