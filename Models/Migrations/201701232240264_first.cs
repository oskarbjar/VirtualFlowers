namespace Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class first : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Teams",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        Team1 = c.String(),
                        Team2 = c.String(),
                        Map = c.String(),
                        Event = c.String(),
                        Result = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropTable("dbo.Deliveries");
        }
        
        public override void Down()
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
            
            DropTable("dbo.Teams");
        }
    }
}
