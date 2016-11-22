using System;

using NServiceBus;

namespace Contracts.Events
{
    [TimeToBeReceived("24:00:00")]
    public class OrderPlaced : IEvent
    {
        public OrderPlaced(Guid orderId)
        {
            OrderId = orderId;
        }

        public Guid OrderId { get; set; }
    }
}