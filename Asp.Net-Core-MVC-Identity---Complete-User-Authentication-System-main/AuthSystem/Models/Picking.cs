using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class Picking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required]
        [Display(Name = "SO No.")]
        public string sono { get; set; }

        public virtual List<picking_master> pickingmaster { get; set; } = new List<picking_master>();

        [NotMapped]
        public bool IsDeleted { get; set; } = false;
    }
}
