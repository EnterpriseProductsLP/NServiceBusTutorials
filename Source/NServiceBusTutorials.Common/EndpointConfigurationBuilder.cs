using System;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Transport;

namespace NServiceBusTutorials.Common
{
    public class EndpointConfigurationBuilder : IBuildEndpointConfigurations
    {
        public EndpointConfiguration GetEndpointConfiguration(string endpointName, string errorQueue = null, string auditQueue = null, int requestedConcurrency = 0)
        {
            return GetEndpointConfiguration<RabbitMQTransport>(endpointName, errorQueue, auditQueue, requestedConcurrency);
        }

        public EndpointConfiguration GetEndpointConfiguration<TTransport>(string endpointName, string errorQueue = null, string auditQueue = null, int requestedConcurrency = 0)
            where TTransport : TransportDefinition, new()
        {
            var endpointConfiguration = GetBaseEndpointConfiguration(endpointName);
            ConfigureAuditing(auditQueue, endpointConfiguration);
            ConfigureMaxConcurrency(requestedConcurrency, endpointConfiguration);
            ConfigureErrorQueue(errorQueue, endpointConfiguration);

            endpointConfiguration.UseTransport<TTransport>();

            return endpointConfiguration;
        }

        private static void ConfigureErrorQueue(string errorQueue, EndpointConfiguration endpointConfiguration)
        {
            if (errorQueue != null)
            {
                endpointConfiguration.SendFailedMessagesTo(errorQueue);
            }
        }

        private static void ConfigureMaxConcurrency(int requestedConcurrency, EndpointConfiguration endpointConfiguration)
        {
            var maxConcurrency = GetMaxConcurrency(requestedConcurrency);
            endpointConfiguration.LimitMessageProcessingConcurrencyTo(maxConcurrency);
        }

        private static void ConfigureAuditing(string auditQueue, EndpointConfiguration endpointConfiguration)
        {
            if (auditQueue != null)
            {
                endpointConfiguration.AuditProcessedMessagesTo(auditQueue);
            }
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
            endpointConfiguration.ExcludeAssemblies("netstandard.dll");
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.UseSerialization<JsonSerializer>();
            LogManager.Use<DefaultFactory>().Level(LogLevel.Info);

            return endpointConfiguration;
        }
    }
}