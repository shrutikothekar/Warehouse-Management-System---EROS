using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class inward_subcomponent
    {
        public int id { get; set; }

        [ForeignKey("inward")]
        public int inwardpacket_id { get; set; }

        [NotMapped]
        public inward inward { get; private set; }

        [Display(Name = "PONO")]
        public string pono { get; set; } = "-";

        [Display(Name = "SONO")]
        public string sono { get; set; } = "-";


        [Display(Name = "DC No.")]
        public string dcno { get; set; } = "-";


        [Display(Name = "Invoice No")]
        public string invoiceno { get; set; } = "-";

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
        [Display(Name = "No of shipper per sub-com ")]
        public int NoOfshippers { get; set; } = 0;

        [NotMapped]
        [Display(Name = "No of Sub-Com per shipper")]
        public int? NoOfSubComQTYperShipper { get; set; } = 0;

        [NotMapped]
        public string shipper { get; set; } = "-";

        [NotMapped]
        public bool IsDeleted { get; set; } = false;
    }
}
