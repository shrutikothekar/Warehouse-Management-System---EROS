using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace eros.Models
{
    public class so_inwardReturn
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required]
        [Display(Name = "Sale Order No.")]
        public string sono { get; set; }
        [Required]
        [Display(Name = "Direct Order No.")]
        public string? dono { get; set; }

        [Required]
        [Display(Name = "SO Date")]
        public string sodate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");

        [NotMapped]
        [Display(Name = "Dispatch Date")]
        public string dDate { get; set; }

        [Required]
        [Display(Name = "Cust. Name")]
        public string customername { get; set; }
        [Display(Name = "Cust. Name")]
        [NotMapped]
        public string dcustomername { get; set; }

        //[Required]
        //[Display(Name = "Contact")]
        //[MinLength(11)]
        //[MaxLength(11)]
        //[RegularExpression(@"^\d{0,11}$", ErrorMessage = "Phone number should have a maximum of 11 digits.")]
        //public string contactno { get; set; }
        [Required]
        [Display(Name = "Contact")]
        [MinLength(10)]
        [MaxLength(10)]
        [RegularExpression(@"^\d{0,10}$", ErrorMessage = "Phone number should have a maximum of 10 digits.")]
        public string contactno { get; set; }
        //[Required]
        //[Display(Name = "Contact")]
        //[RegularExpression(@"^\d{10,11}$", ErrorMessage = "Phone number should have 10 or 11 digits.")]
        //public string contactno { get; set; }


        [Required]
        [Display(Name = "Email")]
        public string emailid { get; set; }

        [Required]
        [Display(Name = "Shipping Address")]
        public string address { get; set; }

        [Required]
        [Display(Name = "City")]
        public string city { get; set; }

        [Required]
        [Display(Name = "State")]
        public string state { get; set; }

        [Required]
        [Display(Name = "Order Date")]
        public string dispatchdate { get; set; }


        [Required]
        [Display(Name = "Sales Return Product Details")]
        public List<so_productReturn> soProduct_detailsReturn { get; set; } = new List<so_productReturn>();
        //public List<so_subcomponent> so_subcomponent { get; set; } = new List<so_subcomponent>();

        public string status { get; set; } = "Pending";

        [NotMapped]
        [Display(Name = "Rem.Qty")]
        public int pqty { get; set; } = 0;

        [NotMapped]
        [Display(Name = "Odr.Qty")]
        public int qty { get; set; } = 0;

        [NotMapped]
        public bool IsDeleted { get; set; } = false;

        //internal bool Any(Func<object, bool> value)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
