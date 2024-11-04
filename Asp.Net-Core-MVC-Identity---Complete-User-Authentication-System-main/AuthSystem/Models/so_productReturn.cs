using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace eros.Models
{
    public class so_productReturn
    {
        [Key]
        public int id { get; set; }

        [ForeignKey("salesReturn")]
        public int orderid { get; set; }

        [NotMapped]
        public so_inwardReturn salesReturn { get; private set; }

        [Required]
        [Display(Name = "Pro. Code")]
        public string productcode { get; set; }

        [Required]
        [Display(Name = "Pro. Name")]
        public string description { get; set; }

        [Required]
        [Display(Name = "Br. Name")]
        public string brand { get; set; }

        [Required]
        [Display(Name = "HSCode")]
        public string hsncode { get; set; } = "NA";

        [Required]
        [Display(Name = "Ordered Qty")]
        public int quantity { get; set; }

        [Required]
        [Display(Name = "UOM")]
        public string uom { get; set; }

        [NotMapped]
        public bool IsDeleted { get; set; } = false;
        [NotMapped]
        [Display(Name = "Cust. Name")]
        public string customername { get; set; }
        [NotMapped]
        [Display(Name = "Sale Order No.")]
        public string sono { get; set; }
        [NotMapped]
        public string pickqty { get; set; }
        [NotMapped]
        public string dispatchdate { get; set; }
    }
}
