using System;
using NServiceBus;

namespace Contracts.Events
{
    public class OrderPlaced : IEvent
    {
        private Guid OrderId;

        public OrderPlaced(Guid orderId)
        {
            OrderId = orderId;
        }

        public Guid Id { get; set; }
    }
}