using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace eros.Models
{
    public class inwardPacket
    {
        [Key]
        public int id { get; set; }

        [ForeignKey("Inward")]
        public int inwardId { get; set; }

        [NotMapped]
        public virtual inward Inward { get; private set; }

        public string? pono { get; set; }

        public string? sono { get; set; }

        [Display(Name = "Brand")]
        public string brand { get; set; }
        [Required]
        [Display(Name = "Product Code")]
        public string productcode { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        public string description { get; set; }

        [NotMapped]
        [Display(Name = "Product Name")]
        public string description1 { get; set; }

        [Display(Name = "Qty/Shp.")]
        //public string? serialno { get; set; }
        public int noqtypershp { get; set; } = 0;

        [Required]
        [Display(Name = "No. of set/product")]
        public string? setofsub_assemb { get; set; }

        [Required]
        [Display(Name = "Ordered Quantity")]
        public int quantity { get; set; }

        [Required]
        [Display(Name = "UOM")]
        public string? uom { get; set; }

        [Required]
        [Display(Name = "No. of Qty")]
        public string? qtyperpkt { get; set; }

        [Required]
        [Display(Name = "No of Pkt")]
        public string? noofpackets { get; set; }

        [NotMapped]
        [Display(Name = "Supplier Name")]
        public string? supplier { get; set; }


        [Required]
        [Display(Name = "Total Shippers")]
        public string? totalpacket { get; set; }

        [Required]
        [Display(Name = "Total sets")]
        public int totalsubassmbly { get; set; } 

        [Display(Name = "Total Sub-Com UOM")]
        public int? totalSubComUOM { get; set; } = 0;

        [Display(Name = "No of Sub-Com QTY per shipper")]
        public int? NoOfSubComQTYperShipper { get; set; } = 0;

        [Required]
        [NotMapped]
        public bool IsDeleted { get; set; } = false;

        [Required]
        [NotMapped]
        [Display(Name = "Remaining Quantity")]
        public int pqty { get; set; } = 0;

        //add for check warrenty status 

        [Display(Name = "Remaining Days")]
        [NotMapped]
        public string remDays { get; set; }

        //[Display(Name = "Warrenty status")]
        //public string? checkstatus { get; set; }
        [NotMapped]
        public string? check { get; set; }

        //[Display(Name = "Sr No.")]
        //public string? serialno { get; set; }

        //[Display(Name = "Template Name")]
        //public string? templatename { get; set; }
        [Display(Name = "SO Qty")]
        public int SOQty { get; set; } = 0;
        [Display(Name = "DL/IN Qty")]
        public int DLQty { get; set; } = 0;
        [Display(Name = "PO Qty")]
        public int POQty { get; set; } = 0;
        // Constructor to set default values
        //public inwardPacket()
        //{
        //    // Set default values for properties
        //    brand = "-";
        //    setofsub_assemb = "-";
        //    uom = "-";
        //    qtyperpkt = "0";
        //    noofpackets = "0";
        //    totalpacket = "0";
        //    totalSubComUOM = 0;
        //    NoOfSubComQTYperShipper = 0;
        //    pqty = 0;
        //    remDays = "-";
        //    check = "-";
        //}

        [Display(Name = "Inward Date")]
        [Required]
        //public string date { get; set; } = DateTime.Now.ToString();
        public string date { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
        public int flag { get; set; } = 0;
        [NotMapped]
        public int pickquantity { get; set; } = 0;


        [NotMapped]
        [Display(Name = "Customer Name")]
        public string Customer { get; set; }
        [NotMapped]
        [Display(Name = "Condition")]
        public string type { get; set; }
        [NotMapped]
        [Display(Name = "Category")]
        public string Category { get; set; }

        [NotMapped]
        [Display(Name = "vendor")]
        public string vendor { get; set; }
        [NotMapped]
        [Display(Name = "partyname")]
        public string partyname { get; set; }
        [NotMapped]
        [Display(Name = "inout")]
        public int inout { get; set; }
        [NotMapped]
        [Display(Name = "status")]
        public string status { get; set; }
        [NotMapped]
        [Display(Name = "ordertype")]
        public string ordertype { get; set; }
    }
}
