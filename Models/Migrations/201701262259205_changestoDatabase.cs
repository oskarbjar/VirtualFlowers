namespace Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changestoDatabase : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Teams", "Team1Id", c => c.Int(nullable: false));
            AddColumn("dbo.Teams", "Team2Id", c => c.Int(nullable: false));
            AddColumn("dbo.Teams", "ResultT1", c => c.Int(nullable: false));
            AddColumn("dbo.Teams", "ResultT2", c => c.Int(nullable: false));
            DropColumn("dbo.Teams", "TeamId");
            DropColumn("dbo.Teams", "Result");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Teams", "Result", c => c.String());
            AddColumn("dbo.Teams", "TeamId", c => c.Int(nullable: false));
            DropColumn("dbo.Teams", "ResultT2");
            DropColumn("dbo.Teams", "ResultT1");
            DropColumn("dbo.Teams", "Team2Id");
            DropColumn("dbo.Teams", "Team1Id");
        }
    }
}
