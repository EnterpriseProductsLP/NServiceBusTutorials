using FluentMigrator;

namespace NServiceBusTutorials.Migrations.OrderedMigrations
{
    [Migration(5)]
    public class Migration5UpdateUspHeartbeatDistributedLockToUseVersion : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.EmbeddedScript("Migration5UpdateUspHeartbeatDistributedLockToUseVersion.sql");
        }
    }
}