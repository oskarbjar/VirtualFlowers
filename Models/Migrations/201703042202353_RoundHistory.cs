namespace Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RoundHistory : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RoundHistories",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Score1 = c.Int(nullable: false),
                        Score2 = c.Int(nullable: false),
                        MatchId = c.Int(nullable: false),
                        Round = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.RoundHistories");
        }
    }
}
