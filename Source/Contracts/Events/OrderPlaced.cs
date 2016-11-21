using System;

using NServiceBus;

namespace Contracts.Events
{
    public class OrderPlaced : IEvent
    {
        public OrderPlaced(Guid orderId)
        {
            OrderId = orderId;
        }

        public Guid OrderId { get; set; }
    }
}