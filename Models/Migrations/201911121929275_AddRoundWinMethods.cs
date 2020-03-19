namespace Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRoundWinMethods : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Matches", "round1BombExplosion", c => c.Boolean(nullable: false));
            AddColumn("dbo.Matches", "round1Defuse", c => c.Boolean(nullable: false));
            AddColumn("dbo.Matches", "round1Timout", c => c.Boolean(nullable: false));
            AddColumn("dbo.Matches", "round1KillWin", c => c.Boolean(nullable: false));
            AddColumn("dbo.Matches", "round16BombExplosion", c => c.Boolean(nullable: false));
            AddColumn("dbo.Matches", "round16Defuse", c => c.Boolean(nullable: false));
            AddColumn("dbo.Matches", "round16Timout", c => c.Boolean(nullable: false));
            AddColumn("dbo.Matches", "round16KillWin", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Matches", "round16KillWin");
            DropColumn("dbo.Matches", "round16Timout");
            DropColumn("dbo.Matches", "round16Defuse");
            DropColumn("dbo.Matches", "round16BombExplosion");
            DropColumn("dbo.Matches", "round1KillWin");
            DropColumn("dbo.Matches", "round1Timout");
            DropColumn("dbo.Matches", "round1Defuse");
            DropColumn("dbo.Matches", "round1BombExplosion");
        }
    }
}
