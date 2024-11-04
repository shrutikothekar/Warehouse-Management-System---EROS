using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class CheckWarrantyStatus
    {
        public int Id { get; set; }

        [Display(Name = "Vender Type")]
        public string vendername { get; set; }

        [Display(Name = "Product Name")]
        public string productname { get; set; }

        [Display(Name = "Product Code")]
        public string productcode { get; set; }
        
        [Display(Name = "Warranty")]
        public string warranty { get; set; }
        
        [Display(Name = "Sr No.")]
        public string srno { get; set; }

        [Display(Name = "Remaining Days")]
        public string remDays { get; set; }

        [Display(Name = "status")]
        public string status { get; set; }

    }
}
