using System;
using NServiceBus;

namespace NServiceBusTutorials.PublishSubscribe.Contracts.Events
{
    public class EventMessage : IEvent
    {
        public EventMessage(Guid eventMessageId)
        {
            EventMessageId = eventMessageId;
        }

        public Guid EventMessageId { get; set; }
    }
}
