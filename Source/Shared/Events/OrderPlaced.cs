using System;
using NServiceBus;

namespace Shared.Events
{
    public class OrderPlaced : IEvent
    {
        public Guid Id { get; set; }
    }
}