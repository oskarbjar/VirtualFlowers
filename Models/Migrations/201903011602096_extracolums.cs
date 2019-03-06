namespace Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class extracolums : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Matches", "BombExplosions", c => c.Int(nullable: false));
            AddColumn("dbo.Matches", "BombDefuses", c => c.Int(nullable: false));
            AddColumn("dbo.Matches", "TimeOut", c => c.Int(nullable: false));
            AddColumn("dbo.Matches", "KnifeKill", c => c.Int(nullable: false));
            AddColumn("dbo.Matches", "MolotovKill", c => c.Int(nullable: false));
            AddColumn("dbo.Matches", "GrenadeKill", c => c.Int(nullable: false));
            AddColumn("dbo.Matches", "ZuesKill", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Matches", "ZuesKill");
            DropColumn("dbo.Matches", "GrenadeKill");
            DropColumn("dbo.Matches", "MolotovKill");
            DropColumn("dbo.Matches", "KnifeKill");
            DropColumn("dbo.Matches", "TimeOut");
            DropColumn("dbo.Matches", "BombDefuses");
            DropColumn("dbo.Matches", "BombExplosions");
        }
    }
}
