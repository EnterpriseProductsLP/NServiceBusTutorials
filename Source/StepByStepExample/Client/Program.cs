using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Contracts.Commands;
using Domain;
using NServiceBus;
using NServiceBusTutorials.Common;

namespace Client
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var asyncMain = AsyncMain();
            var taskAwaiter = asyncMain.GetAwaiter();
            taskAwaiter.GetResult();
        }

        private static async Task AsyncMain()
        {
            Console.Title = "NServiceBusTutorials:  Client";
            Thread.Sleep(2000);

            var endpointConfiguration = NServiceBusUtils.GetDefaultEndpointConfiguration(endpointName: Endpoints.Client, auditQueue: Endpoints.AuditQueue, errorQueue: Endpoints.ErrorQueue);
            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            try
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
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

                var product = ProductBuilder.NextProduct();
                var placeOrder = new PlaceOrder(productId: product.Id, productName: product.Name);

                await endpointInstance.Send(destination: Endpoints.Server, message: placeOrder);
                Console.WriteLine($"Sent a PlaceOrder message with ID: {placeOrder.OrderId}");
            }
        }
    }
}