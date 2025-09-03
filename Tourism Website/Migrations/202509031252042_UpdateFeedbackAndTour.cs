namespace Tourism_Website.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateFeedbackAndTour : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Bookings", "Tourist_UserId", "dbo.Users");
            DropForeignKey("dbo.TravelPackages", "Agency_UserId", "dbo.Users");
            DropForeignKey("dbo.Bookings", "PackageId", "dbo.TravelPackages");
            DropIndex("dbo.Bookings", new[] { "PackageId" });
            DropIndex("dbo.Bookings", new[] { "Tourist_UserId" });
            DropIndex("dbo.TravelPackages", new[] { "Agency_UserId" });
            AlterColumn("dbo.Users", "FullName", c => c.String());
            DropColumn("dbo.Users", "Email");
            DropColumn("dbo.Users", "PhoneNumber");
            DropColumn("dbo.Users", "UserType");
            DropColumn("dbo.Users", "CreatedAt");
            DropTable("dbo.Bookings");
            DropTable("dbo.TravelPackages");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.TravelPackages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Duration = c.Int(nullable: false),
                        Destination = c.String(nullable: false, maxLength: 100),
                        ImagePath = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        AgencyId = c.Int(nullable: false),
                        Agency_UserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Bookings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PackageId = c.Int(nullable: false),
                        TouristId = c.Int(nullable: false),
                        BookingDate = c.DateTime(nullable: false),
                        NumberOfPeople = c.Int(nullable: false),
                        TotalPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.String(nullable: false),
                        SpecialRequests = c.String(),
                        Tourist_UserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Users", "CreatedAt", c => c.DateTime(nullable: false));
            AddColumn("dbo.Users", "UserType", c => c.String());
            AddColumn("dbo.Users", "PhoneNumber", c => c.String());
            AddColumn("dbo.Users", "Email", c => c.String(nullable: false));
            AlterColumn("dbo.Users", "FullName", c => c.String(nullable: false, maxLength: 100));
            CreateIndex("dbo.TravelPackages", "Agency_UserId");
            CreateIndex("dbo.Bookings", "Tourist_UserId");
            CreateIndex("dbo.Bookings", "PackageId");
            AddForeignKey("dbo.Bookings", "PackageId", "dbo.TravelPackages", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TravelPackages", "Agency_UserId", "dbo.Users", "UserId");
            AddForeignKey("dbo.Bookings", "Tourist_UserId", "dbo.Users", "UserId");
        }
    }
}
