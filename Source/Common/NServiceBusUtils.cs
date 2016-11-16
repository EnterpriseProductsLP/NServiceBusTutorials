using NServiceBus;

namespace Common
{
    public static class NServiceBusUtils
    {
        public static EndpointConfiguration GetDefaultEndpointConfiguration(string endpointName)
        {
            var endpointConfiguration = new EndpointConfiguration(endpointName);
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.SendFailedMessagesTo(errorQueue: "error");
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.UseTransport<RabbitMQTransport>();

            return endpointConfiguration;
        }
    }
}