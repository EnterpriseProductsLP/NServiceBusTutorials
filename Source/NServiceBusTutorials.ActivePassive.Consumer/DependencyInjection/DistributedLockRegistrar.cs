using Autofac;
using NServiceBusTutorials.ActivePassive.Consumer.Interfaces;

namespace NServiceBusTutorials.ActivePassive.Consumer.DependencyInjection
{
    public class DistributedLockRegistrar : IRegisterModules
    {
        public void Register(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<DistributedLockManager>().As<IManageDistributedLocks>();
        }
    }
}