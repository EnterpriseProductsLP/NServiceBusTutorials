using System;
using System.Threading.Tasks;

using Autofac;

using NServiceBus;
using NServiceBus.Logging;

using NServiceBusTutorials.ActivePassive.Consumer.DependencyInjection;
using NServiceBusTutorials.ActivePassive.Consumer.Interfaces;
using NServiceBusTutorials.Common.Extensions;

namespace NServiceBusTutorials.ActivePassive.Consumer.MessageHandlers
{
    internal abstract class LockableMessageHandler<T> : IHandleMessages<T>
    {
        protected ILog Logger { get; } = LogManager.GetLogger<WorkCommandHandler>();

        public Task Handle(T message, IMessageHandlerContext context)
        {
                if (CanGetOrUpdateDistributedLock())
                {
                    return HandleInternal(message, context);
                }

                throw new Exception("This application does not have the distributed lock");
        }

        protected abstract Task HandleInternal(T message, IMessageHandlerContext context);

        private bool CanGetOrUpdateDistributedLock()
        {
            using (var lifetimeScope = ContainerProvider.Container.BeginLifetimeScope())
            {
                var distributedLockManager = lifetimeScope.Resolve<IManageDistributedLocks>();
                return distributedLockManager.GetOrMaintainLock().Inline();
            }
        }
    }
}