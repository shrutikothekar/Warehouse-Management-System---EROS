using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace eros.Models
{
    public class Product_Master
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required]
        [Display(Name = "Category Code")]
        public string categorycode { get; set; }

        [Required]
        [Display(Name = "Category Name")]
        public string categoryname { get; set; }

        [Required]
        [Display(Name = "SubCategory")]
        public string subcategory { get; set; }

        [Required]
        [Display(Name = "Model No.")]
        public string productcode { get; set; }

        [Required]
        [Display(Name = "Brand Name")]
        public string brand { get; set; }

        [Required]
        [Display(Name = "Product Description")]
        public string productdescription { get; set; }

        //[Required]
        //[Display(Name = "Version")]
        //public float version { get; set; }

        [Required]
        [Display(Name = "Type Of Product")]
        public bool TypeOfProduct { get; set; }

        [Display(Name = "HSN")]
        public string hsncode { get; set; }

        [Required]
        [Display(Name = "UOM")]
        public string uom { get; set; } = "-";

        //[Display(Name = "Warranty Duration")]
        //[Required(ErrorMessage = "Please enter the warranty duration")]
        //public string Warranty { get; set; } // The numerical value for the duration
        //[NotMapped]
        //public int WarrantyDur { get; set; }
        //[NotMapped]
        //public string WarrantyUnit { get; set; } // The numerical value for the duration


        [NotMapped]
        public bool IsDeleted { get; set; } = false;

        public virtual List<productmaster_packet> Productmaster_Packets { get; set; } = new List<productmaster_packet>();
        //public virtual List<BOQproducts> BOQproducts { get; set; } = new List<BOQproducts>();


    }
}
