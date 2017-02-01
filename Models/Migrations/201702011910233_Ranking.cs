namespace Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Ranking : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.RankingLists", "RankingListId", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.RankingLists", "RankingListId", c => c.Int(nullable: false));
        }
    }
}
