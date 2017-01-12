using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Transport;

namespace NServiceBusTutorials.Common
{
    public class EndpointConfigurationBuilder
    {
        public EndpointConfiguration GetEndpointConfiguration(string endpointName, string auditQueue = null, string errorQueue = null)
        {
            return GetEndpointConfiguration<RabbitMQTransport>(endpointName, auditQueue, errorQueue);
        }

        public EndpointConfiguration GetEndpointConfiguration<TTransport>(string endpointName, string auditQueue = null, string errorQueue = null) where TTransport : TransportDefinition, new()
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

            LogManager.Use<DefaultFactory>().Level(LogLevel.Info);
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.UseTransport<TTransport>();

            return endpointConfiguration;
        }

        private EndpointConfiguration GetBaseEndpointConfiguration(string endpointName)
        {
            var endpointConfiguration = new EndpointConfiguration(endpointName);
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();

            return endpointConfiguration;
        }
    }
}