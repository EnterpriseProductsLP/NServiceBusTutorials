using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Autofac;

using NServiceBus;
using NServiceBus.Logging;

using NServiceBusTutorials.ActivePassive.Consumer.DependencyInjection;
using NServiceBusTutorials.ActivePassive.Consumer.Interfaces;

namespace NServiceBusTutorials.ActivePassive.Consumer.MessageHandlers
{
    internal abstract class LockableMessageHandler<T> : IHandleMessages<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static IList<string> _failedMessages = new List<string>();

        public ILog Logger { get; } = LogManager.GetLogger<WorkCommandHandler>();

        public Task Handle(T message, IMessageHandlerContext context)
        {
            var messageIdentifier = GetMessageIdentifier(message);

            using (var lifetimeScope = ContainerProvider.Container.BeginLifetimeScope())
            {
                var distributedLockManager = lifetimeScope.Resolve<IManageDistributedLocks>();
                if (distributedLockManager.GetOrMaintainLock())
                {
                    HandleFailedMessageRetry(messageIdentifier);
                    return HandleInternal(message, context);
                }

                HandleFailedMessage(messageIdentifier);
                throw new Exception("This application does not have the distributed lock");
            }
        }

        protected abstract string GetMessageIdentifier(T message);

        protected abstract Task HandleInternal(T message, IMessageHandlerContext context);

        private static void HandleFailedMessageRetry(string messageIdentifier)
        {
            if (!_failedMessages.Contains(messageIdentifier))
            {
                return;
            }

            var consoleColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Handled repeated message: {messageIdentifier}");
            Console.ForegroundColor = consoleColor;
            _failedMessages.Remove(messageIdentifier);
        }

        private void HandleFailedMessage(string messageIdentifier)
        {
            Logger.Error($"Could not handle message: {messageIdentifier}");

            if (!_failedMessages.Contains(messageIdentifier))
            {
                _failedMessages.Add(messageIdentifier);
            }
        }
    }
}