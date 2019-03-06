namespace Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveRoundHistory : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.RoundHistories");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.RoundHistories",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Round1 = c.Boolean(nullable: false),
                        Round16 = c.Boolean(nullable: false),
                        TeamId = c.Int(nullable: false),
                        Terrorist = c.Boolean(nullable: false),
                        CounterTerrorist = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
    }
}
