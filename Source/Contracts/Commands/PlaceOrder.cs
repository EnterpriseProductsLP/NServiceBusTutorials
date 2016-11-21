using System;
using Domain;
using NServiceBus;

namespace Contracts.Commands
{
    [TimeToBeReceived("24:00:00")]
    public class PlaceOrder : ICommand
    {
        public PlaceOrder()
        {
            OrderId = Guid.NewGuid();
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

        public Guid OrderId { get; set; }

        public Product Product { get; set; }
    }
}