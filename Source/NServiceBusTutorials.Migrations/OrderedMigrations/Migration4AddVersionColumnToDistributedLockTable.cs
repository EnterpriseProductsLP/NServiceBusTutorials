using FluentMigrator;

namespace NServiceBusTutorials.Migrations.OrderedMigrations
{
    [Migration(4)]
    public class Migration4AddVersionColumnToDistributedLockTable : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.EmbeddedScript("Migration4AddVersionColumnToDistributedLockTable.sql");
        }
    }
}