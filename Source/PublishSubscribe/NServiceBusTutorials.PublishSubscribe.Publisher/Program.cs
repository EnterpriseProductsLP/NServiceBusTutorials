using System;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.PublishSubscribe.Contracts;
using NServiceBusTutorials.PublishSubscribe.Contracts.Events;

namespace NServiceBusTutorials.PublishSubscribe.Publisher
{
    internal class Program
    {
        public static void Main()
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        private static async Task AsyncMain()
        {
            Console.Title = "Pub/Sub:  Publisher";
            Thread.Sleep(1000);

            var endpointConfigurationBuilder = new EndpointConfigurationBuilder();
            var endpointConfiguration = endpointConfigurationBuilder.GetEndpointConfiguration(endpointName: Endpoints.Publisher, auditQueue: null, errorQueue: Endpoints.ErrorQueue);
            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            try
            {
                await Publish(endpointInstance);
            }
            finally
            {
                await endpointInstance.Stop().ConfigureAwait(false);
            }
        }

        private static async Task Publish(IEndpointInstance endpointInstance)
        {
            Console.WriteLine();
            Console.WriteLine("Press enter to publish a message");
            Console.WriteLine("Press any key to exit");

            while (true)
            {
                Console.WriteLine();

                if (Console.ReadKey().Key != ConsoleKey.Enter)
                {
                    return;
                }

                var eventMessage = new EventMessage(Guid.NewGuid());

                await endpointInstance.Publish(message: eventMessage);
                Console.WriteLine($"Published an EventMessage with ID: {eventMessage.EventMessageId}");
            }
        }
    }
}
