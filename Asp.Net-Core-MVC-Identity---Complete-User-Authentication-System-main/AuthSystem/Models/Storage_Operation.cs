using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace eros.Models
{
    public class Storage_Operation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required]
        [Display(Name = "Product Code")]
        public string productcode { get; set; }

        [Required]
        [Display(Name = "Batch Code")]
        public string batchcode { get; set; }

        [Required]
        [Display(Name = "Box No.")]
        public string boxno { get; set; }

        [Required]
        [Display(Name = "Storage Location")]
        public string locationcode { get; set; }
        [Required]
        [Display(Name = "stat flag")]
        public string statusflag { get; set; }
        [Required]
        
        [Display(Name = "GRN NO")]
        public string grnno { get; set; }
        [Display(Name = "Pickflag")]
        public string pickflag { get; set; } = "0";

        [NotMapped]
        public bool IsDeleted { get; set; } = false;

        [NotMapped]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime date { get; set; } 

    }
}
