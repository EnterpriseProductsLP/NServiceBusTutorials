using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.Common.Extensions;
using NServiceBusTutorials.StepByStepExample.Contracts;
using NServiceBusTutorials.StepByStepExample.Contracts.Commands;
using NServiceBusTutorials.StepByStepExample.Domain;

namespace NServiceBusTutorials.StepByStepExample.Client
{
    internal class Program
    {
        public static void Main()
        {
            AsyncMain().Inline();
        }

        private static async Task AsyncMain()
        {
            Console.Title = "End to End Example:  Client";
            await Task.Delay(2000);

            var endpointConfigurationBuilder = new EndpointConfigurationBuilder();
            var endpointConfiguration = endpointConfigurationBuilder.GetEndpointConfiguration(Endpoints.Client, auditQueue: null, errorQueue: Endpoints.ErrorQueue);
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
            Console.WriteLine();
            Console.WriteLine("Press enter to send a message");
            Console.WriteLine("Press any key to exit");

            while (true)
            {
                Console.WriteLine();

                if (Console.ReadKey().Key != ConsoleKey.Enter)
                {
                    return;
                }

                var product = ProductBuilder.NextProduct();
                var placeOrder = new PlaceOrder(product.Id, product.Name);

                await endpointInstance.Send(Endpoints.Server, placeOrder);
                Console.WriteLine($"Sent a PlaceOrder message with ID: {placeOrder.OrderId}");
            }
        }
    }
}
