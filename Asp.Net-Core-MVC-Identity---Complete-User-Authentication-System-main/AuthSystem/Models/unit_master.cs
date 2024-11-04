using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace eros.Models
{
    public class unit_master
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Unit Name")]
        public string unitName { get; set; }    
    }
}
