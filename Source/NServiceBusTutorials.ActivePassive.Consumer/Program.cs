using System;
using System.Configuration;
using System.Reflection;
using System.Threading.Tasks;

using NServiceBusTutorials.ActivePassive.Common;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.Common.Extensions;
using NServiceBusTutorials.Migrations.OrderedMigrations;

namespace NServiceBusTutorials.ActivePassive.Consumer
{
    public class Program
    {
        private static IActivePassiveEndpointInstance _consumer;

        private static bool _ending;

        public static void Main()
        {
            AsyncMain().Inline();
        }

        private static Task AsyncMain()
        {
            Console.Title = $"Active/Passive Example:  Consumer - {ConfigurationProvider.DistributedLockDiscriminator}";
            RunMigrations();

            StartConsumer();
            RunUntilCancelKeyPress();
            return Task.CompletedTask;
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
                            _consumer.Pause().Inline();
                        }
                        catch (Exception ex)
                        {
                            ConsoleUtilities.WriteLineWithColor($"Exception:  {ex.Message}", ConsoleColor.Red);
                        }
                        break;

                    case ConsoleKey.R:
                        try
                        {
                            _consumer.Resume().Inline();
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

        private static async void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            Console.WriteLine("CTRL+C detected");
            Console.WriteLine("Stopping consumer");
            await _consumer.Stop();
            _ending = true;
            Environment.Exit(0);
        }

        private static void StartConsumer()
        {
            var endpointConfigurationBuilder = new EndpointConfigurationBuilder();
            var endpointBuilder = new EndpointBuilder(endpointConfigurationBuilder);
            _consumer = new ActivePassiveEndpointInstance(endpointBuilder, new SqlServerDistributedLockManager());
            _consumer.Start().Inline();
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