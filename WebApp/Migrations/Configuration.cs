namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    using WebApp.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<WebApp.Models.SQLServerContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(WebApp.Models.SQLServerContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //            

            ChesterMigrosStatusMapping[] defaultMappings = new ChesterMigrosStatusMapping[]{
                new ChesterMigrosStatusMapping { 
                    ChesterStatusCode = 1, 
                    ChesterStatusName = "Recorded", 
                    MigrosStatusName = "", 
                    MigrosStatusDescription = "Kayıt alındı" },
                new ChesterMigrosStatusMapping { 
                    ChesterStatusCode = 2, 
                    ChesterStatusName = "New", 
                    MigrosStatusName = "", 
                    MigrosStatusDescription = "." },
                new ChesterMigrosStatusMapping {
                    ChesterStatusCode = 3, 
                    ChesterStatusName = "Assigned", 
                    MigrosStatusName = "", 
                    MigrosStatusDescription = "Teknisyen Bilgilendirildi" },
                new ChesterMigrosStatusMapping { 
                    ChesterStatusCode = 4, 
                    ChesterStatusName = "Dispatched", 
                    MigrosStatusName = "", 
                    MigrosStatusDescription = "Teknisyen Yönlendirildi" },
                new ChesterMigrosStatusMapping { 
                    ChesterStatusCode = 5, 
                    ChesterStatusName = "Partially confirmed", 
                    MigrosStatusName = "", 
                    MigrosStatusDescription = "Müdahale edildi, denemede ya da parça bekliyor" },
                new ChesterMigrosStatusMapping { 
                    ChesterStatusCode = 6, 
                    ChesterStatusName = "Completed", 
                    MigrosStatusName = "Completed", 
                    MigrosStatusDescription = "Sorun başarı ile giderildi" },
                new ChesterMigrosStatusMapping { 
                    ChesterStatusCode = 7, 
                    ChesterStatusName = "Cancelled", 
                    MigrosStatusName = "Cancelled",
                    MigrosStatusDescription = "Servis çağrısı iptal edildi" },
                new ChesterMigrosStatusMapping { 
                    ChesterStatusCode = 8, 
                    ChesterStatusName = "CreditBlocked", 
                    MigrosStatusName = "Suspend", 
                    MigrosStatusDescription = "Çağrı beklemeye alındı. Lütfen Finans Bölümü ile görüşün." }
            };

            SQLServerContext dbContext = new SQLServerContext();
            ChesterMigrosStatusMapping existing;
            foreach (ChesterMigrosStatusMapping mapping in defaultMappings) {

                existing = dbContext.ChesterMigrosStatusMappings.Where(m => m.ChesterStatusCode == mapping.ChesterStatusCode).FirstOrDefault();
                if (existing != null) {
                    continue;
                }

                dbContext.ChesterMigrosStatusMappings.Add(mapping);
            }
            dbContext.SaveChanges();
        }
    }
}
