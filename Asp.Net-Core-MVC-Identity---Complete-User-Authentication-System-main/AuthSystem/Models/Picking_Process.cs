using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class Picking_Process
    {
        [Key]
        
        public int pick_id { get; set; }
        [Required]
        [Display(Name = "SONO")]
        public string sono { get; set; }
       
        public virtual List<Picking_Packet> Picking_Packet { get; set; } = new List<Picking_Packet>();
       

    }
}
