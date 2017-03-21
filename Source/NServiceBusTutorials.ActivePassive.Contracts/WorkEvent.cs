using System;

using NServiceBus;

namespace NServiceBusTutorials.ActivePassive.Contracts
{
    public class WorkEvent : IEvent
    {
        public Guid Identifier { get; set; }
    }
}