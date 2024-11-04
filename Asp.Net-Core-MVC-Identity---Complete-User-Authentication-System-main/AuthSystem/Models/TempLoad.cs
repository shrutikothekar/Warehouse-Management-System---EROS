using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace eros.Models
{
    public class TempLoad
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required]
        [Display(Name = "Order No.")]
        public string sono { get; set; }
        [Required]
        [Display(Name = "Product Code")]
        public string productcode { get; set; }
        [Required]
        [Display(Name = "DC No")]
        public string dcno { get; set; }
        [Required]
        [Display(Name = "DC Date")]
        public string dcdate { get; set; }
        [Required]
        [Display(Name = "invoice No")]
        public string invoiceno { get; set; }
        [Required]
        [Display(Name = "invoice date")]
        public string invoicedate { get; set; }

        [Display(Name = "Location")]
        public string location { get; set; }

        [Required]
        [Display(Name = "Batch Code")]
        public string batchcode { get; set; }
        
        [Required]
        [Display(Name = "Batch Code")]
        public string grnno { get; set; }


        [Display(Name = "Shipper No.")]
        public string boxno { get; set; }

        public string date { get; set; }

        //[Required]
        //public string couriername { get; set; }

        //[Required]
        //public string docketno { get; set; }

        //[Required]
        //public string dispatchdate { get; set; }

        //[Required]
        //public string transportname { get; set; }

        //[Required]
        //public string truckno { get; set; }
        //public string sonosequence { get; set; }

        //public string unloadingsequence { get; set; }

        //[Required]
        //public string drivername { get; set; }

        //[Required]
        //public string contactno { get; set; }
        //[Required]
        //[Display(Name = "Dispatch Type")]
        //public string dispatchtype { get; set; }
        //public string dc_flag { get; set; }
        //public string partial_flag { get; set; }

        //[NotMapped]
        //public string Status { get; set; }
        //[NotMapped]
        //public int remainingScanCounter { get; set; } = 0;
        //[NotMapped]
        //public int scannedShipperCounter { get; set; } = 0;
        //[NotMapped]
        //public int totalShipperCounter { get; set; } = 0;
        //[NotMapped]
        //public string customer { get; set; }
        //[NotMapped]
        //public int quantity { get; set; }
        //[NotMapped]
        //[Display(Name = "Category")]
        //public string Category { get; set; }
    }
}
