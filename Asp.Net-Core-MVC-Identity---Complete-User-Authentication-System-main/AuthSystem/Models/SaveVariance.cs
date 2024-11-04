using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    public class SaveVariance
    {
        //[Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string locationcode { get; set; }
        public string productcode { get; set; }
        public string batchcode { get; set; }
        public string boxno { get; set; }
        public string physicalid { get; set; }
        public string Remark { get; set; }
        public int pstcount { get; set; }

        public int storagecount { get; set; }
        public int stockvariance { get; set; }


    }
}
