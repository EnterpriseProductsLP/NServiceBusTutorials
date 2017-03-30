using System;
using System.Threading.Tasks;

using NServiceBus;

using NServiceBusTutorials.ActivePassive.Contracts;

namespace NServiceBusTutorials.ActivePassive.Consumer.MessageHandlers
{
    internal class WorkCommandHandler : LockableMessageHandler<WorkCommand>
    {
        protected override Task HandleInternal(WorkCommand message, IMessageHandlerContext context)
        {
            Console.WriteLine();

            Logger.Info($"Handled event: {message.Identifier}");
            return Task.CompletedTask;
        }
    }
}