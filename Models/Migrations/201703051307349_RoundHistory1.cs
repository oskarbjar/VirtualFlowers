namespace Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RoundHistory1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RoundHistories", "Round1", c => c.Boolean(nullable: false));
            AddColumn("dbo.RoundHistories", "Round16", c => c.Boolean(nullable: false));
            AddColumn("dbo.RoundHistories", "TeamId", c => c.Int(nullable: false));
            AddColumn("dbo.RoundHistories", "Terrorist", c => c.Boolean(nullable: false));
            AddColumn("dbo.RoundHistories", "CounterTerrorist", c => c.Boolean(nullable: false));
            DropColumn("dbo.RoundHistories", "Score1");
            DropColumn("dbo.RoundHistories", "Score2");
            DropColumn("dbo.RoundHistories", "MatchId");
            DropColumn("dbo.RoundHistories", "Round");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RoundHistories", "Round", c => c.Int(nullable: false));
            AddColumn("dbo.RoundHistories", "MatchId", c => c.Int(nullable: false));
            AddColumn("dbo.RoundHistories", "Score2", c => c.Int(nullable: false));
            AddColumn("dbo.RoundHistories", "Score1", c => c.Int(nullable: false));
            DropColumn("dbo.RoundHistories", "CounterTerrorist");
            DropColumn("dbo.RoundHistories", "Terrorist");
            DropColumn("dbo.RoundHistories", "TeamId");
            DropColumn("dbo.RoundHistories", "Round16");
            DropColumn("dbo.RoundHistories", "Round1");
        }
    }
}
