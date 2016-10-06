using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{

    public class ChesterMigrosStatusMapping
    {
        public int ChesterMigrosStatusMappingID { get; set; }

        [Index]
        public int ChesterStatusCode { get; set; }

        [Index]
        [MaxLength(255)]
        public String ChesterStatusName { get; set; }

        [MaxLength(255)]
        public String MigrosStatusName { get; set; }

        [MaxLength(500)]
        public String MigrosStatusDescription { get; set; }
    }
}