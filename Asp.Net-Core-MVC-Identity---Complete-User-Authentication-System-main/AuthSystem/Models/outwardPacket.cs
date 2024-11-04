using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class outwardPacket
    {
        [Key]
        public int id { get; set; }
        
        //foreighnkey 
        [ForeignKey("Outward")]
        public int outwardId { get; set; }
        [NotMapped]
        public virtual Outward Outward { get; private set; }

        //products
        [Required]
        [Display(Name = "Product Code")]
        public string productcode { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        public string description { get; set; }

        [Required]
        [Display(Name = "Brand")]
        public string brandname { get; set; }

        [Required]
        [Display(Name = "Ordered Qty")]
        public int quantity { get; set; }

        [Required]
        [Display(Name = "UOM")]
        public string uom { get; set; }

        [Required]
        [Display(Name = "No Of Pckts")]
        public string noofpackets { get; set; }

        [NotMapped]
        public bool IsDeleted { get; set; } = false;

        [NotMapped]
        [Display(Name = "Remaining Qty")]
        public int pqty { get; set; } = 0;

        //add for chair product
        [Display(Name = "S.Com. Code")]
        public string sccode { get; set; } = "-";// for multiple product

        [Display(Name = "Sub-Com. Name")]
        public string subcomponents { get; set; } = "-"; // for producrt ,chair

        [Display(Name = " Sub-Com. Qty")]
        public int sqty { get; set; } = 0;

        [Display(Name = "Sub-Com UOM")]
        public string suom { get; set; } = "-";
        //end

    }
}
