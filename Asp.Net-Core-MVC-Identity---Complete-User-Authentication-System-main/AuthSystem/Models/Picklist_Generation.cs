    //model

    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace eros.Models
    {
        public class Picklist_Generation
        {
            [Key]
            public int gen_id { get; set; }
            [Display(Name = "Order Ref. No")]
            public string sono { get; set; }
            public string prdcode { get; set; }

            public string prdname { get; set; }

            public string soqty { get; set; }
        
            public string pickingqty { get; set; } = "0";
            public int? flagstatus { get; set; } =0;
       
            public virtual List<pickstorage> pickstorages { get; set; } = new List<pickstorage>();
            [NotMapped]
            public string status { get; set; } 
            [NotMapped]
            public string date { get; set; }
            [NotMapped]
            public string category { get; set; }
        [NotMapped]
            public string customer { get; set; }

    }
    }
