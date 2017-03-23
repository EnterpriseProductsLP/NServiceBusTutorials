using System;
using System.Configuration;
using System.Reflection;
using System.Threading;

using NServiceBusTutorials.Common;
using NServiceBusTutorials.Migrations.OrderedMigrations;

namespace NServiceBusTutorials.ActivePassive.Consumer
{
    public class Program
    {
        private static WorkConsumer _workConsumer;

        public static void Main()
        {
            Console.Title = "Active/Passive Example:  Consumer 1";
            RunMigrations();

            Thread.Sleep(2000);

            StartWorkConsumer();
            RunUntilCancelKeyPress();
        }

        private static void RunUntilCancelKeyPress()
        {
            Console.WriteLine();
            Console.WriteLine("Press 'P' to pause.");
            Console.WriteLine("Press 'R' to resume.");
            Console.WriteLine("Press any key to exit");

            Console.CancelKeyPress += OnCancelKeyPress;

            do
            {
                ConsoleKeyInfo consoleKeyInfo;
                if (ConsoleHelpers.TryReadKeyAsync(1000, out consoleKeyInfo))
                {
                    var consoleKey = consoleKeyInfo.Key;
                    switch (consoleKey)
                    {
                        case ConsoleKey.P:
                            _workConsumer.Pause();
                            break;
                        case ConsoleKey.R:
                            _workConsumer.Resume();
                            break;
                    }
                }
            }
            while (!_workConsumer.Stopped);

            Console.WriteLine("Consumer stopped");
            Console.WriteLine("Press Enter to Exit");
            Console.ReadLine();
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            Console.WriteLine("CTRL+C detected");
            Console.WriteLine("Stopping consumer");
            _workConsumer.Stop();
        }

        private static void StartWorkConsumer()
        {
            var endpointConfigurationBuilder = new EndpointConfigurationBuilder();
            _workConsumer = new WorkConsumer(endpointConfigurationBuilder, new DistributedLockManager());
            new Thread(_workConsumer.Start).Start();
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
