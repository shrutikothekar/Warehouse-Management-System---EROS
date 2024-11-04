using System.ComponentModel.DataAnnotations;

namespace eros.Models
{
    public class Outward
    {
        [Key]
        public int outward_id { get; set; }

        [Required]
        [Display(Name = "PO No.")]
        public string pono { get; set; }

        [Required]
        [Display(Name = "SO No.")]
        public string sono { get; set; }
       
        [Required]
        [Display(Name = "Vender Type")]
        public string vendername { get; set; }
        
        [Required]
        [Display(Name = "Return Type")]
        public string typeofreturn { get; set; }

        //[Required]
        [Display(Name = "DC No./Ref.No")]
        public string referenceno { get; set; } = "-";

        [Display(Name = "Invoice.No")]
        public string invoiceno { get; set; } = "-";

        [Required]
        [Display(Name = "Party Name")]
        public string partyname { get; set; }

        [Required]
        [Display(Name = "GSTIN No")]
        public string gstinno { get; set; }

        [Display(Name = "Contact")]
        [MinLength(10)]
        [MaxLength(10)]
        [RegularExpression(@"^\d{0,10}$", ErrorMessage = "Phone number should have a maximum of 10 digits.")]
        public string contactno { get; set; }

        [Required]
        [Display(Name = "Address")]
        [StringLength(200)]
        public string address { get; set; }

        //[Required]
        [Display(Name = "DC No")] 
        public string dcno { get; set; } = "-";

        //[Required]
        [Display(Name = "DC Date")]
        public string dcdate { get; set; } 

        [Required]
        [Display(Name = "Date")]
        [StringLength(500)]
        public string date { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");

        [Required]
        [Display(Name = "Time")]
        [StringLength(500)]
        public string time { get; set; } = DateTime.Now.ToString("hh:mm");

        public virtual List<outwardPacket> outwardPacket { get; set; } = new List<outwardPacket>();

        [Required]
        [Display(Name = "Order Type")]
        public string ordertype { get; set; }

        [Display(Name = "Status")] //BY DEFAULT SET AS PENDING STATUS 
        public string status { get; set; } = "Pending";

        

    }
}
