namespace Tourism_Website.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateTouristIdType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Bookings", "TouristId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Bookings", "TouristId", c => c.String(nullable: false));
        }
    }
}
