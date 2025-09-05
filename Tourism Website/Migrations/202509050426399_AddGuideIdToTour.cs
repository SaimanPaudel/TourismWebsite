namespace Tourism_Website.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGuideIdToTour : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tours", "GuideId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tours", "GuideId");
        }
    }
}
