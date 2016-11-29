using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBusTutorials.PublishSubscribe.Contracts.Events;

namespace NServiceBusTutorials.PublishSubscribe.SubscriberOne.MessageHandlers
{
    internal class EventMessageHandler : IHandleMessages<EventMessage>
    {
        private static ILog log = LogManager.GetLogger<EventMessageHandler>();

        public Task Handle(EventMessage message, IMessageHandlerContext context)
        {
            Console.WriteLine();

            log.Info($"SubscriberOne: Handled event: {message.EventMessageId}");
            return Task.CompletedTask;
        }
    }
}
