using Microsoft.EntityFrameworkCore;

namespace eros.Models
{
    [Keyless]
    public class Damage
    {

        public string locationcode { get; set; }


        public string productcode { get; set; }

        public string batchcode { get; set; }


        public string boxes { get; set; }

        //public string remark { get; set; }
    }
}
