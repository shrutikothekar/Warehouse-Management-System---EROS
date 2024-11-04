using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class CombinedViewModel
    {
        public string supplier { get; set; }
        public int poqty { get; set; }
        public string customer { get; set; }
        public int soqty { get; set; }
        public string date { get; set; }
        public string product { get; set; }
        public int currentqty { get; set; }

    }
    public class PurchaseOrder
    {
        public int poqty { get; set; }
        public string supplier { get; set; }
        [NotMapped]
        public string pono { get; set; }
        public string date { get; set; }
        public string product { get; set; }
        public int currentqty { get; set; }
        public string category { get; set; }
    }

    public class SalesOrder
    {
        public int soqty { get; set; }
        public string customer { get; set; }
        [NotMapped]
        public string sono { get; set; }
        public string date { get; set; }
        public string product { get; set; }
        public int currentqty { get; set; }
        public string category { get; set; }

    }

}
