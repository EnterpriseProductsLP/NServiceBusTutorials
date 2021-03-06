﻿using System;
using System.Threading.Tasks;

using NServiceBus;

using NServiceBusTutorials.CallbackUsage.Contracts;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.Common.Extensions;

namespace NServiceBusTutorials.CallbackUsage.Sender
{
    internal class Program
    {
        public static void Main()
        {
            AsyncMain().Inline();
        }

        private static async Task AsyncMain()
        {
            Console.Title = "Callback Usage:  Sender";
            await Task.Delay(1000);

            var endpointConfigurationBuilder = new EndpointConfigurationBuilder();
            var endpointConfiguration = endpointConfigurationBuilder.GetEndpointConfiguration(Endpoints.Sender, Endpoints.ErrorQueue);
            endpointConfiguration.MakeInstanceUniquelyAddressable("1");
            var endpointInstance = await Endpoint.Start(endpointConfiguration);

            try
            {
                Console.WriteLine();
                Console.WriteLine("Press 'E' to send a message with an enum return");
                Console.WriteLine("Press 'I' to send a message with an int return");
                Console.WriteLine("Press 'O' to send a message with an object return");
                Console.WriteLine("Press any other key to exit");

                while (true)
                {
                    var key = Console.ReadKey().Key;
                    Console.WriteLine();

                    switch (key)
                    {
                        case ConsoleKey.E:
                            await SendEnumMessage(endpointInstance).ConfigureAwait(false);
                            continue;
                        case ConsoleKey.I:
                            await SendIntMessage(endpointInstance).ConfigureAwait(false);
                            continue;
                        case ConsoleKey.O:
                            await SendObjectMessage(endpointInstance).ConfigureAwait(false);
                            continue;
                    }

                    return;
                }
            }
            finally
            {
                await endpointInstance.Stop().ConfigureAwait(false);
            }
        }

        private static async Task SendEnumMessage(IEndpointInstance endpointInstance)
        {
            Console.Write("Enum message sent");

            var message = new EnumMessage();
            var sendOptions = new SendOptions();
            sendOptions.SetDestination(Endpoints.Receiver);
            var responseMessage = await endpointInstance.Request<EnumMessageResponse>(message, sendOptions).ConfigureAwait(false);
            Console.WriteLine($"Enum callback received with status: {responseMessage.Status}");
        }

        private static async Task SendIntMessage(IEndpointInstance endpointInstance)
        {
            Console.Write("Int message sent");

            var message = new IntMessage();
            var sendOptions = new SendOptions();
            sendOptions.SetDestination(Endpoints.Receiver);
            var responseMessage = await endpointInstance.Request<IntMessageResponse>(message, sendOptions).ConfigureAwait(false);
            Console.WriteLine($"Int callback received with response: {responseMessage.Value}");
        }

        private static async Task SendObjectMessage(IEndpointInstance endpointInstance)
        {
            Console.Write("Object message sent");

            var message = new ObjectMessage();
            var sendOptions = new SendOptions();
            sendOptions.SetDestination(Endpoints.Receiver);
            var responseMessage = await endpointInstance.Request<ObjectMessageResponse>(message, sendOptions).ConfigureAwait(false);
            Console.WriteLine($"Object callback received with property value: {responseMessage.Property}");
        }
    }
}