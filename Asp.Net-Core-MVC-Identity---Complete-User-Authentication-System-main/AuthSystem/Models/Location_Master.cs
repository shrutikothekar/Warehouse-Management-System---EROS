using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace eros.Models
{
    public class Location_Master
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required]
        [Display(Name = "Location Name")]
        public string Rack { get; set; }
        [Required]
        [Display(Name = "Location Code")]
        public string locationcode { get; set; }
        [Required]
        [Display(Name = "FromShelf")]
        public string FromShelf { get; set; }
        [Required]
        [Display(Name = "ToShelf")]
        public string ToShelf { get; set; }
        

        
    }
}
