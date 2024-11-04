//using System.ComponentModel.DataAnnotations.Schema;
//using System.ComponentModel.DataAnnotations;

//namespace eros.Models
//{
//    public class Loading_Dispatch_Operation
//    {
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        public int id { get; set; }

//        [Required]
//        [Display(Name = "Sale Order No.")]
//        public string sono { get; set; }
//        [Required]
//        [Display(Name = "Pro.Code")]
//        public string productcode { get; set; }

//        [Display(Name = "Pro.Name")]
//        public string productname { get; set; }

//        [Display(Name = "Location")]
//        public string location { get; set; }

//        [Display(Name = "Batch Code")]
//        public string batchcode { get; set; }

//        [Display(Name = "Box No.")]
//        public string boxno { get; set; }

//        public string couriername { get; set; }
//        [Required]
//        public string docketno { get; set; }
//        [Required]
//        public string dispatchdate { get; set; }
//        [Required]
//        public string transportname { get; set; }
//        [Required]
//        public string truckno { get; set; }
//        public string sonosequence { get; set; }

//        public string unloadingsequence { get; set; }
//        [Required]
//        public string drivername { get; set; }
//        [Required]
//        public string contactno { get; set; }
//        [Required]
//        public string dispatchtype { get; set; }
//        public string currentdate { get; set; }
//        public string dc_flag { get; set; }
//        public string partial_flag { get; set; }


//    }
//}

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Nest;

namespace eros.Models
{
    public class Loading_Dispatch_Operation
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

        [Display(Name = "Product Name")]
        public string productname { get; set; }



        [Required]
        [Display(Name = "Batch Code")]
        public string batchcode { get; set; }


        [Display(Name = "Shipper No.")]
        public string boxno { get; set; }

        [Required]
        public string couriername { get; set; }

        [Required]
        public string docketno { get; set; }

        [Required]
        public string dispatchdate { get; set; }

        [Required]
        public string transportname { get; set; }

        [Required]
        public string truckno { get; set; }
        public string sonosequence { get; set; }

        public string unloadingsequence { get; set; }

        [Required]
        public string drivername { get; set; }

        [Required]
        public string contactno { get; set; }
        [Required]
        [Display(Name = "Dispatch Type")]
        public string dispatchtype { get; set; }
        public string currentdate { get; set; }
        public string dc_flag { get; set; }
        public string partial_flag { get; set; }

        [NotMapped]
        public string Status { get; set; }
        [NotMapped]
        public int remainingScanCounter { get; set; } = 0;
        [NotMapped]
        public int scannedShipperCounter { get; set; } = 0;
        [NotMapped]
        public int totalShipperCounter { get; set; } = 0;
        [NotMapped]
        public string customer { get; set;}
        [NotMapped]
        public int quantity { get; set;}
        [NotMapped]
        [Display(Name = "Category")]
        public string Category { get; set; }
        [NotMapped]
        [Display(Name = "GRN")]
        public string grn { get; set; }
        [Display(Name = "Location")]
        public string location { get; set; }
        [NotMapped]
        public string qtyperset { get;set; }

    }
}

