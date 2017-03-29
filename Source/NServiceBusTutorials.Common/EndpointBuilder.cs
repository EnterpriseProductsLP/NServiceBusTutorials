using System.Threading.Tasks;
using NServiceBus;
using NServiceBusTutorials.ActivePassive.Contracts;

namespace NServiceBusTutorials.Common
{
    public class EndpointBuilder : IBuildEndpointInstances
    {
        private readonly EndpointConfigurationBuilder _endpointConfigurationBuilder;

        public EndpointBuilder(EndpointConfigurationBuilder endpointConfigurationBuilder)
        {
            _endpointConfigurationBuilder = endpointConfigurationBuilder;
        }

        public Task<IStartableEndpoint> Create()
        {
            var endpointConfiguration = _endpointConfigurationBuilder.GetEndpointConfiguration(Endpoints.Consumer, Endpoints.ErrorQueue);
            var recoverability = endpointConfiguration.Recoverability();
            recoverability.Immediate(
                immediate =>
                {
                    immediate.NumberOfRetries(0);
                });
            return Endpoint.Create(endpointConfiguration);
        }
    }
}