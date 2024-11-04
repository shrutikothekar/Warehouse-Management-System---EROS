using System.ComponentModel.DataAnnotations;

namespace eros.Models
{
    public class Load_Dispatchtable
    {
        [Key]
        public int Id { get; set; }
        public int load_id { get; set; }

        public string sono { get; set; }
        public string productcode { get; set; }
        public string productname { get; set; }
        public string location { get; set; }
        public string boxno { get; set; }
        public string batchcode { get; set; }
        //public string date { get; set; }
        //public string time { get; set; }
        [Display(Name = "Date")]
        //public DateTime date { get; set; } = DateTime.Now;
        public string date { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");


        [Display(Name = "Time")]
        public string time { get; set; } = DateTime.Now.ToString("hh:mm");

        public string statusflag { get; set; } = "0";
    }
}
