using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class PhysicalStockTake
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //[Required]
        //[Display(Name = "PST Id")]
        public int pstid { get; set; }

        [Required]
        [Display(Name = "Storage Location")]
        public string locationcode { get; set; }

        [Required]
        [Display(Name = "Product Code")]
        public string productcode { get; set; }

        //[Display(Name = "Box no.")]
        //[Required]
        //public string boxno { get; set; }

        [Required]
        [Display(Name = "Batch code")]
        public string batchcode { get; set; }


        //[Display(Name = "Count")]
        //[Required]
        //public string count { get; set; }


        [Display(Name = "Boxes")]
        [Required]
        public string boxes { get; set; }


        public int flag { get; set; }

        [NotMapped]
        [Display(Name = "Box Count")]
        public int boxcount { get; set; }

        public string storageflag { get; set; }

        [Required]
        [Display(Name = "PST Id")]
        public string physicalid { get; set; }
        public string grnno { get; set; }
    }
}
