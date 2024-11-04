using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace eros.Models
{
    [Keyless]
    public class MatchedData
    {
        //public PhysicalStockTake PhysicalStockItem { get; set; }
        public CheckPhysicalStock PhysicalStockItem { get; set; }
        public Storage_Operation StorageOperationItem { get; set; }

        public string Remark { get; set; }
        public int pstcount { get; set; }
        
        public string boxno { get; set; }

        public int storagecount { get; set; }
        public int stockvariance { get; set; }
    }
}
