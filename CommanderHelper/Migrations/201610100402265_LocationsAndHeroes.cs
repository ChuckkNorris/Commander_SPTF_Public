namespace CommanderHelper.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LocationsAndHeroes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Hero",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Name = c.String(),
                        NumberOfLivesSaved = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Location",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        LocationName = c.String(),
                        Latitude = c.Double(nullable: false),
                        Longitude = c.Double(nullable: false),
                        DamageCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.InvaderDetail", "Location_Id", c => c.Long());
            CreateIndex("dbo.InvaderDetail", "Location_Id");
            AddForeignKey("dbo.InvaderDetail", "Location_Id", "dbo.Location", "Id");
            DropColumn("dbo.InvaderDetail", "LocationName");
            DropColumn("dbo.InvaderDetail", "Latitude");
            DropColumn("dbo.InvaderDetail", "Longitude");
        }
        
        public override void Down()
        {
            AddColumn("dbo.InvaderDetail", "Longitude", c => c.Double(nullable: false));
            AddColumn("dbo.InvaderDetail", "Latitude", c => c.Double(nullable: false));
            AddColumn("dbo.InvaderDetail", "LocationName", c => c.String());
            DropForeignKey("dbo.InvaderDetail", "Location_Id", "dbo.Location");
            DropIndex("dbo.InvaderDetail", new[] { "Location_Id" });
            DropColumn("dbo.InvaderDetail", "Location_Id");
            DropTable("dbo.Location");
            DropTable("dbo.Hero");
        }
    }
}
