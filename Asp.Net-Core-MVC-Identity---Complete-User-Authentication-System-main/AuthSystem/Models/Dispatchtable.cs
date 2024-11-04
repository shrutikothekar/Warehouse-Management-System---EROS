using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    
    public class Dispatchtable
    {
       
        public int id { get; set; }
        public string sono { get; set; }
        public string boxno { get; set; }
        public string productcode { get; set; }

        public string productname { get; set; }
    } 
}
