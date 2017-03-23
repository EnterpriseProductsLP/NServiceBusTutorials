﻿using System;
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

            StartMessagePublisher();
            RunUntilCancelKeyPress();
        }

        private static void RunUntilCancelKeyPress()
        {
            Console.CancelKeyPress += OnCancelKeyPress;
            while (!_workConsumer.Stopped)
            {
            }

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

        private static void StartMessagePublisher()
        {
            var endpointConfigurationBuilder = new EndpointConfigurationBuilder();
            _workConsumer = new WorkConsumer(endpointConfigurationBuilder);
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
