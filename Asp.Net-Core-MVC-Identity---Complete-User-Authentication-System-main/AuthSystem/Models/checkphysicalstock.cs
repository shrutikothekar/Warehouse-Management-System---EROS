using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    //[Keyless]
    public class CheckPhysicalStock
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Display(Name = "Storage location")]
        [Required]
        public string locationcode { get; set; }

        [Display(Name = "Product Code")]
        [Required]
        public string productcode { get; set; }
        [Display(Name = "Batch Code")]
        [Required]
        public string batchcode { get; set; }

        [Display(Name = "Boxes")]
        [Required]
        public string boxes { get; set; }

        public int flag { get; set; }

        [Required]
        public string date { get; set; }

        [Display(Name = "PST Id")]
        [Required]
        public string physicalid { get; set; }

        [NotMapped]
        [Display(Name = "Box Count")]
        public int boxcount { get; set; }

        public string storageflag { get; set; }
        //public List<PhysicalStockTake> TableData { get; set; }
        public string grnno { get; set; }

    }
}
