using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class Picking_Packet
    {
        public int id { get; set; }

        [ForeignKey("generateid")]
        public int pick_id { get; set; }

        [NotMapped]
        public virtual Picking_Process generateid { get; private set; }
        public string productcode { get; set; }

        public string productname { get; set; }

        public string soqty { get; set; }

        public string pickingqty { get; set; }

        public string boxno { get; set; }

        public string batchcode { get; set; }

        public string location { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public string month { get; set; }
        public string year { get; set; }
        public string field1 { get; set; }
        public string field2 { get; set; }
        public string field3 { get; set; }
        public string field4 { get; set; }
        public string field5 { get; set; }
        [NotMapped]
        public bool IsSelected { get; set; } = false;
    }
}
