namespace Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ScrapedMatchesLogoAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ScrapedMatches", "Team1Logo", c => c.String());
            AddColumn("dbo.ScrapedMatches", "Team2Logo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ScrapedMatches", "Team2Logo");
            DropColumn("dbo.ScrapedMatches", "Team1Logo");
        }
    }
}
