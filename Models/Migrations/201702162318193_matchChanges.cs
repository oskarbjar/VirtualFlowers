namespace Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class matchChanges : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Players", "PlayerName", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Players", "PlayerName", c => c.Int(nullable: false));
        }
    }
}
