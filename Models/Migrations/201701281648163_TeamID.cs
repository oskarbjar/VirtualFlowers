namespace Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TeamID : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Teams", "MatchId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Teams", "MatchId");
        }
    }
}
