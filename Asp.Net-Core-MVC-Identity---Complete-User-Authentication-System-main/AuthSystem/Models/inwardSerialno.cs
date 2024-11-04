using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace eros.Models
{
    public class inwardSerialno
    {
        [Key]
        public int id { get; set; }


        [NotMapped]
        public int srno { get; set; }

        //[ForeignKey("inward")]
        //public int inwardserial_id { get; set; }

        //[NotMapped]
        //public inward inward { get; private set; }

        [Display(Name = "Serial No.")]
        public string? serialno { get; set; }  // for multiple product

        [Display(Name = "PO No.")]
        public string? pono { get; set; } // for multiple product

        [Display(Name = "DC No.")]
        public string? dcno { get; set; } // for multiple product

        [Display(Name = "IN No.")]
        public string? invoiceno { get; set; } // for multiple product

        [Display(Name = "Product Code")]
        public string? productcode { get; set; }

        [Required]
        [Display(Name = "Ordered Quantity")]
        public int? quantity { get; set; }

        [Required(ErrorMessage = "Please enter the warranty duration")]
        public string? Warranty { get; set; } // The numerical value for the duration

        ////add for check warrenty status 

        //[Display(Name = "Product Name")]
        //public string productname { get; set; }

        //[Display(Name = "Remaining Days")]
        //[NotMapped]
        //public string remDays { get; set; }

        //[Display(Name = "status")]
        //public string status { get; set; }
        //public int flag { get; set; } = 0;

    }
}