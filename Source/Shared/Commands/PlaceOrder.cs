using System;
using NServiceBus;

namespace Shared.Commands
{
    public class PlaceOrder : ICommand
    {
        public Guid Id { get; set; }

        public string Product { get; set; }
    }
}