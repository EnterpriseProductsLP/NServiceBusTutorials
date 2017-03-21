using FluentMigrator;

namespace NServiceBusTutorials.Migrations.OrderedMigrations
{
    [Migration(2)]
    public class Migration_2_Create_uspAddDistributedLock : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.EmbeddedScript("Migration_2_Create_uspAddDistributedLock.sql");
        }
    }
}