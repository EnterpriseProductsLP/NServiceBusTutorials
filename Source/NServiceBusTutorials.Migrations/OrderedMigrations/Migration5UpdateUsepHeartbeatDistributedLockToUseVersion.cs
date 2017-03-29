using FluentMigrator;

namespace NServiceBusTutorials.Migrations.OrderedMigrations
{
    [Migration(5)]
    public class Migration5UpdateUsepHeartbeatDistributedLockToUseVersion : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.EmbeddedScript(EmbeddedSqlScriptName: "Migration5UpdateUsepHeartbeatDistributedLockToUseVersion.sql");
        }
    }
}