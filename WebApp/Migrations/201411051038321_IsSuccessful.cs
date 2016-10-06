namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsSuccessful : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ServiceCalls", "IsSuccessful", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ServiceCalls", "IsSuccessful");
        }
    }
}
