using AspNetCoreHero.ToastNotification.Abstractions;
using AuthSystem.Data;
using eros.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Linq;
using System.Drawing;
using System.Text;
using static eros.Controllers.inwardsController;

namespace eros.Controllers
{
    public class OutwardsController : Controller
    {
        private readonly ErosDbContext _context;
        public INotyfService _notyfService { get; }

        private IWebHostEnvironment _webHostEnvironment;
        string printer = "IMPACT by Honeywell IH-2 (ZPL) (Copy 1)";
        string InstalledPrinters;
        Font printFont;
        StreamReader streamToPrint;

        public OutwardsController(ErosDbContext context, INotyfService notyfService, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _notyfService = notyfService;
            _webHostEnvironment = webHostEnvironment;
        }
        //DEMO REPRI REPLACEMENT
        private static List<DMRPRRP> DMRPRRP_List = new List<DMRPRRP>();
        private static List<Storage_Operation> Remove_StockInventory = new List<Storage_Operation>();
        private static List<DMRPRRP> Update_DMRPRRPInventory = new List<DMRPRRP>();
        private static List<Storage_Operation> Storage_OperationAdd = new List<Storage_Operation>();
        private static List<Storage_Operation> Storage_OperationExAdd = new List<Storage_Operation>();
        public class PickedListData_DMRPRRP
        {
            public string BoxNo { get; set; }
            public string batchno { get; set; }
            public string grnno { get; set; }
            public string productcode { get; set; }
            public string rproductcode { get; set; }
            public int rquantity { get; set; }
            public string ordertype { get; set; }
        }

        [HttpPost]
        public IActionResult SavePickingData_DMRPRRP([FromBody] PickedListData_DMRPRRP pickedListData)
        {
            try
            {
                // Access the selected columns here
                var selectedProductCode = "-";
                var selectedBoxNo = "-";
                var selectedBatchCode = "-";
                var location = "-";
                var loc = "TMP";
                var boxno = pickedListData.BoxNo.Trim();
                var splitbox = pickedListData.BoxNo.Split("-")[1];
                var batchno = pickedListData.batchno.Trim();
                var productcode = pickedListData.productcode.Trim().ToUpper();
                var rowproductcode = pickedListData.rproductcode.Trim().ToUpper();
                int rowquantity = pickedListData.rquantity;

                var counter = 0;
                var boxcount = 1;

                foreach (var row in DMRPRRP_List)
                {
                    var split = row.boxno.Split("-")[1];
                    if (row.batch.Trim() == pickedListData.batchno.Trim() && row.boxno.Trim() == pickedListData.BoxNo.Trim() && row.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper())
                    {
                        counter++;
                    }
                    else if (split == splitbox && row.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper())
                    {
                        boxcount += 1;
                        if (Convert.ToInt32(boxcount) == Convert.ToInt32(pickedListData.rquantity) + 1)
                        {
                            counter++;
                        }
                        else
                        {
                        }
                    }
                }
                if (counter == 0)
                {
                    if (pickedListData.ordertype.Trim() == "Demo")
                    {
                        //int maxId = _context.DMRPRRP.Any() ? _context.DMRPRRP.Max(e => e.id) + 1 : 1;
                        var check = _context.DMRPRRP
                                    .Where(a => a.productcode.Trim() == pickedListData.productcode.Trim()
                                    && a.boxno.Trim() == pickedListData.BoxNo.Trim()
                                    && a.batch.Trim() == pickedListData.batchno.Trim()
                                    && a.statusflag.Trim() == "ST"
                                    && a.pickflag == 0
                                    && a.type == "DM"
                                    && a.ordertype.Trim() == pickedListData.ordertype.Trim())
                                    .AsNoTracking()
                                    .FirstOrDefault();


                        if (check == null)
                        {

                            var check1 = _context.Storage_Operation
                                   .Where(a => a.productcode.Trim() == pickedListData.productcode.Trim()
                                   && a.boxno.Trim() == pickedListData.BoxNo.Trim()
                                   && a.batchcode.Trim() == pickedListData.batchno.Trim()
                                   && a.statusflag.Trim() == "ST"
                                   && a.pickflag == "0")
                                   .FirstOrDefault();

                            if (check1 != null)
                            {
                                Remove_StockInventory.Add(check1);
                                loc = check1.locationcode;
                                DMRPRRP st = new DMRPRRP()
                                {
                                    productcode = check1.productcode,
                                    grn = check1.grnno,
                                    batch = check1.batchcode,
                                    boxno = check1.boxno,
                                    refno = "-",
                                    ordertype = pickedListData.ordertype,
                                    inout = 2,
                                    pickflag = 1,
                                    location = loc,
                                    type = "DM",
                                    statusflag = "LD",
                                    date = DateTime.Now.ToString("yyyy-MM-dd"),
                                    time = DateTime.Now.ToString("HH:mm:ss"),

                                };
                                DMRPRRP_List.Add(st);
                            }
                            else
                            {
                                return Json(new { success = false, message = "The item sticker you have scan is not found in demo inventory or also not in stock inventory !" });
                            }
                        }
                        else
                        {
                            loc = check.location;
                            Update_DMRPRRPInventory.Add(check);

                            check.statusflag = "LD";
                            check.pickflag = 1;
                            check.inout = 2;
                            check.ordertype = pickedListData.ordertype;
                            check.type = "DM";
                            check.location = loc;

                            DMRPRRP_List.Add(check);
                        }
                    }
                    else if (pickedListData.ordertype.Trim() == "Repair")
                    {

                        //IF RPR STOCK AVAILABLE
                        var check = _context.DMRPRRP
                                    .Where(a => a.productcode.Trim() == pickedListData.productcode.Trim()
                                    && a.boxno.Trim() == pickedListData.BoxNo.Trim()
                                    && a.batch.Trim() == pickedListData.batchno.Trim()
                                    && a.statusflag.Trim() == "ST"
                                    && a.pickflag == 0
                                    && a.type == "RPR"
                                    && a.ordertype.Trim() == pickedListData.ordertype.Trim())
                                    .AsNoTracking()
                                    .FirstOrDefault();

                        if (check == null)
                        {
                            //IF ST STOCK AVAILABLE
                            var check1 = _context.Storage_Operation
                                   .Where(a => a.productcode.Trim() == pickedListData.productcode.Trim()
                                   && a.boxno.Trim() == pickedListData.BoxNo.Trim()
                                   && a.batchcode.Trim() == pickedListData.batchno.Trim()
                                   && (a.statusflag.Trim() == "ST" || a.statusflag.Trim() == "DMG")
                                   && a.pickflag == "0")
                                   .FirstOrDefault();

                            if (check1 != null)
                            {
                                Remove_StockInventory.Add(check1);
                                loc = check1.locationcode;
                                DMRPRRP st = new DMRPRRP()
                                {
                                    productcode = check1.productcode,
                                    grn = check1.grnno,
                                    batch = check1.batchcode,
                                    boxno = check1.boxno,
                                    refno = "-",
                                    ordertype = pickedListData.ordertype,
                                    inout = 2,
                                    pickflag = 1,
                                    location = loc,
                                    type = "RPR",
                                    statusflag = "LD",
                                    date = DateTime.Now.ToString("yyyy-MM-dd"),
                                    time = DateTime.Now.ToString("HH:mm:ss"),
                                };
                                DMRPRRP_List.Add(st);
                            }
                            else
                            {
                                return Json(new { success = false, message = "The item sticker you have scan is not found in Repair inventory And not also in stock inventory !" });
                            }
                        }
                        else
                        {
                            loc = check.location;
                            check.statusflag = "LD";
                            check.pickflag = 1;
                            check.inout = 2;
                            check.ordertype = pickedListData.ordertype;
                            check.type = "RPR";
                            check.location = loc;

                            DMRPRRP_List.Add(check);

                            var getdata = _context.DMRPRRP.Where(a => a.id == check.id).AsNoTracking().FirstOrDefault();
                            Update_DMRPRRPInventory.Add(check);

                        }
                    }
                    else if (pickedListData.ordertype.Trim() == "Replacement")
                    {
                        var LIST = _context.Storage_Operation
                            .Where(a => a.productcode.Trim() == pickedListData.productcode.Trim()
                            && a.boxno.Trim() == pickedListData.BoxNo.Trim()
                            && a.batchcode.Trim() == pickedListData.batchno.Trim()
                            && a.grnno.Trim() == pickedListData.grnno.Trim()
                            && a.statusflag.Trim() == "DMG"
                            && a.pickflag == "0")
                            .ToList();

                        var foundlist = _context.DMRPRRP.Where(a => a.productcode.Trim().ToUpper() == pickedListData.productcode.Trim() && a.batch.Trim() == pickedListData.batchno.Trim() && a.grn.Trim() == pickedListData.grnno.Trim() && a.boxno.Trim() == pickedListData.BoxNo.Trim() && a.pickflag == 0 && a.statusflag.Trim() == "DMG").FirstOrDefault();
                        if (foundlist != null)
                        {
                            return Json(new { success = false, message = "Scan product is damage in stock !" });
                        }
                        //IN CASE REPLACEMED STOCK COME BACK AND ONE DMG IS ALREDY IN STOCK
                        var check_if_out_before = _context.Storage_Operation
                            .Where(a => a.productcode.Trim() == pickedListData.productcode.Trim()
                            && a.boxno.Trim() == pickedListData.BoxNo.Trim()
                            && a.batchcode.Trim() == pickedListData.batchno.Trim()
                            && a.grnno.Trim() == pickedListData.grnno.Trim()
                            && (a.statusflag.Trim() == "DMG" || a.statusflag.Trim() == "ST")
                            && a.pickflag == "0")
                            .FirstOrDefault();

                        if (check_if_out_before != null)
                        {
                            loc = check_if_out_before.locationcode;
                            ////DMG STOCK IS FOUND AS DMG BEFORE 
                            //var found = _context.DMRPRRP
                            //    .Where(a => a.productcode.Trim() == pickedListData.productcode.Trim()
                            //    && a.ordertype == pickedListData.ordertype.Trim()
                            //    &&( a.statusflag.Trim() == "DMG" || a.statusflag.Trim() == "ST"))
                            //    .FirstOrDefault();

                            //if (found != null)
                            //{
                            //ADD STOCK FOR REPLCAMENET ENTRY IN DMRPRRP IN 
                            int maxIdR = _context.DMRPRRP.Any() ? _context.DMRPRRP.Max(e => e.id) + 1 : 1;
                            DMRPRRP DM = new DMRPRRP()
                            {
                                id = maxIdR,
                                productcode = pickedListData.productcode,
                                grn = pickedListData.grnno,
                                batch = pickedListData.batchno,
                                boxno = pickedListData.BoxNo,
                                refno = "-",
                                ordertype = pickedListData.ordertype,
                                inout = 1,
                                pickflag = 0,
                                location = loc,
                                type = "RP",
                                statusflag = "ST",
                                date = DateTime.Now.ToString("yyyy-MM-dd"),
                                time = DateTime.Now.ToString("HH:mm:ss"),
                            };
                            DMRPRRP_List.Add(DM);
                            Storage_OperationExAdd.Add(check_if_out_before); //AS IT IS IN STORAGE + NO UPDATE 
                                                                             //END
                                                                             //}

                        }
                        else
                        {
                            return Json(new { success = false, message = "Scan box is not found in stock !" });
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "Scan box is not found in stock !" });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Already Scanned all boxes !" });
                }

                return Json(new
                {
                    success = true,
                    message = "Done !",
                    data = new
                    {
                        productCode = pickedListData.productcode,
                        boxNo = pickedListData.BoxNo,
                        batchCode = pickedListData.batchno,
                        location = loc,
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
        [HttpPost] //  (DEMO REPAIR REPLACMENT)
        public IActionResult RefNoData(string refno, string ordertype)//pono, supplier
        {
            inward inward = new inward();

            if (refno != null)
            {
                var ponodata = _context.inward.Where(a => a.pono == refno.Trim()).OrderBy(a => a.inward_id).FirstOrDefault();
                if (ponodata != null)
                {
                    inward.contactno = ponodata.contactno;
                    inward.address = ponodata.address;
                    inward.partyname = ponodata.partyname;
                    inward.gstinno = ponodata.gstinno;
                    inward.vendername = ponodata.vendername;
                    inward.vendername1 = ponodata.vendername;
                    inward.remark = ponodata.remarks;
                    ViewData["PonoValue"] = ponodata.pono;
                    var packetdata = _context.inwardPacket.Where(a => a.inwardId == ponodata.inward_id).ToList();

                    //existing pono in inward if
                    //var existingpacket = _context.inwardPacket.Where(a => a.pono == refno.Trim())
                    //                    .GroupBy(p => p.description)
                    //                    .Select(group => new
                    //                    {
                    //                        ProductName = group.Key,
                    //                        TotalQuantity = group.Sum(p => p.quantity),
                    //                        TotalSubAssembly = group.Sum(p => p.totalsubassmbly)
                    //                    }).ToList();
                    int i = 0;
                    foreach (var mat in packetdata)
                    {
                        if (mat.flag == 1)
                        {
                            var liscount = _context.inwardPacket.Where(a => a.pono == mat.pono && a.flag == 2).ToList();
                            var count = liscount.Sum(a => a.quantity);
                            var newqty = Math.Abs(mat.quantity - count);
                            mat.quantity = newqty;
                        }
                        else
                        {
                            var liscount = _context.inwardPacket.Where(a => a.pono == mat.pono && a.flag == 1).ToList();
                            var count = liscount.Sum(a => a.quantity);
                            var newqty = Math.Abs(mat.quantity - count);
                            mat.quantity = newqty;
                        }
                        var foundnoofpackets = _context.DMRPRRP.Where(a => a.productcode.Trim() == mat.productcode.Trim()).Select(a => a.boxno).FirstOrDefault();
                        if (foundnoofpackets != null)
                        {
                            var split = foundnoofpackets.Split("-")[1];
                            var split2 = split.Split("/")[1];
                            var num = split[0];
                            var den = split2;
                            mat.noofpackets = den.ToString();
                            mat.setofsub_assemb = den.ToString();
                            mat.qtyperpkt = "1";
                        }

                        inward.inwardPacket.Add(new inwardPacket()
                        {
                            id = i,
                            pono = refno.Trim(),
                            productcode = mat.productcode.ToUpper(),
                            description = mat.description.ToUpper(),
                            description1 = mat.description.ToUpper(),
                            POQty = mat.quantity,
                            brand = mat.brand,
                            quantity = mat.quantity,
                            uom = mat.uom,
                            noofpackets = mat.noofpackets,
                            setofsub_assemb = mat.setofsub_assemb,
                            qtyperpkt = mat.qtyperpkt,
                        });
                    }
                    return Json(new { success = true, data = inward });
                    //return PartialView("DemoDetails", inward);

                }
                else
                {
                    inward.inwardPacket.Add(new inwardPacket() { id = 1 });

                    return Json(new { success = false, data = inward });
                    //return PartialView("DemoDetails", inward);  // Return empty inward object
                }
            }
            else
            {
                inward.inwardPacket.Add(new inwardPacket() { id = 1 });

                return Json(new { success = false, data = inward });
                //return PartialView("DemoDetails", inward);  // Return empty inward object
            }
        }
        public ActionResult FatchListData_DMRPRRP(string productcode, int quantity)
        {
            List<DMRPRRP> filteredList = new List<DMRPRRP>();
            if (DMRPRRP_List.Any())
            {
                filteredList = DMRPRRP_List.Where(a => a.productcode.ToUpper().Trim() == productcode.ToUpper().Trim()).ToList();
            }

            // Return the filtered list of data
            return Json(new { success = true, dataList = filteredList });
        }
        //END
        public class PickedListData1
        {
            public string BoxNo { get; set; }
            public string batchno { get; set; }
            public string productcode { get; set; }
            public string rproductcode { get; set; }
            public int rquantity { get; set; }
            public string ordertype { get; set; }
        }
        private static List<Loading_Dispatch_Operation> tempLoad = new List<Loading_Dispatch_Operation>();

        [HttpPost]
        public IActionResult DeleteDMRPRRP([FromBody] DeleteRequestModel request)
        {
            // Ensure list is not null and contains data
            if (DMRPRRP_List != null && DMRPRRP_List.Count > 0)
            {
                var found = DMRPRRP_List
                    .Where(a => a.productcode.Trim() == request.ProductCode.Trim()
                             && a.boxno.Trim() == request.BoxNo.Trim()
                             && a.batch.Trim() == request.BatchCode.Trim())
                    .FirstOrDefault();
                var one = Storage_OperationAdd.Where(a => a.productcode.Trim() == request.ProductCode.Trim()
                             && a.boxno.Trim() == request.BoxNo.Trim()
                             && a.batchcode.Trim() == request.BatchCode.Trim())
                    .FirstOrDefault();
                var two = Storage_OperationAdd.Where(a => a.productcode.Trim() == request.ProductCode.Trim()
                             && a.boxno.Trim() == request.BoxNo.Trim()
                             && a.batchcode.Trim() == request.BatchCode.Trim())
                    .FirstOrDefault();
                var three = Remove_StockInventory.Where(a => a.productcode.Trim() == request.ProductCode.Trim()
                             && a.boxno.Trim() == request.BoxNo.Trim()
                             && a.batchcode.Trim() == request.BatchCode.Trim())
                    .FirstOrDefault();

                if (found != null)
                {
                    // Remove the item from the list
                    if (one != null)
                    {
                        Storage_OperationAdd.Remove(one);
                    }
                    if (two != null)
                    {
                        Storage_OperationAdd.Remove(two);
                    }
                    if (three != null)
                    {
                        Remove_StockInventory.Remove(three);
                    }
                    DMRPRRP_List.Remove(found);
                    return Json(new { success = true, message = "Item cleared successfully!", data = found });
                }
                else
                {
                    // Item not found
                    return Json(new { success = false, message = "Item not found!" });
                }
            }

            // Return a message if the list is empty
            return Json(new { success = false, message = "List is empty or unavailable!" });
        }

        [HttpPost]
        public IActionResult SaveRPRPickingData([FromBody] PickedListData1 pickedListData)
        {
            try
            {
                // Access the selected columns here
                var selectedProductCode = "-";
                var selectedBoxNo = "-";
                var selectedBatchCode = "-";
                var location = "-";

                var boxno = pickedListData.BoxNo.Trim();
                var splitbox = pickedListData.BoxNo.Split("-")[1];
                var batchno = pickedListData.batchno.Trim();
                var productcode = pickedListData.productcode.Trim().ToUpper();
                var rowproductcode = pickedListData.rproductcode.Trim().ToUpper();
                int rowquantity = pickedListData.rquantity;

                var counter = 0;
                var boxcount = 1;

                foreach (var row in savepolist1)
                {
                    var split = row.boxno.Split("-")[1];
                    if (row.batchcode == pickedListData.batchno && row.boxno == pickedListData.BoxNo && row.productcode.ToUpper() == pickedListData.productcode.ToUpper())
                    {
                        counter++;
                    }
                    else if (split == splitbox && row.productcode.ToUpper() == pickedListData.productcode.ToUpper())
                    {
                        boxcount += 1;
                        if (Convert.ToInt32(boxcount) == Convert.ToInt32(pickedListData.rquantity) + 1)
                        {
                            counter++;
                        }
                        else
                        {
                        }
                    }
                }
                if (counter == 0)
                {
                    if (pickedListData.ordertype.Trim() == "Demo")
                    {
                        var found = _context.Storage_Operation
                            .Where(a => a.productcode.Trim() == pickedListData.productcode.Trim()
                            && a.batchcode.Trim() == pickedListData.batchno.Trim()
                            && a.boxno.Trim() == pickedListData.BoxNo.Trim()
                            && (a.statusflag.Trim() == "ST" || a.statusflag.Trim() == "DM"))
                            .FirstOrDefault();

                        if (found != null)
                        {
                            var pickedData = new Storage_Operation
                            {
                                productcode = found.productcode,
                                boxno = found.boxno,
                                batchcode = found.batchcode,
                                locationcode = found.locationcode,
                                statusflag = found.statusflag,
                                pickflag = "1",
                                grnno = found.grnno,
                            };
                            selectedProductCode = found.productcode;
                            selectedBoxNo = found.boxno;
                            selectedBatchCode = found.batchcode;
                            location = found.locationcode;
                            savepolist1.Add(pickedData);

                        }
                        else
                        {
                            return Json(new { success = false, message = "Scan correct shipper !" });
                        }
                    }
                    else if (pickedListData.ordertype.Trim() == "Repair")
                    {
                        var found = _context.Storage_Operation
                            .Where(a => a.productcode.Trim() == pickedListData.productcode.Trim()
                            && a.batchcode.Trim() == pickedListData.batchno.Trim()
                            && a.boxno.Trim() == pickedListData.BoxNo.Trim()
                            && (a.statusflag.Trim() == "ST" || a.statusflag.Trim() == "RPR"))
                            .FirstOrDefault();

                        if (found != null)
                        {
                            var pickedData = new Storage_Operation
                            {
                                productcode = found.productcode,
                                boxno = found.boxno,
                                batchcode = found.batchcode,
                                locationcode = found.locationcode,
                                statusflag = found.statusflag,
                                pickflag = "1",
                                grnno = found.grnno,
                            };
                            selectedProductCode = found.productcode;
                            selectedBoxNo = found.boxno;
                            selectedBatchCode = found.batchcode;
                            location = found.locationcode;
                            savepolist1.Add(pickedData);

                        }
                        else
                        {
                            return Json(new { success = false, message = "Scan correct shipper !" });
                        }
                    }
                    else if (pickedListData.ordertype.Trim() == "Replacement")
                    {
                        var found = _context.Storage_Operation
                            .Where(a => a.productcode.Trim() == pickedListData.productcode.Trim()
                            && a.batchcode.Trim() == pickedListData.batchno.Trim()
                            && a.boxno.Trim() == pickedListData.BoxNo.Trim()
                            && a.statusflag.Trim() == "ST")
                            .FirstOrDefault();

                        var found1 = _context.Storage_Operation
                           .Where(a => a.productcode.Trim() == pickedListData.productcode.Trim()
                           && a.boxno.Trim().EndsWith(pickedListData.BoxNo.Trim())
                           && a.statusflag.Trim() == "RP")
                           .FirstOrDefault();

                        if (found != null && found1 != null)
                        {
                            var pickedData = new Storage_Operation
                            {
                                productcode = found.productcode,
                                boxno = found.boxno,
                                batchcode = found.batchcode,
                                locationcode = found.locationcode,
                                statusflag = found.statusflag,
                                pickflag = "1",
                                grnno = found.grnno,
                            };
                            selectedProductCode = found.productcode;
                            selectedBoxNo = found.boxno;
                            selectedBatchCode = found.batchcode;
                            location = found.locationcode;
                            savepolist1.Add(pickedData);
                        }
                        else
                        {
                            return Json(new { success = false, message = "Scan correct shipper !" });
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "Data not found in Storage !" });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Quantity shippers Already Filled !" });
                }
                return Json(new
                {
                    success = true,
                    message = "Done !",
                    data = new
                    {
                        productCode = selectedProductCode,
                        boxNo = selectedBoxNo,
                        batchCode = selectedBatchCode,
                        location = location,
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        public ActionResult FatchRPRListData(string productcode, int quantity)
        {
            List<Storage_Operation> filteredList = new List<Storage_Operation>();
            if (savepolist1.Any())
            {
                filteredList = savepolist1.Where(a => a.productcode.ToUpper() == productcode.ToUpper()).ToList();
            }

            // Return the filtered list of data
            return Json(new { success = true, dataList = filteredList });
        }
        private List<SelectListItem> GetRefNo(string ordertype)
        {
            var lstProducts = new List<SelectListItem>();

            // Fetch the data from the database and move it to memory using .ToList()
            lstProducts = _context.inward
                .Where(a => a.status == "Pending" && a.ordertype.Trim() == ordertype.Trim() && a.flag == 1)
                .AsNoTracking()
                .ToList() // Brings the data to memory
                .DistinctBy(a => a.sono.Trim()) // Now you can apply DistinctBy in-memory
                .Select(n => new SelectListItem
                {
                    Value = n.sono,
                    Text = n.sono
                }).ToList();

            // Insert default item
            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select Ref.No----"
            };
            lstProducts.Insert(0, defItem);
            var addNewItem = new SelectListItem()
            {
                Value = "add_newRefId",  // This value can be checked when selected
                Text = "--- Generate New Ref ID --- "
            };
            lstProducts.Add(addNewItem); // Add the new item at the last position

            return lstProducts;
        }

        [HttpPost] //  (PONO, ORDERTYPE)
        public IActionResult radio(string nav)
        {
            if (nav == "Demo")
            {
                ViewBag.partyname = Getpartyname();
                ViewBag.pono = GetPONO();
                ViewBag.GetRefNo = GetRefNo(nav);
                ViewBag.sono = GetSONO();
                ViewBag.description = Getdescription();

                inward applicant = new inward();
                applicant.ordertype = "Demo"; ViewBag.ordertype = applicant.ordertype;
                applicant.inwardPacket.Add(new inwardPacket() { id = 1 });
                return View("ODemoView", applicant);
            }
            else if (nav == "Repair")
            {
                ViewBag.GetRefNo = GetRefNo(nav);
                ViewBag.partyname = Getpartyname();
                ViewBag.pono = GetPONO();
                ViewBag.sono = GetSONO();
                ViewBag.description = Getdescription();

                inward applicant = new inward();
                applicant.ordertype = "Repair"; ViewBag.ordertype = applicant.ordertype;
                applicant.inwardPacket.Add(new inwardPacket() { id = 1 });
                return View("ORepair", applicant);
            }
            else if (nav == "Replacement")
            {
                ViewBag.GetRefNo = GetRefNo(nav);
                ViewBag.partyname = Getpartyname();
                ViewBag.pono = GetPONO();
                ViewBag.sono = GetSONO();
                ViewBag.description = Getdescription();

                inward applicant = new inward();
                applicant.ordertype = "Replacement"; ViewBag.ordertype = applicant.ordertype;
                applicant.inwardPacket.Add(new inwardPacket() { id = 1 });
                return View("OReplacement", applicant);
            }
            else if (nav == "Sales")
            {
                ViewBag.partyname = Getpartyname();
                ViewBag.pono = GetPONO();
                ViewBag.sono = GetSONO();
                ViewBag.description = Getdescription();

                inward applicant = new inward();
                applicant.ordertype = "Sales"; ViewBag.ordertype = applicant.ordertype;
                applicant.inwardPacket.Add(new inwardPacket() { id = 1 });
                return View("OSales", applicant);
            }
            else
            {
                ViewBag.partyname = Getpartyname();
                ViewBag.pono = GetPONO();
                ViewBag.sono = GetSONO();
                ViewBag.description = Getdescription();

                inward applicant = new inward();
                applicant.ordertype = "Purchasereturn"; ViewBag.ordertype = applicant.ordertype;
                applicant.inwardPacket.Add(new inwardPacket() { id = 1 });
                return View("OPurchaseReturn", applicant);
            }

        }

        //productcode, quantity, pono
        public ActionResult checkquantityifgreter(string selectedValue, string selectedValue1, string selectedValue2)
        {
            var readpurchase = _context.so_inward.FirstOrDefault(po => po.sono == selectedValue2);
            if (readpurchase != null)
            {
                var readexistpacket = _context.so_product.Where(a => a.orderid == readpurchase.id && a.productcode.ToUpper() == selectedValue.ToUpper()).FirstOrDefault();

                var existingpacket = _context.inwardPacket.Where(a => a.sono == selectedValue2 && a.productcode.ToUpper() == selectedValue.ToUpper())
                              .GroupBy(p => p.productcode.ToUpper())
                                       .Select(group => new
                                       {
                                           ProductName = group.Key,
                                           TotalQuantity = group.Sum(p => p.quantity),
                                           TotalSubAssembly = group.Sum(p => p.totalsubassmbly)
                                       }).FirstOrDefault();



                if (existingpacket != null)
                {
                    if (readexistpacket.quantity < existingpacket.TotalQuantity + Convert.ToInt32(selectedValue1))
                    {
                        return Json("QuantityGreterError");
                    }
                    else
                    {

                    }
                }
                else
                {
                    if (readexistpacket.quantity < Convert.ToInt32(selectedValue1))
                    {
                        return Json("QuantityGreterError");
                    }
                    else
                    {

                    }
                }
            }
            return NotFound("Order not found");

        }

        [HttpGet]
        public ActionResult GETGRNSR_CODE()
        {
            // Generate your GRN number logic here, similar to how you did for the product code
            int maxId = _context.inward.Any() ? _context.inward.Max(e => e.inward_id) + 1 : 1;
            string grnCode = "S/OUT/EROS-W1" + maxId.ToString("D6"); // Assuming a fixed length of 6 digits

            return Json(grnCode);
        }

        [HttpGet]
        public ActionResult GETGRNP_CODE()
        {
            // Generate your GRN number logic here, similar to how you did for the product code
            int maxId = _context.inward.Any() ? _context.inward.Max(e => e.inward_id) + 1 : 1;
            string grnCode = "PR/OUT/EROS-W1-" + maxId.ToString("D6"); // Assuming a fixed length of 6 digits

            return Json(grnCode);
        }
        public ActionResult GetSumSubComponentpo(string selectedValue, string selectedValue1, string selectedValue2) //productcode, quantity, pono
        {
            var purchase = _context.purchase.FirstOrDefault(p => p.pono == selectedValue2);
            if (purchase != null)
            {
                //var poproductDetailsList = _context.purchase_subcomponent
                //    .Where(pd => pd.purchaseproduct_id == purchase.id && pd.sccode.StartsWith(selectedValue))
                var poproductDetailsList = _context.purchase_subcomponent
                    .Where(pd => pd.purchaseproduct_id == purchase.id)
                    .Select(a => new
                    {
                        a.subcomponents,
                        a.sccode,
                        a.scqty,
                        a.scuom,
                        a.tqty
                    })
                    .ToList();
                if (poproductDetailsList.Any())
                {
                    int totalsubassembly = 0;

                    foreach (var item in poproductDetailsList)
                    {
                        totalsubassembly = item.scqty + totalsubassembly;
                    }

                    totalsubassembly = totalsubassembly * Convert.ToInt32(selectedValue1);
                    return Json(totalsubassembly);

                }
                else
                {
                    return Json(selectedValue1);
                }
            }
            return View();

        }
        public ActionResult GetSumSubComponent(string selectedValue, string selectedValue1, string selectedValue2) //productcode, quantity, pono
        {


            var so_inward = _context.so_inward.FirstOrDefault(p => p.sono == selectedValue2);
            if (so_inward != null)
            {
                //var poproductDetailsList = _context.so_Subcomponents
                //    .Where(pd => pd.soproduct_id == so_inward.id && pd.sccode.StartsWith(selectedValue))
                var poproductDetailsList = _context.so_Subcomponents
                    .Where(pd => pd.soproduct_id == so_inward.id)
                    .Select(a => new
                    {
                        a.subcomponents,
                        a.sccode,
                        a.scqty,
                        a.scuom,
                        a.tqty
                    })
                    .ToList();
                if (poproductDetailsList.Any())
                {
                    int totalsubassembly = 0;

                    foreach (var item in poproductDetailsList)
                    {
                        totalsubassembly = item.scqty + totalsubassembly;
                    }

                    totalsubassembly = totalsubassembly * Convert.ToInt32(selectedValue1);
                    return Json(totalsubassembly);

                }
                else
                {
                    return Json(selectedValue1);
                }
            }
            return View();

        }
        public IActionResult ActionName_description(string selectedValue)
        {
            if (selectedValue != null)
            {
                var purchase = new purchase();
                var product = _context.Product_Master
                    .Include(e => e.Productmaster_Packets)
                    .FirstOrDefault(p => p.productdescription == selectedValue);

                if (product != null)
                {
                    bool productType = product.TypeOfProduct;

                    var result = new
                    {
                        id = product.id,
                        modelno = product.productcode.ToUpper(),
                        brand = product.brand,
                        uom = product.uom,
                        hsncode = product.hsncode,
                        producttype = productType
                    };
                    return Json(result);
                }
            }

            return Ok();
        }
        public IActionResult ActionName_description1(string selectedValue)
        {
            ////var wbridge = _context.Product_Master.Where(n => n.productdescription == selectedValue).FirstOrDefault();
            ////return Json(wbridge);
            var purchase = new purchase();
            var product = _context.Product_Master
                .Include(e => e.Productmaster_Packets)
                .FirstOrDefault(p => p.productdescription == selectedValue);

            if (product != null)
            {
                bool productType = product.TypeOfProduct;

                var result = new
                {
                    id = product.id,
                    modelno = product.productcode.ToUpper(),
                    brand = product.brand,
                    uom = product.uom,
                    hsncode = product.hsncode,
                    producttype = productType
                };
                return Json(result);
            }
            return Ok();
        }

        private List<SelectListItem> Getdescription()
        {
            var lstProducts = new List<SelectListItem>();
            lstProducts = _context.Product_Master.OrderBy(a => a.productdescription.Trim()).AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.productdescription,
                Text = n.productdescription
            }).ToList();
            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "  Select ProductName  "
            };
            lstProducts.Insert(0, defItem);
            return lstProducts;






        }

        [HttpPost] //  (PONO, ORDERTYPE)
        public ActionResult Changesono(string selectedValue)
        {
            if (selectedValue == "Customer")
            {
                List<SelectListItem> wbridge = _context.so_inward.Where(a => a.status == "Pending").AsNoTracking().OrderBy(n => n.sono).Select(n => new SelectListItem
                {
                    Selected = true,
                    Value = n.sono,
                    Text = n.sono.ToString()
                }).ToList();
                return Json(wbridge);
            }
            else
            {
                //return Json(Ok());
                return Ok();
            }
        }
        public IActionResult ActionName1(string optionValue, string optionValue1) //sono, customer
        {
            inward inward = new inward();

            if (optionValue1 == "Customer")
            {
                var ponodata = _context.so_inward.Where(a => a.sono == optionValue).FirstOrDefault();
                if (ponodata != null)
                {
                    inward.contactno = ponodata.contactno;
                    inward.address = ponodata.address;
                    //inward.referenceno = ponodata.sono;
                    inward.partyname = ponodata.customername;
                    //outward.gstinno = ponodata.city;

                    // Store pono value in ViewData
                    ViewData["SonoValue"] = ponodata.sono;

                    //get packet data of purchase by id
                    var packetdata = _context.so_product.Where(a => a.orderid == ponodata.id).ToList();

                    //get subcompoentn data of purchase by id
                    //var productsc = _context.so_Subcomponents.Where(a => a.soproduct_id == ponodata.id).ToList();
                    var productsc = _context.so_Subcomponents.Where(a => a.soproduct_id == ponodata.id && a.pono == optionValue).ToList();

                    //existing pono in inward if
                    var existingpacket = _context.inwardPacket.Where(a => a.sono == optionValue)
                                        .GroupBy(p => p.description)
                                        .Select(group => new
                                        {
                                            ProductName = group.Key,
                                            TotalQuantity = group.Sum(p => p.quantity),
                                            TotalSubAssembly = group.Sum(p => p.totalsubassmbly)
                                        }).ToList();
                    int i = 0;
                    foreach (var mat in packetdata)
                    {
                        i++;
                        int subassemcount = 0;
                        foreach (var mat1 in productsc.Where(pd => pd.sccode.StartsWith(mat.productcode.ToUpper()) && pd.pono == optionValue))
                        {
                            subassemcount = subassemcount + mat1.scqty;
                        }
                        if (subassemcount == 0)
                        {
                            subassemcount = 1;
                        }

                        if (existingpacket.Count > 0)
                        {
                            foreach (var newdata in existingpacket.Where(a => a.ProductName.ToUpper() == mat.description.ToUpper()))
                            {
                                inward.inwardPacket.Add(new inwardPacket()
                                {
                                    id = i,
                                    productcode = mat.productcode.ToUpper(),
                                    description = mat.description.ToUpper(),
                                    quantity = mat.quantity - newdata.TotalQuantity,
                                    uom = mat.uom,
                                    brand = mat.brand,
                                    totalsubassmbly = subassemcount * mat.quantity

                                    //id = i,
                                    //pono = optionValue,
                                    //productcode = mat.productcode,
                                    //description = mat.description,
                                    //quantity = mat.quantity - newdata.TotalQuantity,
                                    //uom = mat.uom,
                                    //Warranty = mat.Warranty,
                                    //totalsubassmbly = subassemcount * mat.quantity,
                                    //templatename = mat.templatename
                                });
                            }
                        }
                        else
                        {
                            inward.inwardPacket.Add(new inwardPacket()
                            {
                                //id = i,
                                //pono = optionValue,
                                //productcode = mat.productcode,
                                //description = mat.description,
                                //quantity = mat.quantity,
                                //uom = mat.uom,
                                //Warranty = mat.Warranty,
                                //totalsubassmbly = subassemcount * mat.quantity,
                                //templatename = mat.templatename

                                id = i,
                                productcode = mat.productcode.ToUpper(),
                                description = mat.description.ToUpper(),
                                quantity = mat.quantity,
                                uom = mat.uom,
                                brand = mat.brand,
                                //templatename = mat.templatename,
                                totalsubassmbly = subassemcount * mat.quantity
                            });
                        }
                    }
                }
                return PartialView("_SoProductDetails", inward);
            }
            else
            {
                return Ok();
            }
        }
        public IActionResult _partialSubComponent1(string productCode, string dcno, string invoiceno, int quantity)
        {
            try
            {

                var inward = new inward();


                var purchase1 = _context.Product_Master.FirstOrDefault(p => p.productcode.ToUpper() == productCode.ToUpper());

                if (purchase1 != null)
                {
                    var poproductDetailsList = _context.productmaster_Packet
                        .Where(pd => pd.productmasterId == purchase1.id)
                        .Select(a => new
                        {
                            a.subcomponents,
                            a.subcomponentcode,
                            a.qty,
                            a.uom,
                            //a.tqty // Include 'tqty' in the select query
                        })
                        .ToList();

                    if (poproductDetailsList.Any())
                    {
                        foreach (var item in poproductDetailsList)
                        {
                            inward.inward_subcomponent.Add(new inward_subcomponent
                            {
                                subcomponents = item.subcomponents,
                                sccode = item.subcomponentcode,
                                scqty = item.qty,
                                scuom = item.uom,
                                tqty = (quantity * item.qty)// Assign 'tqty'
                            });
                        }
                        //return Json(poproductDetailsList);

                        return View(inward); // Return 'poProduct_details' instead of 'poproductDetailsList'
                    }
                    else
                    {
                        //return NotFound($"No product details found for pono: {productcode} and productCode: {productcode}");
                        //return Json(new { message = $"No product details found for productCode: {productcode}" });
                        return View(inward);

                    }
                }
                else
                {
                    //return NotFound($"No purchase record found for pono: {productcode}");
                    return Json(new { message = $"No product details found for pono: {productCode} " });
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        //SALES - SUB COM
        public IActionResult _partialSubComponent(string productCode, string sono, int quantity)
        {
            var inward = new inward();
            var purchase1 = _context.Product_Master.FirstOrDefault(p => p.productcode.ToUpper() == productCode.ToUpper());
            if (purchase1 != null)
            {
                var poproductDetailsList = _context.productmaster_Packet
                    .Where(pd => pd.productmasterId == purchase1.id)
                    .Select(a => new
                    {
                        a.subcomponents,
                        a.subcomponentcode,
                        a.qty,
                        a.uom,
                        //a.tqty // Include 'tqty' in the select query
                    })
                    .ToList();
                if (poproductDetailsList.Any())
                {
                    foreach (var item in poproductDetailsList)
                    {
                        inward.inward_subcomponent.Add(new inward_subcomponent
                        {
                            subcomponents = item.subcomponents,
                            sccode = item.subcomponentcode,
                            scqty = item.qty,
                            scuom = item.uom,
                            tqty = (quantity * item.qty)// Assign 'tqty'
                        });
                    }
                    return View(inward); // Return 'poProduct_details' instead of 'poproductDetailsList'
                }
                else
                {
                    return View(inward);
                }
            }
            else
            {
                //return NotFound($"No purchase record found for pono: {productcode}");
                return Json(new { message = $"No product details found for pono: {productCode} " });
            }
        }

        //PURCHASE - SUB COM
        public IActionResult _partialSubComponentpo(string productCode, string pono, int quantity)
        {
            try
            {
                inward inward = new inward();
                // Find the Purchase record based on the given pono
                var purchase = _context.purchase.FirstOrDefault(p => p.pono == pono);

                if (purchase != null)
                {
                    //// Fetch all Poproductdetail records associated with the found Porderid'
                    //var poproductDetailsList = _context.purchase_subcomponent
                    //    .Where(pd => pd.purchaseproduct_id == purchase1.id )
                    //    .Select(a => new
                    //    {
                    //        a.subcomponents,
                    //        a.sccode,
                    //        a.scqty,
                    //        a.scuom,
                    //        a.tqty
                    //    })
                    //    .ToList();

                    //var poproductDetailsList = _context.purchase_subcomponent
                    //    .Where(pd => pd.purchaseproduct_id == purchase.id && pd.sccode.StartsWith(productCode))
                    var poproductDetailsList = _context.purchase_subcomponent
                    .Where(pd => pd.purchaseproduct_id == purchase.id)
                    .Select(a => new
                    {
                        a.subcomponents,
                        a.sccode,
                        a.scqty,
                        a.scuom,
                        a.tqty
                    })
                    .ToList();
                    if (poproductDetailsList.Any())
                    {
                        decimal totalsubassembly = poproductDetailsList.Sum(item => item.tqty);

                        foreach (var item in poproductDetailsList)
                        {
                            inward.inward_subcomponent.Add(new inward_subcomponent
                            {
                                subcomponents = item.subcomponents,
                                scqty = item.scqty,
                                scuom = item.scuom,
                                sccode = item.sccode,
                                tqty = (item.tqty * quantity)

                            });
                        }
                        return View(inward);

                    }
                    else
                    {
                        //return NotFound($"No product details found for pono: {pono} and productCode: {productCode}");
                        //return Json(new { message = $"No product details found for productCode: {productCode}" });
                        return View(inward);
                    }
                }
                else
                {
                    return NotFound($"No purchase record found for pono: {pono}");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately (log, return error response, etc.)
                return BadRequest($"Error: {ex.Message}");
            }

        }

        [HttpGet]
        public ActionResult Changepono(string selectedValue)
        {
            if (selectedValue == "Supplier")
            {
                List<SelectListItem> wbridge = _context.purchase.AsNoTracking().OrderBy(n => n.pono).Select(n => new SelectListItem
                {
                    Selected = true,
                    Value = n.pono,
                    Text = n.pono.ToString()
                }).ToList();
                return Json(wbridge);
            }
            else
            {
                //return Json(Ok());
                return Ok();
            }
        }

        [HttpPost]
        public IActionResult ActionName2(string optionValue, string optionValue1)
        {
            optionValue.Trim();
            optionValue1.Trim();
            inward outward = new inward();
            if (optionValue1 == "Supplier")
            {
                var salesorder = _context.purchase.Where(a => a.pono.Trim() == optionValue).Include(a => a.poProduct_details).FirstOrDefault();
                var productsc = _context.purchase_subcomponent.Where(a => a.purchaseproduct_id == salesorder.id).ToList();
                var indata = _context.inward.Where(a => a.pono == optionValue && a.ordertype.Trim() == "Purchase").Include(a => a.inwardPacket).FirstOrDefault();

                var newProducts = new List<purchase_subcomponent>();
                outward.partyname = salesorder.suppliername;
                outward.contactno = salesorder.contactno;
                outward.address = salesorder.address;
                outward.gstinno = salesorder.gstinno;
                outward.dcno = indata.dcno;
                outward.dcdate = indata.dcdate;
                outward.invoicedate = indata.invoicedate;
                outward.invoiceno = indata.invoiceno;
                outward.grndate = indata.grndate;
                outward.grnno = indata.grnno;
                outward.batchcode = indata.batchcode;
                outward.pono = indata.pono;
                ViewData["PonoValue"] = salesorder.pono;

                var CheckIn = _context.purchaseReturn.Where(a => a.pono.StartsWith(optionValue.Trim()) && a.status.Trim() == "Return").FirstOrDefault();
                if (CheckIn != null)
                {
                    var CheckPro = _context.poProduct_detailsReturn.Where(a => a.porderid == CheckIn.id).ToList();
                    if (CheckPro.Count > 0)
                    {
                        var product = _context.poProduct_details
                                .Where(a => a.porderid == salesorder.id)
                                .AsEnumerable()
                                .Where(a => CheckPro.Any(f => f.productcode.Trim() == a.productcode.Trim()))
                                .ToList();
                        int i = 0;
                        foreach (var mat in product)
                        {
                            i++;
                            int subassemcount = 0;
                            foreach (var mat1 in productsc.Where(pd => pd.sccode.StartsWith(mat.productcode)))
                            {
                                subassemcount = subassemcount + mat1.scqty;
                            }
                            if (subassemcount == 0)
                            {
                                subassemcount = 1;
                            }
                            var purchaseinward = _context.inwardPacket.Where(a => a.pono.Trim() == indata.pono.Trim() && a.productcode.Trim().ToUpper() == mat.productcode.Trim().ToUpper()).FirstOrDefault();

                            var sumqty = _context.inwardPacket
                                .Where(a => a.pono == optionValue && a.productcode.ToUpper() == mat.productcode.ToUpper())
                                .GroupBy(a => a.productcode.ToUpper())
                                .Select(group => new
                                {
                                    ProductCode = group.Key,
                                    TotalQuantity = group.Sum(item => item.quantity)
                                })
                                .FirstOrDefault(); // Get the first grouped item or null

                            var Rqty = _context.poProduct_detailsReturn.Where(a => a.porderid == CheckIn.id && a.productcode.Trim() == mat.productcode.Trim()).Select(a => a.quantity).FirstOrDefault();

                            outward.inwardPacket.Add(new inwardPacket()
                            {

                                id = i,
                                pono = optionValue,
                                sono = "-",
                                productcode = mat.productcode.ToUpper(),
                                description = mat.description.ToUpper(),
                                POQty = mat.quantity,
                                DLQty = sumqty != null ? Convert.ToInt32(sumqty.TotalQuantity) : 0, // Check if sumqty is not null
                                setofsub_assemb = purchaseinward.setofsub_assemb,
                                qtyperpkt = purchaseinward.qtyperpkt,
                                noofpackets = purchaseinward.noofpackets,
                                quantity = Rqty,
                                uom = mat.uom,
                                brand = mat.brand,
                                totalsubassmbly = subassemcount * mat.quantity
                            });
                        }
                    }

                }
                else
                {
                    var product = _context.poProduct_details.Where(a => a.porderid == salesorder.id).ToList();

                    int i = 0;
                    foreach (var mat in product)
                    {
                        i++;
                        int subassemcount = 0;
                        foreach (var mat1 in productsc.Where(pd => pd.sccode.StartsWith(mat.productcode)))
                        {
                            subassemcount = subassemcount + mat1.scqty;
                        }
                        if (subassemcount == 0)
                        {
                            subassemcount = 1;
                        }
                        var purchaseinward = _context.inwardPacket.Where(a => a.pono.Trim() == indata.pono.Trim() && a.productcode.Trim().ToUpper() == mat.productcode.Trim().ToUpper()).FirstOrDefault();

                        var sumqty = _context.inwardPacket
                            .Where(a => a.pono == optionValue && a.productcode.ToUpper() == mat.productcode.ToUpper())
                            .GroupBy(a => a.productcode.ToUpper())
                            .Select(group => new
                            {
                                ProductCode = group.Key,
                                TotalQuantity = group.Sum(item => item.quantity)
                            })
                            .FirstOrDefault(); // Get the first grouped item or null


                        outward.inwardPacket.Add(new inwardPacket()
                        {

                            id = i,
                            pono = optionValue,
                            sono = "-",
                            productcode = mat.productcode.ToUpper(),
                            description = mat.description.ToUpper(),
                            POQty = mat.quantity,
                            DLQty = sumqty != null ? Convert.ToInt32(sumqty.TotalQuantity) : 0, // Check if sumqty is not null
                            setofsub_assemb = purchaseinward.setofsub_assemb,
                            qtyperpkt = purchaseinward.qtyperpkt,
                            noofpackets = purchaseinward.noofpackets,
                            quantity = 0,
                            uom = mat.uom,
                            brand = mat.brand,
                            totalsubassmbly = subassemcount * mat.quantity
                        });
                    }
                }
                return PartialView("_PoProductDetails", outward);

            }
            else
            { return Json(Ok()); }
        }

        [HttpGet]
        public ActionResult ChangePartyName(string selectedValue)
        {
            if (selectedValue == "Showroom")
            {
                List<SelectListItem> wbridge = _context.Showroom_Master.AsNoTracking()
                                  .OrderBy(n => n.Showroom_name)
                                      .Select(n =>
                                      new SelectListItem
                                      {
                                          Selected = true,
                                          Value = n.Showroom_name,
                                          Text = n.Showroom_name
                                      }).ToList();

                //return Json(Ok());
                return Json(wbridge);
            }
            else if (selectedValue == "Supplier")
            {
                List<SelectListItem> wbridge = _context.Supplier_Master.AsNoTracking()
                                   .OrderBy(n => n.supplier_name)
                                       .Select(n =>
                                       new SelectListItem
                                       {
                                           Selected = true,
                                           Value = n.supplier_name,
                                           Text = n.supplier_name
                                       }).ToList();

                return Json(wbridge);
            }
            else if (selectedValue == "Customer")
            {
                List<SelectListItem> wbridge = _context.Customer_Master.AsNoTracking()
                                  .OrderBy(n => n.customername)
                                      .Select(n =>
                                      new SelectListItem
                                      {
                                          Selected = true,
                                          Value = n.customername,
                                          Text = n.customername
                                      }).ToList();

                return Json(wbridge);
            }
            else
            {
                return Json(Ok());
            }

        }
        public IActionResult ActionName(string optionValue, string optionValue1)
        {

            if (optionValue1 == "Customer")
            {
                var category = _context.Customer_Master.Where(a => a.customername.Equals(optionValue)).FirstOrDefault();
                return Json(new { data = category }); // Return the data to bind to the textbox
            }
            else if (optionValue1 == "Supplier")
            {
                var category1 = _context.Supplier_Master.Where(a => a.supplier_name.Equals(optionValue)).FirstOrDefault();
                return Json(new { data = category1 }); // Return the data to bind to the textbox
            }
            //else if (optionValue1 == "Showroom")
            //{
            //    var category2 = _context.Showroom_Master.Where(a => a.Showroom_name.Equals(optionValue)).FirstOrDefault();
            //    return Json(new { data = category2 }); // Return the data to bind to the textbox
            //}
            else
            {
                //return Json(Ok());
                return Ok();
            }
        }
        private List<SelectListItem> Getpartyname()
        {
            var lstProducts = new List<SelectListItem>();
            lstProducts = _context.Supplier_Master.AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.supplier_name,
                Text = n.supplier_name
            }).ToList();
            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select PartyName----"
            };
            lstProducts.Insert(0, defItem);
            return lstProducts;
        }
        private List<SelectListItem> GetPONO()
        {
            //var lstProducts = new List<SelectListItem>();
            //lstProducts = _context.purchase.Where(a=>a.status=="Pending").AsNoTracking().Select(n =>
            //new SelectListItem
            //{
            //    Value = n.pono,
            //    Text = n.pono
            //}).ToList();

            List<SelectListItem> wbridge = _context.purchase.Where(a => a.status == "Return").AsNoTracking().OrderBy(n => n.pono).Select(n => new SelectListItem
            {
                Selected = false,
                Value = n.pono,
                Text = n.pono.ToString()
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select PONO ----"
            };
            wbridge.Insert(0, defItem);
            return wbridge;

            //lstProducts.Insert(0, defItem);
            //return lstProducts;
        }
        private List<SelectListItem> GetSONO()
        {
            var lstProducts = new List<SelectListItem>();
            lstProducts = _context.so_inward.Where(a => a.status == "Pending").AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.sono,
                Text = n.sono
            }).ToList();
            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select SONO----"
            };
            lstProducts.Insert(0, defItem);
            return lstProducts;
        }
        public IActionResult PendingDetails(int id)
        {
            var purchase = _context.purchase.Where(a => a.id == id).Distinct().AsNoTracking().FirstOrDefault();  //purchase details
            var productdetials = _context.poProduct_details.Where(a => a.porderid == purchase.id).Distinct().AsNoTracking().ToList(); //purchhase product details

            var inward = _context.inward.Where(a => a.pono == purchase.pono).FirstOrDefault(); //purchase product inwarded details 


            var inwarddetailPacket = _context.inwardPacket.Where(a => a.inwardId == inward.inward_id).Distinct().AsNoTracking().ToList(); // inwarded product  details
            foreach (var data in productdetials)
            {
                // inward inward = _context.inward.Where(a => a.pono == purchase.pono).FirstOrDefault();
                foreach (var item in inwarddetailPacket.Where(b => b.productcode.ToUpper() == data.productcode.ToUpper()))
                {
                    int remain = data.quantity - item.quantity;
                    item.pqty = remain;
                    inward.inwardPacket.Add(item);
                }
            }
            // Maintain logs
            var user = HttpContext.Session.GetString("User");
            var logs = new Logs()
            {
                pagename = "Outward Operation",
                task = "Update outward Operation",
                taskid = id,
                action = "Update",
                date = DateTime.Now.ToString("dd/MM/yyyy"),
                time = DateTime.Now.ToString("HH:mm:ss"),
                username = user
            };

            _context.Add(logs);

            return View(inward);
        }

        // GET: Outwards

        public IActionResult Index()
        {

            var products = _context.inward.Where(a => a.flag == 2).OrderByDescending(a => a.inward_id)
                 //
                 .ToList();

            return View(products);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.partyname = Getpartyname();
            ViewBag.pono = GetPONO();
            ViewBag.sono = GetSONO();
            ViewBag.description = Getdescription();

            inward applicant = new inward();

            applicant.inwardPacket.Add(new inwardPacket() { id = 1 });
            return View(applicant);
        }
        public class PickedListData
        {
            public string BoxNo { get; set; }

            public string sono { get; set; }
            public string batchno { get; set; }
            public string productcode { get; set; }
            public string rproductcode { get; set; }
            public int rquantity { get; set; }

        }

        [HttpPost]
        public IActionResult SavePickingDataPR([FromBody] PickedListData pickedListData)
        {
            try
            {
                // Access the selected columns here
                var selectedProductCode = "-";
                var selectedBoxNo = "-";
                var selectedBatchCode = "-";
                var location = "-";

                string sono = pickedListData.sono.Trim();
                var boxno = pickedListData.BoxNo.Trim();
                var splitbox = pickedListData.BoxNo.Split("-")[1];
                var batchno = pickedListData.batchno.Trim();
                var productcode = pickedListData.productcode.Trim().ToUpper();
                var rowproductcode = pickedListData.rproductcode.Trim().ToUpper();
                int rowquantity = pickedListData.rquantity;

                var counter = 0;
                var boxcount = 1;

                foreach (var row in savepolist1)
                {
                    var split = row.boxno.Split("-")[1];
                    if (row.batchcode == pickedListData.batchno && row.boxno == pickedListData.BoxNo && row.productcode.ToUpper() == pickedListData.productcode.ToUpper())
                    {
                        counter++;
                    }
                    else if (split == splitbox && row.productcode.ToUpper() == pickedListData.productcode.ToUpper())
                    {
                        boxcount += 1;
                        if (Convert.ToInt32(boxcount) == Convert.ToInt32(pickedListData.rquantity) + 1)
                        {
                            counter++;
                        }
                        else
                        {
                        }
                    }
                }
                if (counter == 0)
                {
                    var storagedata = _context.Storage_Operation
                        .Where(a => a.productcode.Trim().ToUpper() == productcode.Trim().ToUpper()
                        && a.boxno.Trim() == boxno && a.batchcode.Trim() == batchno
                        && a.statusflag == "ST")
                        .FirstOrDefault();

                    if (storagedata != null)
                    {
                        selectedProductCode = productcode;
                        selectedBoxNo = boxno;
                        selectedBatchCode = batchno;
                        location = storagedata.locationcode;

                        var pickedData = new Storage_Operation
                        {
                            productcode = selectedProductCode,
                            boxno = selectedBoxNo,
                            batchcode = selectedBatchCode,
                            locationcode = location,
                            statusflag = "ST",
                            pickflag = "0",
                            grnno = storagedata.grnno,
                        };
                        savepolist1.Add(pickedData);

                    }
                    else
                    {
                        return Json(new { success = false, message = "Scanned box data not found in storage !" });
                    }

                }
                else
                {
                    //return Json(new { success = false, message = "Repeat Data found !" });
                    return Json(new { success = false, message = "Quantity shippers Already Filled !" });
                }
                return Json(new
                {
                    success = true,
                    //message = "Done !",
                    data = new
                    {
                        productCode = selectedProductCode,
                        boxNo = selectedBoxNo,
                        batchCode = selectedBatchCode,
                        location = location,
                    }
                });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        private static List<Storage_Operation> savepolist1 = new List<Storage_Operation>();
        [HttpPost]
        public IActionResult SavePR_List(string productCode, int soQty, int dlQty, int srQty, string sono)
        {
            try
            {
                var batchcode = "-";
                var listdata = savepolist1.Where(a => a.productcode.Trim().ToUpper() == productCode.Trim().ToUpper()).ToList();

                foreach (var item in listdata)
                {
                    batchcode = item.batchcode;

                    var storageupdate = savepolist1
                                        .Where(a => a.productcode.Trim().ToUpper() == productCode.Trim().ToUpper()
                                        && a.boxno.Trim() == item.boxno.Trim() && a.batchcode.Trim() == item.batchcode.Trim()
                                        && (a.statusflag.Trim() == "ST" || a.statusflag.Trim() == "DMG"))
                                        .FirstOrDefault();

                    if (storageupdate != null)
                    {
                        if (storageupdate.statusflag == "ST")
                        {
                            storageupdate.locationcode = "TMP";
                            storageupdate.statusflag = "PR";
                        }
                        else if (storageupdate.statusflag == "DMG")
                        {
                            storageupdate.statusflag = "PR";
                        }
                        _context.Storage_Operation.Update(storageupdate);

                    }
                    else
                    {
                        return Json(new { success = false, message = "Data not found in storage operation !" });
                    }
                }

                ////reduce quntity from purchase
                //var soinward = _context.purchase.Where(a => a.pono.Trim() == sono.Trim()).Include(a => a.poProduct_details).FirstOrDefault();
                //var soproduct = _context.poProduct_details.Where(a => a.porderid == soinward.id && a.productcode.Trim().ToUpper() == productCode.Trim().ToUpper() && a.quantity == soQty).FirstOrDefault();

                //if(soinward != null &&  soproduct != null )
                //{
                //    //var updateso = (soproduct.quantity) - (srQty);
                //    //soproduct.quantity = updateso;
                //    //_context.poProduct_details.Update(soproduct);

                //    ////maintain logs
                //    //var user = HttpContext.Session.GetString("User");
                //    //var logs = new Logs();
                //    //logs.pagename = "Outward Operation"; logs.action = "Update";
                //    //logs.task = "Outward Purchase Return Products, update in stoarge & purchase order !";
                //    //logs.taskid = soproduct.porderid;
                //    //logs.action = "Update";
                //    //logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                //    //logs.time = DateTime.Now.ToString("HH:mm:ss");
                //    //logs.username = user;
                //    //_context.Add(logs);
                //}
                //else
                //{
                //    return Json(new { success = false, message = "purchase-returned list of product " + productCode + " are not found in purchase return." });
                //}


                //var indataa = _context.inward.Where(a => a.pono == sono.Trim() && a.batchcode == batchcode).Include(a => a.inwardPacket).FirstOrDefault();
                //var indata = _context.inwardPacket.Where(a => a.pono == sono.Trim() && a.productcode.ToUpper() == productCode.ToUpper() && a.inwardId == indataa.inward_id).FirstOrDefault();
                //indata.quantity = (indata.quantity) - (srQty);
                //_context.inwardPacket.Update(indata);

                //_context.SaveChanges();
                return Json(new { success = true, message = "Purchase-returned list of " + productCode + " is saved successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while saving the sale-returned list: " + ex.Message });
            }
        }

        public ActionResult FatchListDataPR(string productcode, int quantity, string pono)
        {
            List<Storage_Operation> filteredList = new List<Storage_Operation>();
            //var getdata = _context.inward.Where(a => a.pono.Trim() == pono.Trim()).FirstOrDefault();
            // Filter the savepolist based on the productcode
            if (savepolist1.Any())
            {
                filteredList = savepolist1.Where(a => a.productcode.ToUpper() == productcode.ToUpper()).ToList();
            }
            //, getdata = getdata 
            // Return the filtered list of data
            return Json(new { success = true, dataList = filteredList });
        }

        private string GenerateBatchCode()
        {
            DateTime today = DateTime.Today;
            string year = today.ToString("yy");
            char month = "ABCDEFGHIJKL"[today.Month - 1];
            string day = today.ToString("dd");
            return $"{year}{month}{day}";
        }
        private string Generategrn()
        {
            DateTime today = DateTime.Today;
            string year = today.ToString("yy");
            char month = "ABCDEFGHIJKL"[today.Month - 1];
            string day = today.ToString("dd");
            return $"HG/GRN/{year}{month}{day}";
        }

        //GET beg data from vender name to get supplier, customer,showroom
        [HttpPost]
        public IActionResult Create(inward applicant, int stickerDecision)
        {
            if (applicant.ordertype.Trim() == "Demo" || applicant.ordertype.Trim() == "Repair" || applicant.ordertype.Trim() == "Replacement")
            {
                if (DMRPRRP_List.Count == 0)
                {
                    Update_DMRPRRPInventory.Clear();
                    Remove_StockInventory.Clear();
                    DMRPRRP_List.Clear();
                    return Json(new { success = false, message = "Please scan the shippers before submit !" });
                }

                var ponodata1 = _context.inward.Where(a => a.pono == applicant.pono.Trim() && a.status.Trim() == "Pending").OrderBy(a => a.inward_id).FirstOrDefault();
                if (ponodata1 != null)
                {
                    //FIRSTLY INAWARDING DONE
                    if (ponodata1.flag == 2)
                    {
                        return Json(new { success = false, message = "You have already completed Outwarding. Please proceed with Inwarding!" });
                    }
                    //FIRSTLY OUTWARDING DONE
                    else
                    {

                    }
                }

                applicant.flag = 2;

                if (applicant.grnno == null)
                {
                    applicant.grnno = "-";
                }
                if (applicant.grndate == null)
                {
                    applicant.grndate = "-";
                }
                if (applicant.dcno == null)
                {
                    applicant.dcno = "-";
                }
                if (applicant.invoiceno == null)
                {
                    applicant.invoiceno = "-";
                }
                if (applicant.invoicedate == null)
                {
                    applicant.invoicedate = "-";
                }
                if (applicant.dcdate == null)
                {
                    applicant.dcdate = "-";
                }

                int smaxId = _context.Loading_Dispatch_Operation.Any() ? _context.Loading_Dispatch_Operation.Max(e => e.id) + 0 : 0;
                //for subcmpoennt
                foreach (var a in applicant.inwardPacket)
                {
                    if (a.noofpackets == null)
                    {
                        a.noofpackets = "0";
                    }
                    if (a.qtyperpkt == null)
                    {
                        a.qtyperpkt = "0";
                    }
                    if (a.setofsub_assemb == null)
                    {
                        a.setofsub_assemb = "0";
                    }
                    if (a.totalpacket == null)
                    {
                        a.totalpacket = "0";
                    }

                    a.flag = 2;
                    //a.pono = "-";
                    //a.sono = "-";
                    var purchase1 = _context.Product_Master.FirstOrDefault(p => p.productcode.ToUpper() == a.productcode.ToUpper());
                    if (purchase1 != null)
                    {
                        var poproductDetailsList = _context.productmaster_Packet
                            .Where(pd => pd.productmasterId == purchase1.id && pd.subcomponentcode.StartsWith(a.productcode.ToUpper()))
                            .Select(a => new
                            {
                                a.subcomponents,
                                a.subcomponentcode,
                                a.qty,
                                a.uom,
                            })
                            .ToList();
                        if (poproductDetailsList.Any())
                        {
                            foreach (var item in poproductDetailsList)
                            {
                                applicant.inward_subcomponent.Add(new inward_subcomponent
                                {
                                    subcomponents = item.subcomponents,
                                    sccode = item.subcomponentcode,
                                    scqty = item.qty,
                                    scuom = item.uom,
                                    tqty = (a.quantity * item.qty),
                                    dcno = applicant.dcno ?? "-",
                                    invoiceno = applicant.invoiceno ?? "-"
                                });
                            }
                        }
                    }

                }

                if (applicant.ordertype.Trim() == "Demo")
                {
                    //sonoid = "DEMO/OUT/" + DateTime.Now.ToString("ddMMyyyy/HHmmss");
                    foreach (var item in applicant.inwardPacket)
                    {
                        applicant.pono = applicant.pono;
                        applicant.sono = applicant.pono;
                        item.pono = applicant.pono;
                        item.sono = applicant.pono;
                    }
                    if (applicant.vendername == "-------")
                    {
                        applicant.vendername = applicant.vendername1.Trim();
                    }
                    if (applicant.typeofreturn == "-------")
                    {
                        applicant.typeofreturn = applicant.typeofreturn1.Trim();
                    }
                    if (applicant.typeofreturn == "Returned" || applicant.typeofreturn == "Non-Returnable")
                    {
                        var ponodata = _context.inward
                                        .Where(a => a.pono.Trim() == applicant.pono.Trim())
                                        .OrderBy(a => a.inward_id)
                                        .FirstOrDefault();

                        if (ponodata != null)
                        {
                            var packetdata = _context.inwardPacket.Where(a => a.inwardId == ponodata.inward_id).ToList();
                            if (packetdata.Count > 0)
                            {
                                var actualqty = packetdata.Sum(a => Convert.ToInt32(a.quantity));
                                foreach (var mat in packetdata)
                                {
                                    //FIRSTLY - INWARDING DONE
                                    if (mat.flag == 1)
                                    {
                                        var liscount = _context.inwardPacket.Where(a => a.pono == mat.pono && a.flag == 2).ToList();
                                        var currentqty = applicant.inwardPacket.ToList().Sum(a => a.quantity);
                                        var count = liscount.Sum(a => a.quantity) + currentqty;
                                        if (count <= actualqty)
                                        {
                                            var foundlist2 = _context.inward.Where(a => a.sono.Trim() == applicant.pono.Trim()).ToList();
                                            if (count == actualqty)
                                            {
                                                foreach (var newitem in foundlist2)
                                                {
                                                    newitem.status = "Completed";
                                                    applicant.status = "Completed";
                                                    _context.inward.Update(newitem);
                                                    _context.SaveChanges();
                                                }
                                            }
                                            else
                                            {
                                                foreach (var newitem in foundlist2)
                                                {
                                                    newitem.status = "Pending";
                                                    applicant.status = "Pending";
                                                    _context.inward.Update(newitem);
                                                    _context.SaveChanges();
                                                }
                                            }
                                        }
                                        else if (count > actualqty)
                                        {
                                            return Json(new { success = false, message = "Your form was not submitted. Please enter the correct quantity!" }); // Return JSON for errors
                                        }
                                    }
                                    //FIRSTLY - OUTWARDING DONE
                                    else
                                    {
                                        var liscount = _context.inwardPacket.Where(a => a.pono == mat.pono && a.flag == 1).ToList();
                                        var currentqty = applicant.inwardPacket.ToList().Sum(a => a.quantity);
                                        var count = liscount.Sum(a => a.quantity) + currentqty;
                                        if (count <= actualqty)
                                        {
                                            var foundlist2 = _context.inward.Where(a => a.sono.Trim() == applicant.pono.Trim()).ToList();
                                            if (count == actualqty)
                                            {
                                                foreach (var newitem in foundlist2)
                                                {
                                                    newitem.status = "Completed";
                                                    applicant.status = "Completed";
                                                    _context.inward.Update(newitem);
                                                    _context.SaveChanges();
                                                }
                                            }
                                            else
                                            {
                                                foreach (var newitem in foundlist2)
                                                {
                                                    newitem.status = "Pending";
                                                    applicant.status = "Pending";
                                                    _context.inward.Update(newitem);
                                                    _context.SaveChanges();
                                                }
                                            }
                                        }
                                        else if (count > actualqty)
                                        {
                                            return Json(new { success = false, message = "Your form was not submitted. Please enter the correct quantity!" }); // Return JSON for errors
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Check the returntype and set the status accordingly
                            if (applicant.typeofreturn == "Returnable")
                            {
                                applicant.status = "Pending";
                            }
                            else if (applicant.typeofreturn == "Non-Returnable")
                            {
                                applicant.status = "Completed";
                            }
                            else if (applicant.typeofreturn == "Returned")
                            {
                                applicant.status = "Completed";

                            }
                        }
                    }

                    if (DMRPRRP_List.Count > 0)
                    {
                        //ADD IN DMPRPRPR
                        foreach (var item in DMRPRRP_List)
                        {
                            var typeget = applicant.inwardPacket.Where(a => a.productcode.Trim().ToUpper() == item.productcode.ToUpper().Trim()).Select(a => a.type).FirstOrDefault();
                            //ADD AS LD 
                            int dmmaxid = _context.DMRPRRP.Any() ? _context.DMRPRRP.Max(e => e.id) + 1 : 1;
                            DMRPRRP dm = new DMRPRRP()
                            {
                                id = dmmaxid,
                                productcode = item.productcode,
                                grn = item.grn,
                                batch = item.batch,
                                boxno = item.boxno,
                                refno = applicant.sono,
                                ordertype = applicant.ordertype,
                                inout = applicant.flag,
                                pickflag = 1,
                                location = item.location ?? "TMP",
                                type = item.type,
                                //type = item.type,
                                statusflag = "LD",
                                date = DateTime.Now.ToString("yyyy-MM-dd"),
                                time = DateTime.Now.ToString("HH:mm:ss"),
                                returntype = applicant.typeofreturn,
                                partyname = applicant.partyname.Trim(),
                                condition = typeget,
                                from = applicant.vendername.Trim(),
                            };
                            _context.DMRPRRP.Add(dm);
                            _context.SaveChanges();
                        }
                        //END
                        //UPDATE DMRPRRP
                        if (Update_DMRPRRPInventory.Count > 0)
                        {
                            foreach (var item in Update_DMRPRRPInventory)
                            {
                                var found_DMRPRRP = _context.DMRPRRP
                                           .Where(a => a.id == item.id
                                           && a.productcode.Trim() == item.productcode.Trim()
                                           && a.boxno.Trim() == item.boxno.Trim()
                                           && a.batch.Trim() == item.batch.Trim())
                                          .FirstOrDefault();

                                if (found_DMRPRRP != null)
                                {
                                    //EXISTING UPDATE FROM ST - TO - LD
                                    found_DMRPRRP.pickflag = 1;
                                    _context.DMRPRRP.Update(found_DMRPRRP);
                                    _context.SaveChanges();
                                }
                            }
                        }
                        //END
                        //CHECK IN STORAGE IF FOUND UPDATE AND REMOVE IT FROM STOCK INVENTORY AND ADD IN THE DEMO INVENTORY
                        if (Remove_StockInventory.Count > 0)
                        {
                            foreach (var item in Remove_StockInventory)
                            {
                                var check = _context.Storage_Operation
                                .Where(a => a.id == item.id
                                && a.productcode.Trim() == item.productcode.Trim()
                                && a.batchcode.Trim() == item.batchcode.Trim()
                                && a.boxno.Trim() == item.boxno.Trim())
                                .FirstOrDefault();

                                if (check != null)
                                {
                                    check.statusflag = "DMOUT";
                                    _context.Storage_Operation.Update(check);
                                    _context.SaveChanges();
                                }
                            }

                        }
                        //END
                    }
                }
                else if (applicant.ordertype.Trim() == "Repair")
                {
                    //sonoid = "REPAIR/OUT/" + DateTime.Now.ToString("ddMMyyyy/HHmmss");
                    foreach (var item in applicant.inwardPacket)
                    {
                        applicant.pono = applicant.pono;
                        applicant.sono = applicant.pono;
                        item.pono = applicant.pono;
                        item.sono = applicant.pono;
                    }
                    if (applicant.vendername == "-------")
                    {
                        applicant.vendername = applicant.vendername1.Trim();
                    }
                    if (applicant.typeofreturn == "-------")
                    {
                        applicant.typeofreturn = applicant.typeofreturn1.Trim();
                    }
                    //if (applicant.typeofreturn == "Returned" || applicant.typeofreturn == "Non-Returnable")
                    //{
                    //    var foundlist = _context.inward.Where(a => a.sono.Trim() == applicant.pono.Trim()).ToList();
                    //    if (foundlist.Count > 0)
                    //    {
                    //        foreach (var item in foundlist)
                    //        {
                    //            item.status = "Completed";
                    //            _context.inward.Update(item);
                    //        }
                    //    }
                    //}
                    if (applicant.typeofreturn.Trim() == "Returned" || applicant.typeofreturn.Trim() == "Non-Returnable" || applicant.typeofreturn.Trim() == "Returned-Replacement")
                    {
                        var ponodata = _context.inward
                                        .Where(a => a.pono.Trim() == applicant.pono.Trim())
                                        .OrderBy(a => a.inward_id)
                                        .FirstOrDefault();

                        if (ponodata != null)
                        {
                            var packetdata = _context.inwardPacket.Where(a => a.inwardId == ponodata.inward_id).ToList();
                            if (packetdata.Count > 0)
                            {
                                var actualqty = packetdata.Sum(a => Convert.ToInt32(a.quantity));
                                foreach (var mat in packetdata)
                                {
                                    //FIRSTLY - INWARDING DONE
                                    if (mat.flag == 1)
                                    {
                                        var liscount = _context.inwardPacket.Where(a => a.pono == mat.pono && a.flag == 2).ToList();
                                        var currentqty = applicant.inwardPacket.ToList().Sum(a => a.quantity);
                                        var count = liscount.Sum(a => a.quantity) + currentqty;
                                        if (count <= actualqty)
                                        {
                                            var foundlist2 = _context.inward.Where(a => a.sono.Trim() == applicant.pono.Trim()).ToList();
                                            if (count == actualqty)
                                            {
                                                foreach (var newitem in foundlist2)
                                                {
                                                    newitem.status = "Completed";
                                                    applicant.status = "Completed";
                                                    _context.inward.Update(newitem);
                                                    _context.SaveChanges();
                                                }
                                            }
                                            else
                                            {
                                                foreach (var newitem in foundlist2)
                                                {
                                                    newitem.status = "Pending";
                                                    applicant.status = "Pending";
                                                    _context.inward.Update(newitem);
                                                    _context.SaveChanges();
                                                }
                                            }
                                        }
                                        else if (count > actualqty)
                                        {
                                            return Json(new { success = false, message = "Your form was not submitted. Please enter the correct quantity!" }); // Return JSON for errors
                                        }
                                    }
                                    //FIRSTLY - OUTWARDING DONE
                                    else
                                    {
                                        var liscount = _context.inwardPacket.Where(a => a.pono == mat.pono && a.flag == 1).ToList();
                                        var currentqty = applicant.inwardPacket.ToList().Sum(a => a.quantity);
                                        var count = liscount.Sum(a => a.quantity) + currentqty;
                                        if (count <= actualqty)
                                        {
                                            var foundlist2 = _context.inward.Where(a => a.sono.Trim() == applicant.pono.Trim()).ToList();
                                            if (count == actualqty)
                                            {
                                                foreach (var newitem in foundlist2)
                                                {
                                                    newitem.status = "Completed";
                                                    applicant.status = "Completed";
                                                    _context.inward.Update(newitem);
                                                    _context.SaveChanges();
                                                }
                                            }
                                            else
                                            {
                                                foreach (var newitem in foundlist2)
                                                {
                                                    newitem.status = "Pending";
                                                    applicant.status = "Pending";
                                                    _context.inward.Update(newitem);
                                                    _context.SaveChanges();
                                                }
                                            }
                                        }
                                        else if (count > actualqty)
                                        {
                                            return Json(new { success = false, message = "Your form was not submitted. Please enter the correct quantity!" }); // Return JSON for errors
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Check the returntype and set the status accordingly
                            if (applicant.typeofreturn == "Returnable")
                            {
                                applicant.status = "Pending";
                            }
                            else if (applicant.typeofreturn == "Non-Returnable")
                            {
                                applicant.status = "Completed";
                            }
                            else if (applicant.typeofreturn == "Returned")
                            {
                                applicant.status = "Completed";

                            }
                        }
                    }

                    if (DMRPRRP_List.Count > 0)
                    {
                        //ADD IN DMRPRPRP
                        foreach (var item in DMRPRRP_List)
                        {
                            var typeget = applicant.inwardPacket.Where(a => a.productcode.Trim().ToUpper() == item.productcode.ToUpper().Trim()).Select(a => a.type).FirstOrDefault();
                            //ADD AS LD 

                            int dmmaxid = _context.DMRPRRP.Any() ? _context.DMRPRRP.Max(e => e.id) + 1 : 1;
                            DMRPRRP dm = new DMRPRRP()
                            {
                                id = dmmaxid,
                                productcode = item.productcode,
                                grn = item.grn,
                                batch = item.batch,
                                boxno = item.boxno,
                                refno = applicant.sono,
                                ordertype = applicant.ordertype,
                                inout = applicant.flag,
                                pickflag = 1,
                                location = item.location ?? "TMP",
                                type = item.type,
                                statusflag = "LD",
                                date = DateTime.Now.ToString("yyyy-MM-dd"),
                                time = DateTime.Now.ToString("HH:mm:ss"),
                                returntype = applicant.typeofreturn,
                                partyname = applicant.partyname.Trim(),
                                condition = typeget,
                                from = applicant.vendername.Trim(),
                            };
                            _context.DMRPRRP.Add(dm);
                            _context.SaveChanges();

                        }
                        //END
                        //UPDATE DMRPRPRP
                        if (Update_DMRPRRPInventory.Count > 0)
                        {
                            foreach (var kk in Update_DMRPRRPInventory)
                            {
                                var check = _context.DMRPRRP
                                .Where(a => a.id == kk.id
                                && a.productcode.Trim() == kk.productcode.Trim()
                                && a.batch.Trim() == kk.batch.Trim()
                                && a.boxno.Trim() == kk.boxno.Trim())
                                .FirstOrDefault();

                                if (check != null)
                                {
                                    //EXISTING UPDATE FROM ST - TO - LD
                                    check.pickflag = 1;
                                    _context.DMRPRRP.Update(check);
                                    _context.SaveChanges();
                                }
                            }
                        }
                        //END
                        //CHECK IN STORAGE IF FOUND UPDATE AND REMOVE IT FROM STOCK INVENTORY AND ADD IN THE DEMO INVENTORY
                        if (Remove_StockInventory.Count > 0)
                        {
                            foreach (var item in Remove_StockInventory)
                            {
                                var check = _context.Storage_Operation
                                .Where(a => a.id == item.id
                                && a.productcode.Trim() == item.productcode.Trim()
                                && a.batchcode.Trim() == item.batchcode.Trim()
                                && a.boxno.Trim() == item.boxno.Trim()
                                && a.pickflag == "0"
                                && (a.statusflag == "ST" || a.statusflag == "DMG"))
                                .FirstOrDefault();

                                if (check != null)
                                {
                                    check.statusflag = "RPROUT";
                                    _context.Storage_Operation.Update(check);
                                    _context.SaveChanges();
                                }
                            }
                        }
                        //END
                    }

                }
                else if (applicant.ordertype.Trim() == "Replacement")
                {
                    //sonoid = "REPLACEMENT/OUT/" + DateTime.Now.ToString("ddMMyyyy/HHmmss");
                    foreach (var item in applicant.inwardPacket)
                    {
                        applicant.pono = applicant.pono;
                        applicant.sono = applicant.pono;
                        item.pono = applicant.pono;
                        item.sono = applicant.pono;
                    }
                    if (applicant.vendername == "-------")
                    {
                        applicant.vendername = applicant.vendername1.Trim();
                    }
                    if (applicant.typeofreturn == "-------")
                    {
                        applicant.typeofreturn = applicant.typeofreturn1.Trim();
                    }
                    //if (applicant.typeofreturn == "Returned" || applicant.typeofreturn == "Non-Returnable")
                    //{
                    //    var foundlist = _context.inward.Where(a => a.sono.Trim() == applicant.pono.Trim()).ToList();
                    //    if (foundlist.Count > 0)
                    //    {
                    //        foreach (var item in foundlist)
                    //        {
                    //            item.status = "Completed";
                    //            _context.inward.Update(item);
                    //        }
                    //    }
                    //}
                    if (applicant.typeofreturn == "Returned" || applicant.typeofreturn == "Non-Returnable")
                    {
                        var ponodata = _context.inward
                                        .Where(a => a.pono.Trim() == applicant.pono.Trim())
                                        .OrderBy(a => a.inward_id)
                                        .FirstOrDefault();

                        if (ponodata != null)
                        {
                            var packetdata = _context.inwardPacket.Where(a => a.inwardId == ponodata.inward_id).ToList();
                            if (packetdata.Count > 0)
                            {
                                var actualqty = packetdata.Sum(a => Convert.ToInt32(a.quantity));
                                foreach (var mat in packetdata)
                                {
                                    //FIRSTLY - INWARDING DONE
                                    if (mat.flag == 1)
                                    {
                                        var liscount = _context.inwardPacket.Where(a => a.pono == mat.pono && a.flag == 2).ToList();
                                        var currentqty = applicant.inwardPacket.ToList().Sum(a => a.quantity);
                                        var count = liscount.Sum(a => a.quantity) + currentqty;
                                        if (count <= actualqty)
                                        {
                                            var foundlist2 = _context.inward.Where(a => a.sono.Trim() == applicant.pono.Trim()).ToList();
                                            if (count == actualqty)
                                            {
                                                foreach (var newitem in foundlist2)
                                                {
                                                    newitem.status = "Completed";
                                                    applicant.status = "Completed";
                                                    _context.inward.Update(newitem);
                                                    _context.SaveChanges();
                                                }
                                            }
                                            else
                                            {
                                                foreach (var newitem in foundlist2)
                                                {
                                                    newitem.status = "Pending";
                                                    applicant.status = "Pending";
                                                    _context.inward.Update(newitem);
                                                    _context.SaveChanges();
                                                }
                                            }
                                        }
                                        else if (count > actualqty)
                                        {
                                            return Json(new { success = false, message = "Your form was not submitted. Please enter the correct quantity!" }); // Return JSON for errors
                                        }
                                    }
                                    //FIRSTLY - OUTWARDING DONE
                                    else
                                    {
                                        var liscount = _context.inwardPacket.Where(a => a.pono == mat.pono && a.flag == 1).ToList();
                                        var currentqty = applicant.inwardPacket.ToList().Sum(a => a.quantity);
                                        var count = liscount.Sum(a => a.quantity) + currentqty;
                                        if (count <= actualqty)
                                        {
                                            var foundlist2 = _context.inward.Where(a => a.sono.Trim() == applicant.pono.Trim()).ToList();
                                            if (count == actualqty)
                                            {
                                                foreach (var newitem in foundlist2)
                                                {
                                                    newitem.status = "Completed";
                                                    applicant.status = "Completed";
                                                    _context.inward.Update(newitem);
                                                    _context.SaveChanges();
                                                }
                                            }
                                            else
                                            {
                                                foreach (var newitem in foundlist2)
                                                {
                                                    newitem.status = "Pending";
                                                    applicant.status = "Pending";
                                                    _context.inward.Update(newitem);
                                                    _context.SaveChanges();
                                                }
                                            }
                                        }
                                        else if (count > actualqty)
                                        {
                                            return Json(new { success = false, message = "Your form was not submitted. Please enter the correct quantity!" }); // Return JSON for errors
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Check the returntype and set the status accordingly
                            if (applicant.typeofreturn == "Returnable")
                            {
                                applicant.status = "Pending";
                            }
                            else if (applicant.typeofreturn == "Non-Returnable")
                            {
                                applicant.status = "Completed";
                            }
                            else if (applicant.typeofreturn == "Returned")
                            {
                                applicant.status = "Completed";

                            }
                        }
                    }

                    if (DMRPRRP_List.Count > 0)
                    {
                        //ADD IN DMRPRPRP AS LD
                        foreach (var item in DMRPRRP_List)
                        {
                            //ADD AS LD 
                            int dmmaxid = _context.DMRPRRP.Any() ? _context.DMRPRRP.Max(e => e.id) + 1 : 1;
                            var typeget = applicant.inwardPacket.Where(a => a.productcode.Trim().ToUpper() == item.productcode.ToUpper().Trim()).Select(a => a.type).FirstOrDefault();
                            DMRPRRP dm = new DMRPRRP()
                            {
                                id = dmmaxid,
                                productcode = item.productcode,
                                grn = item.grn,
                                batch = item.batch,
                                boxno = item.boxno,
                                refno = applicant.sono,
                                ordertype = applicant.ordertype,
                                inout = applicant.flag,
                                pickflag = 1,
                                location = item.location,
                                type = item.type,
                                statusflag = "LD",
                                date = DateTime.Now.ToString("yyyy-MM-dd"),
                                time = DateTime.Now.ToString("HH:mm:ss"),
                                returntype = applicant.typeofreturn,
                                partyname = applicant.partyname.Trim(),
                                condition = typeget,
                                from = applicant.vendername.Trim(),
                            };
                            _context.DMRPRRP.Add(dm);
                            _context.SaveChanges();
                        }
                        //END
                        //UPDATE IN DMRPRRP
                        if (Storage_OperationExAdd.Count > 0)
                        {
                            foreach (var nitem in Storage_OperationExAdd)
                            {
                                var check = _context.Storage_Operation
                                .Where(a => a.id == nitem.id
                                && a.productcode.Trim() == nitem.productcode.Trim()
                                && a.batchcode.Trim() == nitem.batchcode.Trim()
                                && a.boxno.Trim() == nitem.boxno.Trim()
                                && a.grnno.Trim() == nitem.grnno.Trim()
                                && (a.statusflag.Trim() == "ST" || a.statusflag.Trim() == "DMG")
                                && a.pickflag.Trim() == "0")
                                .FirstOrDefault();

                                if (check != null)
                                {
                                    if (check.statusflag == "ST")
                                    {
                                        var check_dmrprrp = _context.DMRPRRP
                                            .Where(a => a.productcode.Trim() == nitem.productcode.Trim()
                                            && a.boxno.Trim() == nitem.boxno.Trim()
                                            && a.batch.Trim() == nitem.batchcode.Trim()
                                            && a.statusflag.Trim() == "ST"
                                            && a.ordertype.Trim() == applicant.ordertype.Trim()
                                            && a.pickflag == 0)
                                            .FirstOrDefault();

                                        if (check_dmrprrp != null)
                                        {
                                            check_dmrprrp.pickflag = 1;
                                            _context.DMRPRRP.Update(check_dmrprrp);
                                            _context.SaveChanges();
                                        }

                                        check.pickflag = "1";
                                        check.statusflag = "LD";
                                        _context.Storage_Operation.Update(check);
                                        _context.SaveChanges();
                                    }
                                    else
                                    {
                                        var check_dmrprrp = _context.DMRPRRP
                                            .Where(a => a.productcode.Trim() == nitem.productcode.Trim()
                                            && a.boxno.Trim() == nitem.boxno.Trim()
                                            && a.batch.Trim() == nitem.batchcode.Trim()
                                            && a.statusflag.Trim() == "DMG"
                                            && a.ordertype.Trim() == applicant.ordertype.Trim()
                                            && a.pickflag == 0)
                                            .FirstOrDefault();

                                        if (check_dmrprrp != null)
                                        {
                                            check_dmrprrp.pickflag = 1;
                                            _context.DMRPRRP.Update(check_dmrprrp);
                                            _context.SaveChanges();
                                        }

                                        check.pickflag = "1";
                                        _context.Storage_Operation.Update(check);
                                        _context.SaveChanges();
                                    }
                                }
                            }
                        }
                        //ENDY
                    }
                }

                int maxId = _context.inward.Any() ? _context.inward.Max(e => e.inward_id) + 1 : 1;
                applicant.inward_id = maxId;
                List<inwardPacket> packetsToRemove = new List<inwardPacket>();


                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "Inward Operation";
                //logs.task = "Create "+applicant.ordertype+ " Inward !";
                logs.taskid = maxId;
                logs.task = maxId.ToString() + " inward operation " + applicant.ordertype + " Create";
                logs.action = "Create";
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.username = user;
                _context.Add(logs);
                try
                {
                    _context.Add(applicant);
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Catch Exception : " + ex.Message });
                }
                DMRPRRP_List.Clear();
                Remove_StockInventory.Clear();
                Update_DMRPRRPInventory.Clear();

                //FOR SUBCOMPONENTWISE 2
                //return RedirectToAction("Index");
                return Json(new { success = true, message = "Demo Outward Process done successfully!" });
            }

            else if (applicant.ordertype == "Sales")
            {
                applicant.batchcode = applicant.batchcode.ToUpper();
                int maxId = _context.inward.Any() ? _context.inward.Max(e => e.inward_id) + 1 : 1;
                applicant.inward_id = maxId;
                applicant.flag = 2;
                int flag = 0;
                applicant.gstinno = "-";
                applicant.typeofreturn = "-";
                applicant.pono = "-";
                //applicant.dcno = applicant.dcno.ToUpper();
                //applicant.invoiceno = applicant.invoiceno.ToUpper();
                if (!string.IsNullOrEmpty(applicant.grnno))
                {
                    applicant.grnno = applicant.grnno.ToUpper();
                }
                else
                {
                    applicant.grnno = "-";
                }
                if (!string.IsNullOrEmpty(applicant.grndate))
                {
                    applicant.grndate = applicant.grndate;
                }
                else
                {
                    applicant.grndate = "-";
                }
                if (!string.IsNullOrEmpty(applicant.dcno))
                {
                    applicant.dcno = applicant.dcno.ToUpper();
                }
                else
                {
                    applicant.dcno = "-";
                }
                if (!string.IsNullOrEmpty(applicant.invoiceno))
                {
                    applicant.invoiceno = applicant.invoiceno.ToUpper();
                }
                else
                {
                    applicant.invoiceno = "-";
                }
                if (!string.IsNullOrEmpty(applicant.invoicedate))
                {
                    applicant.invoicedate = applicant.invoicedate;
                }
                else
                {
                    applicant.invoicedate = "-";
                }
                if (!string.IsNullOrEmpty(applicant.dcdate))
                {
                    applicant.dcdate = applicant.dcdate;
                }
                else
                {
                    applicant.dcdate = "-";
                }
                //condiotn for purchase
                //applicant.date = DateTime.Now;
                //applicant.date = DateTime.Now.ToUniversalTime();

                List<inwardPacket> sss = new List<inwardPacket>();

                //subcom
                var inward_subcomponent = new List<inward_subcomponent>();
                foreach (var a in applicant.inwardPacket)
                {
                    a.date = applicant.date;
                    a.flag = applicant.flag;
                    var purchase1 = _context.so_inward.FirstOrDefault(p => p.sono == applicant.sono);
                    if (purchase1 != null)
                    {
                        // Fetch all Poproductdetail records associated with the found Porderid'

                        var poproductDetailsList = _context.so_Subcomponents
                            .Where(pd => pd.soproduct_id == purchase1.id)
                            .Select(a => new
                            {
                                a.subcomponents,
                                a.sccode,
                                a.scqty,
                                a.scuom,
                                a.tqty,
                                a.pono

                            })
                            .ToList();
                        if (poproductDetailsList.Any())
                        {
                            decimal totalsubassembly = poproductDetailsList.Sum(item => item.tqty);

                            foreach (var item in poproductDetailsList)
                            {
                                var newProduct = new inward_subcomponent
                                {
                                    sccode = item.sccode,
                                    subcomponents = item.subcomponents,
                                    scqty = item.scqty,
                                    scuom = item.scuom,
                                    tqty = (a.quantity * item.scqty),
                                    pono = "-",
                                    sono = applicant.sono,
                                };
                                inward_subcomponent.Add(newProduct);
                            }
                        }
                    }
                    applicant.inward_subcomponent.AddRange(inward_subcomponent);
                }


                //var SO_Exist = _context.inward.Where(p => p.sono == applicant.sono).Include(a => a.inwardPacket).AsNoTracking().FirstOrDefault(); //IF DATA NULL THEN UPDATE
                var SO_Exist = _context.inward.Where(p => p.sono == applicant.sono).AsNoTracking().ToList(); //IF DATA NULL THEN UPDATE
                int total = 0;
                if (SO_Exist.Count > 0)
                {
                    var existingpacket = _context.inwardPacket.Where(a => a.sono == applicant.sono)
                            .GroupBy(p => p.description)
                                     .Select(group => new
                                     {
                                         ProductName = group.Key,
                                         TotalQuantity = group.Sum(p => p.quantity),
                                         TotalSubAssembly = group.Sum(p => p.totalsubassmbly)
                                     }).ToList();


                    foreach (var a in existingpacket)
                    {
                        foreach (var packets in applicant.inwardPacket.Where(b => b.description == a.ProductName))
                        {
                            packets.date = applicant.date;
                            packets.flag = applicant.flag;
                            if ((packets.setofsub_assemb == null || packets.setofsub_assemb == "0") && (packets.qtyperpkt == null || packets.qtyperpkt == "0") && (packets.noofpackets == null || packets.noofpackets == "0"))
                            {
                                packets.quantity = 0;
                                packets.totalsubassmbly = 0;
                            }
                            if (packets.quantity == 0 || packets.quantity == null)
                            {
                                packets.setofsub_assemb = "0";
                                packets.qtyperpkt = "0";
                                packets.noofpackets = "0";
                            }
                            if (packets.setofsub_assemb == null)
                            {
                                packets.setofsub_assemb = "0";
                            }
                            if (packets.qtyperpkt == null)
                            {
                                packets.qtyperpkt = "0";
                            }
                            if (packets.noofpackets == null)
                            {
                                packets.noofpackets = "0";
                            }
                            if (packets.totalpacket == null)
                            {
                                packets.totalpacket = "0";
                            }
                            if (packets.totalsubassmbly == null)
                            {
                                packets.totalsubassmbly = 0;
                            }

                            total = a.TotalQuantity + packets.quantity;
                        }
                        // Find the purchase order details
                        var inwardOrder = _context.so_inward
                            .Where(po => po.sono == applicant.sono)
                            .Include(po => po.soProduct_details)
                            .FirstOrDefault();

                        if (inwardOrder != null)
                        {
                            // Get the list of purchase product details for this purchase order
                            var productDetails = _context.so_product
                                .Where(pd => pd.orderid == inwardOrder.id)
                                .ToList();
                            //foreach (var packetQuantity in productDetails.Where(a => a.description == a.description))
                            foreach (var packetQuantity in productDetails.Where(b => b.description.ToUpper() == a.ProductName.ToUpper()))
                            //&& b.productcode == a.productcode
                            {
                                // Find the corresponding product detail for the packet
                                var productDetail = productDetails.FirstOrDefault(pd => pd.id == packetQuantity.id);
                                if (productDetail != null)
                                {
                                    // Compare the existing quantity with the newly entered quantity
                                    if (total == productDetail.quantity)
                                    {
                                        var exist = _context.inward.Where(inwardPacket => inwardPacket.sono == applicant.sono).ToList();
                                        foreach (var item in exist)
                                        {
                                            item.status = "Completed";
                                        }
                                        // Quantity is suffici`ent (Completed)
                                        // You can update the status in the database or take other actions as needed
                                        applicant.status = "Completed";
                                    }
                                    else if (total > productDetail.quantity)
                                    {
                                        // Quantity is greater than the actual quantity
                                        _notyfService.Error("Quantity is greater than the actual quantity!");
                                        return RedirectToAction("Index");
                                    }
                                    else
                                    {
                                        // Quantity is insufficient (Pending)
                                        // You can update the status in the database or take other actions as needed
                                        flag = 1;
                                        applicant.status = "Pending";
                                    }
                                }
                            }


                            // Save changes to the database if needed
                            //  _context.SaveChanges();
                        }
                    }


                    //update purchase table status
                    var purchase = _context.so_inward.Where(a => a.sono == applicant.sono).FirstOrDefault();
                    if (purchase != null)
                    {
                        if (flag == 1)
                        {
                            applicant.status = "Pending";
                            purchase.status = "Pending";
                        }
                        else
                        {
                            applicant.status = "Completed";
                            purchase.status = "Completed";
                        }
                    }


                    _context.Update(purchase);
                    _context.Add(applicant);

                    //maintain logs
                    var user = HttpContext.Session.GetString("User");
                    var logs = new Logs();
                    logs.pagename = "Outward Operation";
                    //logs.task = "Create "+applicant.ordertype+ " outward !";
                    logs.task = maxId.ToString() + " Outward operation " + applicant.ordertype + " Create"; logs.taskid = maxId;
                    logs.action = "Create";
                    logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                    logs.time = DateTime.Now.ToString("HH:mm:ss");
                    logs.username = user;
                    _context.Add(logs);

                    _context.SaveChanges();
                    if (stickerDecision == 1)
                    {
                        //set sticker 
                        //set sticker 
                        string printprn = "";
                        var path = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn";
                        var value = $"{_webHostEnvironment.WebRootPath}\\Sticker\\ErosVALUE.prn";
                        //foreach (inwardPacket Packet in applicant.inwardPacket)
                        //{
                        //    if (System.IO.File.Exists(path) == true)
                        //    {
                        //        System.IO.File.Delete(value);
                        //        System.IO.File.Copy(path, value);


                        //        for (int q = 1; q <= int.Parse(Packet.quantity.ToString()); q++)
                        //        {
                        //            for (int i = 1; i <= int.Parse(Packet.noofpackets); i++)
                        //            {
                        //                try
                        //                {
                        //                    string value1 = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn"; // Replace with the actual file path

                        //                    // Perform file manipulation operations
                        //                    string fileContent = System.IO.File.ReadAllText(value1);
                        //                    fileContent = fileContent.Replace("<D001>", Packet.productcode);
                        //                    fileContent = fileContent.Replace("<D002>", Packet.description);
                        //                    fileContent = fileContent.Replace("<D003>", applicant.grnno);
                        //                    fileContent = fileContent.Replace("<D004>", q + "-" + i + "/" + Packet.noofpackets);
                        //                    fileContent = fileContent.Replace("<D005>", Packet.qtyperpkt + "/" + Packet.setofsub_assemb);
                        //                    fileContent = fileContent.Replace("<D006>", applicant.batchcode);

                        //                    System.IO.File.WriteAllText(value, fileContent);

                        //                    //  printprn = printprn + value + Environment.NewLine;

                        //                    //------Replacement of above comment command ------ //
                        //                    string fileContent1 = System.IO.File.ReadAllText(value);
                        //                    printprn = printprn + fileContent1 + Environment.NewLine;
                        //                    //------Replacement of above comment command ------ //
                        //                }

                        //                catch
                        //                {
                        //                    // Handle any exceptions
                        //                }
                        //            }
                        //        }

                        //    }
                        //    else
                        //    {

                        //    }
                        //}
                        foreach (inwardPacket Packet in applicant.inwardPacket)
                        {
                            if (System.IO.File.Exists(path) == true)
                            {
                                System.IO.File.Delete(value);
                                System.IO.File.Copy(path, value);

                                int noofpkt = Convert.ToInt32(Packet.noofpackets);
                                if (noofpkt > 0)
                                {
                                    for (int q = 1; q <= int.Parse(Packet.quantity.ToString()); q++)
                                    {
                                        for (int i = 1; i <= int.Parse(Packet.noofpackets); i++)
                                        {
                                            try
                                            {
                                                string value1 = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn"; // Replace with the actual file path

                                                // Perform file manipulation operations
                                                string fileContent = System.IO.File.ReadAllText(value1);
                                                fileContent = fileContent.Replace("<D001>", Packet.productcode.ToString().Trim().ToUpper());
                                                fileContent = fileContent.Replace("<D002>", Packet.description.ToString().Trim().ToUpper());
                                                fileContent = fileContent.Replace("<D003>", applicant.grnno.ToString().Trim());
                                                fileContent = fileContent.Replace("<D004>", q + "-" + i + "/" + Packet.noofpackets.ToString().Trim());
                                                fileContent = fileContent.Replace("<D005>", Packet.qtyperpkt.ToString().Trim() + "/" + Packet.setofsub_assemb.ToString().Trim());
                                                fileContent = fileContent.Replace("<D006>", applicant.batchcode.ToString().Trim());

                                                System.IO.File.WriteAllText(value, fileContent);

                                                //  printprn = printprn + value + Environment.NewLine;

                                                //------Replacement of above comment command ------ //
                                                string fileContent1 = System.IO.File.ReadAllText(value);
                                                printprn = printprn + fileContent1 + Environment.NewLine;
                                                //------Replacement of above comment command ------ //
                                            }

                                            catch
                                            {
                                                // Handle any exceptions
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    int qty = Packet.quantity;
                                    int qtypershp = Packet.noqtypershp;
                                    float totalshipperCeil = (float)qty / qtypershp;
                                    int totalshipper = (int)Math.Ceiling(totalshipperCeil);

                                    int shp = 1;
                                    for (int i = 1; i <= totalshipper; i++)
                                    {
                                        int remainingQty = qty - ((i - 1) * qtypershp); // Calculate remaining quantity for this shipper

                                        // Determine the quantity for this shipper
                                        int currentQty;
                                        if (i < totalshipper)
                                        {
                                            currentQty = qtypershp;
                                        }
                                        else
                                        {
                                            currentQty = remainingQty;
                                        }
                                        string value1 = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn";
                                        string fileContent = System.IO.File.ReadAllText(value1);
                                        fileContent = fileContent.Replace("<D001>", Packet.productcode.ToString().Trim().ToUpper());
                                        fileContent = fileContent.Replace("<D002>", Packet.description.ToString().Trim().ToUpper());
                                        fileContent = fileContent.Replace("<D003>", applicant.grnno.ToString().Trim());
                                        fileContent = fileContent.Replace("<D004>", $"{i}-{currentQty}/{shp.ToString().Trim()}");
                                        fileContent = fileContent.Replace("<D005>", Packet.qtyperpkt.ToString().Trim() + "/" + Packet.setofsub_assemb.ToString().Trim());
                                        fileContent = fileContent.Replace("<D006>", applicant.batchcode.ToString().Trim());
                                        System.IO.File.WriteAllText(value, fileContent);

                                        //  printprn = printprn + value + Environment.NewLine;

                                        //------Replacement of above comment command ------ //
                                        string fileContent1 = System.IO.File.ReadAllText(value);
                                        printprn = printprn + fileContent1 + Environment.NewLine;
                                        //------Replacement of above comment command ------ //
                                    }
                                }


                            }
                            else
                            {

                            }
                        }

                        byte[] byteArray = Encoding.ASCII.GetBytes(printprn);
                        MemoryStream stream = new MemoryStream(byteArray);
                        return File(stream, "text/plain", "example.prn");
                    }
                    return RedirectToAction("Index"); // Replace "Index" with the name of your action method
                }
                else
                {
                    //int maxId = _context.inward.Any() ? _context.inward.Max(e => e.inward_id) + 1 : 1;
                    //applicant.inward_id = maxId;
                    List<inwardPacket> packetsToRemove = new List<inwardPacket>();
                    //check pending or complete
                    foreach (var a in applicant.inwardPacket)
                    {
                        a.date = applicant.date;
                        a.flag = applicant.flag;
                        if ((a.setofsub_assemb == null || a.setofsub_assemb == "0") && (a.qtyperpkt == null || a.qtyperpkt == "0") && (a.noofpackets == null || a.noofpackets == "0"))
                        {
                            a.quantity = 0;
                            a.totalsubassmbly = 0;
                        }
                        if (a.quantity == 0 || a.quantity == null)
                        {
                            a.setofsub_assemb = "0";
                            a.qtyperpkt = "0";
                            a.noofpackets = "0";
                        }
                        if (a.setofsub_assemb == null)
                        {
                            a.setofsub_assemb = "0";
                        }
                        if (a.qtyperpkt == null)
                        {
                            a.qtyperpkt = "0";
                        }
                        if (a.noofpackets == null)
                        {
                            a.noofpackets = "0";
                        }
                        if (a.totalpacket == null)
                        {
                            a.totalpacket = "0";
                        }
                        if (a.totalsubassmbly == null)
                        {
                            a.totalsubassmbly = 0;
                        }

                        // Find the purchase order details
                        var inwardOrder = _context.so_inward
                            .Where(po => po.sono == applicant.sono)
                            .Include(po => po.soProduct_details)
                            .FirstOrDefault();
                        if (inwardOrder != null)
                        {
                            // Get the list of purchase product details for this purchase order
                            var productDetails = _context.so_product
                                .Where(pd => pd.orderid == inwardOrder.id)
                                .ToList();
                            //foreach (var packetQuantity in productDetails.Where(a => a.description == a.description))
                            foreach (var packetQuantity in productDetails.Where(b => b.description == a.description && b.productcode.ToUpper() == a.productcode.ToUpper()))
                            {
                                // Find the corresponding product detail for the packet
                                var productDetail = productDetails.FirstOrDefault(pd => pd.id == packetQuantity.id);
                                if (productDetail != null)
                                {
                                    // Compare the existing quantity with the newly entered quantity
                                    if (a.quantity == productDetail.quantity)
                                    {
                                        // Quantity is sufficient (Completed)
                                        // You can update the status in the database or take other actions as needed
                                        applicant.status = "Completed";
                                    }
                                    else if (a.quantity > productDetail.quantity)
                                    {
                                        // Quantity is greater than the actual quantity
                                        _notyfService.Error("Quantity is greater than the actual quantity!");
                                        return RedirectToAction("Index");
                                    }
                                    else
                                    {
                                        // Quantity is insufficient (Pending)
                                        // You can update the status in the database or take other actions as needed
                                        flag = 1;
                                        applicant.status = "Pending";
                                    }
                                }
                            }
                            // Save changes to the database if needed
                            //  _context.SaveChanges();
                        }

                    }

                    //update purchase table status
                    var purchase = _context.so_inward.Where(a => a.sono == applicant.sono).FirstOrDefault();
                    if (purchase != null)
                    {
                        if (flag == 1)
                        {
                            applicant.status = "Pending";
                            purchase.status = "Pending";
                        }
                        else
                        {
                            applicant.status = "Completed";
                            purchase.status = "Completed";
                        }
                    }


                    _context.Update(purchase);
                    _context.Add(applicant);

                    //maintain logs
                    var user = HttpContext.Session.GetString("User");
                    var logs = new Logs();
                    logs.pagename = "Outward Operation";
                    //logs.task = "Create "+applicant.ordertype+ " outward !";
                    logs.task = maxId.ToString() + " Outward operation " + applicant.ordertype + " Create"; logs.taskid = maxId;
                    logs.action = "Create";
                    logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                    logs.time = DateTime.Now.ToString("HH:mm:ss");
                    logs.username = user;
                    _context.Add(logs);

                    _context.SaveChanges();
                    if (stickerDecision == 1)
                    {
                        //set sticker 
                        string printprn = "";
                        var path = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn";
                        var value = $"{_webHostEnvironment.WebRootPath}\\Sticker\\ErosVALUE.prn";
                        foreach (inwardPacket Packet in applicant.inwardPacket)
                        {
                            if (System.IO.File.Exists(path) == true)
                            {
                                System.IO.File.Delete(value);
                                System.IO.File.Copy(path, value);


                                for (int q = 1; q <= int.Parse(Packet.quantity.ToString()); q++)
                                {
                                    for (int i = 1; i <= int.Parse(Packet.noofpackets); i++)
                                    {
                                        try
                                        {
                                            string value1 = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn"; // Replace with the actual file path

                                            // Perform file manipulation operations
                                            string fileContent = System.IO.File.ReadAllText(value1);
                                            fileContent = fileContent.Replace("<D001>", Packet.productcode.ToUpper());
                                            fileContent = fileContent.Replace("<D002>", Packet.description.ToUpper());
                                            fileContent = fileContent.Replace("<D003>", applicant.grnno);
                                            fileContent = fileContent.Replace("<D004>", q + "-" + i + "/" + Packet.noofpackets);
                                            fileContent = fileContent.Replace("<D005>", Packet.qtyperpkt + "/" + Packet.setofsub_assemb);
                                            fileContent = fileContent.Replace("<D006>", applicant.batchcode);

                                            System.IO.File.WriteAllText(value, fileContent);

                                            //  printprn = printprn + value + Environment.NewLine;

                                            //------Replacement of above comment command ------ //
                                            string fileContent1 = System.IO.File.ReadAllText(value);
                                            printprn = printprn + fileContent1 + Environment.NewLine;
                                            //------Replacement of above comment command ------ //
                                        }

                                        catch
                                        {
                                            // Handle any exceptions
                                        }
                                    }
                                }

                            }
                            else
                            {

                            }
                        }
                        ////byte[] fileBytes = Encoding.UTF8.GetBytes(printprn);
                        ////string contentType = "text/plain";
                        ////string fileName = "example.txt";
                        ////return File(fileBytes, contentType, fileName);
                        //byte[] byteArray = Encoding.ASCII.GetBytes(printprn);
                        //MemoryStream stream = new MemoryStream(byteArray);
                        //return File(stream, "text/plain", "example.prn");
                        byte[] byteArray = Encoding.ASCII.GetBytes(printprn);
                        MemoryStream stream = new MemoryStream(byteArray);
                        return File(stream, "text/plain", "example.prn");
                    }
                    //return View(Index);

                    return RedirectToAction("Index"); // Replace "Index" with the name of your action method

                }
            }
            else if (applicant.ordertype == "Purchasereturn")
            {
                if (!string.IsNullOrEmpty(applicant.grnno))
                {
                    applicant.grnno = applicant.grnno.ToUpper();
                }
                else
                {
                    applicant.grnno = "-";
                }
                if (!string.IsNullOrEmpty(applicant.grndate))
                {
                    applicant.grndate = applicant.grndate;
                }
                else
                {
                    applicant.grndate = "-";
                }
                applicant.batchcode = applicant.batchcode.ToUpper();
                //condition for sale return
                applicant.sono = "-";
                applicant.flag = 2;
                int flag = 0;
                applicant.typeofreturn = "-";
                //applicant.date = DateTime.Now   ;
                //applicant.date = DateTime.Now.ToUniversalTime();
                applicant.sono = "-";
                if (!string.IsNullOrEmpty(applicant.dcno))
                {
                    applicant.dcno = applicant.dcno.ToUpper();
                }
                else
                {
                    applicant.dcno = "-";
                }
                if (!string.IsNullOrEmpty(applicant.invoiceno))
                {
                    applicant.invoiceno = applicant.invoiceno.ToUpper();
                }
                else
                {
                    applicant.invoiceno = "-";
                }
                if (!string.IsNullOrEmpty(applicant.invoicedate))
                {
                    applicant.invoicedate = applicant.invoicedate;
                }
                else
                {
                    applicant.invoicedate = "-";
                }
                if (!string.IsNullOrEmpty(applicant.dcdate))
                {
                    applicant.dcdate = applicant.dcdate;
                }
                else
                {
                    applicant.dcdate = "-";
                }
                //applicant.dcno = applicant.dcno.ToUpper();
                //applicant.invoiceno = applicant.invoiceno.ToUpper();

                int maxId = _context.inward.Any() ? _context.inward.Max(e => e.inward_id) + 1 : 1;
                applicant.inward_id = maxId;

                //purchase - subcom
                foreach (var a in applicant.inwardPacket)
                {
                    a.date = applicant.date;
                    a.flag = applicant.flag;
                    if ((a.setofsub_assemb == null || a.setofsub_assemb == "0") && (a.qtyperpkt == null || a.qtyperpkt == "0") && (a.noofpackets == null || a.noofpackets == "0" || a.noqtypershp == null))
                    {
                        //a.quantity = 0;
                        a.totalsubassmbly = 0;
                        a.noqtypershp = 0;
                    }
                    if (a.quantity == 0 || a.quantity == null)
                    {
                        a.setofsub_assemb = "0";
                        a.qtyperpkt = "0";
                        a.noofpackets = "0";
                    }
                    if (a.setofsub_assemb == null)
                    {
                        a.setofsub_assemb = "0";
                    }
                    if (a.qtyperpkt == null)
                    {
                        a.qtyperpkt = "0";
                    }
                    if (a.noofpackets == null)
                    {
                        a.noofpackets = "0";
                    }
                    if (a.totalpacket == null)
                    {
                        a.totalpacket = "0";
                    }
                    if (a.totalsubassmbly == null)
                    {
                        a.totalsubassmbly = 0;
                    }

                    if ((a.setofsub_assemb == null || a.setofsub_assemb == "0") && (a.qtyperpkt == null || a.qtyperpkt == "0") && (a.noofpackets == null || a.noofpackets == "0"))
                    {
                        //a.quantity = 0;
                        a.totalsubassmbly = 0;
                    }
                    if (a.quantity == 0 || a.quantity == null)
                    {
                        a.setofsub_assemb = "0";
                        a.qtyperpkt = "0";
                        a.noofpackets = "0";
                    }
                    if (applicant.grnno == null)
                    {
                        applicant.grnno = "-";
                    }
                    if (a.totalpacket == null)
                    {
                        a.totalpacket = "0";
                    }
                    var inward_subcomponent = new List<inward_subcomponent>();
                    var purchase1 = _context.purchase.FirstOrDefault(p => p.pono == applicant.pono);
                    if (purchase1 != null)
                    {
                        // Fetch all Poproductdetail records associated with the found Porderid'
                        var poproductDetailsList = _context.purchase_subcomponent
                            .Where(pd => pd.purchaseproduct_id == purchase1.id)
                            .Select(a => new
                            {
                                a.subcomponents,
                                a.sccode,
                                a.scqty,
                                a.scuom,
                                a.tqty
                            })
                            .ToList();
                        //var poproductDetailsList = _context.purchase_subcomponent
                        //   .Where(pd => pd.purchaseproduct_id == purchase.id && pd.sccode.StartsWith(productCode))
                        //   .Select(a => new
                        //   {
                        //       a.subcomponents,
                        //       a.sccode,
                        //       a.scqty,
                        //       a.scuom,
                        //       a.tqty
                        //   })
                        //   .ToList();
                        if (poproductDetailsList.Any())
                        {
                            decimal totalsubassembly = poproductDetailsList.Sum(item => item.tqty);
                            foreach (var item in poproductDetailsList)
                            {
                                var newProduct = new inward_subcomponent
                                {
                                    sccode = item.sccode,
                                    subcomponents = item.subcomponents,
                                    scqty = item.scqty,
                                    scuom = item.scuom,
                                    tqty = (a.quantity * item.scqty)
                                };
                                inward_subcomponent.Add(newProduct);
                            }
                        }
                    }
                    applicant.inward_subcomponent.AddRange(inward_subcomponent);
                }

                List<inwardPacket> packetsToRemove = new List<inwardPacket>();

                //Existing data is updated into inward opration
                foreach (var a in applicant.inwardPacket)
                {
                    a.date = applicant.date;
                    a.flag = applicant.flag;
                    a.pono = applicant.pono;
                    a.sono = "-";

                }

                if (savepolist1.Count > 0)
                {
                    int maxIdPR = _context.PR_model.Any() ? _context.PR_model.Max(e => e.id) + 0 : 0;
                    foreach (var item in savepolist1)
                    {
                        var find = _context.Storage_Operation.Where(a => a.productcode.Trim() == item.productcode.Trim()
                        && a.batchcode.Trim() == item.batchcode.Trim()
                        && a.grnno.Trim() == item.grnno
                        && a.boxno.Trim() == item.boxno.Trim()).FirstOrDefault();
                        if (find != null)
                        {
                            find.locationcode = item.locationcode;
                            find.statusflag = "PR";
                        }
                        maxIdPR++;
                        PR_model PR_model = new PR_model()
                        {
                            id = maxIdPR,
                            ordertype = applicant.ordertype,
                            productcode = item.productcode,
                            batchcode = item.batchcode,
                            boxno = item.boxno,
                            locationcode = item.locationcode,
                            statusflag = "PR",
                            pickflag = item.pickflag,
                            grnno = item.grnno,
                            refno = applicant.pono,
                            date = DateTime.Now.ToString("dd-MM-yyyy")  // Corrected assignment
                        };
                        _context.PR_model.Add(PR_model);
                        _context.SaveChanges();
                        _context.Storage_Operation.Update(find);
                        _context.SaveChanges();
                    }
                }
                _context.Add(applicant);
                _context.SaveChanges();
                savepolist1.Clear();

                var pooredr = _context.purchase.Where(a => a.pono.Trim() == applicant.pono.Trim()).FirstOrDefault();
                var pooredr1 = _context.purchaseReturn.Where(a => a.pono.Trim().StartsWith(applicant.pono.Trim())).FirstOrDefault();
                if (pooredr != null && pooredr1 == null)
                {
                    pooredr.status = "Completed";
                    _context.purchase.Update(pooredr);
                    _context.SaveChanges();

                }
                else if (pooredr != null && pooredr1 != null)
                {
                    pooredr.status = "Completed";
                    _context.purchase.Update(pooredr);
                    _context.SaveChanges();

                    pooredr1.status = "Completed";
                    _context.purchaseReturn.Update(pooredr1);
                    _context.SaveChanges();
                }

                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "Outward Operation";
                //logs.task = "Create "+applicant.ordertype+ " outward !";
                logs.task = maxId.ToString() + " Outward operation " + applicant.ordertype + " Create";
                logs.taskid = maxId;
                logs.action = "Create";
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.username = user;
                _context.Add(logs);
                _context.SaveChanges();
                //    if (stickerDecision == 0)
                //    {
                //        string printprn = "";
                //        var path = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn";
                //        var value = $"{_webHostEnvironment.WebRootPath}\\Sticker\\ErosVALUE.prn";

                //        foreach (inwardPacket Packet in applicant.inwardPacket.Where(a=>Convert.ToInt32(a.quantity) > 0))
                //        //.Where(f => f.check == "true")
                //        {
                //        if (System.IO.File.Exists(path) == true)
                //            {
                //                System.IO.File.Delete(value);
                //                System.IO.File.Copy(path, value);

                //                int noofpkt = Convert.ToInt32(Packet.noofpackets);
                //                if (noofpkt > 0)
                //                {
                //                    for (int q = 1; q <= int.Parse(Packet.quantity.ToString()); q++)
                //                    {
                //                        for (int i = 1; i <= int.Parse(Packet.noofpackets); i++)
                //                        {
                //                            try
                //                            {
                //                                string value1 = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn"; // Replace with the actual file path

                //                                string fileContent = System.IO.File.ReadAllText(value1);
                //                                fileContent = fileContent.Replace("<D001>", Packet.productcode.ToString().Trim().ToUpper());
                //                                fileContent = fileContent.Replace("<D002>", Packet.description.ToString().Trim().ToUpper());
                //                                fileContent = fileContent.Replace("<D003>", applicant.grnno.ToString().Trim());
                //                                fileContent = fileContent.Replace("<D004>", q + "-" + i + "/" + Packet.noofpackets.ToString().Trim());
                //                                fileContent = fileContent.Replace("<D005>", Packet.qtyperpkt.ToString().Trim() + "/" + Packet.setofsub_assemb.ToString().Trim());
                //                                fileContent = fileContent.Replace("<D006>", applicant.batchcode.ToString().Trim());

                //                                System.IO.File.WriteAllText(value, fileContent);

                //                                string fileContent1 = System.IO.File.ReadAllText(value);
                //                                printprn = printprn + fileContent1 + Environment.NewLine;
                //                            }

                //                            catch
                //                            {
                //                            }
                //                        }
                //                    }
                //                }
                //                else
                //                {
                //                    int qty = Packet.quantity;
                //                    int qtypershp = Packet.noqtypershp;
                //                    float totalshipperCeil = (float)qty / qtypershp;
                //                    int totalshipper = (int)Math.Ceiling(totalshipperCeil);

                //                    int shp = 1;
                //                    for (int i = 1; i <= totalshipper; i++)
                //                    {
                //                        int remainingQty = qty - ((i - 1) * qtypershp); // Calculate remaining quantity for this shipper

                //                        int currentQty;
                //                        if (i < totalshipper)
                //                        {
                //                            currentQty = qtypershp;
                //                        }
                //                        else
                //                        {
                //                            currentQty = remainingQty;
                //                        }
                //                        string value1 = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn";
                //                        string fileContent = System.IO.File.ReadAllText(value1);
                //                        fileContent = fileContent.Replace("<D001>", Packet.productcode.ToString().Trim().ToUpper());
                //                        fileContent = fileContent.Replace("<D002>", Packet.description.ToString().Trim().ToUpper());
                //                        fileContent = fileContent.Replace("<D003>", applicant.grnno.ToString().Trim());
                //                        fileContent = fileContent.Replace("<D004>", $"{i}-{currentQty}/{shp.ToString().Trim()}");
                //                        fileContent = fileContent.Replace("<D005>", Packet.qtyperpkt.ToString().Trim() + "/" + Packet.setofsub_assemb.ToString().Trim());
                //                        fileContent = fileContent.Replace("<D006>", applicant.batchcode.ToString().Trim());
                //                        System.IO.File.WriteAllText(value, fileContent);

                //                        string fileContent1 = System.IO.File.ReadAllText(value);
                //                        printprn = printprn + fileContent1 + Environment.NewLine;
                //                    }
                //                }


                //            }
                //            else
                //            {

                //            }
                //        }

                //}
                //return RedirectToAction("Index"); 
                return Json(new { success = true, message = "Purchase return process complete successfully  !" });
            }
            else
            {
                return Json(Ok());
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            //Outward outward = _context.Outward.Include(e => e.outwardPacket).Where(a => a.outward_id == id).FirstOrDefault();
            //return View(outward);
            var outwards = _context.inward
                       .Where(po => po.inward_id == id)
                       .Include(po => po.inwardPacket)
                       .FirstOrDefault();
            //outwards.date = DateTime.Now;
            if (outwards == null)
            {
                // If the record is not found, you can handle the error accordingly, e.g., show a 404 page or redirect to the index page.
                return NotFound(); // or return RedirectToAction("Index");
            }
            return View(outwards);
        }

        [HttpPost]
        public IActionResult Edit(inward applicant)
        {
            applicant.sono = "-";
            int flag = 0;
            try
            {

                foreach (var a in applicant.inwardPacket)
                {
                    // Find the purchase order details
                    var inwardOrder = _context.purchase
                        .Where(po => po.pono == applicant.pono)
                        .Include(po => po.poProduct_details)
                        .FirstOrDefault();
                    if (inwardOrder != null)
                    {
                        // Get the list of purchase product details for this purchase order
                        var productDetails = _context.poProduct_details
                            .Where(pd => pd.porderid == inwardOrder.id)
                            .ToList();

                        foreach (var packetQuantity in productDetails)
                        {
                            // Find the corresponding product detail for the packet
                            var productDetail = productDetails.FirstOrDefault(pd => pd.id == packetQuantity.id);
                            if (productDetail != null)
                            {
                                // Compare the existing quantity with the newly entered quantity
                                if (a.quantity == productDetail.quantity)
                                {
                                    // Quantity is sufficient (Completed)
                                    // You can update the status in the database or take other actions as needed
                                    applicant.status = "Completed";
                                }
                                else if (a.quantity > productDetail.quantity)
                                {
                                    // Quantity is greater than the actual quantity
                                    _notyfService.Error("Quantity is greater than the actual quantity!");
                                    return RedirectToAction("Index");
                                }
                                else
                                {
                                    // Quantity is insufficient (Pending)
                                    // You can update the status in the database or take other actions as needed
                                    flag = 1;
                                    applicant.status = "Pending";
                                }
                            }
                        }
                        // Save changes to the database if needed
                        //  _context.SaveChanges();
                    }
                }
                //update purchase table status
                var purchase = _context.purchase.Where(a => a.pono == applicant.pono).FirstOrDefault();
                if (purchase != null)
                {
                    if (flag == 1)
                    {
                        applicant.status = "Pending";
                        purchase.status = "Pending";
                    }
                    else
                    {
                        applicant.status = "Completed";
                        purchase.status = "Completed";
                    }
                }
                _context.Update(purchase);

                // Remove existing inward packet details and update the applicant

                List<inwardPacket> inwardDetails = _context.inwardPacket.Where(e => e.inwardId == applicant.inward_id).AsNoTracking().ToList();
                _context.inwardPacket.RemoveRange(inwardDetails);
                _context.SaveChanges();

                // Maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs()
                {
                    pagename = "Outward Operation",
                    //task = "Update outward Operation",
                    taskid = applicant.inward_id,
                    task = applicant.inward_id.ToString(),
                    action = "Update",
                    date = DateTime.Now.ToString("dd/MM/yyyy"),
                    time = DateTime.Now.ToString("HH:mm:ss"),
                    username = user
                };

                _context.Add(logs);

                _context.Update(applicant);
                _context.SaveChanges();
                _notyfService.Success("SalesOrder Completed Successfully");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!so_inwardExists(applicant.inward_id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            inward supplier_Master = _context.inward.Include(e => e.inwardPacket).Where(a => a.inward_id == id).FirstOrDefault();

            // Maintain logs
            var user = HttpContext.Session.GetString("User");
            var logs = new Logs()
            {
                pagename = "Outward Operation",
                //task = "View outward Operation",
                taskid = id,
                task = id.ToString(),
                action = "View",
                date = DateTime.Now.ToString("dd/MM/yyyy"),
                time = DateTime.Now.ToString("HH:mm:ss"),
                username = user
            };

            _context.Add(logs);
            _context.SaveChanges();

            return View(supplier_Master);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            var data = await _context.inward.Include(e => e.inwardPacket).FirstOrDefaultAsync(a => a.inward_id == id);
            if (_context.inward == null)
            {
                return Problem("Entity set 'AuthDbContext.customer_master'  is null.");
            }
            var customer = _context.inward.Include(e => e.inwardPacket).Where(a => a.inward_id == id).FirstOrDefault(); ;
            if (customer != null)
            {
                _context.inward.Remove(customer);

                // Maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs()
                {
                    pagename = "Outward Operation",
                    task = data.inward_id + "$" + data.pono + "$" + data.sono + "$" + data.vendername + "$" + data.typeofreturn + "$" + data.partyname + "$" + data.gstinno + "$" + data.contactno + "$" + data.address + "$" + data.dcno + "$" + data.invoiceno + "$" + data.dcdate + "$" + data.invoicedate + "$" + data.grnno + "$" + data.grndate + "$" + data.remarks + "$" + data.batchcode + "$" + data.date + "$" + data.time + "$" + data.ordertype + "$" + data.status + "$" + data.flag,
                    //task = id.ToString(),
                    taskid = Convert.ToInt32(id),
                    action = "Delete",
                    date = DateTime.Now.ToString("dd/MM/yyyy"),
                    time = DateTime.Now.ToString("HH:mm:ss"),
                    username = user
                };

                _context.Add(logs);
            }

            await _context.SaveChangesAsync();
            _notyfService.Error("Record Deleted Succesfully");
            return RedirectToAction(nameof(Index));
        }
        private bool so_inwardExists(int id)
        {
            return (_context.so_inward?.Any(e => e.id == id)).GetValueOrDefault();
        }

    }
}
