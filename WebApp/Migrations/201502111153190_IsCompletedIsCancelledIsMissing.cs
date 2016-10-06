namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsCompletedIsCancelledIsMissing : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ServiceCalls", "IsCompleted", c => c.Boolean(nullable: false));
            AddColumn("dbo.ServiceCalls", "IsCancelled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ServiceCalls", "IsMissing", c => c.Boolean(nullable: false));
            CreateIndex("dbo.ServiceCalls", "IsCompleted");
            CreateIndex("dbo.ServiceCalls", "IsCancelled");
            CreateIndex("dbo.ServiceCalls", "IsMissing");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ServiceCalls", new[] { "IsMissing" });
            DropIndex("dbo.ServiceCalls", new[] { "IsCancelled" });
            DropIndex("dbo.ServiceCalls", new[] { "IsCompleted" });
            DropColumn("dbo.ServiceCalls", "IsMissing");
            DropColumn("dbo.ServiceCalls", "IsCancelled");
            DropColumn("dbo.ServiceCalls", "IsCompleted");
        }
    }
}
