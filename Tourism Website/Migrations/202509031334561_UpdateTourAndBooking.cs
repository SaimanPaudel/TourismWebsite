namespace Tourism_Website.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateTourAndBooking : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TravelPackages", "Agency_UserId", "dbo.Users");
            DropForeignKey("dbo.Bookings", "PackageId", "dbo.TravelPackages");
            DropIndex("dbo.Bookings", new[] { "PackageId" });
            DropIndex("dbo.TravelPackages", new[] { "Agency_UserId" });
            AddColumn("dbo.Bookings", "TourId", c => c.Int(nullable: false));
            AlterColumn("dbo.Bookings", "TouristId", c => c.String(nullable: false));
            CreateIndex("dbo.Bookings", "TourId");
            AddForeignKey("dbo.Bookings", "TourId", "dbo.Tours", "Id", cascadeDelete: true);
            DropColumn("dbo.Bookings", "PackageId");
            DropTable("dbo.TravelPackages");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.TravelPackages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Duration = c.Int(nullable: false),
                        Destination = c.String(),
                        ImagePath = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        AgencyId = c.String(),
                        Agency_UserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Bookings", "PackageId", c => c.Int(nullable: false));
            DropForeignKey("dbo.Bookings", "TourId", "dbo.Tours");
            DropIndex("dbo.Bookings", new[] { "TourId" });
            AlterColumn("dbo.Bookings", "TouristId", c => c.String());
            DropColumn("dbo.Bookings", "TourId");
            CreateIndex("dbo.TravelPackages", "Agency_UserId");
            CreateIndex("dbo.Bookings", "PackageId");
            AddForeignKey("dbo.Bookings", "PackageId", "dbo.TravelPackages", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TravelPackages", "Agency_UserId", "dbo.Users", "UserId");
        }
    }
}
