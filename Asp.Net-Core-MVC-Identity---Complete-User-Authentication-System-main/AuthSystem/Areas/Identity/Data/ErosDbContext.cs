using AuthSystem.Areas.Identity.Data;
using AuthSystem.Models;
using eros.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using static eros.Controllers.Loading_Dispatch_OperationController;

namespace AuthSystem.Data;

public class ErosDbContext : IdentityDbContext<ApplicationUser>
{
    internal object OtherEntity;
    
    public ErosDbContext(DbContextOptions<ErosDbContext> options)
        : base(options)
    {
    }

    public ErosDbContext()
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<so_product>()
            .HasOne(b => b.sales)
            .WithMany(a => a.soProduct_details)
            .HasForeignKey(b => b.orderid);
        base.OnModelCreating(builder);

        builder.Entity<so_productReturn>()
            .HasOne(b => b.salesReturn)
            .WithMany(a => a.soProduct_detailsReturn)
            .HasForeignKey(b => b.orderid);
        base.OnModelCreating(builder);

        builder.Entity<purchase_product>()
            .HasOne(b => b.purchase)
            .WithMany(a => a.poProduct_details)
            .HasForeignKey(b => b.porderid);
        base.OnModelCreating(builder);
        
        builder.Entity<purchase_productReturn>()
            .HasOne(b => b.purchaseReturn)
            .WithMany(a => a.poProduct_detailsReturn)
            .HasForeignKey(b => b.porderid);
        base.OnModelCreating(builder);

        builder.Entity<productmaster_packet>()
            .HasOne(b => b.productmaster)
            .WithMany(a => a.Productmaster_Packets)
            .HasForeignKey(b => b.productmasterId);
        base.OnModelCreating(builder);

        builder.Entity<pickstorage>()
            .HasOne(b => b.Picklist_Generation)
            .WithMany(a => a.pickstorages)
            .HasForeignKey(b => b.genid);
        base.OnModelCreating(builder);

        builder.Entity<consignee>()
            .HasOne(b => b.customer_id)
            .WithMany(a => a.Consignee_masters)
            .HasForeignKey(b => b.customerid);
        base.OnModelCreating(builder);

        builder.Entity<purchase_subcomponent>()
            .HasOne(b => b.purchase)
            .WithMany(a => a.purchase_subcomponent)
            .HasForeignKey(b => b.purchaseproduct_id);
        base.OnModelCreating(builder);

        builder.Entity<inward_subcomponent>()
            .HasOne(b => b.inward)
            .WithMany(a => a.inward_subcomponent)
            .HasForeignKey(b => b.inwardpacket_id);
        base.OnModelCreating(builder);

        builder.Entity<so_subcomponent>()
            .HasOne(b => b.so_inward)
            .WithMany(a => a.so_subcomponent)
            .HasForeignKey(b => b.soproduct_id);
        base.OnModelCreating(builder);
    }

    internal Task savechangesasync()
    {
        throw new NotImplementedException();
    }
    //public DbSet<eros.Models.TransactionList> TransactionList { get; set; }

    public DbSet<SHPList> SHPList { get; set; }
    public DbSet<DMRPRRP> DMRPRRP { get; set; }
    public DbSet<category_master> category_master { get; set; }
    public DbSet<eros.Models.PR_model> PR_model { get; set; } = default!;
    public DbSet<eros.Models.TempLoad> TempLoad { get; set; } = default!;
    public DbSet<eros.Models.ReprintingRemark> ReprintingRemark { get; set; } = default!;
    public DbSet<eros.Models.pickstorage> pickstorage { get; set; } = default!;
    public DbSet<eros.Models.Customer_Master> Customer_Master { get; set; } = default!;
    public DbSet<eros.Models.Location_Master> Location_Master { get; set; } = default!;
    public DbSet<eros.Models.rack_master> rack_master { get; set; } = default!;
    public DbSet<eros.Models.state_master> state_Master { get; set; } = default!;
    public DbSet<eros.Models.storage_master> storage_master { get; set; } = default!;
    public DbSet<eros.Models.subcategory_master> subcategory_Master { get; set; } = default!;
    public DbSet<eros.Models.Supplier_Master> Supplier_Master { get; set; } = default!;
    public DbSet<eros.Models.category_master> Category_Master { get; set; } = default!;
    public DbSet<eros.Models.usertype_master> usertype_Master { get; set; } = default!;
    public DbSet<eros.Models.Showroom_Master> Showroom_Master { get; set; } = default!;
    public DbSet<eros.Models.Picking> Picking { get; set; } = default!;
    public DbSet<eros.Models.picking_master> picking_master { get; set; } = default!;
    public DbSet<eros.Models.consignee> consignee { get; set; } = default!;
    public DbSet<eros.Models.SelectListModels> SelectListModels { get; set; } = default!;

    public DbSet<eros.Models.Product_Master> Product_Master { get; set; } = default!;
    public DbSet<eros.Models.productmaster_packet> productmaster_Packet { get; set; } = default!;

    //public DbSet<eros.Models.picklistgeneration> picklistgenerations { get; set; } = default!;
    //public DbSet<eros.Models.picklistgnerateprd> picklistgnerateprds { get; set; } = default!;

    public DbSet<eros.Models.inward> inward { get; set; } = default!;
    public DbSet<eros.Models.inwardPacket> inwardPacket { get; set; } = default!;
    public DbSet<eros.Models.inward_subcomponent> inward_subcomponent { get; set; } = default!;
    public DbSet<eros.Models.inwardSerialno> inwardSerialno { get; set; } = default!;


    public DbSet<eros.Models.Outward> Outward { get; set; } = default!;
    public DbSet<eros.Models.outwardPacket> outwardPacket { get; set; } = default!;


    public DbSet<eros.Models.purchase> purchase { get; set; } = default!;
    public DbSet<eros.Models.purchase_product> poProduct_details { get; set; } = default!;
    public DbSet<eros.Models.purchaseReturn> purchaseReturn { get; set; } = default!;
    public DbSet<eros.Models.purchase_productReturn> poProduct_detailsReturn { get; set; } = default!;
    public DbSet<eros.Models.purchase_subcomponent> purchase_subcomponent { get; set; } = default!;


    public DbSet<eros.Models.so_inward> so_inward { get; set; } = default!;
    public DbSet<eros.Models.so_product> so_product { get; set; } = default!;    
    public DbSet<eros.Models.so_inwardReturn> so_inwardReturn { get; set; } = default!;
    public DbSet<eros.Models.so_productReturn> so_productReturn { get; set; } = default!;

    public DbSet<eros.Models.so_subcomponent> so_Subcomponents { get; set; } = default!;


    public DbSet<eros.Models.MenuModel> MenuModel { get; set; } = default!;
    public DbSet<eros.Models.UserManagment> UserManagement { get; set; } = default!;


    public DbSet<eros.Models.Storage_Operation> Storage_Operation { get; set; } = default!;
    public DbSet<eros.Models.InStockQty> InStockQty { get; set; } = default!;
    public DbSet<eros.Models.unit_master> unit_master { get; set; } = default!;
    public DbSet<eros.Models.Picking_Process> Picking_Process { get; set; } = default!;
    public DbSet<eros.Models.Picking_Packet> Picking_Packet { get; set; } = default!;
    public DbSet<eros.Models.Picklist_Generation> Picklist_Generation { get; set; } = default!;
    public DbSet<eros.Models.Picking_Operation> Picking_Operation { get; set; } = default!;
    public DbSet<eros.Models.Transport_Master> Transport_Master { get; set; } = default!;
    public DbSet<eros.Models.Courier_Master> Courier_Master { get; set; } = default!;
    public DbSet<eros.Models.Loading_Dispatch_Operation> Loading_Dispatch_Operation { get; set; } = default!;
    public DbSet<eros.Models.Dispatchtable> Dispatchtable { get; set; } = default!;
    public DbSet<eros.Models.Courier_Transport> Courier_Transport { get; set; } = default!;
    public DbSet<eros.Models.PhysicalStockTake> PhysicalStockTake { get; set; } = default!;
    public DbSet<eros.Models.OverAllCount> OverAllCount { get; set; } = default!;
    public DbSet<eros.Models.MatchedData> MatchedData { get; set; } = default!;
    public DbSet<eros.Models.CheckPhysicalStock> checkphysicalstock { get; set; } = default!;
    //public DbSet<eros.Models.poid> poid { get; set; } = default!;

    public DbSet<eros.Models.Load_Dispatchtable> Load_Dispatchtable { get; set; } = default!;
    public DbSet<eros.Models.LoadData_table> LoadData_table { get; set; } = default!;


    public DbSet<eros.Models.Cascade.country> Countries { get; set; } = default!;
    public DbSet<eros.Models.Cascade.city> cities { get; set; } = default!;
    public DbSet<eros.Models.Cascade.state> states { get; set; } = default!;



    public DbSet<eros.Models.uom> uom { get; set; } = default!;
    public DbSet<eros.Models.loginlog> loginlog { get; set; } = default!;
    public DbSet<eros.Models.Logs> logs { get; set; } = default!;
    public DbSet<eros.Models.Report> reports { get; set; } = default!;

    public DbSet<eros.Models.SaveVariance> SaveVariance { get; set; } = default!;

    public DbSet<eros.Models.productwisesave> productwisesave { get; set; } = default!;

}
