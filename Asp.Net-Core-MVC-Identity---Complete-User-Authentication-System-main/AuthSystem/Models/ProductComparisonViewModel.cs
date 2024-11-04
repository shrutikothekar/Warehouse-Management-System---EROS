namespace eros.Models
{
    public class ProductComparisonViewModel
    {

        public string ProductCode { get; set; }
        public int StorageMinTotalCount { get; set; }
        public int PhysicalStockMinTotalCount { get; set; }

        public int StockVariance { get; set; }
        public string Remark { get; set; }

        public string physicalid { get; set; }
    }
}
