//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace eros.Models
//{
//    public class LoadData_table
//    {
//        [Key]
//        public int srno { get; set; }

//        public int iid { get; set; }
//        public string loadingsheetno { get; set; }

//        public string sono { get; set; }
//        public string unloadsequence { get; set; }

//        [Display(Name = "Date")]
//        public DateTime date { get; set; } = DateTime.Now.ToUniversalTime();


//        [Display(Name = "Time")]
//        public string time { get; set; } = DateTime.Now.ToString("hh:mm");

//        public string complete_flag { get; set; }
//        [NotMapped]
//        public string Status { get; set; }

//    }
//}

using System.ComponentModel.DataAnnotations;

namespace eros.Models
{
    public class LoadData_table
    {
        [Key]
        public int srno { get; set; }

        public int loadingsheetno { get; set; }

        public string sono { get; set; }
        public string unloadsequence { get; set; }
        public string shipment_date { get; set; }
        public string shipment_time { get; set; }
        public string shipment_month { get; set; }
        public string shipment_year { get; set; }
        public string complete_flag { get; set; }

    }
}
