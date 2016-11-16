using System;
using System.Threading.Tasks;
using NServiceBus;
using Shared;
using Shared.Commands;

namespace Client
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        private static async Task AsyncMain()
        {
            Console.Title = "NServiceBusTutorials:  Client";

            // The endpoint name will be used to determine queue names and serves
            // as the address, or identity, of the endpoint
            var endpointConfiguration = new EndpointConfiguration(Endpoints.Client);


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
            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            

            // Initialize the endpoint with the finished configuration
            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            try
            {
                await SendOrder(endpointInstance);
            }
            finally
            {
                await endpointInstance.Stop().ConfigureAwait(false);
            }
        }

        private static async Task SendOrder(IEndpointInstance endpointInstance)
        {
            Console.WriteLine("Press enter to send a message");
            Console.WriteLine("Press any key to exit");

            while (true)
            {
                Console.WriteLine();

                if (Console.ReadKey().Key != ConsoleKey.Enter)
                {
                    return;
                }

                var id = Guid.NewGuid();
                var placeOrder = new PlaceOrder(id, "New Shoes");

                await endpointInstance.Send(Endpoints.Server, placeOrder);
                Console.WriteLine($"Sent a PlaceOrder message with ID: {id}");
            }
        }
    }
}