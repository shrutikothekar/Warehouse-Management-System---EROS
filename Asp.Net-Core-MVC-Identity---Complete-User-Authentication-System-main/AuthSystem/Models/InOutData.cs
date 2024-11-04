namespace eros.Models
{
    public class InOutData
    {
        public inward Inward { get; set; }
        public List<inwardPacket> InwardPackets { get; set; }

        public Picklist_Generation Picklist { get; set; }
        public List<pickstorage> Pickstorge { get; set; }

        public List<Loading_Dispatch_Operation> LoadingDispatch { get; set; }

        public so_inward SaleOrder { get; set; }
        public List<so_product> SaleOrderProduct { get; set; }

        public purchase Purchase { get; set; }
        public List<purchase_product> PurchaseProduct { get; set; }
    }
}
