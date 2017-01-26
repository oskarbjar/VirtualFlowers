namespace Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Deliveries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderedDate = c.DateTime(nullable: false),
                        DeliveredDate = c.DateTime(),
                        Status = c.Int(nullable: false),
                        Description = c.String(),
                        ExtraInfo = c.String(),
                        Address = c.String(),
                        City = c.String(),
                        Name = c.String(),
                        Zip = c.String(),
                        TelephoneNumber = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Deliveries");
        }
    }
}
