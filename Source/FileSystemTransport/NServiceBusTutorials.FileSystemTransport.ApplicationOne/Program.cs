using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.FileSystemTransport.Contracts;
using NServiceBusTutorials.FileSystemTransport.Transport;

namespace NServiceBusTutorials.FileSystemTransport.ApplicationOne
{
    class Program
    {
        public static void Main()
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        private static async Task AsyncMain()
        {
            Console.Title = "FileSystem Transport:  Application One";

            var endpointConfiguration = NServiceBusUtils.GetEndpointConfiguration<FileTransport>(endpointName: Endpoints.EndpointOne, auditQueue: null, errorQueue: Endpoints.ErrorQueue);
            endpointConfiguration.DisableFeature<TimeoutManager>();

            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            try
            {
                for (var i = 0; i < 100; i++)
                {
                    var messageA = new MessageA();
                    await endpointInstance.Send(Endpoints.EndpointTwo, messageA).ConfigureAwait(false);
                }

                Console.WriteLine();
                Console.WriteLine("MessageA sent. Press any key to exit");
                Console.ReadKey();
            }
            finally
            {
                await endpointInstance.Stop().ConfigureAwait(false);
            }
        }
    }
}
