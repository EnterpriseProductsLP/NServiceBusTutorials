using FluentMigrator;

namespace NServiceBusTutorials.Migrations.OrderedMigrations
{
    [Migration(3)]
    public class Migration3CreateUspDeleteDistributedLock : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.EmbeddedScript("Migration3CreateUspDeleteDistributedLock.sql");
        }
    }
}