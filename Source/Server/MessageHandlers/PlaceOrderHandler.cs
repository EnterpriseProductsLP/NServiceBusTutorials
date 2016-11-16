using System.Threading.Tasks;
using Contracts.Commands;
using Contracts.Events;
using NServiceBus;
using NServiceBus.Logging;

namespace Server.MessageHandlers
{
    public class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        private static ILog log = LogManager.GetLogger<PlaceOrderHandler>();

        public Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            var orderId = message.OrderId;
            var productId = message.Product.Id;
            var productName = message.Product.Name;

            log.Info($"Order: {orderId} placed for Product:[Id: {productId}, Name: {productName}]");
            log.Info($"Publishing: OrderPlaced for Order Id: {orderId}");

            var orderPlaced = new OrderPlaced(orderId: orderId);
            return context.Publish(message: orderPlaced);
        }
    }
}