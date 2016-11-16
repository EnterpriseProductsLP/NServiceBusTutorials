using System;
using System.Threading.Tasks;
using Contracts;
using NServiceBus;

namespace Subscriber
{
    class Program
    {
        public static void Main(string[] args)
        {
        }

        private static async Task AsyncMain()
        {
            Console.Title = "NServiceBusTutorials:  Subscriber";

            // The endpoint name will be used to determine queue names and serves
            // as the address, or identity, of the endpoint
            var endpointConfiguration = new EndpointConfiguration(Endpoints.Subscriber);

            // Configure where to send failed messages
            endpointConfiguration.SendFailedMessagesTo("error");

            // Use JSON to serialize and deserialize messages (which are just
            // plain classes) to and from message queues
            endpointConfiguration.UseSerialization<JsonSerializer>();

            // Ask NServiceBus to automatically create message queues
            endpointConfiguration.EnableInstallers();

            // Store information in memory for this example, rather than in
            // a database. In this sample, only subscription information is stored
            endpointConfiguration.UsePersistence<InMemoryPersistence>();

            // Configure NServiceBus to use the RabbitMQ Transport
            endpointConfiguration.UseTransport<RabbitMQTransport>();

            // Initialize the endpoint with the finished configuration
            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            try
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
            finally
            {
                await endpointInstance.Stop().ConfigureAwait(false);
            }
        }

    }
}
