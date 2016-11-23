using NServiceBus;
using NServiceBus.Transport;

namespace NServiceBusTutorials.Common
{
    public static class NServiceBusUtils
    {
        public static EndpointConfiguration GetDefaultEndpointConfiguration(string endpointName, string auditQueue = null, string errorQueue = null)
        {
            return GetEndpointConfiguration<RabbitMQTransport>(endpointName, auditQueue, errorQueue);
        }

        public static EndpointConfiguration GetEndpointConfiguration<TTransport>(string endpointName, string auditQueue = null, string errorQueue = null) where TTransport : TransportDefinition, new()
        {
            var endpointConfiguration = GetBaseEndpointConfiguration(endpointName);
            if (auditQueue != null)
            {
                endpointConfiguration.AuditProcessedMessagesTo(auditQueue: auditQueue);
            }
            if (errorQueue != null)
            {
                endpointConfiguration.SendFailedMessagesTo(errorQueue: errorQueue);
            }
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.UseTransport<TTransport>();

            return endpointConfiguration;
        }

        private static EndpointConfiguration GetBaseEndpointConfiguration(string endpointName)
        {
            var endpointConfiguration = new EndpointConfiguration(endpointName);
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();

            return endpointConfiguration;
        }
    }
}