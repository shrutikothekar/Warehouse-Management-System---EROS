using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace eros.Models
{
    public class picking_master
    {

        [Key]
        public int id { get; set; }

        [Display(Name = "SONO")]

        [ForeignKey("sono")]
        public int pickingid { get; set; }

        [Required]
        [Display(Name = "Product Code")]
        public string productcode { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        public string productname { get; set; }


        [Required]
        [Display(Name = "Box No.")]
        public string boxno { get; set; }

        [Required]
        [Display(Name = "Batch No.")]
        public string batchno { get; set; }

        [Required]
        [Display(Name = "InStock Qty")]
        public string instockqty { get; set; }

        [Required]
        [Display(Name = "Location")]
        public string location { get; set; }

        [Required]
        [Display(Name = "Required Qty")]
        public int quantity { get; set; }

        [NotMapped]
        public bool IsDeleted { get; set; } = false;
    }
}
