using NServiceBus;

namespace NServiceBusTutorials.ActivePassive.Common
{
    public interface IActivePassiveEndpointInstance : IStartableEndpoint, IEndpointInstance
    {
    }
}