namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MailSubjectIndex : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Mails", "Subject");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Mails", new[] { "Subject" });
        }
    }
}
