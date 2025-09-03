namespace Tourism_Website.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateTours : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Feedbacks",
                c => new
                    {
                        FeedbackId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        TourId = c.Int(nullable: false),
                        Rating = c.Int(nullable: false),
                        Comments = c.String(maxLength: 500),
                        DatePosted = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.FeedbackId)
                .ForeignKey("dbo.Tours", t => t.TourId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.TourId);
            
            CreateTable(
                "dbo.Tours",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 100),
                        Destination = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DurationDays = c.Int(nullable: false),
                        ImagePath = c.String(),
                        CreatedByUserId = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        FullName = c.String(),
                    })
                .PrimaryKey(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Feedbacks", "UserId", "dbo.Users");
            DropForeignKey("dbo.Feedbacks", "TourId", "dbo.Tours");
            DropIndex("dbo.Feedbacks", new[] { "TourId" });
            DropIndex("dbo.Feedbacks", new[] { "UserId" });
            DropTable("dbo.Users");
            DropTable("dbo.Tours");
            DropTable("dbo.Feedbacks");
        }
    }
}
