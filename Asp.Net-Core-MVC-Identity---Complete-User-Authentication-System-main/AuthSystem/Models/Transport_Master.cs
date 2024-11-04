using System.ComponentModel.DataAnnotations;

namespace eros.Models
{
    public class Transport_Master
    {
        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Transport Code")]
        public string transportcode { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Transport Name")]
        public string transportname { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "contactperson")]
        public string contactperson { get; set; }

        [Required]
        [Display(Name = "Mobile No.")]
        [MinLength(10)]
        [MaxLength(10)]
        [RegularExpression(@"^\d{0,10}$", ErrorMessage = "Phone number should have a maximum of 10 digits.")]
        public string contactno { get; set; }

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
        [MinLength(6)]
        [MaxLength(6)]
        [RegularExpression(@"^\d{0,6}$", ErrorMessage = "Pincode should have a maximum of 6 digits.")]
        [Display(Name = "PinCode")]
        public string pincode { get; set; }
    }
}
