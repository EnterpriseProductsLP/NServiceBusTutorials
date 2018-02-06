using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBusTutorials.StepByStepExample.Contracts.Events;

namespace NServiceBusTutorials.StepByStepExample.Subscriber.MessageHandlers
{
    internal class OrderPlacedHandler : IHandleMessages<OrderPlaced>
    {
        private static readonly ILog Log = LogManager.GetLogger<OrderPlacedHandler>();

        public Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            throw new Exception("Subscriber could not handle message.");

            Console.WriteLine();

            Log.Info($"Handling: OrderPlaced for Order Id: {message.OrderId}");
            return Task.CompletedTask;
        }
    }
}