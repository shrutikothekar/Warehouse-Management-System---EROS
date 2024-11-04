using System.ComponentModel.DataAnnotations;

namespace eros.Models
{
    public class ReprintingRemark
    {
        public int id { get; set; }

        public string sono { get; set; }

        public string batch { get; set; }

        public string grn { get; set; }
        public string remark { get; set; }

        public string date { get; set; } = DateTime.Now.ToString("dd-MM-yyyy");
        public string time { get; set; } = DateTime.Now.ToString("hh:mm");
    }
}
