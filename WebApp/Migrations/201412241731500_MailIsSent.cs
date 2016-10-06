namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MailIsSent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Mails", "IsSent", c => c.Boolean(nullable: false));
            CreateIndex("dbo.Mails", "IsSent");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Mails", new[] { "IsSent" });
            DropColumn("dbo.Mails", "IsSent");
        }
    }
}
