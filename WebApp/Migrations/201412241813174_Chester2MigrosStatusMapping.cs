namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Chester2MigrosStatusMapping : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChesterMigrosStatusMappings",
                c => new
                    {
                        ChesterMigrosStatusMappingID = c.Int(nullable: false, identity: true),
                        ChesterStatusCode = c.Int(nullable: false),
                        ChesterStatusName = c.String(maxLength: 255),
                        MigrosStatusName = c.String(maxLength: 255),
                        MigrosStatusDescription = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.ChesterMigrosStatusMappingID)
                .Index(t => t.ChesterStatusCode)
                .Index(t => t.ChesterStatusName);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.ChesterMigrosStatusMappings", new[] { "ChesterStatusName" });
            DropIndex("dbo.ChesterMigrosStatusMappings", new[] { "ChesterStatusCode" });
            DropTable("dbo.ChesterMigrosStatusMappings");
        }
    }
}
