using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBusTutorials.StepByStepExample.Contracts.Commands;
using NServiceBusTutorials.StepByStepExample.Contracts.Events;

namespace NServiceBusTutorials.StepByStepExample.Server.MessageHandlers
{
    internal class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        private static readonly ILog Log = LogManager.GetLogger<PlaceOrderHandler>();

        public Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            var orderId = message.OrderId;
            var productId = message.Product.Id;
            var productName = message.Product.Name;

            Console.WriteLine();

            Log.Info($"Order: {orderId} placed for Product:[Id: {productId}, Name: {productName}]");
            Log.Info($"Publishing: OrderPlaced for Order Id: {orderId}");

            var orderPlaced = new OrderPlaced(orderId);
            return context.Publish(orderPlaced);
        }
    }
}
