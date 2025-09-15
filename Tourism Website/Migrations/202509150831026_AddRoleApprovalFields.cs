namespace Tourism_Website.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddRoleApprovalFields : DbMigration
    {
        public override void Up()
        {
            // Only add the columns if they don't already exist
            Sql(@"
IF COL_LENGTH('dbo.Users', 'RequestedRole') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD RequestedRole NVARCHAR(50) NULL;
END;

IF COL_LENGTH('dbo.Users', 'IsRoleApproved') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD IsRoleApproved BIT NULL;
END;
");
        }

        public override void Down()
        {
            // Only drop the columns if they exist
            Sql(@"
IF COL_LENGTH('dbo.Users', 'IsRoleApproved') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Users DROP COLUMN IsRoleApproved;
END;

IF COL_LENGTH('dbo.Users', 'RequestedRole') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Users DROP COLUMN RequestedRole;
END;
");
        }
    }
}
