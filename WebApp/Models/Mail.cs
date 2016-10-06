using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    public class Mail
    {
        public int MailID { get; set; }

        [MaxLength(255)]
        public String To { get; set; }

        [Index]
        [MaxLength(255)]
        public String Subject { get; set; }

        [MaxLength(255)]
        public String Template { get; set; }

        [MaxLength(1000)]
        public String ParametersJSON { get; set; }

        [Index]
        public Boolean IsSent { get; set; }

        [MaxLength(1000)]
        public String Body { get; set; }
    }
}