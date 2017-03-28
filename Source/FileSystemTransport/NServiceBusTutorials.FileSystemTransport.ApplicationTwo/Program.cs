using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.Common.Extensions;
using NServiceBusTutorials.FileSystemTransport.Contracts;
using NServiceBusTutorials.FileSystemTransport.Transport;

namespace NServiceBusTutorials.FileSystemTransport.ApplicationTwo
{
    internal class Program
    {
        public static void Main()
        {
            AsyncMain().Inline();
        }

        private static async Task AsyncMain()
        {
            Console.Title = "FileSystem Transport:  Application Two";

            var endpointConfigurationBuilder = new EndpointConfigurationBuilder();
            var endpointConfiguration = endpointConfigurationBuilder.GetEndpointConfiguration<FileTransport>(Endpoints.EndpointTwo, Endpoints.ErrorQueue);
            endpointConfiguration.DisableFeature<TimeoutManager>();

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
