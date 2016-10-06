namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mail : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Mails",
                c => new
                    {
                        MailID = c.Int(nullable: false, identity: true),
                        To = c.String(maxLength: 255),
                        Subject = c.String(maxLength: 255),
                        Template = c.String(maxLength: 255),
                        ParametersJSON = c.String(maxLength: 1000),
                    })
                .PrimaryKey(t => t.MailID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Mails");
        }
    }
}
