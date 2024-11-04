using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class Picking_Operation
    {
        [Key]
        public int pick_id { get; set; }


        [Display(Name = "Order No.")]
        public string sono { get; set; }
        [Display(Name = "Product Code")]
        public string productcode { get; set; }
    
 
        [Display(Name = "Picking Qty")]
        public string pickingqty { get; set; }
        [Display(Name = "Box No.")]
        public string boxno { get; set; }

        [Display(Name = "Batch Code")]
        public string batchcode { get; set; }

        [Display(Name = "location")]
        public string location { get; set; }

        [Display(Name = "Pick Box Qty.")]
        public string instockqty { get; set; }

        [Display(Name = "Date")]
        public string date { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");


        [Display(Name = "Time")]
        public string time { get; set; } = DateTime.Now.ToString("hh:mm");
       
        [Display(Name = "Qty Picked")]
        public int qtypicked { get; set; } = 0;

        [Display(Name = "Balance Qty")]
        public int balanceqty { get; set; }
        public string locationpicked { get; set; }
        public string boxpicked { get; set; }
        [NotMapped]
        public string Status { get; set; }
        public int flag { get; set; } = 0;

    }
}
//using System.ComponentModel.DataAnnotations;

//namespace eros.Models
//{
//    public class poid
//    {
//        [Display(Name = "Id")]

//        public int Id { get; set; }
//        [Display(Name = "SONO")]

//        public string sono { get; set; }
//        [Display(Name = "Status")]

//        public int? flagstatus { get; set; } = 0;

//        public virtual List<Picking_Operation> Picking_Operation { get; set; } = new List<Picking_Operation>();

//    }
//}