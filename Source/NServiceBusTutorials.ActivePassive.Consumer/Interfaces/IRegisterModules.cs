using Autofac;

namespace NServiceBusTutorials.ActivePassive.Consumer.Interfaces
{
    public interface IRegisterModules
    {
        void Register(ContainerBuilder containerBuilder);
    }
}