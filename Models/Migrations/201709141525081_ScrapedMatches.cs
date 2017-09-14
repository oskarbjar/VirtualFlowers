namespace Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ScrapedMatches : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ScrapedMatches",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MatchId = c.Int(nullable: false),
                        MatchUrl = c.String(),
                        Name = c.String(),
                        Event = c.String(),
                        Start = c.DateTime(nullable: false),
                        SportName = c.String(),
                        Json = c.String(),
                        Json4MinFTR = c.String(),
                        Json5MinFTR = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ScrapedMatches");
        }
    }
}
