// © Copyright 2012, Enterprise Products Partners L.P. (Enterprise), All Rights Reserved.
// Permission to use, copy, modify, or distribute this software source code, binaries or 
// related documentation, is strictly prohibited, without written consent from Enterprise. 
// For inquiries about the software, contact Enterprise: Enterprise Products Company Law
// Department, 1100 Louisiana, 10th Floor, Houston, Texas 77002, phone 713-381-6500.

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
            log.Info($"Order for Product:{message.Product} placed with id: {message.Id}");
            log.Info($"Publishing: OrderPlaced for Order Id: {message.Id}");

            var orderPlaced = new OrderPlaced(orderId: message.Id);
            return context.Publish(orderPlaced);
        }
    }
}