using FluentMigrator;

namespace NServiceBusTutorials.Migrations.OrderedMigrations
{
    [Migration(2)]
    public class Migration2CreateUspHeartbeatDistributedLock : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.EmbeddedScript(EmbeddedSqlScriptName: "Migration2CreateUspHeartbeatDistributedLock.sql");
        }
    }
}