using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace eros.Models
{
    public class consignee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [ForeignKey("customer_id")]
        public int customerid { get; set; }
        [NotMapped]
        public virtual Customer_Master customer_id { get; private set; }


        //  public virtual Customer_Master customer_id { get; private set; }
        [Display(Name = "Name")]
        public string? consigneename { get; set; }

        [Display(Name = "Mobile No.")]
        //[MinLength(10)]
        //[MaxLength(10)]
        //[RegularExpression(@"^\d{0,10}$", ErrorMessage = "Phone number should have a maximum of 10 digits.")]
        public string? consigneecontact { get; set; } 

        [Display(Name = " Contact Person")]
        public string? consigneeperson { get; set; } 

        [Display(Name = "Address")]
        public string? consigneeaddress { get; set; }

        [Display(Name = " Email")]
        public string? consigneeemail { get; set; } 

        [Display(Name = "City")]
        public string? consigneecity { get; set; } 

        [Display(Name = " State")]
        public string? consigneestate { get; set; }

        [Display(Name = " Pincode")]
        public string? consigneepincode { get; set; } 

        [NotMapped]
        public bool IsDeleted { get; set; } = false;
    }
}
