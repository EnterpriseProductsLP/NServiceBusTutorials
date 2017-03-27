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
        private static WorkConsumer _consumer;

        public static void Main()
        {
            Console.Title = $"Active/Passive Example:  Consumer - {ConfigurationProvider.DistributedLockDiscriminator}";
            // RunMigrations();

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
                        try
                        {
                            _consumer.Pause();
                        }
                        catch (Exception ex)
                        {
                            ConsoleUtilities.WriteLineWithColor($"Exception:  {ex.Message}", ConsoleColor.Red);
                        }
                        break;

                    case ConsoleKey.R:
                        try
                        {
                            _consumer.Resume();
                        }
                        catch (Exception ex)
                        {
                            ConsoleUtilities.WriteLineWithColor($"Exception:  {ex.Message}", ConsoleColor.Red);
                        }

                        break;
                }
            }
            while (!_consumer.Stopped);
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            Console.WriteLine("CTRL+C detected");
            Console.WriteLine("Stopping consumer");
            _consumer.Stop();
        }

        private static void StartConsumer()
        {
            var endpointConfigurationBuilder = new EndpointConfigurationBuilder();
            _consumer = new WorkConsumer(endpointConfigurationBuilder, new DistributedLockManager());
            new Thread(_consumer.Run).Start();
        }

        private static void RunMigrations()
        {
            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
                var migrationRunnerBuilder = new MigrationRunnerBuilder(connectionString, Assembly.GetAssembly(typeof(Migration1CreateLockingTable)));
                var migrationRunner = migrationRunnerBuilder.BuildMigrationRunner();
                migrationRunner.MigrateUp();
            }
            catch (Exception ex)
            {
                ConsoleUtilities.WriteLineWithColor($"Exception:  {ex.Message}", ConsoleColor.Red);
            }
        }
    }
}