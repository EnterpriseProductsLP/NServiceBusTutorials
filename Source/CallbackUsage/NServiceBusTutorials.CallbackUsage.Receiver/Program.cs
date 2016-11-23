﻿using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBusTutorials.CallbackUsage.Contracts;
using NServiceBusTutorials.Common;

namespace NServiceBusTutorials.CallbackUsage.Receiver
{
    class Program
    {
        static void Main(string[] args)
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        private static async Task AsyncMain()
        {
            Console.Title = "Callback Usage:  Receiver";

            var endpointConfiguration = NServiceBusUtils.GetDefaultEndpointConfiguration(endpointName: Endpoints.Receiver, auditQueue: Endpoints.AuditQueue, errorQueue: Endpoints.ErrorQueue);
            endpointConfiguration.MakeInstanceUniquelyAddressable(discriminator: "1");
            var endpointInstance = await Endpoint.Start(configuration: endpointConfiguration);

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