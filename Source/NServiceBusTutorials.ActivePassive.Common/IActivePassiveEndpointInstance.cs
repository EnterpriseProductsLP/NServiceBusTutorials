using System.Threading.Tasks;
using NServiceBus;

namespace NServiceBusTutorials.ActivePassive.Common
{
    public interface IActivePassiveEndpointInstance : IStartableEndpoint, IEndpointInstance
    {
        Task Pause();

        Task Resume();

        bool Stopped { get; }
    }
}