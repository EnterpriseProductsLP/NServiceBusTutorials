using System.Threading.Tasks;
using NServiceBus;

namespace NServiceBusTutorials.ActivePassive.Common
{
    public interface IActivePassiveEndpointInstance : IEndpointInstance
    {
        Task Pause();

        Task Resume();

        Task Start();
    }
}