namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ServiceCallStub : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ServiceCalls",
                c => new
                    {
                        ServiceCallID = c.Int(nullable: false, identity: true),
                        MigrosCagriNo = c.String(),
                        ChesterServiceCallID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ServiceCallID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ServiceCalls");
        }
    }
}
