using System;
using System.Threading;
using NServiceBusTutorials.Common;

namespace NServiceBusTutorials.ActivePassive.Publisher
{
    public class Program
    {
        private static WorkProducer _producer;

        private static bool _ending;

        public static void Main()
        {
            Console.Title = "Active/Passive Example:  Publisher";

            StartProducer();
            RunUntilCancelKeyPress();
        }

        private static void RunUntilCancelKeyPress()
        {
            Console.WriteLine();
            Console.WriteLine("Press 'P' to pause.");
            Console.WriteLine("Press 'R' to resume.");

            Console.CancelKeyPress += OnCancelKeyPress;
            do
            {
                var consoleKey = Console.ReadKey().Key;
                switch (consoleKey)
                {
                    case ConsoleKey.P:
                        try
                        {
                            _producer.Pause();
                        }
                        catch (Exception ex)
                        {
                            ConsoleUtilities.WriteLineWithColor($"Exception:  {ex.Message}", ConsoleColor.Red);
                        }
                        break;

                    case ConsoleKey.R:
                        try
                        {
                            _producer.Resume();
                        }
                        catch (Exception ex)
                        {
                            ConsoleUtilities.WriteLineWithColor($"Exception:  {ex.Message}", ConsoleColor.Red);
                        }

                        break;
                }
            }
            while (!_ending);
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            Console.WriteLine("CTRL+C detected");
            Console.WriteLine("Stopping publisher");
            _producer.Stop();
            _ending = true;
            Environment.Exit(0);
        }

        private static void StartProducer()
        {
            var endpointConfigurationBuilder = new EndpointConfigurationBuilder();
            _producer = new WorkProducer(endpointConfigurationBuilder);
            new Thread(_producer.Run).Start();
        }
    }
}