using System.ComponentModel.DataAnnotations;

namespace eros.Models
{
    public class Showroom_Master
    {
        [Key]
        public int Showroomid { get; set; }

        [Required]
        //[StringLength(100)]
        [Display(Name = "Showroom Name")]
        public string Showroom_name { get; set; }

        [Required]
        //[StringLength(100)]
        [Display(Name = "Address")]
        public string address { get; set; }

        [Required]
        [Display(Name = "GSTIN No.")]
        //[StringLength(50)]
        public string gstno { get; set; }

        [Required]
        [MinLength(10)]
        [MaxLength(10)]
        [RegularExpression(@"^\d{0,10}$", ErrorMessage = "Phone number should have a maximum of 10 digits.")]
        [Display(Name = "Mobile No")]
        public string contactno { get; set; }

    }

}
