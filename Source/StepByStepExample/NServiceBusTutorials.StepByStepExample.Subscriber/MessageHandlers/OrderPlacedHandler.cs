using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBusTutorials.StepByStepExample.Contracts.Events;

namespace NServiceBusTutorials.StepByStepExample.Subscriber.MessageHandlers
{
    internal class OrderPlacedHandler : IHandleMessages<OrderPlaced>
    {
        private static ILog log = LogManager.GetLogger<OrderPlacedHandler>();

        public Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            Console.WriteLine();

            log.Info($"Handling: OrderPlaced for Order Id: {message.OrderId}");
            throw new Exception("Could not process order.");
            return Task.CompletedTask;
        }
    }
}