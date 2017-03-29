using NServiceBus;
using NServiceBus.Transport;

namespace NServiceBusTutorials.Common
{
    public interface IBuildEndpointConfigurations
    {
        EndpointConfiguration GetEndpointConfiguration(string endpointName, string errorQueue = null, string auditQueue = null, int requestedConcurrency = 0);

        EndpointConfiguration GetEndpointConfiguration<TTransport>(string endpointName, string errorQueue = null, string auditQueue = null, int requestedConcurrency = 0)
            where TTransport : TransportDefinition, new();
    }
}