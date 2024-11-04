using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class subcategory_master
    {
        [Key]
        public int subcategory_id { get; set; }

        [Required]
        [Display(Name = "Category Name")]
        public string categoryname { get; set; }

        [Required]
        [Display(Name = "SubCategory Name")]
        public string subcategory_name { get; set; }

        [NotMapped]
        public bool IsDeleted { get; set; } = false;
    }
}
