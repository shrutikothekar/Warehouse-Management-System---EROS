using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace eros.Models
{
    public class purchase_productReturn
    {
        [Key]
        public int id { get; set; }


        [ForeignKey("purchaseReturn")]
        public int porderid { get; set; }

        [NotMapped]
        public purchaseReturn purchaseReturn { get; private set; }

        //[Display(Name = "Pro. Desp")]
        //public string productname { get; set; } = "-";

        [Required]
        [Display(Name = "Pro. Code")]
        public string productcode { get; set; }

        [Required]
        [Display(Name = "Pro. Name")]
        public string description { get; set; }

        [Required]
        [Display(Name = "Brand Name")]
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
