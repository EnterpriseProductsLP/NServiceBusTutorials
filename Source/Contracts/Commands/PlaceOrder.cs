using System;
using Domain;
using NServiceBus;

namespace Contracts.Commands
{
    public class PlaceOrder : ICommand
    {
        public PlaceOrder()
        {
            OrderId = new Guid();
        }

        public PlaceOrder(Product product)
            : this()
        {
            Product = product;
        }

        public PlaceOrder(Guid productId, string productName)
            : this(new Product(id: productId, name: productName))
        {
        }

        public Guid OrderId { get; }

        public Product Product { get; }
    }
}