using FluentMigrator;

namespace NServiceBusTutorials.Migrations.OrderedMigrations
{
    [Migration(1)]
    public class Migration_1_Create_Locking_Table : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Schema("Framework");

            Create.Table("DistributedLock").InSchema("Framework")
                .WithColumn("DistributedLockId").AsInt64().PrimaryKey("PK_DistributedLock").Identity()
                .WithColumn("CreationDateTime").AsDateTime().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("Discriminator").AsGuid()
                .WithColumn("Key").AsAnsiString(100);

            Create.Index("UK_DistributedLock").OnTable("DistributedLock").InSchema("Framework").OnColumn("Key").Unique();
            Create.Index("AK_DistributedLock").OnTable("DistributedLock").InSchema("Framework").OnColumn("Key").Ascending().OnColumn("Discriminator").Ascending();
        }
    }
}
