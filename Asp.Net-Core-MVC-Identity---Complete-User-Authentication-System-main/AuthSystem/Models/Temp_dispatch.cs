using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    
    public class Temp_dispatch
    {
        public int id { get; set; }
        public string sono {get; set;}
        public  string location { get; set; }
        public string batchcode { get; set; }

        public string pickqty { get; set; }
        public string balqty { get; set; }

    }
}
