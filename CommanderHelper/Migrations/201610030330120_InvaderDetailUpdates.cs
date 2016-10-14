namespace CommanderHelper.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InvaderDetailUpdates : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvaderDetail", "LocationName", c => c.String());
            AddColumn("dbo.InvaderDetail", "OriginalTweet", c => c.String());
            AlterColumn("dbo.InvaderDetail", "Latitude", c => c.Double(nullable: false));
            AlterColumn("dbo.InvaderDetail", "Longitude", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.InvaderDetail", "Longitude", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.InvaderDetail", "Latitude", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.InvaderDetail", "OriginalTweet");
            DropColumn("dbo.InvaderDetail", "LocationName");
        }
    }
}
