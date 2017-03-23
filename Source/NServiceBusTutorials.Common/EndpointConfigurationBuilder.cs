using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Transport;

namespace NServiceBusTutorials.Common
{
    public class EndpointConfigurationBuilder
    {
        public EndpointConfiguration GetEndpointConfiguration(string endpointName, string errorQueue = null, string auditQueue = null)
        {
            return GetEndpointConfiguration<RabbitMQTransport>(endpointName, errorQueue, auditQueue);
        }

        public EndpointConfiguration GetEndpointConfiguration<TTransport>(string endpointName, string errorQueue = null, string auditQueue = null) where TTransport : TransportDefinition, new()
        {
            var endpointConfiguration = GetBaseEndpointConfiguration(endpointName);
            if (auditQueue != null)
            {
                endpointConfiguration.AuditProcessedMessagesTo(auditQueue);
            }

            if (errorQueue != null)
            {
                endpointConfiguration.SendFailedMessagesTo(errorQueue);
            }

            endpointConfiguration.UseTransport<TTransport>();

            return endpointConfiguration;
        }

        private EndpointConfiguration GetBaseEndpointConfiguration(string endpointName)
        {
            var endpointConfiguration = new EndpointConfiguration(endpointName);
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.UseSerialization<JsonSerializer>();
            LogManager.Use<DefaultFactory>().Level(LogLevel.Info);

            return endpointConfiguration;
        }
    }
}