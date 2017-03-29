using System.Threading.Tasks;

using NServiceBus;

namespace NServiceBusTutorials.Common
{
    public interface IBuildEndpointInstances
    {
        Task<IStartableEndpoint> Create();
    }
}