using Microsoft.EntityFrameworkCore;

namespace eros.Models
{
    [Keyless]
    public class OverAllCount
    {
        public string ProductCode { get; set; }
        //public string Boxes1 { get; set; }
        public string Boxes { get; set; }
        public int TotalCount { get; set; }


        public string phystorDenominator { get; set; }
        public int ProductCount { get; set; }
        public bool IsCountEqualToDenominator { get; set; }

        //[NotMapped]
        public List<string> MissingBoxes { get; set; }
        //[NotMapped]
        public int MinTotalCount { get; set; }

    }
}
