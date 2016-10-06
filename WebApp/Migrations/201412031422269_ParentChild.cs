namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ParentChild : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ServiceCalls", "ParentServiceCallID", c => c.Int(nullable: false));
            AddColumn("dbo.ServiceCalls", "HasChild", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ServiceCalls", "HasChild");
            DropColumn("dbo.ServiceCalls", "ParentServiceCallID");
        }
    }
}
