using FluentMigrator;

namespace NServiceBusTutorials.Migrations.OrderedMigrations
{
    [Migration(1)]
    public class Migration1CreateLockingTable : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Schema(schemaName: "Framework");

            Create.Table(tableName: "DistributedLock")
                .InSchema(schemaName: "Framework")
                .WithColumn(name: "DistributedLockId").AsInt64().PrimaryKey(primaryKeyName: "PK_DistributedLock").Identity()
                .WithColumn(name: "Discriminator").AsAnsiString(100)
                .WithColumn(name: "Heartbeat").AsDateTime().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn(name: "Key").AsAnsiString(100);

            Create.Index(indexName: "AK_DistributedLock").OnTable(tableName: "DistributedLock").InSchema(schemaName: "Framework").OnColumn(columnName: "Key").Unique();
            Create.Index(indexName: "AK_DistributedLock2").OnTable(tableName: "DistributedLock").InSchema(schemaName: "Framework").OnColumn(columnName: "Key").Ascending().OnColumn(columnName: "Discriminator").Unique();
        }
    }
}