using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Autofac;

using NServiceBus;
using NServiceBus.Logging;

using NServiceBusTutorials.ActivePassive.Consumer.DependencyInjection;
using NServiceBusTutorials.ActivePassive.Consumer.Interfaces;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.Common.Extensions;

namespace NServiceBusTutorials.ActivePassive.Consumer.MessageHandlers
{
    internal abstract class LockableMessageHandler<T> : IHandleMessages<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static IList<string> _failedMessages = new List<string>();

        protected ILog Logger { get; } = LogManager.GetLogger<WorkCommandHandler>();

        public Task Handle(T message, IMessageHandlerContext context)
        {
            var messageIdentifier = GetMessageIdentifier(message);

                if (CanGetOrUpdateDistributedLock())
                {
                    HandleFailedMessageRetry(messageIdentifier);
                    return HandleInternal(message, context);
                }

                HandleFailedMessage(messageIdentifier);
                throw new Exception("This application does not have the distributed lock");
        }

        protected abstract string GetMessageIdentifier(T message);

        protected abstract Task HandleInternal(T message, IMessageHandlerContext context);

        private bool CanGetOrUpdateDistributedLock()
        {
            using (var lifetimeScope = ContainerProvider.Container.BeginLifetimeScope())
            {
                var distributedLockManager = lifetimeScope.Resolve<IManageDistributedLocks>();
                return distributedLockManager.GetOrMaintainLock().Inline();
            }
        }

        private static void HandleFailedMessageRetry(string messageIdentifier)
        {
            if (!_failedMessages.Contains(messageIdentifier))
            {
                return;
            }

            ConsoleUtilities.WriteLineWithColor($"Handled repeated message: {messageIdentifier}", ConsoleColor.Green);
        }

        private void HandleFailedMessage(string messageIdentifier)
        {
            ConsoleUtilities.WriteLineWithColor($"Could not handle message: {messageIdentifier}", ConsoleColor.Red);

            if (!_failedMessages.Contains(messageIdentifier))
            {
                _failedMessages.Add(messageIdentifier);
            }
        }
    }
}