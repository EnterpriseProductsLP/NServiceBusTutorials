using FluentMigrator;

namespace NServiceBusTutorials.Migrations.OrderedMigrations
{
    [Migration(5)]
    public class Migration_5_Update_usepHeartbeatDistributedLock_To_Use_Version : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.EmbeddedScript("Migration_5_Update_usepHeartbeatDistributedLock_To_Use_Version.sql");
        }
    }
}