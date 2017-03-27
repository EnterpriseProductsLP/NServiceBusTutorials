using System.Data.SqlClient;
using System.Reflection;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;

namespace NServiceBusTutorials.Common
{
    public class MigrationRunnerBuilder
    {
        private readonly string _connectionString;

        private readonly Assembly _migrationAssembly;

        public MigrationRunnerBuilder(string connectionString, Assembly migrationAssembly)
        {
            _connectionString = connectionString;
            _migrationAssembly = migrationAssembly;
        }

        public MigrationRunner BuildMigrationRunner()
        {
            var announcer = new ConsoleAnnouncer();
            var connection = new SqlConnection(_connectionString);
            var migrationGenerator = new SqlServer2014Generator();
            var processorOptions = new ProcessorOptions();
            var databaseFactory = new SqlServerDbFactory();
            var runnerContext = new RunnerContext(announcer);
            var serverProcessor = new SqlServerProcessor(
                connection,
                migrationGenerator,
                announcer,
                processorOptions,
                databaseFactory);
            return new MigrationRunner(_migrationAssembly, runnerContext, serverProcessor);
        }
    }
}
