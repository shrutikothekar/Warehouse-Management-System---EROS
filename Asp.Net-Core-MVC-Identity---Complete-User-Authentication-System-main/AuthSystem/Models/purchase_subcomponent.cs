using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class purchase_subcomponent
    {
        //KEY
        public int id { get; set; }

        //FK

        [ForeignKey("purchase")]
        public int purchaseproduct_id { get; set; }

        [NotMapped]
        public purchase purchase { get; private set; }

        //DATA

        [Display(Name = "PONO")]
        public string pono { get; set; } = "-";

        //[Display(Name = "SONO")]
        //public string sono { get; set; } = "-";

        [Display(Name = "S.Com. Code")]
        public string sccode { get; set; } = "-";// for multiple product

        [Display(Name = "SubComponents")]
        public string subcomponents { get; set; } = "-";   // for multiple product

        [Display(Name = "S.Com. Qty")]
        public int scqty { get; set; } = 0;// for multiple product

        [Display(Name = "S.Com. UOM")]
        public string scuom { get; set; } = "-";// for multiple product

        //[NotMapped]
        [Display(Name = "Total. Qty")]
        public int tqty { get; set; } = 0;



        [NotMapped]
        public bool IsDeleted { get; set; } = false;

    }
}
