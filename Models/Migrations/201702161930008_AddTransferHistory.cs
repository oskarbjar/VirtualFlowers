namespace Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTransferHistory : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TransferHistories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TransferDate = c.DateTime(nullable: false),
                        OldTeamId = c.Int(nullable: false),
                        OldTeamName = c.String(),
                        NewTeamId = c.Int(nullable: false),
                        NewTeamName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TransferHistories");
        }
    }
}
