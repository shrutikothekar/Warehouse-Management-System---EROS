using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class productmaster_packet
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("productmaster")]
        public int productmasterId { get; set; }

        [NotMapped]
        public virtual Product_Master productmaster { get; private set; }

        [Display(Name = "S.Com. Code")]
        public string subcomponentcode { get; set; }
        

        [Display(Name = "S.Com. Name")]
        public string subcomponents { get; set; } 


        [Display(Name = "S.Com. Qty")]
        public int qty { get; set; } 

        [Display(Name = "S.Com. UOM")]
        public string uom { get; set; } 

        [NotMapped]
        public bool IsDeleted { get; set; } = false;
    }
}
