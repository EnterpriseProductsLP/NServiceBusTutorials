using NServiceBus;

namespace NServiceBusTutorials.ActivePassive.Common
{
    public interface IActivePassiveEndpoint : IStartableEndpoint, IEndpointInstance
    {
    }
}