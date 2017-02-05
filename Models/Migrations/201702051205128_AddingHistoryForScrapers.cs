namespace Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingHistoryForScrapers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ScrapeHistoryRankingLists",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LastDayScraped = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ScrapeHistoryTeams",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TeamId = c.Int(nullable: false),
                        LastDayScraped = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ScrapeHistoryTeams");
            DropTable("dbo.ScrapeHistoryRankingLists");
        }
    }
}
