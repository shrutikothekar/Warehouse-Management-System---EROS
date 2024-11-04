//using System.ComponentModel.DataAnnotations.Schema;

//namespace eros.Models
//{
//    public class pickstorage
//    {
//        public int Id { get; set; }

//        [ForeignKey("picklist_generation")]
//        public int genid { get; set; }

//        [NotMapped]
//        public virtual Picklist_Generation Picklist_Generation { get; private set; }
//        public string sono { get; set; }
//        public string productcode { get; set; }
//        public string location { get; set; }
//        public string batchcode { get; set; }
//        public string boxno { get; set; }
//        public int flag { get; set; } = 0 ;

//    }
//}
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class pickstorage
    {
        public int Id { get; set; }

        [ForeignKey("picklist_generation")]
        public int genid { get; set; }

        [NotMapped]
        public virtual Picklist_Generation Picklist_Generation { get; private set; }
        public string sono { get; set; }
        public string productcode { get; set; }
        public string location { get; set; }
        public string batchcode { get; set; }
        public string boxno { get; set; }
        public int flag { get; set; } = 0;

    }
}
