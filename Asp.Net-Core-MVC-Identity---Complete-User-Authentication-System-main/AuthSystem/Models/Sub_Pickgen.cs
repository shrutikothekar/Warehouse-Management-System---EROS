using System.ComponentModel.DataAnnotations;
namespace eros.Models
{
    public class Sub_Pickgen
    {
       
        public int id { get; set; }
        public string prdcode { get; set; }

        public string prdname { get; set; }

        public string soqty { get; set; }
        public string batchcode { get; set; }
        public string batchcodewiseqty { get; set; }
        public string pickingqty { get; set; }
        public string location { get; set; }
        public string boxno { get; set; }

        public string instockqty { get; set; }
    }
}
