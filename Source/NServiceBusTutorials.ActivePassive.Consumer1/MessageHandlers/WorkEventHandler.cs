using System;
using System.Threading.Tasks;

using NServiceBus;

using NServiceBusTutorials.ActivePassive.Contracts;

namespace NServiceBusTutorials.ActivePassive.Consumer.MessageHandlers
{
    internal class WorkEventHandler : LockableMessageHandler<WorkEvent>
    {
        protected override string GetMessageIdentifier(WorkEvent message)
        {
            return message.Identifier.ToString();
        }

        protected override Task HandleInternal(WorkEvent message, IMessageHandlerContext context)
        {
            Console.WriteLine();

            Logger.Info($"SubscriberOne: Handled event: {message.Identifier}");
            return Task.CompletedTask;
        }
    }
}