using System;

namespace ClientUI.Data
{
    public class Order
    {
        public string OrderId { get; set; }
        public DateTimeOffset PlacedAt { get; set; }
    }
}
