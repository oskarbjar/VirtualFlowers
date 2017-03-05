namespace Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RoundHistory2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Matches", "FirstRound1HWinCt", c => c.Boolean(nullable: false));
            AddColumn("dbo.Matches", "FirstRound1HWinTerr", c => c.Boolean(nullable: false));
            AddColumn("dbo.Matches", "FirstRound2HWinCT", c => c.Boolean(nullable: false));
            AddColumn("dbo.Matches", "FirstRound2HWinTerr", c => c.Boolean(nullable: false));
            DropColumn("dbo.Matches", "FirstRound1HWinCtTerr");
            DropColumn("dbo.Matches", "FirstRound2HWinCtTerr");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Matches", "FirstRound2HWinCtTerr", c => c.String());
            AddColumn("dbo.Matches", "FirstRound1HWinCtTerr", c => c.String());
            DropColumn("dbo.Matches", "FirstRound2HWinTerr");
            DropColumn("dbo.Matches", "FirstRound2HWinCT");
            DropColumn("dbo.Matches", "FirstRound1HWinTerr");
            DropColumn("dbo.Matches", "FirstRound1HWinCt");
        }
    }
}
