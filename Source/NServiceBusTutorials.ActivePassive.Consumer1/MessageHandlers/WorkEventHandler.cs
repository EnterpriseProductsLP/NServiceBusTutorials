using System;
using System.Threading.Tasks;

using NServiceBus;
using NServiceBus.Logging;

using NServiceBusTutorials.ActivePassive.Contracts;

namespace NServiceBusTutorials.ActivePassive.Consumer.MessageHandlers
{
    internal class WorkEventHandler : IHandleMessages<WorkEvent>
    {
        private static ILog log = LogManager.GetLogger<WorkEventHandler>();

        public Task Handle(WorkEvent message, IMessageHandlerContext context)
        {
            Console.WriteLine();

            log.Info($"SubscriberOne: Handled event: {message.Identifier}");
            return Task.CompletedTask;
        }
    }
}
