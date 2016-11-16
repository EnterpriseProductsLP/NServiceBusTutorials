using System.Threading.Tasks;
using Contracts.Events;
using NServiceBus;
using NServiceBus.Logging;

namespace Subscriber.MessageHandlers
{
    class OrderPlacedHandler : IHandleMessages<OrderPlaced>
    {
        private static ILog log = LogManager.GetLogger<OrderPlacedHandler>();
        public Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            log.Info($"Hadling: OrderPlaced for Order Id: {message.Id}");
            return Task.CompletedTask;
        }
    }
}
