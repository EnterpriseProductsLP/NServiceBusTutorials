﻿using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.Common.Extensions;
using NServiceBusTutorials.PublishSubscribe.Contracts;
using NServiceBusTutorials.PublishSubscribe.Contracts.Events;

namespace NServiceBusTutorials.PublishSubscribe.SubscriberOne
{
    internal class Program
    {
        public static void Main()
        {
            AsyncMain().Inline();
        }

        private static async Task AsyncMain()
        {
            Console.Title = "Pub/Sub:  SubscriberOne";

            var endpointConfigurationBuilder = new EndpointConfigurationBuilder();
            var endpointConfiguration = endpointConfigurationBuilder.GetEndpointConfiguration(Endpoints.SubscriberOne, Endpoints.ErrorQueue);
            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
            await endpointInstance.Subscribe<EventMessage>().ConfigureAwait(false);

            try
            {
                Console.WriteLine();
                Console.WriteLine("SubscriberOne Subscribed:  Press any key to exit");
                Console.ReadKey();
            }
            finally
            {
                await endpointInstance.Stop().ConfigureAwait(false);
            }
        }
    }
}
