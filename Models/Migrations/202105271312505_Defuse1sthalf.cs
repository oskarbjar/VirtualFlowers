namespace Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Defuse1sthalf : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Matches", "BombDefuses1stHalf", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Matches", "BombDefuses1stHalf");
        }
    }
}
