using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.StepByStepExample.Contracts;
using NServiceBusTutorials.StepByStepExample.Contracts.Commands;
using NServiceBusTutorials.StepByStepExample.Domain;

namespace NServiceBusTutorials.StepByStepExample.Client
{
    internal class Program
    {
        public static void Main()
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        private static async Task AsyncMain()
        {
            Console.Title = "End to End Example:  Client";
            Thread.Sleep(2000);

            var endpointConfigurationBuilder = new EndpointConfigurationBuilder();
            var endpointConfiguration = endpointConfigurationBuilder.GetEndpointConfiguration(endpointName: Endpoints.Client, auditQueue: null, errorQueue: Endpoints.ErrorQueue);
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
                var placeOrder = new PlaceOrder(productId: product.Id, productName: product.Name);

                await endpointInstance.Send(destination: Endpoints.Server, message: placeOrder);
                Console.WriteLine($"Sent a PlaceOrder message with ID: {placeOrder.OrderId}");
            }
        }
    }
}
