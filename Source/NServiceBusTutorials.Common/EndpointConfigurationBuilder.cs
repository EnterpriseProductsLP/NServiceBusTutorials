using System;

using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Transport;

namespace NServiceBusTutorials.Common
{
    public class EndpointConfigurationBuilder
    {
        public EndpointConfiguration GetEndpointConfiguration(string endpointName, string errorQueue = null, string auditQueue = null, int requestedConcurrency = 0)
        {
            return GetEndpointConfiguration<RabbitMQTransport>(endpointName, errorQueue, auditQueue, requestedConcurrency);
        }

        public EndpointConfiguration GetEndpointConfiguration<TTransport>(string endpointName, string errorQueue = null, string auditQueue = null, int requestedConcurrency = 0) where TTransport : TransportDefinition, new()
        {
            var endpointConfiguration = GetBaseEndpointConfiguration(endpointName);
            if (auditQueue != null)
            {
                endpointConfiguration.AuditProcessedMessagesTo(auditQueue);
            }

            var maxConcurrency = GetMaxConcurrency(requestedConcurrency);
            endpointConfiguration.LimitMessageProcessingConcurrencyTo(maxConcurrency);

            if (errorQueue != null)
            {
                endpointConfiguration.SendFailedMessagesTo(errorQueue);
            }

            endpointConfiguration.UseTransport<TTransport>();

            return endpointConfiguration;
        }

        private static int GetMaxConcurrency(int requestedConcurrency)
        {
            var maxConcurrency = Math.Max(requestedConcurrency, 1);
            var maxLogicalConcurrency = Math.Max(Environment.ProcessorCount, 2);
            return Math.Min(maxConcurrency, maxLogicalConcurrency);
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