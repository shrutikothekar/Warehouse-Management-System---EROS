using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    [Keyless]
    public class Courier_Transport
    {
        public int id { get; set; }
        public string sono { get; set; }
        public string couriername { get; set; }
        public string docketno { get; set; }

        public string dispatchdate { get; set; }
        public string transportname { get; set; }
        public string truckno { get; set; }

        public string drivername { get; set; }
        public string contactno { get; set; }
        public string dispatchtype { get; set; }
    }
}
