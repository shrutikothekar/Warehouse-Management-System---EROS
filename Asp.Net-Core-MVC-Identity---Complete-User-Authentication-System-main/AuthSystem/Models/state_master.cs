using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class state_master
    {
        [Key]
        [Display(Name = "State Id")]
        public int state_id { get; set; }
        [Required]
        [Display(Name = "State Code")]
        public string statecode { get; set; }
        [Required]
        [Display(Name = "State Name")]
        public string statename { get; set; }

        [NotMapped]
        public bool IsDeleted { get; set; } = false;
    }
}
