using System;
using System.Threading;

using NServiceBusTutorials.ActivePassive.Publisher.Producer;
using NServiceBusTutorials.Common;

namespace NServiceBusTutorials.ActivePassive.Publisher
{
    public class Program
    {
        private static WorkProducer _workProducer;

        public static void Main()
        {
            Console.Title = "Active/Passive Example:  Publisher";

            Thread.Sleep(2000);

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
                        if (_workProducer.CanPause)
                        {
                            _workProducer.Pause();
                        }
                        break;

                    case ConsoleKey.R:
                        if (_workProducer.CanResume)
                        {
                            _workProducer.Resume();
                        }

                        break;
                }
            }
            while (!_workProducer.Stopped);
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            Console.WriteLine("CTRL+C detected");
            Console.WriteLine("Stopping publisher");
            _workProducer.Stop();
        }

        private static void StartProducer()
        {
            var endpointConfigurationBuilder = new EndpointConfigurationBuilder();
            _workProducer = new WorkProducer(endpointConfigurationBuilder);
            new Thread(_workProducer.Start).Start();
        }
    }
}