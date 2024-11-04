using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace eros.Models
{
    public class storage_master
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int id { get; set; }


        [Required]
        [Display(Name = "Area Code")]
        public string areacode { get; set; }

        [Required]
        [Display(Name = "Area Name")]
        public string areaname { get; set; }

        [Required]
        [Display(Name = "Rack Name")]
        public string rackname { get; set; }

        [NotMapped]
        public bool IsDeleted { get; set; } = false;
    }
}
