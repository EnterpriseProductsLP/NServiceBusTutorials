using System;
using System.Configuration;

using NServiceBusTutorials.Migrations;

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
            var migrationRunnerBuilder = new MigrationRunnerBuilder(connectionString);
            var migrationRunner = migrationRunnerBuilder.BuildMigrationRunner();
            migrationRunner.MigrateUp();
            Console.ReadLine();
        }
    }
}
