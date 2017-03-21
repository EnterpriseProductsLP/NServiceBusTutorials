using FluentMigrator;

namespace NServiceBusTutorials.Migrations.OrderedMigrations
{
    [Migration(3)]
    public class Migration_3_Create_uspDeleteDistributedLock : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.EmbeddedScript("Migration_3_Create_uspDeleteDistributedLock.sql");
        }
    }
}