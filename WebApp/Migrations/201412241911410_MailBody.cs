namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MailBody : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Mails", "Body", c => c.String(maxLength: 1000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Mails", "Body");
        }
    }
}
