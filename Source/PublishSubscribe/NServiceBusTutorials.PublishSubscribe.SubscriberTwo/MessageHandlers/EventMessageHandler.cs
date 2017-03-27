using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBusTutorials.PublishSubscribe.Contracts.Events;

namespace NServiceBusTutorials.PublishSubscribe.SubscriberTwo.MessageHandlers
{
    internal class EventMessageHandler : IHandleMessages<EventMessage>
    {
        private static readonly ILog Log = LogManager.GetLogger<EventMessageHandler>();

        public Task Handle(EventMessage message, IMessageHandlerContext context)
        {
            Console.WriteLine();

            Log.Info($"SubscriberTwo: Handled event: {message.EventMessageId}");
            return Task.CompletedTask;
        }
    }
}
