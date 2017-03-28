using System;
using NServiceBus;
using NServiceBusTutorials.StepByStepExample.Domain;

namespace NServiceBusTutorials.StepByStepExample.Contracts.Commands
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
            : this(new Product(productId, productName))
        {
        }

        public Guid OrderId { get; set; }

        public Product Product { get; set; }
    }
}