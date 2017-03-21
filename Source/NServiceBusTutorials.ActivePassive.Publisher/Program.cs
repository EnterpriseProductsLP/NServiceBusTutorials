using System;
using System.Configuration;
using System.Reflection;
using System.Threading;

using NServiceBus;

using NServiceBusTutorials.ActivePassive.Contracts;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.Migrations.OrderedMigrations;

namespace NServiceBusTutorials.ActivePassive.Publisher
{
    public class Program
    {
        private static MessagePublisher _messagePublisher;

        public static void Main()
        {
            Console.Title = "Active/Passive Example:  Publisher";
            RunMigrations();

            Thread.Sleep(2000);

            StartMessagePublisher();
            RunUntilCancelKeyPress();
        }

        private static void RunUntilCancelKeyPress()
        {
            Console.CancelKeyPress += OnCancelKeyPress;
            while (!_messagePublisher.Stopped)
            {
            }

            Console.WriteLine("Publisher stopped");
            Console.WriteLine("Exited gracefully");
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            Console.WriteLine("CTRL+C detected");
            Console.WriteLine("Stopping publisher");
            _messagePublisher.Stop();
        }

        private static void StartMessagePublisher()
        {
            var endpointConfigurationBuilder = new EndpointConfigurationBuilder();
            var endpointConfiguration = endpointConfigurationBuilder.GetEndpointConfiguration(endpointName: Endpoints.Publisher, auditQueue: Endpoints.AuditQueue, errorQueue: Endpoints.ErrorQueue);
            var endpointInstance = Endpoint.Create(endpointConfiguration).ConfigureAwait(false).GetAwaiter().GetResult();
            _messagePublisher = new MessagePublisher(endpointInstance);
            new Thread(_messagePublisher.Start).Start();
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
