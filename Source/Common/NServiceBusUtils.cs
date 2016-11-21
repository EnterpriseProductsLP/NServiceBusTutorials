﻿using NServiceBus;

namespace Common
{
    public static class NServiceBusUtils
    {
        public static EndpointConfiguration GetDefaultEndpointConfiguration(string endpointName)
        {
            var endpointConfiguration = new EndpointConfiguration(endpointName);
            endpointConfiguration.AuditProcessedMessagesTo(auditQueue: "auditQueue");
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.SendFailedMessagesTo(errorQueue: "errorQueue");
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.UseTransport<RabbitMQTransport>();

            return endpointConfiguration;
        }
    }
}