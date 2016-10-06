namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MissingPropertiesAndIndices2 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.ServiceCalls", new[] { "MigrosCagriNo" });
            AlterColumn("dbo.ServiceCalls", "MigrosCagriNo", c => c.String(maxLength: 32));
            CreateIndex("dbo.ServiceCalls", "MigrosCagriNo");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ServiceCalls", new[] { "MigrosCagriNo" });
            AlterColumn("dbo.ServiceCalls", "MigrosCagriNo", c => c.String());
            CreateIndex("dbo.ServiceCalls", "MigrosCagriNo");
        }
    }
}
