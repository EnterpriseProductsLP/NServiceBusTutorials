using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.FileSystemTransport.Contracts;
using NServiceBusTutorials.FileSystemTransport.Transport;

namespace NServiceBusTutorials.FileSystemTransport.ApplicationTwo
{
    class Program
    {
        public static void Main()
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        private static async Task AsyncMain()
        {
            Console.Title = "FileSystem Transport:  Application Two";

            var endpointConfiguration = NServiceBusUtils.GetEndpointConfiguration<FileTransport>(endpointName: Endpoints.EndpointTwo, auditQueue: null, errorQueue: Endpoints.ErrorQueue);
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
