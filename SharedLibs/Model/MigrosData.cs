using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibs.Model
{
    public class MigrosData
    {
        public String user{ get; set; } 
		public String password{ get; set; } 
		public String Migros_Cagri_No{ get; set; } 
		public String Cihaz_Seri_No{ get; set; } 
		public String Magaza_Kodu{ get; set; } 
		public String Cagri_Ilgilisi{ get; set; } 
		public String Cagri_Durum_Kodu{ get; set; } 
		public String Magaza_Telefonu{ get; set; } 
		public String Cagri_Aciklamasi{ get; set; }
        System.DateTime Cagri_Acilis_Tarih_Saati{ get; set; }
    }
}
