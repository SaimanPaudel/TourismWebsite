namespace Tourism_Website.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserTouristAndBookingChanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Email", c => c.String(nullable: false));
            AddColumn("dbo.Users", "Password", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.Users", "Role", c => c.String(maxLength: 50));
            AlterColumn("dbo.Users", "FullName", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Users", "FullName", c => c.String());
            DropColumn("dbo.Users", "Role");
            DropColumn("dbo.Users", "Password");
            DropColumn("dbo.Users", "Email");
        }
    }
}
