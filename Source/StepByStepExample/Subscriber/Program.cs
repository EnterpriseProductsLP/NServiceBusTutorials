using System;
using System.Threading.Tasks;
using Contracts;
using NServiceBus;
using NServiceBusTutorials.Common;

namespace Subscriber
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
            Console.Title = "NServiceBusTutorials:  Subscriber";

            var endpointConfiguration = NServiceBusUtils.GetDefaultEndpointConfiguration(endpointName: Endpoints.Subscriber, auditQueue: Endpoints.AuditQueue, errorQueue: Endpoints.ErrorQueue);
            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            try
            {
                Console.WriteLine();
                Console.WriteLine();
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