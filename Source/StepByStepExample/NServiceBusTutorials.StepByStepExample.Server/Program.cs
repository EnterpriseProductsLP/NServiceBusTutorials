using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.Common.Extensions;
using NServiceBusTutorials.StepByStepExample.Contracts;

namespace NServiceBusTutorials.StepByStepExample.Server
{
    internal class Program
    {
        public static void Main()
        {
            AsyncMain().Inline();
        }

        private static async Task AsyncMain()
        {
            Console.Title = "End to End Example:  Server";
            await Task.Delay(1000);

            var endpointConfigurationBuilder = new EndpointConfigurationBuilder();
            var endpointConfiguration = endpointConfigurationBuilder.GetEndpointConfiguration(Endpoints.Server, Endpoints.ErrorQueue);
            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);


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