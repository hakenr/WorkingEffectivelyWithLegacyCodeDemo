using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class ServiceCall
    {
        // ID
        public int ServiceCallID { get; set; }

        [Index]
        public int ParentServiceCallID { get; set; }

        [Index]
        public bool HasChild { get; set; }

        // RETURNED ID
        [Index]
        public int ChesterServiceCallID { get; set; }

        // ADDITIONAL DATA
        public int EquipmentID { get; set; }
        
        public int AddressID { get; set; }
        
        public int ContactPersonID { get; set; }
        
        [MaxLength(255)]
        public String ContactPersonName { get; set; }
        
        // STATE
        [Index]
        public bool IsSuccessful { get; set; }
        
        // ERROR MESSAGE
        [MaxLength(255)]
        public String Message { get; set; }

        // MIGROS CREATE PARAMS
        [Index]
        [MaxLength(32)]
        public String MigrosCagriNo { get; set; }
        
        [MaxLength(255)]
        public String CihazSeriNo { get; set; }
        
        [MaxLength(255)]
        public String MagazaKodu { get; set; }
        
        [MaxLength(255)]
        public String CagriIlgilisi { get; set; }
        
        [MaxLength(255)]
        public String CagriDurumKodu { get; set; }
        
        [MaxLength(255)]
        public String MagazaTelefonu { get; set; }
        
        [MaxLength(255)]
        public String CagriAciklamasi { get; set; }
        
        public DateTime? CagriAcilisTarihSaati { get; set; }

        [Index]
        public bool IsCompleted { get; set; }
        [Index]
        public bool IsCancelled { get; set; }
        [Index]
        public bool IsMissing { get; set; }
    }
}