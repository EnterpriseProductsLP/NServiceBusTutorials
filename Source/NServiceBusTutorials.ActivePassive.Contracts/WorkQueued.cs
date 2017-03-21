using System;

using NServiceBus;

namespace NServiceBusTutorials.ActivePassive.Contracts
{
    public class WorkQueued : IEvent
    {
        public Guid Identifier { get; set; }
    }
}