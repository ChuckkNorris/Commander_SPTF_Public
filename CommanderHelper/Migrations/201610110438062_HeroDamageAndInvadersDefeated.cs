namespace CommanderHelper.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class HeroDamageAndInvadersDefeated : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Hero", "DamageCausedAtLocation", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Hero", "NumberOfInvadersDefeated", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Hero", "NumberOfInvadersDefeated");
            DropColumn("dbo.Hero", "DamageCausedAtLocation");
        }
    }
}
