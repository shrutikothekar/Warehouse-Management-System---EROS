using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace eros.Models
{
    public class Customer_Master
    {
        internal int Customerid;
        internal int id;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int customer_id { get; set; }

        [Required]
        [Display(Name = "Customer Name")]
        public string customername { get; set; }
        [Required]
        [Display(Name = "Contact Person")]
        public string contactperson { get; set; }
        [Required]
        [Display(Name = "GST No.")]
        public string gstno { get; set; }

        [Required]
        [Display(Name = "Mobile No.")]
        [MinLength(10)]
        [MaxLength(10)]
        [RegularExpression(@"^\d{0,10}$", ErrorMessage = "Phone number should have a maximum of 10 digits.")]
        public string contactno { get; set; }
        [Display(Name = "Email")]
        //[EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string? emailid { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string address { get; set; }

        [Required]
        [Display(Name = "City")]
        public string city { get; set; }

        [Required]
        [Display(Name = "State")]
        public string state { get; set; }

        [Required]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [Required]
        [MinLength(6)]
        [MaxLength(6)]
        [RegularExpression(@"^\d{0,6}$", ErrorMessage = "Pincode should have a maximum of 6 digits.")]
        [Display(Name = "PinCode")]
        public string pincode { get; set; }
        public virtual List<consignee> Consignee_masters { get; set; } = new List<consignee>();

        [NotMapped]
        public bool IsDeleted { get; set; } = false;
    }
}
