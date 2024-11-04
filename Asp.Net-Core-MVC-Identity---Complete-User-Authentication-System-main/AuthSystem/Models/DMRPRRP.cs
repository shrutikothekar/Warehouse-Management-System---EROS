using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class DMRPRRP
    {
        public int id { get; set; }
        public string productcode { get; set; }
        public string grn { get; set; }
        public string batch { get; set; }
        public string boxno { get; set; }
        public string refno { get; set; }
        public int inout { get; set; }
        public int pickflag { get; set; }
        public string ordertype { get; set; }
        public string statusflag { get; set; }
        public string location { get; set; }
        public string type { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public string returntype { get; set; }
        public string partyname { get; set; }
        //added
        public string condition { get; set; }
        public string from { get; set; }
        [NotMapped]
        public string Status { get; set; }
        [NotMapped]
        public string qty { get; set; }
        [NotMapped]
        public string pqty { get; set; }

    }
}
