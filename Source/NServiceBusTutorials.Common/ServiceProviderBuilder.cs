using System.Reflection;

using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace NServiceBusTutorials.Common
{
    public class ServiceProviderBuilder
    {
        private readonly string _connectionString;

        private readonly Assembly _migrationAssembly;

        public ServiceProviderBuilder(string connectionString, Assembly migrationAssembly)
        {
            _connectionString = connectionString;
            _migrationAssembly = migrationAssembly;
        }

        public ServiceProvider BuildMigrationRunner()
        {
            var serviceProvider = new ServiceCollection()
                // Logging is the replacement for the old IAnnouncer
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                // Registration of all FluentMigrator-specific services
                .AddFluentMigratorCore()
                // Configure the runner
                .ConfigureRunner(
                    builder => builder
                        // Use SQLite
                        .AddSqlServer2016()
                        // The SQLite connection string
                        .WithGlobalConnectionString(_connectionString)
                        // Specify the assembly with the migrations
                        .WithMigrationsIn(_migrationAssembly)
                ).BuildServiceProvider();

            return serviceProvider;
        }
    }
}