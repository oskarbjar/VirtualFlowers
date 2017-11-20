namespace Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ScrapedMatchesExtraInfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ScrapedMatches", "Team1Id", c => c.Int(nullable: false));
            AddColumn("dbo.ScrapedMatches", "Team1Name", c => c.String());
            AddColumn("dbo.ScrapedMatches", "Team2Id", c => c.Int(nullable: false));
            AddColumn("dbo.ScrapedMatches", "Team2Name", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ScrapedMatches", "Team2Name");
            DropColumn("dbo.ScrapedMatches", "Team2Id");
            DropColumn("dbo.ScrapedMatches", "Team1Name");
            DropColumn("dbo.ScrapedMatches", "Team1Id");
        }
    }
}
