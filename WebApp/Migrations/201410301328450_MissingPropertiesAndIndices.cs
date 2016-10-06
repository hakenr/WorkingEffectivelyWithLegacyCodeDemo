namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MissingPropertiesAndIndices : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ServiceCalls", "equipmentID", c => c.Int(nullable: false));
            AddColumn("dbo.ServiceCalls", "addressID", c => c.Int(nullable: false));
            AddColumn("dbo.ServiceCalls", "contactPersonID", c => c.Int(nullable: false));
            CreateIndex("dbo.ServiceCalls", "ChesterServiceCallID");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ServiceCalls", new[] { "ChesterServiceCallID" });
            DropColumn("dbo.ServiceCalls", "contactPersonID");
            DropColumn("dbo.ServiceCalls", "addressID");
            DropColumn("dbo.ServiceCalls", "equipmentID");
        }
    }
}
