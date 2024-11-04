using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class purchase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required]
        [Display(Name = "Purchase Order No. ")]
        public string pono { get; set; }

        [Required]
        [Display(Name = "PO Date")]
        public string podate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd"); 
        [NotMapped]
        [Display(Name = "In Date")]
        public string InDate { get; set; }

        [Required]
        [Display(Name = "Supplier Name")]
        public string suppliername { get; set; }

        //[Required]
        //[Display(Name = "Contact")]
        //[MinLength(11)]
        //[MaxLength(11)]
        //[RegularExpression(@"^\d{0,11}$", ErrorMessage = "Phone number should have a maximum of 11 digits.")]
        //public string contactno { get; set; }

        [Required(ErrorMessage = "Contact number is required")]
        [Display(Name = "Contact")]
        [MinLength(10, ErrorMessage = "Contact number should have 10 digits.")]
        [MaxLength(10, ErrorMessage = "Contact number should have 10 digits.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Contact number should have exactly 10 digits.")]
        public string contactno { get; set; }



        [Required]
        [Display(Name = "GST No.")]
        public string gstinno { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string address { get; set; }

        [Required]
        [Display(Name = "Status")] //BY DEFAULT SET AS PENDING STATUS 
        public string status { get; set; } = "Pending";

        //Product_Purchases = poProduct_details
        //public virtual List<purchase_product> poProduct_details { get; set; } = new List<purchase_product>();
        public List<purchase_product> poProduct_details { get; set; } = new List<purchase_product>();
        
        public List<purchase_subcomponent> purchase_subcomponent { get; set; } = new List<purchase_subcomponent>();

        [Required]
        [NotMapped]
        [Display(Name = "Remaining. Qty")]
        public int pqty { get; set; } = 0;

        [Required]
        [NotMapped]
        [Display(Name = "Ordered. Qty")]
        public int qty { get; set; } = 0;

        //[Display(Name = "Timestamp")]
        //public DateTime Timestamp { get; set; } = DateTime.Now;

        [NotMapped]
        public bool IsDeleted { get; set; } = false;
    }
}
