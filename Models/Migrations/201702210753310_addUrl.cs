namespace Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addUrl : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ErrorLoggers", "url", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ErrorLoggers", "url");
        }
    }
}
