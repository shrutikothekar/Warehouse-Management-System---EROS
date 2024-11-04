using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class InStockQty
    {
        public int id { get; set; }
        [Display(Name = "Product Code")]
        public string productcode { get; set; }
        [Display(Name = "Current Stock")]
        public int currentqty { get; set; }
        [Display(Name = "Picked Stock")]
        public int stcokallocate { get; set; }
        [NotMapped]
        [Display(Name = "Picked Stock")]
        public int allotstock { get; set; }
        [Display(Name = "Product Name")]
        [NotMapped]
        public string productname { get; set; }
        [Display(Name = "Category")]
        [NotMapped]
        public string category { get; set; }
        [Display(Name = "In Qty")]
        [NotMapped]
        public int inqty { get; set; }


        [Display(Name = "Out Qty")]
        [NotMapped]
        public int outqty { get; set; }


        [Display(Name = "Total Qty")]
        [NotMapped]
        public int TotalInQty { get; set; }


        [Display(Name = "(< 30 days)")]
        [NotMapped]
        public int less30days { get; set; }


        [Display(Name = "(30 to 60 days)")]
        [NotMapped]
        public int days30to60 { get; set; }
        
        
        [Display(Name = "(60 to 90 days)")]
        [NotMapped]
        public int days60to90 { get; set; }


        [Display(Name = "(90 to 180 days)")]
        [NotMapped]
        public int days90to180 { get; set; }


        [Display(Name = "(180 to 270 days)")]
        [NotMapped]
        public int days180to270 { get; set; }


        [Display(Name = "(270 to 365 days)")]
        [NotMapped]
        public int days270to365 { get; set; }


        [Display(Name = "(365 to 1460 days)")]
        [NotMapped]
        public int days365to1460 { get; set; }


        [Display(Name = "(> 1460 days)")]
        [NotMapped]
        public int above1460days { get; set; }

    }
}
