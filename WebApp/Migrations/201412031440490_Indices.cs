namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Indices : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.ServiceCalls", "ParentServiceCallID");
            CreateIndex("dbo.ServiceCalls", "HasChild");
            CreateIndex("dbo.ServiceCalls", "IsSuccessful");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ServiceCalls", new[] { "IsSuccessful" });
            DropIndex("dbo.ServiceCalls", new[] { "HasChild" });
            DropIndex("dbo.ServiceCalls", new[] { "ParentServiceCallID" });
        }
    }
}
