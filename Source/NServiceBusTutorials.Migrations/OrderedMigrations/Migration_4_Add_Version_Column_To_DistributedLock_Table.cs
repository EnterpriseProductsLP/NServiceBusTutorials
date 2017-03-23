using FluentMigrator;

namespace NServiceBusTutorials.Migrations.OrderedMigrations
{
    [Migration(4)]
    public class Migration_4_Add_Version_Column_To_DistributedLock_Table : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.EmbeddedScript("Migration_4_Add_Version_Column_To_DistributedLock_Table.sql");
        }
    }
}