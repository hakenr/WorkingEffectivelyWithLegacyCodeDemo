namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NullableDateTime : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ServiceCalls", "CagriAcilisTarihSaati", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ServiceCalls", "CagriAcilisTarihSaati", c => c.DateTime(nullable: false));
        }
    }
}
