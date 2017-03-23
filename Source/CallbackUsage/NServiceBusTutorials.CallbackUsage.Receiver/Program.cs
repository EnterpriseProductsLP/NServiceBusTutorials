using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBusTutorials.CallbackUsage.Contracts;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.Common.Extensions;

namespace NServiceBusTutorials.CallbackUsage.Receiver
{
    internal class Program
    {
        public static void Main()
        {
            AsyncMain().Inline();
        }

        private static async Task AsyncMain()
        {
            Console.Title = "Callback Usage:  Receiver";

            var endpointConfigurationBuilder = new EndpointConfigurationBuilder();
            var endpointConfiguration = endpointConfigurationBuilder.GetEndpointConfiguration(Endpoints.Receiver, Endpoints.AuditQueue, Endpoints.ErrorQueue);
            endpointConfiguration.MakeInstanceUniquelyAddressable("1");
            var endpointInstance = await Endpoint.Start(endpointConfiguration);

            try
            {
                Console.WriteLine();
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
