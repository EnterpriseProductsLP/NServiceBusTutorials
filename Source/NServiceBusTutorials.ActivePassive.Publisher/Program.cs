using System;
using System.Configuration;
using System.Reflection;
using System.Threading;

using NServiceBus;

using NServiceBusTutorials.ActivePassive.Contracts;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.Common.Extensions;
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

            StartWorkPublisher();
            RunUntilCancelKeyPress();
        }

        private static void RunUntilCancelKeyPress()
        {
            Console.CancelKeyPress += OnCancelKeyPress;
            while (!_workProducer.Stopped)
            {
            }

            Console.WriteLine("Publisher stopped");
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

        private static void StartWorkPublisher()
        {
            var endpointConfigurationBuilder = new EndpointConfigurationBuilder();
            var endpointConfiguration = endpointConfigurationBuilder.GetEndpointConfiguration(Endpoints.Publisher, Endpoints.ErrorQueue);
            var startableEndpoint = Endpoint.Create(endpointConfiguration).Inline();
            _workProducer = new WorkProducer(startableEndpoint);
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
