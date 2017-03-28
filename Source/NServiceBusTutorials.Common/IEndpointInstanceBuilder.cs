using System.Threading.Tasks;

using NServiceBus;

namespace NServiceBusTutorials.Common
{
    public interface IEndpointInstanceBuilder
    {
        Task<IStartableEndpoint> Create();
    }
}