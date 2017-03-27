using System;

using NServiceBus;

namespace NServiceBusTutorials.ActivePassive.Contracts
{
    public class WorkCommand : ICommand
    {
        public Guid Identifier { get; set; }
    }
}