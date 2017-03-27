using System;
using System.Configuration;
using System.Reflection;
using System.Threading;
using NServiceBusTutorials.ActivePassive.Publisher.Producer;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.Migrations.OrderedMigrations;

namespace NServiceBusTutorials.ActivePassive.Publisher
{
    public class Program
    {
        private static WorkProducer _workProducer;

        public static void Main()
        {
            Console.Title = "Active/Passive Example:  Publisher";
            RunMigrations();

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
            while (_workProducer.CurrentState != ProcessState.Stopped);

            Console.WriteLine("Consumer stopped");
            Console.WriteLine("Press Enter to Exit");
            Console.ReadLine();
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

        private static void RunMigrations()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
            var migrationRunnerBuilder = new MigrationRunnerBuilder(connectionString, Assembly.GetAssembly(typeof(Migration_1_Create_Locking_Table)));
            var migrationRunner = migrationRunnerBuilder.BuildMigrationRunner();
            migrationRunner.MigrateUp();
        }
    }
}
