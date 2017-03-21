using System;
using System.Configuration;
using System.Reflection;

using NServiceBusTutorials.Common;
using NServiceBusTutorials.Migrations;
using NServiceBusTutorials.Migrations.OrderedMigrations;

namespace NServiceBusTutorials.ActivePassive.Publisher
{
    public class Program
    {
        public static void Main()
        {
            RunMigrations();
        }

        private static void RunMigrations()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
            var migrationRunnerBuilder = new MigrationRunnerBuilder(connectionString, Assembly.GetAssembly(typeof(Migration_1_Create_Locking_Table)));
            var migrationRunner = migrationRunnerBuilder.BuildMigrationRunner();
            migrationRunner.MigrateUp();
            Console.ReadLine();
        }
    }
}
