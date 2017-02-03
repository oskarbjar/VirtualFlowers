namespace Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedRankingInfoForMatches : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Matches", "Team1RankValue", c => c.Double(nullable: false));
            AddColumn("dbo.Matches", "Team2RankValue", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Matches", "Team2RankValue");
            DropColumn("dbo.Matches", "Team1RankValue");
        }
    }
}
