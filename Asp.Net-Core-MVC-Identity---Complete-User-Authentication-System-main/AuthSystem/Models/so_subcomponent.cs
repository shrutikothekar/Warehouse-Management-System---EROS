using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class so_subcomponent
    {
        public int id { get; set; }

        [ForeignKey("so_inward")]
        public int soproduct_id { get; set; }

        [NotMapped]
        public so_inward so_inward { get; private set; }

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
