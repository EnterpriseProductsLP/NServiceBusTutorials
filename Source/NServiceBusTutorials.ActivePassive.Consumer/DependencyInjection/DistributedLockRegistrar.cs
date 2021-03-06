using Autofac;
using NServiceBusTutorials.ActivePassive.Consumer.Interfaces;

namespace NServiceBusTutorials.ActivePassive.Consumer.DependencyInjection
{
    public class DistributedLockRegistrar
    {
        public void Register(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<SqlServerDistributedLockManager>().As<IManageDistributedLocks>();
        }
    }
}