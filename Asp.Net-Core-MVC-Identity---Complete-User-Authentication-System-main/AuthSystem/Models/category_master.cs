using System.ComponentModel.DataAnnotations;

namespace eros.Models
{
    public class category_master
    {
        [Key]
        public int id { get; set; }

        [Required]
        [Display(Name = "Category ID")]
        public string categoryid { get; set; }

        [Required]
        [Display(Name = "Category Name")]
        public string categoryname { get; set; }
     

    }
}

