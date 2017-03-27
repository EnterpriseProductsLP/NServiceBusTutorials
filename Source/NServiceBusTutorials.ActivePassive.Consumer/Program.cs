using System;
using System.Configuration;
using System.Reflection;
using System.Threading;

using NServiceBusTutorials.ActivePassive.Common;
using NServiceBusTutorials.ActivePassive.Consumer.Consumer;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.Migrations.OrderedMigrations;

namespace NServiceBusTutorials.ActivePassive.Consumer
{
    public class Program
    {
        private static WorkConsumer _workConsumer;

        public static void Main()
        {
            Console.Title = $"Active/Passive Example:  Consumer - {ConfigurationProvider.DistributedLockDiscriminator}";
            RunMigrations();

            Thread.Sleep(2000);

            StartConsumer();
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
                        if (_workConsumer.CanPause)
                        {
                            _workConsumer.Pause();
                        }
                        break;

                    case ConsoleKey.R:
                        if (_workConsumer.CanResume)
                        {
                            _workConsumer.Resume();
                        }

                        break;
                }
            }
            while (!_workConsumer.Stopped);
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            Console.WriteLine("CTRL+C detected");
            Console.WriteLine("Stopping consumer");
            _workConsumer.Stop();
        }

        private static void StartConsumer()
        {
            var endpointConfigurationBuilder = new EndpointConfigurationBuilder();
            _workConsumer = new WorkConsumer(endpointConfigurationBuilder, new DistributedLockManager());
            new Thread(_workConsumer.Start).Start();
        }

        private static void RunMigrations()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
            var migrationRunnerBuilder = new MigrationRunnerBuilder(connectionString, Assembly.GetAssembly(typeof(Migration1CreateLockingTable)));
            var migrationRunner = migrationRunnerBuilder.BuildMigrationRunner();
            migrationRunner.MigrateUp();
        }
    }
}