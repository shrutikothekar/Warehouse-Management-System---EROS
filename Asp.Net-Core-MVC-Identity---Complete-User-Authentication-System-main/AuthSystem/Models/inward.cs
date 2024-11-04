using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace eros.Models
{
    public class inward
    {
        [Key]
        public int inward_id { get; set; }

        [Display(Name = "Purchase Order No.")]

        [Required]
        public string? pono { get; set; }
        [Display(Name = "Sale Order No.")]

        [Required]
        public string? sono { get; set; }

        [Required]
        [Display(Name = "Vender Type")]
        public string vendername { get; set; }
        [NotMapped]
        [Display(Name = "Vender Type")]
        public string vendername1 { get; set; }

        [Required]
        [Display(Name = "Return Type")]
        public string typeofreturn { get; set; }
        
        [NotMapped]
        [Display(Name = "Return Type")]
        public string typeofreturn1 { get; set; }

        [Required]
        [Display(Name = "Party Name")]
        public string partyname { get; set; }


        [Display(Name = "GST No.")]
        public string? gstinno { get; set; }

        //[Required]
        //[Display(Name = "Mobile No.")]
        //[MinLength(11)] 
        //[MaxLength(11)]
        //[RegularExpression(@"^\d{0,11}$", ErrorMessage = "Phone number should have a maximum of 11 digits.")]
        //public string contactno { get; set; }

        [Required(ErrorMessage = "Mobile No. is required")]
        [Display(Name = "Mobile No.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile No. should have exactly 10 digits.")]
        public string contactno { get; set; }


        //[Required(ErrorMessage = "Mobile No. is required")]
        //[Display(Name = "Mobile No.")]
        //[RegularExpression(@"^\d{10,11}$", ErrorMessage = "Mobile No. should have 10 digits.")]
        //public string contactno { get; set; }


        [Required]
        [Display(Name = "Address")]
        [StringLength(200)]
        public string address { get; set; }

        //[Display(Name = "DC No")]
        //public string? dcno { get; set; }

        //[Display(Name = "In. No")]
        //public string? invoiceno { get; set; }

        //[RegularExpression("^[A-Z0-9/_\\-]*$", ErrorMessage = "DC No must be in correct formate !")]


        //[RegularExpression("^[A-Z0-9/_\\-]*$", ErrorMessage = "Invoice  No must be in correct formate !")]

        [Display(Name = "DC No.")]
        public string? dcno { get; set; }

        [Display(Name = "Invoice No.")]
        public string? invoiceno { get; set; }


        //[Display(Name = "DC No")]
        //[RegularExpression("^[A-Z0-9]*$", ErrorMessage = "DC No must contain only uppercase letters and numbers.")]
        //public string? dcno { get; set; }

        //[Display(Name = "In. No")]
        //[RegularExpression("^[A-Z0-9]*$", ErrorMessage = "In. No must contain only uppercase letters and numbers.")]
        //public string? invoiceno { get; set; }

        [Display(Name = "DC Date")]
        public string? dcdate { get; set; }

        [Display(Name = "In Date")]
        public string? invoicedate { get; set; }
        //= DateTime.Now.ToString("yyyy-MM-dd");

        [Required(ErrorMessage = "GRN code is required.")]
        [Display(Name = "GRN No.")]
        public string grnno { get; set; }

        [Required]
        [Display(Name = "GRN Date")]
        public string grndate { get; set; }

        [Display(Name = " Remarks")]
        [StringLength(500)]
        public string? remarks { get; set; } 

        [Display(Name = "Batch Code")]
        [RegularExpression(@"^\d{2}[A-La-l]\d{2}$", ErrorMessage = "Batch code should be in the format YYMDD - (Year Month Date).")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "Batch code should be exactly 5 characters long.")]
        [Required(ErrorMessage = "Batch code is required.")]
        public string batchcode { get; set; }
        [Display(Name = "Inward Date")]
        [Required]
        //public string date { get; set; } = DateTime.Now.ToString();
        public string date { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");


        [Display(Name = "Time")]
        [Required]
        public string time { get; set; } = DateTime.Now.ToString("hh:mm");

        public virtual List<inwardPacket> inwardPacket { get; set; } = new List<inwardPacket>();

        [NotMapped]
        public List<SelectedRows> SelectedRows { get; set; } = new List<SelectedRows>();

        public List<inward_subcomponent> inward_subcomponent { get; set; } = new List<inward_subcomponent>();

        [Display(Name = "Order Type")]
        public string ordertype { get; set; }

        [Display(Name = "Status")] 
        public string status { get; set; } = "Pending";

        [NotMapped]
        public bool IsDeleted { get; set; } = false;

        public int flag { get; set; } = 0;

        [NotMapped]
        public int printflag { get; set; }
        
        [NotMapped]

        [Display(Name = "Reprinting Remark")]
        public string remark { get; set; }

        //public inward()
        //{
        //    // Set default values for properties
        //    vendername = "-";
        //    typeofreturn = "-";
        //    gstinno = "-";
        //    dcno = "-";
        //    invoiceno = "-";
        //    dcdate = "-";
        //    invoicedate = "-";
        //    remarks = "-";
        //    ordertype = "-";
        //}
        [NotMapped]
        public string productcode { get; set; }
        [NotMapped]
        public int qty { get; set; }
        [NotMapped]
        public int pqty { get; set; }
    }
}

