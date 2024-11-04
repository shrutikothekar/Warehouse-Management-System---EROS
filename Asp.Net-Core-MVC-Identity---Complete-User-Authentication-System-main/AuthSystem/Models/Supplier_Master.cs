using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class Supplier_Master
    {
        internal int id;

        [Key]
        public int supplierid { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Supplier Code")]
        public string suppliercode { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Supplier Name")]
        public string supplier_name { get; set; }

        [Required]
        [Display(Name = "Br. Name")]
        public string brand { get; set; }

        [Required]
        [Display(Name = "GSTIN No.")]
        public string gstno { get; set; }

        //[Required]
        //[Display(Name = "Mobile No.")]
        //[MinLength(10)]
        //[MaxLength(10)]
        //[RegularExpression(@"^\d{0,10}$", ErrorMessage = "Phone number should have a maximum of 10 digits.")]

        [Required]
        [Display(Name = "Mobile No.")]
        [MinLength(10)]
        [MaxLength(10)]
        [RegularExpression(@"^\d{0,10}$", ErrorMessage = "Phone number should have a maximum of 10 digits.")]
        public string contactno { get; set; }
        //[Required]
        //[Display(Name = "Contact")]
        //[RegularExpression(@"^\d{10,11}$", ErrorMessage = "Phone number should have 10 or 11 digits.")]
        //public string contactno { get; set; }


        [Display(Name = "Email")]
        //[EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string? emailid { get; set; }

        //[Display(Name = "Email")]
        //public string? emailid { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Address")]
        public string address { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "City")]
        public string city { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "State")]
        public string state { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Country")]
        public string country { get; set; }

        [Required]
        [MinLength(6)]
        [MaxLength(6)]
        [RegularExpression(@"^\d{0,6}$", ErrorMessage = "Pincode should have a maximum of 6 digits.")]
        [Display(Name = "PinCode")]
        public string pincode { get; set; }

        [NotMapped]
        public bool IsDeleted { get; set; } = false;

    }
}
