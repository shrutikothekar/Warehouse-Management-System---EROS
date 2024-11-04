using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class rack_master
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [Required]

        [Display(Name = "Rack Name")]
        public string rackname { get; set; }
        [Required]
        [Display(Name = "Shelf")]
        public string shelves { get; set; }
        [Required]
        [Display(Name = "Bin")]
        public string bin { get; set; }

        [NotMapped]
        public bool IsDeleted { get; set; } = false;
    }
}
