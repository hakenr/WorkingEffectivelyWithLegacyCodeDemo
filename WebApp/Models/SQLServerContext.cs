using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace WebApp.Models
{
    public class SQLServerContext : DbContext
    {
        public DbSet<ServiceCall> ServiceCalls { get; set; }
        public DbSet<Mail> Mails { get; set; }
        public DbSet<ChesterMigrosStatusMapping> ChesterMigrosStatusMappings { get; set; }
    }
}