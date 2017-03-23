using Autofac;

namespace NServiceBusTutorials.ActivePassive.Consumer.Interfaces
{
    public class DistributedLockRegistrar : IRegisterModules
    {
        public void Register(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<DistributedLockManager>().As<IManageDistributedLocks>();
        }
    }
}