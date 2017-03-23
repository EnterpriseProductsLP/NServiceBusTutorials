using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.Common.Extensions;
using NServiceBusTutorials.FileSystemTransport.Contracts;
using NServiceBusTutorials.FileSystemTransport.Transport;

namespace NServiceBusTutorials.FileSystemTransport.ApplicationOne
{
    internal class Program
    {
        public static void Main()
        {
            AsyncMain().Inline();
        }

        private static async Task AsyncMain()
        {
            Console.Title = "FileSystem Transport:  Application One";

            var endpointConfigurationBuilder = new EndpointConfigurationBuilder();
            var endpointConfiguration = endpointConfigurationBuilder.GetEndpointConfiguration<FileTransport>(Endpoints.EndpointOne, null, Endpoints.ErrorQueue);
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
