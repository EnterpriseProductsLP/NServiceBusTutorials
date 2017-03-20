﻿using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.StepByStepExample.Contracts;

namespace NServiceBusTutorials.StepByStepExample.Subscriber
{
    internal class Program
    {
        public static void Main()
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        private static async Task AsyncMain()
        {
            Console.Title = "End to End Example:  Subscriber";

            var endpointConfigurationBuilder = new EndpointConfigurationBuilder();
            var endpointConfiguration = endpointConfigurationBuilder.GetEndpointConfiguration(endpointName: Endpoints.Subscriber, auditQueue: null, errorQueue: Endpoints.ErrorQueue);
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