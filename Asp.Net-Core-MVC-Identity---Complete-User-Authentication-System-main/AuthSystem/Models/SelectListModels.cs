using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace eros.Models
{
    public class SelectListModels
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [Required]

        [Display(Name = "Rack Name")]
        public string Value { get; set; }
        [Required]
        [Display(Name = "Shelf")]
        public string Text
        {
            get; set;
        }
        [NotMapped]
        public bool IsDeleted { get; set; } = false;
    }
}
