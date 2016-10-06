namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdditionalFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ServiceCalls", "ContactPersonName", c => c.String(maxLength: 255));
            AddColumn("dbo.ServiceCalls", "Message", c => c.String(maxLength: 255));
            AddColumn("dbo.ServiceCalls", "CihazSeriNo", c => c.String(maxLength: 255));
            AddColumn("dbo.ServiceCalls", "MagazaKodu", c => c.String(maxLength: 255));
            AddColumn("dbo.ServiceCalls", "CagriIlgilisi", c => c.String(maxLength: 255));
            AddColumn("dbo.ServiceCalls", "CagriDurumKodu", c => c.String(maxLength: 255));
            AddColumn("dbo.ServiceCalls", "MagazaTelefonu", c => c.String(maxLength: 255));
            AddColumn("dbo.ServiceCalls", "CagriAciklamasi", c => c.String(maxLength: 255));
            AddColumn("dbo.ServiceCalls", "CagriAcilisTarihSaati", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ServiceCalls", "CagriAcilisTarihSaati");
            DropColumn("dbo.ServiceCalls", "CagriAciklamasi");
            DropColumn("dbo.ServiceCalls", "MagazaTelefonu");
            DropColumn("dbo.ServiceCalls", "CagriDurumKodu");
            DropColumn("dbo.ServiceCalls", "CagriIlgilisi");
            DropColumn("dbo.ServiceCalls", "MagazaKodu");
            DropColumn("dbo.ServiceCalls", "CihazSeriNo");
            DropColumn("dbo.ServiceCalls", "Message");
            DropColumn("dbo.ServiceCalls", "ContactPersonName");
        }
    }
}
