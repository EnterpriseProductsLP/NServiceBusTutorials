using System;
using NServiceBus;

namespace Contracts.Commands
{
    public class PlaceOrder : ICommand
    {
        public PlaceOrder(Guid id, string product)
        {
            Id = id;
            Product = product;
        }

        public Guid Id { get; set; }

        public string Product { get; set; }
    }
}