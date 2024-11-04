using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class purchase_product
    {
    
        [Key]
        public int id { get; set; }


        [ForeignKey("purchase")]
        public int porderid { get; set; }

        [NotMapped]
        public purchase purchase { get; private set; }

        //[Display(Name = "Pro. Desp")]
        //public string productname { get; set; } = "-";

        [Required]
        [Display(Name = "Pro. Code")]
        public string productcode { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string description { get; set; }

        [Required]
        [Display(Name = "Brand ")]
        public string brand { get; set; }

        [Required]
        [Display(Name = "Ordered Qty")]
        public int quantity { get; set; }

        [Required]
        [Display(Name = "UOM")]
        public string uom { get; set; }

        [NotMapped]
        public bool IsDeleted { get; set; } = false;


    }
}
