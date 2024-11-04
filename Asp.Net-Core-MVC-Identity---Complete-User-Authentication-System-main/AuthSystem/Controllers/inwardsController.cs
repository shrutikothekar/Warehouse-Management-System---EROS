using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AuthSystem.Data;
using eros.Models;
using AspNetCoreHero.ToastNotification.Abstractions;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.ComponentModel;
using System.Net.NetworkInformation;
using AspNetCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics.Metrics;
using Microsoft.Identity.Client.Extensibility;
using Humanizer;
using System.Net.WebSockets;
using System.Globalization;
using System.Runtime.Intrinsics.X86;
using System.Runtime.ConstrainedExecution;
using static eros.Controllers.InStockController;
using System.IO;
using static eros.Controllers.Picking_OperationController;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Math;
using Newtonsoft.Json;
using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using Newtonsoft.Json.Linq;
using Nest;
using Syncfusion.EJ2.Linq;
using static eros.Controllers.inwardsController;
using DocumentFormat.OpenXml.Presentation;
using Font = DocumentFormat.OpenXml.Presentation.Font;
using Microsoft.CodeAnalysis.Elfie.Extensions;
using DocumentFormat.OpenXml.Spreadsheet;
using Elasticsearch.Net;
namespace eros.Controllers
{
    public class inwardsController : Controller
    {
        private readonly ErosDbContext _context;
        public INotyfService _notyfService { get; }

        private IWebHostEnvironment _webHostEnvironment;
        string printer = "192.168.1.100";
        //string printer = "IMPACT by Honeywell IH-2 (ZPL)";
        string InstalledPrinters;
        Font printFont;
        StreamReader streamToPrint;


        private static List<Loading_Dispatch_Operation> savepolist = new List<Loading_Dispatch_Operation>();
        private static List<Storage_Operation> savepolist1 = new List<Storage_Operation>();
        private static List<inwardPacket> SHPlist = new List<inwardPacket>();

        //DEMO, REPRI, REPLACEMENT
        private static List<DMRPRRP> DMRPRRP_List = new List<DMRPRRP>();
        private static List<Storage_Operation> Storage_OperationAdd = new List<Storage_Operation>();
        private static List<Storage_Operation> Storage_OperationExAdd = new List<Storage_Operation>();
        //private static List<DMRPRRP> DMRPRRP_Update_List = new List<DMRPRRP>();
        private static List<Storage_Operation> Remove_StockInventory = new List<Storage_Operation>();
        //private static List<Loading_Dispatch_Operation> Remove_DispatchInventory = new List<Loading_Dispatch_Operation>();
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
        public class DeleteRequestModel
        {
            public string LocationCode { get; set; }
            public string BoxNo { get; set; }
            public string BatchCode { get; set; }
            public string ProductCode { get; set; }
        }

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


        [HttpGet]
        public IActionResult GetDMRPRRP_Details(string refid)
        {
            if (string.IsNullOrEmpty(refid))
            {
                return BadRequest("RefId is missing.");
            }

            var getdata = _context.inward.Where(a => a.pono.Trim() == refid.Trim()).OrderBy(a => a.inward_id).FirstOrDefault();

            if (getdata != null)
            {
                var retuntype = _context.inward.Where(a => a.pono.Trim() == getdata.pono.Trim()).OrderByDescending(a => a.inward_id).FirstOrDefault();
                var data = new
                {
                    partyname = getdata.partyname.Trim(),
                    refno = refid,
                    returntype = retuntype.typeofreturn,
                    vendor = getdata.vendername,
                    gstno = getdata.gstinno,
                    mobileno = getdata.contactno,
                    address = getdata.address,
                    batch = getdata.batchcode,
                    grn = getdata.grndate,
                    date = getdata.date,
                    status = getdata.status.Trim(),
                    IsCompleted = retuntype.status.Trim() == "Completed" , // Check if status is 'Completed'
                    dcno = retuntype.dcno,
                    dcdate = retuntype.dcdate,
                    inno = retuntype.invoiceno,
                    indate = retuntype.invoicedate,
                    ordertype = getdata.ordertype,
                };

                // Return data as JSON
                return Json(new { success = true, data = data });
            }

            return Json(new { success = false, message = "No Data found!" });
        }
        public class InwardDataModel
        {
            public string Refno { get; set; } // Add refno property
            public string Partyname { get; set; }
            public string Vendor { get; set; }
            public string Gstno { get; set; }
            public string Mobileno { get; set; }
            public string Address { get; set; }
            public string Batch { get; set; }
            public string Grn { get; set; }
            public DateTime Date { get; set; } // Change to DateTime if needed
            public string Dcno { get; set; }
            public DateTime Dcdate { get; set; } // Change to DateTime if needed
            public string Invoiceno { get; set; }
            public DateTime Invoicedate { get; set; } // Change to DateTime if needed
            public string ReturnType { get; set; }
        }
        [HttpPost]
        public IActionResult UpdateDMRPRRP_Data([FromBody] InwardDataModel data)
        {
            if (data == null)
            {
                return Json(new { success = false, message = "Invalid data." });
            }

            try
            {
                var getdata = _context.inward.FirstOrDefault(a => a.pono.Trim() == data.Refno.Trim());
              

                int maxId = _context.inward.Any() ? _context.inward.Max(e => e.inward_id) + 1 : 1;
                int maxId1 = _context.inwardPacket.Any() ? _context.inwardPacket.Max(e => e.id) + 1 : 1;
               
                var inward = new inward
                {
                    inward_id = maxId,
                    pono = data.Refno,
                    sono = data.Refno,
                    partyname = data.Partyname,
                    vendername = data.Vendor,
                    gstinno = data.Gstno,
                    contactno = data.Mobileno,
                    address = data.Address,
                    batchcode = GenerateBatchCode(),
                    grndate = data.Grn,
                    grnno = Generategrn(),
                    date = DateTime.Now.ToString("yyyy-MM-dd"),
                    dcno = data.Dcno,
                    dcdate = DateTime.Now.ToString("yyyy-MM-dd"),
                    invoiceno = data.Invoiceno,
                    invoicedate = DateTime.Now.ToString("yyyy-MM-dd"),
                    typeofreturn = data.ReturnType,
                    remark = "NA",
                    ordertype = getdata?.ordertype, 
                    status = "Completed",
                    flag = 1,
                };
                var inwardpacket = new inwardPacket
                {
                    id = maxId1,
                    inwardId = maxId, 
                    pono = data.Refno,
                    sono = data.Refno,
                    brand = "-",
                    productcode = "-",
                    description = "-",
                    setofsub_assemb = "0",
                    quantity = 0,
                    uom = "-",
                    qtyperpkt = "0",
                    noofpackets = "0",
                    totalpacket = "0",
                    totalsubassmbly = 0,
                    totalSubComUOM = 0,
                    NoOfSubComQTYperShipper = 0,
                    SOQty = 0,
                    DLQty = 0,
                    POQty = 0,
                    noqtypershp = 0,
                    date = DateTime.Now.ToString("yyyy-MM-dd"),
                    flag = 1,
                };
                _context.inward.Add(inward);
                _context.inwardPacket.Add(inwardpacket);
                _context.SaveChanges();
                return Json(new { success = true, message = "Data saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving data: " + ex.Message });
            }
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
                    if (row.batch.Trim() == pickedListData.batchno.Trim() && row.boxno == pickedListData.BoxNo && row.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper())
                    {
                        counter++;
                    }
                    else if (split == splitbox && row.productcode.ToUpper().Trim() == pickedListData.productcode.Trim().ToUpper())
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
                        var check_if_out_before = _context.DMRPRRP
                            .Where(a => a.productcode.Trim() == pickedListData.productcode.Trim() 
                            && a.batch.Trim() == pickedListData.batchno.Trim() 
                            && a.boxno.Trim() == pickedListData.BoxNo.Trim()
                            && a.ordertype.Trim() == pickedListData.ordertype.Trim()
                            && a.statusflag.Trim() == "LD"
                            && a.pickflag == 1)
                            .FirstOrDefault();

                        var foundinstorage_also = _context.Storage_Operation
                            .Where(a => a.productcode.Trim() == pickedListData.productcode.Trim()
                            && a.batchcode.Trim() == pickedListData.batchno.Trim()
                            && a.boxno.Trim() == pickedListData.BoxNo.Trim()
                            && a.statusflag.Trim() == "ST")
                            .FirstOrDefault();
                        if(check_if_out_before!=null && foundinstorage_also != null)
                        {
                            return Json(new { success = false, message = "Scan shipper is alredy in stock !" });
                        }
                        else
                        {
                            if (check_if_out_before == null)
                            {

                                return Json(new { success = false, message = "The scanned product shipper does not match the expected product shipper !" });

                                //int maxId = _context.DMRPRRP.Any() ? _context.DMRPRRP.Max(e => e.id) + 1 : 1;

                                //DMRPRRP st = new DMRPRRP()
                                //{
                                //    id = maxId,
                                //    productcode = pickedListData.productcode,
                                //    grn = pickedListData.grnno,
                                //    batch = pickedListData.batchno,
                                //    boxno = pickedListData.BoxNo,
                                //    refno = "-",
                                //    ordertype = pickedListData.ordertype,
                                //    inout = 1,
                                //    pickflag = 0,
                                //    location = "TMP",
                                //    type = "DM",
                                //    statusflag = "ST",
                                //    date = DateTime.Now.ToString("yyyy-MM-dd"),
                                //    time = DateTime.Now.ToString("HH:mm:ss"),
                                //};
                                //DMRPRRP_List.Add(st);
                            }
                            else
                            {
                                check_if_out_before.statusflag = "ST";
                                check_if_out_before.pickflag = 0;
                                check_if_out_before.inout = 1;
                                check_if_out_before.location = check_if_out_before.location;
                                check_if_out_before.ordertype = pickedListData.ordertype;
                                check_if_out_before.type = "DM";
                                check_if_out_before.location = loc;

                                DMRPRRP_List.Add(check_if_out_before);
                            }
                        }
                    }
                    else if (pickedListData.ordertype.Trim() == "Repair")
                    {
                        //IN CASE OUT BEFORE A TIME  FROM STOCK INVENTORY (LD) AND IN AS (ST)
                        var check_if_out_before = _context.DMRPRRP
                            .Where(a => a.productcode.Trim() == pickedListData.productcode.Trim()
                            && a.batch.Trim() == pickedListData.batchno.Trim()
                            && a.boxno.Trim() == pickedListData.BoxNo.Trim()
                            && a.ordertype.Trim() == pickedListData.ordertype.Trim()
                            && a.statusflag.Trim() == "LD"
                            && a.pickflag == 1)
                            .FirstOrDefault();

                        if (check_if_out_before == null)
                        {
                            //var loc = "TMP";
                            //GET STOCK IN CASE IF STOCK IS ALEDY DISPATCH AND NOW INFOUNF AAS REPAIR 
                            var check1 = _context.Loading_Dispatch_Operation
                                          .Where(a => a.productcode.Trim() == pickedListData.productcode.Trim()
                                          && a.boxno.Trim() == pickedListData.BoxNo.Trim()
                                          && a.batchcode.Trim() == pickedListData.batchno.Trim())
                                          .FirstOrDefault();

                            var Found = _context.Storage_Operation.Where(a => a.productcode.Trim() == pickedListData.productcode.Trim()
                                        && a.batchcode.Trim() == pickedListData.batchno.Trim()
                                        && a.boxno.Trim() == pickedListData.BoxNo.Trim()
                                        && a.statusflag.Trim() == "LD")
                                .FirstOrDefault();

                            if(check1 != null)
                            {
                                if(Found!= null)
                                {
                                    loc = Found.locationcode;
                                    Remove_StockInventory.Add(Found);
                                }
                            }
                            else
                            {
                                return Json(new { success = false, message = "Please scan the correct item for repair." +
                                    " It's not found in dispatched items and is also not found in the outward repair inventory!" });
                            }

                            int maxId = _context.DMRPRRP.Any() ? _context.DMRPRRP.Max(e => e.id) + 1 : 1;

                            DMRPRRP st = new DMRPRRP()
                            {
                                id = maxId,
                                productcode = pickedListData.productcode,
                                grn = pickedListData.grnno,
                                batch = pickedListData.batchno,
                                boxno = pickedListData.BoxNo,
                                refno = "-",
                                ordertype = pickedListData.ordertype,
                                inout = 1,
                                pickflag = 0,
                                location = loc,
                                type = "RPR",
                                statusflag = "ST",
                                date = DateTime.Now.ToString("yyyy-MM-dd"),
                                time = DateTime.Now.ToString("HH:mm:ss"),
                            };
                            DMRPRRP_List.Add(st);
                        }
                        else//not null condition + add to in the stock
                        {
                            var foundinstorage = _context.Storage_Operation
                                .Where(a => a.productcode.ToUpper().Trim() == pickedListData.productcode.ToUpper().Trim()
                                && a.boxno.Trim() == pickedListData.BoxNo.Trim()
                                && a.batchcode.Trim() == pickedListData.batchno.Trim()
                                && a.grnno.Trim() == pickedListData.grnno.Trim()
                                && a.statusflag.Trim() == "ST")
                                .FirstOrDefault();

                            if (foundinstorage != null)
                            {
                                return Json(new { success = false, message = "Please scan correct shipper, Scan shipper is already in stock ! " });
                            }
                            else
                            {
                                check_if_out_before.statusflag = "ST";
                                check_if_out_before.pickflag = 0;
                                check_if_out_before.inout = 1;
                                check_if_out_before.ordertype = pickedListData.ordertype;
                                check_if_out_before.type = "RPR";
                                check_if_out_before.location = check_if_out_before.location;

                                DMRPRRP_List.Add(check_if_out_before);
                            }

                        }
                    }
                    else if (pickedListData.ordertype.Trim() == "Replacement")
                    {
                        //IN CASE REPLACEMED STOCK COME BACK AND ONE DMG IS ALREDY IN STOCK
                        var check_if_out_before = _context.Storage_Operation
                            .Where(a => a.productcode.Trim() == pickedListData.productcode.Trim()
                            && (a.statusflag.Trim() == "DMG" || a.statusflag.Trim() == "NONRPR"))
                            .FirstOrDefault();
                        //var loc = "TMP";
                        if (check_if_out_before == null)
                        {
                            //NEW STOCK FOR REPLACEMENT IN 
                            int maxId = _context.Storage_Operation.Any() ? _context.Storage_Operation.Max(e => e.id) + 1 : 1;
                            Storage_Operation st = new Storage_Operation()
                            {
                                id = maxId,
                                productcode = pickedListData.productcode,
                                batchcode = pickedListData.batchno,
                                boxno = pickedListData.BoxNo,
                                locationcode = loc,
                                statusflag = "ST",
                                grnno = pickedListData.grnno,
                                pickflag = "0",
                            };
                            Storage_OperationAdd.Add(st);
                            //END

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
                            //END
                        }
                        else
                        {
                            Storage_OperationExAdd.Add(check_if_out_before); //AS IT IS IN STORAGE + NO UPDATE 
                        }
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
                        //location = "TMP",
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
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

        //FOR SALE RETURN AND PURCHASE RETURN 
        public class PickedListData1
        {
            public string BoxNo { get; set; }
            public string batchno { get; set; }
            public string grnno { get; set; }
            public string productcode { get; set; }
            public string rproductcode { get; set; }
            public int rquantity { get; set; }
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
                    var loadingdata = _context.Loading_Dispatch_Operation
                                        .Where(a => a.boxno.Trim() == boxno.Trim()
                                        && a.batchcode.Trim() == batchno.Trim()
                                        && a.productcode.Trim().ToUpper() == productcode.ToUpper())
                                        .FirstOrDefault();

                    if (loadingdata != null)
                    {
                        var storagedata = _context.Storage_Operation
                                            .Where(a => a.productcode.Trim().ToUpper() == productcode.Trim().ToUpper()
                                            && a.boxno.Trim() == boxno.Trim()
                                            && a.batchcode.Trim() == batchno.Trim()
                                            && a.statusflag == "LD")
                                            .FirstOrDefault();

                        var loadeddata = _context.Loading_Dispatch_Operation
                                            .Where(a => a.productcode.Trim().ToUpper() == productcode.Trim().ToUpper()
                                            && a.boxno.Trim() == boxno.Trim()
                                            && a.batchcode.Trim() == batchno.Trim())
                                            .FirstOrDefault();

                       
                        if (storagedata != null)
                        {
                            location = storagedata.locationcode;
                        }
                        else
                        {
                            if (storagedata == null)
                            {
                                int maxId = _context.Storage_Operation.Any() ? _context.Storage_Operation.Max(e => e.id) + 1 : 1;

                                Storage_Operation st = new Storage_Operation()
                                {
                                    id = maxId,
                                    productcode = pickedListData.productcode,
                                    batchcode = pickedListData.batchno,
                                    boxno = pickedListData.BoxNo,
                                    locationcode = "TMP",
                                    statusflag = "LD",
                                    pickflag = "1",
                                    grnno = pickedListData.grnno,
                                };
                                _context.Storage_Operation.Add(st);
                                _context.SaveChanges();
                            }
                            location = "TMP";
                        }
                        if (storagedata != null && loadeddata != null)
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
                                statusflag = "LD",
                                pickflag = "1",
                                grnno = "-",
                            };
                            savepolist1.Add(pickedData);
                        }
                        else if (storagedata == null && loadeddata != null)
                        {
                            int maxId = 0;

                            if (savepolist1.Count == 0)
                            {
                                maxId = _context.Storage_Operation.Any() ? _context.Storage_Operation.Max(e => e.id) + 1 : 1;
                            }
                            else
                            {   //get last id from savepolist1 and next id should be the maxid
                                maxId = savepolist1.Max(e => e.id) + 1;
                            }
                            selectedProductCode = productcode;
                            selectedBoxNo = boxno;
                            selectedBatchCode = batchno;

                            var pickedData = new Storage_Operation
                            {
                                id = maxId,
                                productcode = selectedProductCode,
                                boxno = selectedBoxNo,
                                batchcode = selectedBatchCode,
                                locationcode = location,
                                statusflag = "LD",
                                pickflag = "1",
                                grnno = "TMP_SGRN",
                            };
                            savepolist1.Add(pickedData);
                            //_context.Storage_Operation.Add(pickedData);
                            //_context.SaveChanges();

                        }
                        else if (storagedata == null && loadeddata == null)
                        {
                            return Json(new { success = false, message = "Scanned box data not found in storage !" });
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "Data not found in Loading_Dispatch_Operation." });
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

        //END
        public ActionResult StockMovementList() //GetInwardDataStock
        {
            List<InStockQty> inStockQuantities = new List<InStockQty>();

            var productcodes = _context.Storage_Operation
                .Where(a => a.statusflag == "PI" || a.statusflag == "ST")
                .Select(a => a.productcode.Trim().ToUpper())
                .Distinct()
                .ToList();

            
            foreach (var b in productcodes)
            {
                var productname = _context.Product_Master
                    .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper())
                    .Select(a => a.productdescription.Trim().ToUpper())
                    .FirstOrDefault();
                var category = _context.Product_Master
                    .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper())
                    .Select(a => a.categoryname.Trim().ToUpper())
                    .FirstOrDefault();

                var storedata = _context.Storage_Operation
                    .Where(a => a.productcode.Trim().ToUpper() == b && (a.statusflag == "PI" || a.statusflag == "ST"))
                    .ToList();
                var groupedData = storedata
                    .GroupBy(q => GetSecondDigit(q.boxno))
                    .ToDictionary(group => group.Key, group => group.Count());

                var storedata1 = _context.Storage_Operation
                    .Where(a => a.productcode.Trim().ToUpper() == b && a.statusflag == "PI")
                    .ToList();
                var groupedData1 = storedata1
                    .GroupBy(q => GetSecondDigit(q.boxno))
                    .ToDictionary(group => group.Key, group => group.Count());

                int secondDigitCount = 0;
                int secondDigitCount1 = 0;
                foreach (var kvp in groupedData)
                {
                    var secondDigit = kvp.Key.Trim();
                    secondDigitCount = kvp.Value;
                }
                foreach (var kvp in groupedData1)
                {
                    var secondDigit = kvp.Key.Trim();
                    secondDigitCount1 = kvp.Value;
                }

                var instcok = new InStockQty
                {
                    productcode = b,
                    stcokallocate = secondDigitCount1,
                    currentqty = secondDigitCount,
                    productname = productname,
                    category = category,
                };

                inStockQuantities.Add(instcok);
            }

            inStockQuantities = inStockQuantities.OrderBy(item => item.id).ToList();

            return View(inStockQuantities);
        }
        public class DamageStatusModel
        {
            public string Pono { get; set; }
            public string Productcode { get; set; }
            public string Description { get; set; }
            public string Brand { get; set; }
            public string Quantity { get; set; }
            public string Setofsub_assemb { get; set; }
            public string Qtyperpkt { get; set; }
            public string Noofpackets { get; set; }
            public string Totalpacket { get; set; }
            public string Totalsubassmbly { get; set; }
            public string Uom { get; set; }
            public string Batchcode { get; set; }
        }
        
        //public IActionResult ChangeDate(DateTime selectedDate, string selectedPono)
        //{
        //    var selectdate = _context.purchase.Where(a => a.pono == selectedPono).Select(a => a.podate).FirstOrDefault();
        //    DateTime podate = DateTime.Parse(selectdate);

        //    if (selectedDate >= podate)
        //    {
        //        return Json(new { success = true, message = "Date is valid." });
        //    }
        //    else
        //    {
        //        return Json(new { success = false, message = "Please Select correct date !" });
        //    }
        //}

        public IActionResult ChangeDate(DateTime selectedDate, string selectedPono)
        {
            var selectDateResult = _context.purchase
                                        .Where(a => a.pono == selectedPono)
                                        .Select(a => a.podate)
                                        .FirstOrDefault();

            if (DateTime.TryParse(selectDateResult, out DateTime podate))
            {
                if (selectedDate >= podate && selectedDate <= DateTime.Now)
                {
                    return Json(new { success = true, message = "Date is valid." });
                }
                else
                {
                    return Json(new { success = false, message = "Please select a date between the PODate and the current date." });
                }
            }
            else
            {
                return Json(new { success = false, message = "Invalid PODate format." });
            }
        }

        [HttpGet]
        public IActionResult GeneratePono()
        {
            int nextNumber = 1;
            string seriesPrefix = "";
            var pono = "";
            //get the last pono value by spliting the value  then continue the next 
            seriesPrefix = "BR/PO/2-3/";
            var lastSono = _context.purchase
                .Where(e => e.pono.StartsWith("BR/PO/2-3/")) // Filter by prefix
                .OrderByDescending(e => e.pono)
                .Select(e => e.pono)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(lastSono))
            {
                var parts = lastSono.Split('/');
                if (parts.Length == 4 && int.TryParse(parts[3], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }
            pono = $"{seriesPrefix}{nextNumber}";
   
            // Return the generated PONO as JSON
            return Json(new { pono = pono });
        }

        private purchase GetPurchaseModel()
        {
            return new purchase { podate = DateTime.Now.ToString("yyyy-MM-dd") };
        }

        //changedate function
        //public IActionResult InStockQty()
        //{
        //    List<InStockQty> inStockQuantities = new List<InStockQty>();

        //    // ... Your existing code ...
        //    var indata = _context.inward
        //        .Where(a => a.flag == 1)
        //        .Select(a => a.inward_id)
        //        .ToList();

        //    var inPacketData = _context.inwardPacket
        //        .Where(a => indata.Contains(a.inwardId))
        //        .ToList();

        //    var productdata = _context.inwardPacket
        //        .Select(a => new { a.productcode, a.noofpackets })
        //        .Distinct()
        //        .ToList();

        //    var productcodes = _context.Storage_Operation.Select(a => a.productcode).Distinct().ToList();


        //    foreach (var b in productcodes)
        //    {
        //        // Group by ST
        //        var storedata = _context.Storage_Operation
        //            .Where(a => a.productcode == b)// && a.statusflag == "ST" && a.statusflag == "PI"
        //            .ToList();


        //        var groupedData = storedata
        //            .GroupBy(item => new { ProductCode = item.productcode, BoxNumber = item.boxno?.Split('-')[1] })
        //            .Select(group => new OverAllCount
        //            {
        //                ProductCode = group.Key.ProductCode,
        //                Boxes = group.Key.BoxNumber,
        //                IndividualCount = group.Count(),
        //                //TotalCount = physicalStockTakeData.Count() // Total count across all ProductCodes
        //                TotalCount = storedata.Count(item => item.productcode == group.Key.ProductCode),

        //            })
        //            .ToList();

        //        //productwise quantity
        //        var minIndividualCounts = groupedData
        //        .GroupBy(item => new { ProductCode = item.ProductCode, Denominator = GetDenominatorFromBoxes(item.Boxes) })
        //         .Select(group => new
        //         {
        //             ProductCode = group.Key.ProductCode,
        //             Denominator = group.Key.Denominator,
        //             MinIndividualCount = group.Min(item => item.IndividualCount)
        //         })
        //         .ToDictionary(x => new { x.ProductCode, x.Denominator }, x => x.MinIndividualCount);

        //        //var groupedData = storedata
        //        //    .GroupBy(q => GetSecondDigit(q.boxno))
        //        //    .ToDictionary(group => group.Key, group => group.Count());


        //        // Group by PI
        //        var storedata1 = _context.Storage_Operation
        //            .Where(a => a.productcode == b && a.statusflag == "PI")
        //            .ToList();

        //        //var groupedData1 = storedata1
        //        //    .GroupBy(q => GetSecondDigit(q.boxno.Trim()))
        //        //    .ToDictionary(group => group.Key, group => group.Count());

        //        var groupedData1 = storedata1
        //            .GroupBy(item => new { ProductCode = item.productcode, BoxNumber = item.boxno?.Split('-')[1] })
        //            .Select(group => new OverAllCount
        //            {
        //                ProductCode = group.Key.ProductCode,
        //                Boxes = group.Key.BoxNumber,
        //                IndividualCount = group.Count(),
        //                //TotalCount = physicalStockTakeData.Count() // Total count across all ProductCodes
        //                TotalCount = storedata.Count(item => item.productcode == group.Key.ProductCode),

        //            })
        //            .ToList();

        //        //productwise quantity
        //        var minIndividualCounts1 = groupedData1
        //        .GroupBy(item => new { ProductCode = item.ProductCode, Denominator = GetDenominatorFromBoxes(item.Boxes) })
        //         .Select(group => new
        //         {
        //             ProductCode = group.Key.ProductCode,
        //             Denominator = group.Key.Denominator,
        //             MinIndividualCount = group.Min(item => item.IndividualCount)
        //         })
        //         .ToDictionary(x => new { x.ProductCode, x.Denominator }, x => x.MinIndividualCount);


        //        int secondDigitCount = 0;
        //        int secondDigitCount1 = 0;
        //        int currentqtycount = 0;

        //        foreach (var kvp in minIndividualCounts)
        //        {
        //            //var secondDigit = kvp.Key.Trim();
        //            var secondDigit = kvp.Key;
        //            secondDigitCount = kvp.Value;

        //            currentqtycount += secondDigitCount;
        //        }

        //        foreach (var kvp in minIndividualCounts1)
        //        {
        //            //var secondDigit = kvp.Key.Trim();
        //            var secondDigit = kvp.Key;
        //            secondDigitCount1 = kvp.Value;
        //        }

        //        var instcok = new InStockQty
        //        {
        //            productcode = b,
        //            stcokallocate = secondDigitCount1,
        //            //currentqty = secondDigitCount,
        //            currentqty = currentqtycount,
        //        };

        //        inStockQuantities.Add(instcok);
        //    }

        //    return View(inStockQuantities);
        //}

        private string GetDenominatorFromBoxes(string boxes)
        {
            // Assuming the format is always "n/m"
            string[] parts = boxes?.Split('/');

            return parts?.Length == 2 ? parts[1] : null;
        }

        //public IActionResult InStockQty()
        //{
        //    List<InStockQty> inStockQuantities = new List<InStockQty>();

        //    var indata = _context.inward
        //        .Where(a => a.flag == 1)
        //        .Select(a => a.inward_id)
        //        .ToList();

        //    var inPacketData = _context.inwardPacket
        //        .Where(a => indata.Contains(a.inwardId))
        //        .ToList();

        //    var productcodes = _context.Storage_Operation
        //        .Where(a => a.statusflag == "PI" || a.statusflag == "ST")
        //        .Select(a => a.productcode.Trim().ToUpper())
        //        .Distinct()
        //        .ToList();

        //    var productmasterdata = _context.Product_Master
        //        .Where(a => !productcodes.Contains(a.productcode.Trim().ToUpper()))
        //        .ToList();

        //    foreach (var item in productmasterdata)
        //    {
        //        var instcok1 = new InStockQty
        //        {
        //            productcode = item.productcode.Trim().ToUpper(),
        //            stcokallocate = 0,
        //            currentqty = 0,
        //            productname = item.productdescription.Trim().ToUpper(),
        //            category = item.categoryname.Trim(),
        //        };
        //        inStockQuantities.Add(instcok1);
        //    }

        //    foreach (var b in productcodes)
        //    {
        //        var productname = _context.Product_Master
        //            .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper())
        //            .Select(a => a.productdescription.Trim().ToUpper())
        //            .FirstOrDefault();
        //        var category = _context.Product_Master
        //            .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper())
        //            .Select(a => a.categoryname.Trim().ToUpper())
        //            .FirstOrDefault();

        //        var storedata = _context.Storage_Operation
        //            .Where(a => a.productcode.Trim().ToUpper() == b && (a.statusflag == "PI" || a.statusflag == "ST"))
        //            .ToList();
        //        var groupedData = storedata
        //            .GroupBy(q => GetSecondDigit(q.boxno))
        //            .ToDictionary(group => group.Key, group => group.Count());

        //        var storedata1 = _context.Storage_Operation
        //            .Where(a => a.productcode.Trim().ToUpper() == b && a.statusflag == "PI")
        //            .ToList();
        //        var groupedData1 = storedata1
        //            .GroupBy(q => GetSecondDigit(q.boxno))
        //            .ToDictionary(group => group.Key, group => group.Count());

        //        int secondDigitCount = 0;
        //        int secondDigitCount1 = 0;
        //        foreach (var kvp in groupedData)
        //        {
        //            var secondDigit = kvp.Key.Trim();
        //            secondDigitCount = kvp.Value;
        //        }
        //        foreach (var kvp in groupedData1)
        //        {
        //            var secondDigit = kvp.Key.Trim();
        //            secondDigitCount1 = kvp.Value;
        //        }

        //        var instcok = new InStockQty
        //        {
        //            productcode = b,
        //            stcokallocate = secondDigitCount1,
        //            currentqty = secondDigitCount,
        //            productname = productname,
        //            category = category,
        //        };

        //        inStockQuantities.Add(instcok);
        //    }

        //    inStockQuantities = inStockQuantities.OrderBy(item => item.id).ToList();

        //    return View(inStockQuantities);
        //}
        private int GetTotalBoxes(string boxno)
        {
            var dashParts = boxno.Split('-');
            if (dashParts.Length == 2)
            {
                var slashParts = dashParts[1].Split('/');
                if (slashParts.Length == 2 && int.TryParse(slashParts[1].Trim(), out int totalBoxes))
                {
                    return totalBoxes;
                }
            }
            return 0; // Or handle error case appropriately
        }

        private int GetBoxIndex(string boxno)
        {
            var dashParts = boxno.Split('-');
            if (dashParts.Length == 2)
            {
                var slashParts = dashParts[1].Split('/');
                if (slashParts.Length == 2 && int.TryParse(slashParts[0].Trim(), out int boxIndex))
                {
                    return boxIndex;
                }
            }
            return 0; // Or handle error case appropriately
        }

        private List<string> GetPossibleBoxes(int totalBoxes)
        {
            var boxes = new List<string>();
            for (int i = 1; i <= totalBoxes; i++)
            {
                boxes.Add($"{i}/{totalBoxes}");
            }
            return boxes;
        }
        private int GetSpliBox(string boxno)
        {
            var dashParts = boxno.Split('-');
            if (dashParts.Length == 2)
            {
                var slashParts = dashParts[1].Split('/');
                if (slashParts.Length == 2 && int.TryParse(slashParts[1].Trim(), out int totalBoxes))
                {
                    return totalBoxes;
                }
            }
            return 0; // Or handle error case appropriately
        }
        public IActionResult InStockQty()
        {
            List<InStockQty> inStockQuantities = new List<InStockQty>();

            var indata = _context.inward
                .Where(a => a.flag == 1)
                .Select(a => a.inward_id)
                .ToList();

            var inPacketData = _context.inwardPacket
                .Where(a => indata.Contains(a.inwardId))
                .ToList();

            var productcodes = _context.Storage_Operation
                .Where(a => a.statusflag == "PI" || a.statusflag == "ST")
                .Select(a => a.productcode.Trim().ToUpper())
                .Distinct()
                .ToList();

            foreach (var b in productcodes)
            {
                var productMaster = _context.Product_Master
                    .FirstOrDefault(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper());

                if (productMaster != null)
                {
                    var productName = productMaster.productdescription.Trim().ToUpper();
                    var category = productMaster.categoryname.Trim().ToUpper();

                    var storeData = _context.Storage_Operation
                        .Where(a => a.productcode.Trim().ToUpper() == b && (a.statusflag == "PI" || a.statusflag == "ST"))
                        .ToList();

                    var result = new List<KeyValuePair<string, int>>();
                    int minCount = int.MaxValue;

                    var box = storeData.Select(a => a.boxno.Trim()).FirstOrDefault();
                    var splitbox = GetSpliBox(box);

                    var groupedData = storeData
                        .GroupBy(q => GetSecondDigit(q.boxno))
                        .ToDictionary(group => group.Key, group => group.Count());
                    var possibleBoxes = new List<string>();
                    if (groupedData.Count == 0)
                    {
                        minCount = 0;
                    }
                    else
                    {
                        possibleBoxes = GetPossibleBoxes(splitbox);
                    }
                    foreach (var item in possibleBoxes)
                    {
                        int count = 0;
                        foreach (var kvp in groupedData)
                        {
                            if (kvp.Key.Contains(item))
                            {
                                count = kvp.Value;
                                break;
                            }
                        }
                        result.Add(new KeyValuePair<string, int>(item, count));
                    }
                    if (result.Count > 0)
                    {
                        minCount = result.Min(kvp => kvp.Value);
                    }
                    else
                    {
                        minCount = 0;
                    }

                    var instcok = new InStockQty
                    {
                        productcode = b,
                        stcokallocate = storeData.Count(a => a.statusflag == "PI"),
                        currentqty = minCount,
                        productname = productName,
                        category = category,
                    };

                    inStockQuantities.Add(instcok);
                }
            }

            inStockQuantities = inStockQuantities.OrderBy(item => item.id).ToList();

            return View(inStockQuantities);
        }

        //public IActionResult InStockQty()
        //{
        //    List<InStockQty> inStockQuantities = new List<InStockQty>();

        //    var indata = _context.inward
        //        .Where(a => a.flag == 1)
        //        .Select(a => a.inward_id)
        //        .ToList();

        //    var inPacketData = _context.inwardPacket
        //        .Where(a => indata.Contains(a.inwardId))
        //        .ToList();

        //    var productcodes = _context.Storage_Operation
        //        .Where(a => a.statusflag == "PI" || a.statusflag == "ST")
        //        .Select(a => a.productcode.Trim().ToUpper())
        //        .Distinct()
        //        .ToList();


        //    foreach (var b in productcodes)
        //    {
        //        var productMaster = _context.Product_Master
        //            .FirstOrDefault(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper());

        //        if (productMaster != null)
        //        {
        //            var productName = productMaster.productdescription.Trim().ToUpper();
        //            var category = productMaster.categoryname.Trim().ToUpper();

        //            var storeData = _context.Storage_Operation
        //                .Where(a => a.productcode.Trim().ToUpper() == b && (a.statusflag == "PI" || a.statusflag == "ST"))
        //                .ToList();

        //            var groupedData = storeData
        //                .GroupBy(q => GetSecondDigit(q.boxno))
        //                .ToDictionary(group => group.Key, group => group.Count());

        //            var storeDataPI = _context.Storage_Operation
        //                .Where(a => a.productcode.Trim().ToUpper() == b && a.statusflag == "PI")
        //                .ToList();

        //            var groupedDataPI = storeDataPI
        //                .GroupBy(q => GetSecondDigit(q.boxno))
        //                .ToDictionary(group => group.Key, group => group.Count());

        //            int minCount = int.MaxValue; // Initialize with a large number for finding minimum

        //            int secondDigitCount = 0;
        //            int secondDigitCountPI = 0;


        //            if (groupedData.Count == 0)
        //            {
        //                minCount = 0;
        //            }
        //            else
        //            {
        //                // Find minimum count from groupedData
        //                foreach (var kvp in groupedData)
        //                {
        //                    var count = kvp.Value;
        //                    if (count < minCount)
        //                    {
        //                        minCount = count;
        //                    }
        //                }
        //            }



        //            // Use the minimum count as currentqty
        //            var instcok = new InStockQty
        //            {
        //                productcode = b,
        //                stcokallocate = secondDigitCountPI,
        //                currentqty = minCount, // Assign minimum count as currentqty
        //                productname = productName,
        //                category = category,
        //            };

        //            inStockQuantities.Add(instcok);
        //        }
        //    }

        //    inStockQuantities = inStockQuantities.OrderBy(item => item.id).ToList();

        //    return View(inStockQuantities);


        //    inStockQuantities = inStockQuantities.OrderBy(item => item.id).ToList();

        //    return View(inStockQuantities);
        //}
        public ActionResult checkquantityifgreter(string selectedValue, string selectedValue1, string selectedValue2)
        {
            var readpurchase = _context.purchase.FirstOrDefault(po => po.pono == selectedValue2);
            if (readpurchase != null)
            {
                var readexistpacket = _context.poProduct_details.Where(a => a.porderid == readpurchase.id && a.productcode.Trim().ToUpper() == selectedValue.Trim().ToUpper()).FirstOrDefault();

                var existingpacket = _context.inwardPacket.Where(a => a.pono == selectedValue2 && a.productcode.ToUpper() == selectedValue.ToUpper())
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




            //if (inwardOrder != null)
            //{
            //    var productDetail = _context.poProduct_details
            //        .FirstOrDefault(pd => pd.porderid == inwardOrder.id && pd.productcode == selectedValue);
            //    if (productDetail != null)
            //    {
            //        int selectedQuantity;
            //        if (int.TryParse(selectedValue1, out selectedQuantity))
            //        {
            //            if (productDetail.quantity < selectedQuantity)
            //            {
            //                return Json("QuantityGreterError");
            //            }
            //        }
            //        else
            //        {
            //            return BadRequest("Invalid quantity format");
            //        }
            //    }
            //    else
            //    {
            //        return NotFound("Product detail not found");
            //    }
            //}

        }
        public IActionResult callAgain(string product,string qty,string sono)
        {
            var data = SHPlist.Where(a => a.productcode.Trim() == product.Trim() && a.pono.Trim() == sono.Trim()).ToList();
            if (data.Count > 0)
            {
                return Ok(new { data = data });
            }
            else
            {
                return Ok();
            }
            return View();
        }
        [HttpGet]
        public ActionResult GETGRNDIN_CODE()
        {
            // Generate your GRN number logic here, similar to how you did for the product code
            int maxId = _context.inward.Any() ? _context.inward.Max(e => e.inward_id) + 1 : 1;
            string grnCode = "D/IN/EROS-W1-" + maxId.ToString("D6"); // Assuming a fixed length of 6 digits

            return Json(grnCode);
        }
        [HttpGet]
        public ActionResult GETGRNRIN_CODE()
        {
            // Generate your GRN number logic here, similar to how you did for the product code
            int maxId = _context.inward.Any() ? _context.inward.Max(e => e.inward_id) + 1 : 1;
            string grnCode = "R/IN/EROS-W1-" + maxId.ToString("D6"); // Assuming a fixed length of 6 digits

            return Json(grnCode);
        }
        [HttpGet]
        public ActionResult GETGRNRPIN_CODE()
        {
            // Generate your GRN number logic here, similar to how you did for the product code
            int maxId = _context.inward.Any() ? _context.inward.Max(e => e.inward_id) + 1 : 1;
            string grnCode = "RP/IN/EROS-W1-" + maxId.ToString("D6"); // Assuming a fixed length of 6 digits
            return Json(grnCode);
        }
        [HttpGet]
        public ActionResult GETGRNP_CODE()
        {
            int nextNumber = 1;
            string seriesPrefix = "HG/GRN-";
            string grnData = "";

            var lastGrn = _context.inward
                .Where(e => e.grnno.StartsWith(seriesPrefix))
                .OrderByDescending(e => e.inward_id)
                .Select(e => e.grnno)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(lastGrn))
            {
                var parts = lastGrn.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[1], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }
            var maxId = _context.inward.Any() ? _context.inward.Max(e => e.inward_id) + 1 : 1;
            grnData = $"{seriesPrefix}{maxId}";
            return Content(grnData);

            //// Generate your GRN number logic here, similar to how you did for the product code
            //int maxId = _context.inward.Any() ? _context.inward.Max(e => e.inward_id) + 1 : 1;
            //string grnCode = "P/IN/EROS-W1-" + maxId.ToString("D6"); // Assuming a fixed length of 6 digits
            //return Json(grnCode);
        }
        [HttpGet]
        public ActionResult GETGRNPRR_CODE()
        {
            // Generate your GRN number logic here, similar to how you did for the product code
            int maxId = _context.inward.Any() ? _context.inward.Max(e => e.inward_id) + 1 : 1;
            string grnCode = "PR/OUT/EROS-W1-" + maxId.ToString("D6"); // Assuming a fixed length of 6 digits

            return Json(grnCode);
        }
        [HttpGet]
        public ActionResult GETGRNSR_CODE()
        {
            // Generate your GRN number logic here, similar to how you did for the product code
            int maxId = _context.inward.Any() ? _context.inward.Max(e => e.inward_id) + 1 : 1;
            string grnCode = "SR/IN/EROS-W1-" + maxId.ToString("D6"); // Assuming a fixed length of 6 digits
            return Json(grnCode);
        }
        [HttpGet]
        public ActionResult GETGRNDO_CODE()
        {
            // Generate your GRN number logic here, similar to how you did for the product code
            int maxId = _context.inward.Any() ? _context.inward.Max(e => e.inward_id) + 1 : 1;
            string grnCode = "D/OUT/EROS-W1-" + maxId.ToString("D6"); // Assuming a fixed length of 6 digits

            return Json(grnCode);
        }
        [HttpGet]
        public ActionResult GETGRNRO_CODE()
        {
            // Generate your GRN number logic here, similar to how you did for the product code
            int maxId = _context.inward.Any() ? _context.inward.Max(e => e.inward_id) + 1 : 1;
            string grnCode = "R/OUT/EROS-W1-" + maxId.ToString("D6"); // Assuming a fixed length of 6 digits

            return Json(grnCode);
        }
        [HttpGet]
        public ActionResult GETGRNRPO_CODE()
        {
            // Generate your GRN number logic here, similar to how you did for the product code
            int maxId = _context.inward.Any() ? _context.inward.Max(e => e.inward_id) + 1 : 1;
            string grnCode = "RP/OUT/EROS-W1-" + maxId.ToString("D6"); // Assuming a fixed length of 6 digits

            return Json(grnCode);
        }
        private List<SelectListItem> GetRefNo(string ordertype)
        {
            var lstProducts = new List<SelectListItem>();
            //var list =  _context.inward.Where(a => a.ordertype.Trim() == ordertype.Trim()).GroupBy(a=>a.sono.Trim()).ToList();
            // Fetch the data from the database and move it to memory using .ToList()
            lstProducts = _context.inward
                .Where(a => a.status == "Pending" && a.ordertype.Trim() == ordertype.Trim() && a.flag == 2)
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
            // Add an option at the last position for adding a new Ref ID
            var addNewItem = new SelectListItem()
            {
                Value = "add_newRefId",  // This value can be checked when selected
                Text = "--- Generate New Ref ID ---"
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
                ViewBag.sono = GetSONO();
                ViewBag.description = Getdescription();
                ViewBag.GetRefNo = GetRefNo(nav);
                inward applicant = new inward();
                applicant.ordertype = "Demo";
                ViewBag.ordertype = applicant.ordertype;
                applicant.inwardPacket.Add(new inwardPacket() { id = 1 });
                return View("DemoView", applicant);
            }
            else if (nav == "Repair")
            {
                ViewBag.partyname = Getpartyname();
                ViewBag.pono = GetPONO();
                ViewBag.sono = GetSONO();
                ViewBag.description = Getdescription();
                ViewBag.GetRefNo = GetRefNo(nav);
                inward applicant = new inward();
                applicant.ordertype = "Repair"; ViewBag.ordertype = applicant.ordertype;
                applicant.inwardPacket.Add(new inwardPacket() { id = 1 });
                return View("Repair", applicant);
            }
            else if (nav == "Replacement")
            {
                ViewBag.partyname = Getpartyname();
                ViewBag.pono = GetPONO();
                ViewBag.sono = GetSONO();
                ViewBag.description = Getdescription();
                ViewBag.GetRefNo = GetRefNo(nav);
                inward applicant = new inward();
                applicant.ordertype = "Replacement"; ViewBag.ordertype = applicant.ordertype;
                applicant.inwardPacket.Add(new inwardPacket() { id = 1 });
                return View("Replacement", applicant);
            }
            else if (nav == "Purchase")
            {
                ViewBag.partyname = Getpartyname();
                ViewBag.pono = GetPONO();
                ViewBag.sono = GetSONO();
                ViewBag.description = Getdescription();

                inward applicant = new inward();
                applicant.ordertype = "Purchase"; ViewBag.ordertype = applicant.ordertype;
                applicant.inwardPacket.Add(new inwardPacket() { id = 1 });
                return View("Purchase", applicant);
            }
            //else if (nav == "Salereturn")
            //{
            //    ViewBag.partyname = Getpartyname();
            //    ViewBag.pono = GetPONO();
            //    ViewBag.sono = GetSONO();
            //    ViewBag.description = Getdescription();

            //    inward applicant = new inward();
            //    applicant.ordertype = "Salereturn";
            //    applicant.inwardPacket.Add(new inwardPacket() { id = 1 });
            //    return View("Salereturn", applicant);
            //}
            else
            {
                ViewBag.partyname = Getpartyname();
                ViewBag.pono = GetPONO();
                ViewBag.sono = GetSONO();
                ViewBag.description = Getdescription();

                inward applicant = new inward();
                applicant.ordertype = "Salereturn"; ViewBag.ordertype = applicant.ordertype;
                applicant.inwardPacket.Add(new inwardPacket() { id = 1 });
                return View("SaleReturn", applicant);
            }

        }
        public IActionResult GetInwardData(int inoutType, string ordertype, string vendername, string status)
        {
            List<inward> pendingOrders = new List<inward>();

            if (inoutType == 1 || inoutType == 2)
            {
                pendingOrders = _context.inward.ToList();

                if (inoutType == 1)
                {
                    //pendingOrders = _context.inward
                    //.Where(po => po.flag == 1 &&
                    //(po.ordertype == "Demo" || po.ordertype == "Repair" || po.ordertype == "Replacement") && !(po.ordertype == "Purchase" || po.ordertype == "Salereturn"))
                    //.AsNoTracking()
                    //.ToList();

                    //pendingOrders = _context.inward
                    //.Where(po => po.flag == 1 && po.ordertype == "Demo" || po.ordertype == "Repair" || po.ordertype == "Replacement").ToList();

                    pendingOrders = _context.inward
                    .Where(po => po.flag == 1 && (po.ordertype == "Demo" || po.ordertype == "Repair" || po.ordertype == "Replacement"))
                    .Include(po => po.inwardPacket)
                    .ToList();

                    if (ordertype != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 1 && po.ordertype == ordertype)
                        .AsNoTracking()
                        .ToList();
                    }
                    if (vendername != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 1 && po.vendername == vendername)
                        .AsNoTracking()
                        .ToList();
                    }
                    if (status != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 1 && po.status == status)
                        .AsNoTracking()
                        .ToList();
                    }
                    if (ordertype != null && vendername != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 1 && po.ordertype == ordertype && po.vendername == vendername)
                        .AsNoTracking()
                        .ToList();
                    }
                    if (ordertype != null && status != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 1 && po.ordertype == ordertype && po.status == status && po.status != "All")
                        .AsNoTracking()
                        .ToList();
                    }
                    if (vendername != null && ordertype != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 1 && po.vendername == vendername && po.ordertype == ordertype)
                        .AsNoTracking()
                        .ToList();
                    }
                    if (vendername != null && status != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 1 && po.vendername == vendername && po.status == status && po.status != "All")
                        .AsNoTracking()
                        .ToList();
                    }
                    if (status != null && ordertype != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 1 && po.status == status && po.status != "All" && po.ordertype == ordertype)
                        .AsNoTracking()
                        .ToList();
                    }
                    if (status != null && vendername != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 1 && po.status == status && po.status != "All" && po.vendername == vendername)
                        .AsNoTracking()
                        .ToList();
                    }
                    if (ordertype != null && status != null && vendername != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 1 && po.ordertype == ordertype && po.status != "All" && po.vendername == vendername)
                        .AsNoTracking()
                        .ToList();
                    }
                    if (ordertype != null && vendername != null && status != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 1 && po.ordertype == ordertype && po.vendername == vendername && po.status != "All")
                        .AsNoTracking()
                        .ToList();
                    }
                    if (status == "All" && ordertype == null && vendername == null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 1)
                        .AsNoTracking()
                        .ToList();
                    }
                    if (status == "All" && ordertype == null && vendername != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 1 && po.status != "All" && po.vendername == vendername)
                        .AsNoTracking()
                        .ToList();
                    }
                    if (status == "All" && ordertype != null && vendername == null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 1 && po.status != "All" && po.ordertype == ordertype)
                        .AsNoTracking()
                        .ToList();
                    }
                    if (status == "All" && ordertype != null && vendername != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 1 && po.status != "All" && po.vendername == vendername && po.ordertype == ordertype)
                        .AsNoTracking()
                        .ToList();
                    }
                }
                else if (inoutType == 2)
                {

                    //pendingOrders = _context.inward
                    //.Where(po => po.flag == 2 &&
                    //(po.ordertype == "Demo" || po.ordertype == "Repair" || po.ordertype == "Replacement") && !(po.ordertype == "Sales" || po.ordertype == "Purchasereturn"))
                    //.AsNoTracking()
                    //.ToList();

                    //pendingOrders = _context.inward
                    //.Where(po => po.flag == 2 && po.ordertype == "Demo" || po.ordertype == "Repair" || po.ordertype == "Replacement").ToList();

                    pendingOrders = _context.inward
    .Where(po => po.flag == 2 && (po.ordertype == "Demo" || po.ordertype == "Repair" || po.ordertype == "Replacement"))
    .ToList();


                    if (ordertype != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 2 && po.ordertype == ordertype)
                        .AsNoTracking()
                        .ToList();
                    }
                    if (vendername != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 2 && po.vendername == vendername)
                        .AsNoTracking()
                        .ToList();
                    }
                    if (status != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 2 && po.status == status)
                        .AsNoTracking()
                        .ToList();
                    }
                    if (ordertype != null && vendername != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 2 && po.ordertype == ordertype && po.vendername == vendername)
                        .AsNoTracking()
                        .ToList();
                    }
                    if (ordertype != null && status != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 2 && po.ordertype == ordertype && po.status == status && po.status != "All")
                        .AsNoTracking()
                        .ToList();
                    }
                    if (vendername != null && ordertype != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 2 && po.vendername == vendername && po.ordertype == ordertype)
                        .AsNoTracking()
                        .ToList();
                    }
                    if (vendername != null && status != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 2 && po.vendername == vendername && po.status == status && po.status != "All")
                        .AsNoTracking()
                        .ToList();
                    }
                    if (status != null && ordertype != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 2 && po.status == status && po.status != "All" && po.ordertype == ordertype)
                        .AsNoTracking()
                        .ToList();
                    }
                    if (status != null && vendername != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 2 && po.status == status && po.status != "All" && po.vendername == vendername)
                        .AsNoTracking()
                        .ToList();
                    }
                    if (ordertype != null && status != null && vendername != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 2 && po.ordertype == ordertype && po.status != "All" && po.vendername == vendername)
                        .AsNoTracking()
                        .ToList();
                    }
                    if (ordertype != null && vendername != null && status != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 2 && po.ordertype == ordertype && po.vendername == vendername && po.status != "All")
                        .AsNoTracking()
                        .ToList();
                    }
                    if (status == "All" && ordertype == null && vendername == null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 2)
                        .AsNoTracking()
                        .ToList();
                    }
                    if (status == "All" && ordertype == null && vendername != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 2 && po.status != "All" && po.vendername == vendername)
                        .AsNoTracking()
                        .ToList();
                    }
                    if (status == "All" && ordertype != null && vendername == null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 2 && po.status != "All" && po.ordertype == ordertype)
                        .AsNoTracking()
                        .ToList();
                    }
                    if (status == "All" && ordertype != null && vendername != null)
                    {
                        pendingOrders = _context.inward
                        .Where(po => po.flag == 2 && po.status != "All" && po.vendername == vendername && po.ordertype == ordertype)
                        .AsNoTracking()
                        .ToList();
                    }
                }

                pendingOrders = pendingOrders.ToList();
            }
            //pendingOrders = _context.inward
            //        .AsNoTracking()
            //        .ToList();
            return PartialView("_inwardView", pendingOrders);

        }
        public ActionResult InwardPendingList()
        {
            inward inward = new inward();


            var q1 = _context.inward.ToList(); // Get a list of pending purchase orders

            List<inward> intList = new List<inward>(); // Create an empty list



            return View("InwardPendingList", intList);
        }   // go to then GetInwardData
        public IActionResult CheckCategoryName(string description)
        {
            var inward = new inward();
            var product = _context.Product_Master
                .Include(e => e.Productmaster_Packets)
                .FirstOrDefault(p => p.productdescription == description);

            if (product != null && product.categoryname == "Chairs")
            {
                var packets = product.Productmaster_Packets;

                if (packets != null && packets.Any())
                {
                    int i = 0;
                    foreach (var packet in packets)
                    {
                        i++;
                        inward.inwardPacket.Add(new inwardPacket()
                        {
                            id = i,
                            //subcomponents = packet.subcomponents,
                            //suom = packet.uom,
                            //sqty = packet.qty,
                            // Map other properties from productmaster_packet to inwardPacket
                        });
                    }
                    return PartialView("_partialCategoryChair_in", inward);
                }
            }

            // If the category is not "Chairs" or no packets are available, you can return an empty partial view or handle it as per your requirements.
            // Replace "YourEmptyView" with the actual name of your empty view.
            return Ok();
        }
        public IActionResult ActionName_description(string selectedValue)
        {
            if (selectedValue != null)
            {
                var product = _context.Product_Master
                .Include(e => e.Productmaster_Packets)
                .FirstOrDefault(p => p.productdescription.ToUpper() == selectedValue.ToUpper());
                if (product != null)
                {
                    var result = new
                    {
                        modelno = product.productcode.ToUpper(),
                        uom = product.uom,
                        brand = product.brand,
                        hsncode = product.hsncode,
                        categoryname = product.categoryname,

                        // Include other properties as needed
                    };
                    return Json(result);
                }
            }
            
            return Ok();
        }
        private List<SelectListItem> Getdescription()
        {
            var lstProducts = new List<SelectListItem>();
            lstProducts = _context.Product_Master.AsNoTracking().OrderBy(a=>a.productdescription).Select(n =>
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
        public inwardsController(ErosDbContext context, INotyfService notyfService, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _notyfService = notyfService;
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpGet]
        public ActionResult Changepono(string selectedValue)
        {
            if (selectedValue == "Supplier")
            {
                //List<SelectListItem> wbridge = _context.purchase.AsNoTracking()
                //                  .OrderBy(n => n.pono)
                //                      .Select(n =>
                //                      new SelectListItem
                //                      {
                //                          Selected = true,
                //                          Value = n.pono,
                //                          Text = n.pono.ToString()
                //                      }).ToList();
                ////return Json(Ok());
                //return Json(wbridge);
                //List<SelectListItem> wbridge = _context.purchase.Where(a=>a.status=="Pending").AsNoTracking().OrderBy(n => n.pono).Select(n => new SelectListItem { 
                List<SelectListItem> wbridge = _context.purchase.Where(a => a.status == "Pending").AsNoTracking().OrderBy(n => n.pono).Select(n => new SelectListItem
                {
                    Selected = true,
                    Value = n.pono,
                    Text = n.pono.ToString()
                }).ToList();
                return Json(wbridge);
            }
            else
            {
                return Ok();
            }
        }

        public class TableDataItem
        {
            public string Product { get; set; }
            public string Qty { get; set; }
        }

        [HttpPost]
        public ActionResult CreateStickers(string product, string qty, List<TableDataItem> tableData, string sono)
        {
            try
            {
                if (tableData.Any(a => a.Qty.Contains("Quantities")))
                {
                    return Json(new { success = false, message = " If you want to clear the data and generate again , then enter the generate shipper value !" });
                }
                var data = SHPlist.Where(a => a.productcode.Trim() == product.Trim()).ToList();
                if (data.Count > 0)
                {
                    foreach(var item in data)
                    {
                        SHPlist.Remove(item);
                    }
                }
                var getCount = tableData.Count();
                foreach (var item in tableData)
                {
                    // Create a new instance of InwardPacket
                    inwardPacket ipvar = new inwardPacket()
                    {
                        noqtypershp = Convert.ToInt32(item.Product),
                        productcode = product.Trim(),
                        quantity = Convert.ToInt32(item.Qty),
                        pono = sono.Trim(),
                    };
                    SHPlist.Add(ipvar);
                }
                return Json(new { success = true, message = "Data Save Successfully !" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult CheckQtyInList(string product, int qty, int qty1)
        {
            if (product != null && qty != null)
            {
                if (qty == 1)
                {
                    return Json(new { success = true, data = qty });
                }
                else
                {
                    return Json(new { success = true, data = qty });
                }
            }
            else
            {
                return Json(new { success = false, message = "Value found null !" });
            }
        }

        [HttpPost] //  (PONO, ORDERTYPE)
        public IActionResult ActionName1(string optionValue, string optionValue1)//pono, supplier
        {
            inward inward = new inward();

            if (optionValue1 == "Supplier")
            {

                var ponodata = _context.purchase.Where(a => a.pono == optionValue).FirstOrDefault();
                if (ponodata != null)
                {
                    inward.contactno = ponodata.contactno;
                    inward.address = ponodata.address;
                    //inward.dcno = purchaseOrder.pono;
                    inward.partyname = ponodata.suppliername;
                    inward.gstinno = ponodata.gstinno;

                    // Store pono value in ViewData
                    ViewData["PonoValue"] = ponodata.pono;

                    //get packet data of purchase by id
                    var packetdata = _context.poProduct_details.Where(a => a.porderid == ponodata.id).ToList();

                    //get subcompoentn data of purchase by id
                    //var productsc = _context.purchase_subcomponent.Where(a => a.purchaseproduct_id == ponodata.id).ToList();
                    var productsc = _context.purchase_subcomponent.Where(a => a.purchaseproduct_id == ponodata.id && a.pono == optionValue).ToList();

                    //existing pono in inward if
                    var existingpacket = _context.inwardPacket.Where(a => a.pono == optionValue)
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
                        //foreach (var mat1 in productsc.Where(pd => pd.sccode.StartsWith(mat.productcode)))
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
                                    pono = optionValue,
                                    productcode = mat.productcode.ToUpper(),
                                    description = mat.description.ToUpper(),
                                    POQty = mat.quantity,
                                    quantity = mat.quantity - newdata.TotalQuantity,
                                    uom = mat.uom,
                                    brand = mat.brand,
                                    totalsubassmbly = subassemcount * mat.quantity,
                                    //templatename = mat.templatename
                                });
                            }
                        }
                        else
                        {
                            inward.inwardPacket.Add(new inwardPacket()
                            {
                                id = i,
                                pono = optionValue,
                                productcode = mat.productcode.ToUpper(),
                                description = mat.description.ToUpper(),
                                POQty = mat.quantity,
                                uom = mat.uom,
                                brand = mat.brand,
                                totalsubassmbly = subassemcount * (mat.quantity),
                                //templatename = mat.templatename
                            });
                        }
                    }
                }
                return PartialView("_PoProductDetails", inward);
                //return PartialView("partialDetail", product1);
            }
            else
            {
                //return Json(Ok());
                return Ok();
            }
        }
        
        [HttpPost] //  (DEMO REPAIR REPLACMENT)
        public IActionResult RefNoData(string refno, string ordertype)//pono, supplier
        {
            inward inward = new inward();

            if (refno!=null)
            {

                var ponodata = _context.inward.Where(a => a.pono == refno.Trim()).OrderBy(a=>a.inward_id).FirstOrDefault();
                if (ponodata != null)
                {
                    inward.contactno = ponodata.contactno;
                    inward.address = ponodata.address;
                    inward.partyname = ponodata.partyname;
                    inward.gstinno = ponodata.gstinno;
                    inward.vendername = ponodata.vendername;
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
                        if(mat.flag == 1)
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
                        if(foundnoofpackets != null)
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
                            brand = mat.brand.ToUpper(),
                            description = mat.description.ToUpper(),
                            POQty = mat.quantity,
                            quantity = mat.quantity,
                            uom = mat.uom,
                            noofpackets = mat.noofpackets,
                            setofsub_assemb = mat.setofsub_assemb,
                            qtyperpkt = mat.qtyperpkt,
                        });
                    }
                    return Json(new { success = true, data = inward });
                    //return PartialView("_PoProductDetails", inward);

                }
                else
                {
                    return Json(new { success = false, data = "" });
                    //return PartialView("_PoProductDetails", inward=null);
                }
            }
            else
            {
                return Json(new { success = false, data = "" });
                //return Ok();
                //return PartialView("DemoDetails", inward = null);
            }
        }
        
        //DEMO - SUB COM
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
        public IActionResult _partialSubComponent2(string productCode, string dcno, string invoiceno, int quantity)
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

        //REPLACEMENT - SUB COM
        public IActionResult _partialSubComponent3(string productCode, string dcno, string invoiceno, int quantity)
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

        //SALE-RETURN - SUBCOM
        public IActionResult _partialSubComponentso(string productCode, string sono, int quantity)
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
        public IActionResult _partialSubComponent(string productCode, string pono, int quantity)
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
                                tqty = item.tqty

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
        public ActionResult GetSumSubComponent(string selectedValue, string selectedValue1, string selectedValue2) //productcode, quantity, pono
        {
            var purchase = _context.purchase.FirstOrDefault(p => p.pono == selectedValue2);

            {
                var poproductDetailsList = _context.purchase_subcomponent
                    .Where(pd => pd.purchaseproduct_id == purchase.id && pd.sccode.StartsWith(selectedValue))
                //var poproductDetailsList = _context.purchase_subcomponent
                //    .Where(pd => pd.purchaseproduct_id == purchase.id)
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
        public ActionResult GetSumSubComponentso(string selectedValue, string selectedValue1, string selectedValue2) //productcode, quantity, pono
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

        [HttpGet]
        public ActionResult Changesono(string selectedValue)
        {
            if (selectedValue == "Customer")
            {
                List<SelectListItem> wbridge = _context.so_inward.AsNoTracking().OrderBy(n => n.sono).Select(n => new SelectListItem
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
        public IActionResult SavePickingData([FromBody] PickedListData pickedListData)
        {
            try
            {
                // Access the selected columns here
                var selectedProductCode = "-";
                var selectedBoxNo = "-";
                var selectedBatchCode = "-" ;
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

                foreach(var row in savepolist1)
                {
                    var split = row.boxno.Split("-")[1];
                    if(row.batchcode == pickedListData.batchno && row.boxno == pickedListData.BoxNo && row.productcode.ToUpper() == pickedListData.productcode.ToUpper())
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
                if(counter == 0)
                {
                    var loadingdata = _context.Picking_Operation
                                        .Where(a => a.sono.Trim() == sono.Trim()
                                        && a.boxno.Trim() == boxno.Trim()
                                        && a.batchcode.Trim() == batchno.Trim() 
                                        && a.productcode.Trim().ToUpper() == productcode.ToUpper()
                                        && a.flag == 1)
                                        .FirstOrDefault();

                    if (loadingdata != null)
                    {
                        var storagedata = _context.Storage_Operation
                                            .Where(a => a.productcode.Trim().ToUpper() == productcode.Trim().ToUpper() 
                                            && a.boxno.Trim() == boxno.Trim()
                                            && a.batchcode.Trim() == batchno.Trim()
                                            && a.statusflag == "LD")
                                            .FirstOrDefault();

                        //if (storagedata == null)
                        //{
                        //    int maxId = _context.Storage_Operation.Any() ? _context.Storage_Operation.Max(e => e.id) + 1 : 1;
                        //    var grn = "-";
                        //    var loc = "-";
                        //    var pick = _context.pickstorage
                        //        .Where(a => a.productcode.Trim() == productcode.Trim()
                        //                 && a.batchcode.Trim() == batchno.Trim()
                        //                 && a.boxno.Trim() == boxno.Trim())
                        //        .FirstOrDefault();
                        //    if (pick != null)
                        //    {
                        //       loc = pick.location;
                        //    }
                        //    Storage_Operation st = new Storage_Operation()
                        //    {
                        //        id = maxId,
                        //        productcode = productcode.Trim(),
                        //        batchcode = batchno.Trim(),
                        //        boxno = boxno.Trim(),
                        //        locationcode = loc,
                        //        statusflag = "LD",
                        //        pickflag = "1",
                        //        grnno = "-",
                        //    };
                        //    _context.Storage_Operation.Add(st);
                        //    _context.SaveChanges();
                        //}

                        var loadeddata = _context.Loading_Dispatch_Operation
                                            .Where(a => a.productcode.Trim().ToUpper() == productcode.Trim().ToUpper()
                                            && a.boxno.Trim() == boxno.Trim()
                                            && a.batchcode.Trim() == batchno.Trim()
                                            && a.sono.Trim() == sono.Trim())
                                            .FirstOrDefault();

                        var pickstore = _context.pickstorage
                                            .Where(a => a.boxno.Trim() == boxno.Trim() 
                                            && batchno.Trim() == batchno.Trim() 
                                            && a.sono.Trim() == sono.Trim() 
                                            && a.productcode.Trim() == productcode.Trim())
                                            .FirstOrDefault();

                        if (pickstore != null)
                        {
                            location = pickstore.location;
                        }
                        else
                        {
                            location = "TMP";
                        }
                        if (storagedata != null && loadeddata != null)
                        {
                             selectedProductCode = productcode;
                             selectedBoxNo = boxno;
                             selectedBatchCode = batchno;

                            var pickedData = new Storage_Operation
                            {
                                productcode = selectedProductCode,
                                boxno = selectedBoxNo,
                                batchcode = selectedBatchCode,
                                locationcode = location,
                                statusflag = "LD",
                                pickflag = "1",
                                grnno = "-",
                            };
                            savepolist1.Add(pickedData);
                        }
                        else if (storagedata == null && loadeddata != null)
                        {
                            int maxId = 0;

                            if (savepolist1.Count == 0)
                            {
                                 maxId = _context.Storage_Operation.Any() ? _context.Storage_Operation.Max(e => e.id) + 1 : 1;
                            }
                            else
                            {   //get last id from savepolist1 and next id should be the maxid
                                maxId = savepolist1.Max(e => e.id) + 1;
                            }
                            selectedProductCode = productcode;
                            selectedBoxNo = boxno;
                            selectedBatchCode = batchno;
                            var grn = "-";
                            var found = _context.inward.Where(a => a.batchcode.Trim() == batchno.Trim()).Select(a => a.grnno).FirstOrDefault();
                            if (found != null)
                            {
                                grn = found;
                            }

                            var pickedData = new Storage_Operation
                            {
                                id = maxId,
                                productcode = selectedProductCode,
                                boxno = selectedBoxNo,
                                batchcode = selectedBatchCode,
                                locationcode = location,
                                statusflag = "LD",
                                pickflag = "1",
                                grnno = grn,
                            };
                            savepolist1.Add(pickedData);
                            _context.Storage_Operation.Add(pickedData);
                            _context.SaveChanges();

                        }
                        else if (storagedata == null && loadeddata == null)
                        {
                            return Json(new { success = false, message = "Scanned box data not found in storage !" });
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "Data not found in Loading_Dispatch_Operation." });
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

        public IActionResult CheckReprintData()
        {
            var check = savepolist1.Count();
            if(check > 0)
            {
                return Json(new { success = true, message = "Done" });
            }
            else
            {
                return Json(new { success = false, message = "Please save the scan boxess of returned products !" });
            }
        }

        private string GetSecondDigit11(string boxno)
        {
            string[] parts = boxno.Split('-');
            return parts.Length == 2 ? parts[1].Trim() : string.Empty;
        }

        public ActionResult FatchListData(string productcode, int quantity)
        {
            List<Loading_Dispatch_Operation> filteredList = new List<Loading_Dispatch_Operation>();

            if (savepolist1.Any())
            {
                // Filter the list based on the provided product code
                filteredList = savepolist1
                    .Where(a => a.productcode.ToUpper() == productcode.ToUpper())
                    .Select(item => new Loading_Dispatch_Operation
                    {
                        productcode = item.productcode,
                        batchcode = item.batchcode,
                        boxno = item.boxno,
                        location = item.locationcode,
                    }).ToList();
            }

            // Return the filtered list of data
            return Json(new { success = true, dataList = filteredList });
        }

        private string GetSecondDigit(string boxno)
        {
            string[] parts = boxno.Split('-');
            if (parts.Length == 2)
            {
                return parts[1];
            }
            return string.Empty;
        }

        //var groupedData = listdata.GroupBy(item => item.boxno.Split("-")[1])
        //                .Select(group => new {
        //                    BoxNo = group.Key,
        //                    Items = group.ToList()
        //                })
        //                .ToList();

        //foreach (var group in groupedData)
        //{
        //    var splitBoxNo = group.BoxNo.Split("/");
        //    if (splitBoxNo.Length >= 2) // Check if the split result contains at least two elements
        //    {
        //        if (group.Items.Count != srQty) // Checking if the count of items in the current box matches srQty
        //        {
        //            var box = splitBoxNo[1];
        //            if (Convert.ToInt32(box) != groupedData.Count) // Checking if the count of unique box numbers matches srQty
        //            {
        //                boolValue = true;
        //                return Json(new { success = false, message = "Please Scan All Boxes Then Save It!" });
        //            }
        //        }
        //    }
        //}

        [HttpPost]
        public IActionResult SaveSR_List(string productCode, int soQty,int dlQty, int srQty , string sono)
        {
            try
            {
                var listdata  = savepolist1.Where(a=>a.productcode.ToUpper() == productCode.ToUpper()).ToList();

                if (listdata != null)
                {
                    foreach (var item in listdata)
                    {
                        if (item.statusflag == "LD")
                        {
                            item.statusflag = "SR";
                            item.locationcode = "TMP";
                        }
                        else if (item.statusflag == "DMG")
                        {
                            item.statusflag = "DMG";
                        }
                    }
                    return Json(new { success = true, message = "The sale-returned list of " + productCode + " has been saved successfully. Please complete the storage process afterward!" });
                }
                else
                {
                   return Json(new { success = false, message = "Scan Data not found , Please scan the product first !" });
                }
            }
            catch (Exception ex)
            {
                // Handle any errors
                return Json(new { success = false, message = "An error occurred while saving the sale-returned list: " + ex.Message });
            }
        }


        //public IActionResult SavePickingData([FromBody] PickedListData pickedListData)
        //{
        //    try
        //    {
        //        string pattern = @"[^a-zA-Z0-9\-\/$ ]";
        //        pickedListData.batchno = Regex.Replace(pickedListData.batchno.Trim(), pattern, "");

        //        string sono = pickedListData.sono.Trim();
        //        var boxno = pickedListData.BoxNo.Trim();
        //        var splitbox = pickedListData.BoxNo.Split("-")[1];
        //        var batchno = pickedListData.batchno.Trim();
        //        var productcode = pickedListData.productcode.Trim();
        //        var rowproductcode = pickedListData.rproductcode.Trim();
        //        int rowquantity = pickedListData.rquantity;

        //        var checkinpicklist = _context.Picklist_Generation.Where(a => a.sono == sono && a.prdcode == productcode && Convert.ToInt32(a.pickingqty) >= pickedListData.rquantity).FirstOrDefault();
        //        if (checkinpicklist != null)
        //        {
        //            var pickstorage = _context.pickstorage
        //                                .Where(a => a.sono == sono &&
        //                                            a.productcode == productcode &&
        //                                            a.boxno.Contains(splitbox) &&
        //                                            a.batchcode == batchno &&
        //                                            a.flag == 1)
        //                                .Select(a => new
        //                                {
        //                                    ProductCode = a.productcode,
        //                                    BoxNo = a.boxno,
        //                                    BatchCode = a.batchcode,
        //                                    Location = a.location // Assuming location is a property in the pickstorage entity
        //                                })
        //                                .FirstOrDefault();

        //            if (pickstorage != null)
        //            {
        //                var loadingdata = _context.Loading_Dispatch_Operation.Where(a => a.sono.Trim() == sono && a.boxno.Trim() == boxno && a.batchcode.Trim() == batchno && a.productcode.Trim() == productcode).FirstOrDefault();

        //                if (loadingdata != null)
        //                {
        //                    var storagedata = _context.Storage_Operation.Where(a => a.productcode.Trim() == productcode && a.boxno.Trim() == boxno && a.batchcode.Trim() == batchno && a.locationcode.Trim() == pickstorage.Location && a.statusflag == "LD").FirstOrDefault();

        //                    if (storagedata != null)
        //                    {
        //                        // Access the selected columns here
        //                        var selectedProductCode = productcode;
        //                        var selectedBoxNo = boxno;
        //                        var selectedBatchCode = batchno;
        //                        var location = pickstorage.Location;

        //                        // Create a new instance of a class to represent your data
        //                        var pickedData = new Picking_Operation
        //                        {
        //                            productcode = selectedProductCode,
        //                            boxno = selectedBoxNo,
        //                            batchcode = selectedBatchCode,
        //                            location = location
        //                        };

        //                        //var loadinggrupdata = _context.Loading_Dispatch_Operation.Where(a => a.sono == sono && a.productcode == productcode).ToList();
        //                        //if(loadinggrupdata != null)
        //                        //{
        //                        //    foreach(var aa in loadinggrupdata)
        //                        //    {
        //                        //        var storagegrup = _context.Storage_Operation.Where(a => a.productcode == aa.productcode && a.boxno == aa.boxno && a.batchcode == aa.batchcode).ToList();
        //                        //    }
        //                        //}

        //                        // Add the new instance to the list
        //                        savepolist.Add(pickedData);
        //                        // Further processing if needed

        //                        return Json(new
        //                        {
        //                            success = true,
        //                            message = "Done !",
        //                            data = new
        //                            {
        //                                productCode = selectedProductCode,
        //                                boxNo = selectedBoxNo,
        //                                batchCode = selectedBatchCode,
        //                                location = location,
        //                            }
        //                        });
        //                    }
        //                    else
        //                    {
        //                        return Json(new { success = false, message = "Data not found in storage !" });
        //                    }
        //                }
        //                else
        //                {
        //                    return Json(new { success = false, message = "Data not found in Loading_Dispatch_Operation." });
        //                }
        //            }
        //            else
        //            {
        //                return Json(new { success = false, message = "Data not found in pickstorage." });
        //            }
        //        }
        //        else
        //        {
        //            return Json(new { success = false, message = "No matching picklist record found." });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "An error occurred: " + ex.Message });
        //    }
        //}
        public class DamageData
        {
            public string Location { get; set; }
            public string BoxNo { get; set; }
            public string BatchCode { get; set; }
            public string ProductCode { get; set; }
        }
        [HttpPost]
        public IActionResult SaleReturnDelete([FromBody] DamageData damageData)
        {
            try
            {
                // Access and process the received data
                var storage = savepolist.Where(a => a.location.Trim() == damageData.Location.Trim() && a.boxno.Trim() == damageData.BoxNo.Trim() && a.batchcode.Trim() == damageData.BatchCode.Trim() && a.productcode.Trim().ToUpper() == damageData.ProductCode.Trim().ToUpper()).FirstOrDefault();

                if(storage != null)
                {
                    savepolist.Remove(storage);
                    //_context.SaveChanges();
                }
                return Json(new { success = true, message = "Product " + storage.productcode + " of shipper " + storage.boxno + " is delete successfully  !" });

            }
            catch (Exception ex)
            {
                // Handle any errors
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
        [HttpPost]
        public IActionResult PurchaseReturnDelete([FromBody] DamageData damageData)
        {
            try
            {
                // Access and process the received data
                var storage = savepolist1.Where(a => a.locationcode.Trim() == damageData.Location.Trim() && a.boxno.Trim() == damageData.BoxNo.Trim() && a.batchcode.Trim() == damageData.BatchCode.Trim() && a.productcode.Trim().ToUpper() == damageData.ProductCode.Trim().ToUpper()).FirstOrDefault();

                if (storage != null)
                {
                    savepolist1.Remove(storage);
                    //_context.SaveChanges();
                }
                return Json(new { success = true, message = "Product " + storage.productcode + " of shipper " + storage.boxno + " is delete successfully  !" });

            }
            catch (Exception ex)
            {
                // Handle any errors
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
        [HttpPost]
        public IActionResult SaleReturnDamage([FromBody] DamageData damageData)
        {
            try
            {
                // Access and process the received data
                var storage = _context.Storage_Operation.Where(a => a.locationcode.Trim() == damageData.Location.Trim() && a.boxno.Trim() == damageData.BoxNo.Trim() && a.batchcode.Trim() == damageData.BatchCode.Trim() && a.productcode.Trim().ToUpper() == damageData.ProductCode.ToUpper().Trim() && a.statusflag == "LD").FirstOrDefault();

                if(storage != null)
                {
                    storage.statusflag = "DMG";
                    _context.Storage_Operation.Update(storage);
                    _context.SaveChanges();
                    return Json(new { success = true, message = "Product "+storage.productcode+ " of shipper "+storage.boxno+" is update as damage product !" });
                }
                else
                {
                    return Json(new { success = false, message = "Product shipper Not found in storage !" });
                }
            }
            catch (Exception ex)
            {
                // Handle any errors
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult PurchaseReturnDamage([FromBody] DamageData damageData)
        {
            try
            {
                // Access and process the received data
                var storage = _context.Storage_Operation.Where(a => a.locationcode.Trim() == damageData.Location.Trim() && a.boxno.Trim() == damageData.BoxNo.Trim() && a.batchcode.Trim() == damageData.BatchCode.Trim() && a.productcode.ToUpper().Trim() == damageData.ProductCode.Trim().ToUpper() && a.statusflag == "ST").FirstOrDefault();

                if(storage != null)
                {
                    storage.statusflag = "DMG";
                    _context.Storage_Operation.Update(storage);
                    _context.SaveChanges();
                    return Json(new { success = true, message = "Product "+storage.productcode+ " of shipper "+storage.boxno+" is update as damage product !" });
                }
                else
                {
                    return Json(new { success = false, message = "Product shipper Not found in storage !" });
                }
            }
            catch (Exception ex)
            {
                // Handle any errors
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

       [HttpPost]
        public IActionResult ActionName2(string optionValue, string optionValue1)
        {
            optionValue1.Trim();
            optionValue.Trim();
            inward inward = new inward();
            var indata = _context.Loading_Dispatch_Operation.Where(a => a.sono.Trim() == optionValue.Trim()).FirstOrDefault();
            if (optionValue1 == "Customer")
            {
                var salesorder = _context.so_inward.Where(a => a.sono == optionValue ).Include(a => a.soProduct_details).FirstOrDefault();
                var productsc = _context.so_Subcomponents.Where(a => a.soproduct_id == salesorder.id).ToList();
                var newProducts = new List<purchase_subcomponent>();

                inward.contactno = salesorder.contactno.Trim();
                inward.address = salesorder.address.Trim();
                inward.partyname = salesorder.customername.Trim();

                if (indata != null)
                {
                    inward.dcno = indata.dcno;
                    inward.dcdate = indata.dcdate;
                    inward.invoicedate = indata.invoicedate;
                    inward.invoiceno = indata.invoiceno;
                    inward.grnno = "-";
                    inward.batchcode = indata.batchcode;
                    inward.grndate = indata.currentdate;
                    // inward.remarks;
                }
                //Store pono value in ViewData
                ViewData["SonoValue"] = salesorder.sono;


                var f1 = _context.so_inwardReturn.Where(a => a.sono.Trim().StartsWith(optionValue.Trim()) && a.status.Trim() == "Return").FirstOrDefault();
                if (f1 != null)
                {
                    var FUND = _context.so_productReturn.Where(a => a.orderid == f1.id).ToList();
                    var product = _context.so_product
                                .Where(a => a.orderid == salesorder.id)
                                .AsEnumerable()
                                .Where(a => FUND.Any(f => f.productcode.Trim() == a.productcode.Trim()))
                                .ToList();

                    int i = 0;
                    foreach (var mat in product)
                    {
                        var dispatchloading = _context.Loading_Dispatch_Operation.Where(a => a.sono.Trim() == optionValue && a.productcode.Trim().ToUpper() == mat.productcode.Trim().ToUpper()).ToList();
                        var picklistgenration = _context.Picklist_Generation.Where(a => a.sono.Trim() == optionValue && a.prdcode.Trim() == mat.productcode.Trim() && Convert.ToInt32(a.pickingqty) <= mat.quantity).FirstOrDefault();
                        var purchaseinward = _context.inwardPacket.Where(a => a.productcode.Trim().ToUpper() == mat.productcode.Trim().ToUpper()).FirstOrDefault();

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
                        var fqty = FUND.Where(a => a.productcode.Trim() == mat.productcode.Trim()).Select(a => a.quantity).FirstOrDefault();
                        if (purchaseinward.setofsub_assemb == null || purchaseinward.qtyperpkt == null || purchaseinward.noofpackets == null)
                        {
                            purchaseinward.setofsub_assemb = "0";
                            purchaseinward.qtyperpkt = "0";
                            purchaseinward.noofpackets = "0";
                        }
                        inward.inwardPacket.Add(new inwardPacket()
                        {
                            id = i,
                            sono = optionValue,
                            productcode = mat.productcode,
                            description = mat.description,
                            quantity = fqty,
                            brand = mat.brand,
                            SOQty = mat.quantity,
                            DLQty = Convert.ToInt32(picklistgenration.pickingqty),
                            setofsub_assemb = purchaseinward.setofsub_assemb,
                            qtyperpkt = purchaseinward.qtyperpkt,
                            noofpackets = purchaseinward.noofpackets,
                            uom = mat.uom,
                            totalsubassmbly = subassemcount * mat.quantity,
                        });


                    }
                }
                else
                {
                    var product = _context.so_product
                        .Where(a => a.orderid == salesorder.id)
                        .ToList();

                    int i = 0;
                    foreach (var mat in product)
                    {
                        var dispatchloading = _context.Loading_Dispatch_Operation.Where(a => a.sono.Trim() == optionValue && a.productcode.Trim().ToUpper() == mat.productcode.Trim().ToUpper()).ToList();
                        var picklistgenration = _context.Picklist_Generation.Where(a => a.sono.Trim() == optionValue && a.prdcode.Trim() == mat.productcode.Trim() && Convert.ToInt32(a.pickingqty) <= mat.quantity).FirstOrDefault();
                        var purchaseinward = _context.inwardPacket.Where(a => a.productcode.Trim().ToUpper() == mat.productcode.Trim().ToUpper()).FirstOrDefault();

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
                        if (purchaseinward.setofsub_assemb == null || purchaseinward.qtyperpkt == null || purchaseinward.noofpackets == null)
                        {
                            purchaseinward.setofsub_assemb = "0";
                            purchaseinward.qtyperpkt = "0";
                            purchaseinward.noofpackets = "0";
                        }
                        inward.inwardPacket.Add(new inwardPacket()
                        {
                            id = i,
                            sono = optionValue,
                            productcode = mat.productcode,
                            description = mat.description,
                            quantity = 0,
                            brand = mat.brand,
                            SOQty = mat.quantity,
                            DLQty = Convert.ToInt32(picklistgenration.pickingqty),
                            setofsub_assemb = purchaseinward.setofsub_assemb,
                            qtyperpkt = purchaseinward.qtyperpkt,
                            noofpackets = purchaseinward.noofpackets,
                            uom = mat.uom,
                            totalsubassmbly = subassemcount * mat.quantity,
                        });


                    }
                }

                return PartialView("_SoProductDetails", inward);
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
                //return Json(Ok());
                return Ok();
            }
        }
        [HttpPost]    //forr demo, repair, replacement
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
            else if (optionValue1 == "Showroom")
            {
                var category2 = _context.Showroom_Master.Where(a => a.Showroom_name.Equals(optionValue)).FirstOrDefault();
                return Json(new { data = category2 }); // Return the data to bind to the textbox
            }
            else
            {
                //return Json(Ok());
                return Ok();
            }
        }
        public IActionResult Index()
        {

            var products = _context.inward.Where(a => a.flag == 1).OrderByDescending(a=>a.inward_id)
                 .ToList();

            return View(products);
        }
        [HttpGet]
        public IActionResult FilterOrdersData(string orderType)
        {
            if (string.IsNullOrEmpty(orderType))
            {
                return Json(new { success = false, message = "Please select the order type!" });
            }

            try
            {
                if (orderType == "Purchase")
                {
                    //var data = _context.inward
                    //    .Where(a => a.ordertype == "Purchase")
                    //    .OrderByDescending(a => a.inward_id)
                    //    .ToList();

                    //return Json(new { success = true, data = data });
                    var products = _context.inward.Where(a => a.ordertype == "Purchase").AsNoTracking().OrderByDescending(a => a.inward_id).ToList();


                    //ADDED  LOGIC
                    List<inwardPacket> ip = new List<inwardPacket>();
                    List<inward> inwardlist = new List<inward>();
                    foreach (var item in products)
                    {
                        var found = _context.inwardPacket.Where(a => a.inwardId == item.inward_id).ToList();
                        if (found.Count > 0)
                        {
                            ip.AddRange(found);
                        }
                    }
                    foreach (var item in ip)
                    {
                        var data = _context.inward.Where(a => a.pono.Trim() == item.pono.Trim() && a.flag == item.flag && a.ordertype == "Purchase").AsNoTracking().FirstOrDefault();
                        if (data != null)
                        {
                            data.productcode = item.productcode;
                            inwardlist.Add(data);
                        }
                    }
                    products.Clear();
                    products.AddRange(inwardlist);
    //                products.AddRange(
    //inwardlist.GroupBy(x => x.productcode)
    //          .Select(g => g.First())
    //          .ToList()
//);
                    return Json(new { success = true, data = products });
                }
                else if (orderType == "Demo" || orderType == "Repair" || orderType == "Replacement")
                {
                    var data = _context.inward
                        .Where(a => a.ordertype == "Demo" && a.flag == 1)
                        .OrderByDescending(a => a.inward_id )
                        .ToList();

                    return Json(new { success = true, data = data });
                }
                else if (orderType == "Sale")
                {
                    var data = _context.Loading_Dispatch_Operation
                                .Where(a => !string.IsNullOrEmpty(a.sono))  // Filter out null or empty sono values
                                .GroupBy(a => a.sono.Trim())               // Group by sono, trimming any whitespace
                                .Select(g => g.First())                    // Select the first record from each group (i.e., distinct by sono)
                                .ToList();
                    foreach(var item in data)
                    {
                        var found = _context.so_inward.Where(a => a.sono.Trim() == item.sono.Trim()).Select(a => a.customername.Trim()).FirstOrDefault();
                        if (found != null)
                        {
                            item.customer = found;
                        }
                    }

                    return Json(new { success = true, data = data });


                }
                else
                {
                    return Json(new { success = false, message = "Invalid order type selected." });
                }
            }
            catch (Exception ex)
            {
                // Log the full exception for better debugging
                return Json(new { success = false, message = "An error occurred while fetching data.", error = ex.Message });
            }
        }

        public IActionResult Index1()
        {
            var products = _context.inward.Where(a=>a.ordertype == "Purchase").AsNoTracking().OrderByDescending(a=>a.inward_id).ToList();
            

            //ADDED  LOGIC
            List<inwardPacket> ip = new List<inwardPacket>();
            List<inward> inwardlist = new List<inward>();
            foreach(var item in products)
            {
                var found = _context.inwardPacket.Where(a => a.inwardId == item.inward_id).ToList();
                if(found.Count > 0)
                {
                    ip.AddRange(found);
                }
            }
            foreach(var item in ip)
            {
                var data = _context.inward.Where(a => a.pono.Trim() == item.pono.Trim() && a.flag == item.flag).AsNoTracking().FirstOrDefault();
                if (data != null)
                {
                    data.productcode = item.productcode;
                    inwardlist.Add(data);
                }
            }
            products.Clear();
            products.AddRange(inwardlist);
    //        products.AddRange(
    //inwardlist.GroupBy(x => x.productcode)
    //          .Select(g => g.First())
    //          .ToList()
//);
            return View(products);
            //END
        }
        // GET: inwards/Create
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

        //GET beg data from vender name to get supplier, customer,showroom
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
            ////var lstProducts = new List<SelectListItem>();
            ////lstProducts = _context.purchase.Where(a=>a.status=="Pending").AsNoTracking().Select(n =>
            ////new SelectListItem
            ////{
            ////    Value = n.pono,
            ////    Text = n.pono
            ////}).ToList();

            //List<SelectListItem> wbridge = _context.purchase.Where(a => a.status == "Pending").AsNoTracking().OrderBy(n => n.pono).Select(n => new SelectListItem
            //{
            //    Selected = false, //bydefult select 
            //    Value = n.pono,
            //    Text = n.pono.ToString()
            //}).ToList();

            //var defItem = new SelectListItem()
            //{
            //    Value = "",
            //    Text = "----Select PONO ----"
            //};
            //wbridge.Insert(0, defItem);
            //return wbridge;

            ////lstProducts.Insert(0, defItem);
            ////return lstProducts;
            var lstProducts = new List<SelectListItem>();
            lstProducts = _context.purchase.Where(a => a.status == "Pending").AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.pono,
                Text = n.pono
            }).ToList();
            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select PONO----"
            };
            lstProducts.Insert(0, defItem);
            return lstProducts;
        }

        private List<SelectListItem> GetSONO()
        {
            List<SelectListItem> wbridge = _context.so_inward.Where(a => a.status == "Return").AsNoTracking().OrderBy(n => n.sono).Select(n => new SelectListItem
            {
                Selected = false,
                Value = n.sono,
                Text = n.sono.ToString()
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select SONO ----"
            };
            wbridge.Insert(0, defItem);
            return wbridge;
        }
        [HttpPost]
        public IActionResult CHECKSC(int stickerDecision, string tableData)
        {
            List<Dictionary<string, string>> data = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(tableData);

            foreach (var row in data)
            {
                var productCode = row["inwardPacket[0].productcode"];
                var description = row["inwardPacket[0].description"];

                var existingProduct = _context.Product_Master
                    .FirstOrDefault(p => p.productcode.ToUpper() == productCode.ToUpper() && p.productdescription.ToUpper() == description.ToUpper());

                if (existingProduct != null)
                {
                    var scdata = _context.productmaster_Packet.Any(a => a.productmasterId == existingProduct.id);
                    if (scdata)
                    {
                        continue;
                    }
                    else
                    {
                        return Json(new { success = false, message = "Access Denied !" });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Product not found !" });
                }
            }
            return Json(new { success = true });
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

        //[HttpPost]
        //public ActionResult CreateDMRPRRP([FromBody] MyRequestModel requestData)
        //{
        //    inward inward = new inward()
        //    {
        //        pono = requestData.Applicant.FirstOrDefault(x => x.Name == "pono")?.Value,
        //        vendername = requestData.Applicant.FirstOrDefault(x => x.Name == "VendorName")?.Value,
        //        typeofreturn = requestData.Applicant.FirstOrDefault(x => x.Name == "TypeOfReturn")?.Value,
        //        partyname = requestData.Applicant.FirstOrDefault(x => x.Name == "PartyName")?.Value,
        //        gstinno = requestData.Applicant.FirstOrDefault(x => x.Name == "GstinNo")?.Value,
        //        ordertype = requestData.Applicant.FirstOrDefault(x => x.Name == "OrderType")?.Value,
        //        contactno = requestData.Applicant.FirstOrDefault(x => x.Name == "ContactNo")?.Value,
        //        address = requestData.Applicant.FirstOrDefault(x => x.Name == "Address")?.Value,
        //        dcno = requestData.Applicant.FirstOrDefault(x => x.Name == "DcNo")?.Value,
        //        dcdate = requestData.Applicant.FirstOrDefault(x => x.Name == "DcDate")?.Value,
        //        invoiceno = requestData.Applicant.FirstOrDefault(x => x.Name == "InvoiceNo")?.Value,
        //        invoicedate = requestData.Applicant.FirstOrDefault(x => x.Name == "InvoiceDate")?.Value,
        //    };
        //    inwardPacket inwardPacket = new inward.inward
        //    // Return some response, e.g., a file or status
        //    return Ok();// Example: return a file response
        //}

        //// Model to capture incoming data
        //public class MyRequestModel
        //{
        //    public List<ApplicantModel> Applicant { get; set; }
        //    public string StickerDecision { get; set; }
        //    public List<TableDataModel> TableData { get; set; }
        //}

        //public class ApplicantModel
        //{
        //    public string Name { get; set; }
        //    public string Value { get; set; }
        //}

        //public class TableDataModel
        //{
        //    public string ProductCode { get; set; }
        //    public string Description { get; set; }
        //    public string Brand { get; set; }
        //    public int Quantity { get; set; }
        //    public string Type { get; set; }
        //    public string Uom { get; set; }
        //    public string SetOfSubAssembly { get; set; }
        //    public string NoOfShipperQty { get; set; }
        //    public string NoOfPackets { get; set; }
        //}
        public class CreateInwardRequest
        {
            public string FormData { get; set; } // This could be a JSON string or query string data
            public List<RowData> RowData { get; set; } // List of rows
        }

        public class RowData
        {
            public string ProductCode { get; set; }
            public string Description { get; set; }
            public string Brand { get; set; }
            public string Quantity { get; set; }
            public string Type { get; set; }
            public string Uom { get; set; }
            public string SetOfSubAssembly { get; set; }
            public string NoOfShipperQty { get; set; }
            public string NoOfPackets { get; set; }
        }
        [HttpPost]
        public IActionResult CreateDEMO([FromBody] CreateInwardRequest request)
        {
            // Check if the request is null
            if (request == null)
            {
                return BadRequest("Request is null.");
            }

            // Access form data and row data
            var formData = request.FormData; // This will be a string
            var rowData = request.RowData; // This will be a List<RowData>

            // Log for debugging
            Console.WriteLine("FormData: " + formData);
            if (rowData != null)
            {
                foreach (var row in rowData)
                {
                    Console.WriteLine($"ProductCode: {row.ProductCode}, Quantity: {row.Quantity}");
                }
            }
            else
            {
                Console.WriteLine("RowData is null");
            }

            // Implement your logic here to save data

            // Return appropriate response
            return Ok("Data processed successfully."); // Or return some other result based on your logic
        }
    

        [HttpPost]
        public IActionResult Create(inward applicant, int stickerDecision, string rowData)
        {
            if (applicant.ordertype.Trim() == "Demo" || applicant.ordertype.Trim() == "Repair" || applicant.ordertype.Trim() == "Replacement")
            {
                //applicant.batchcode = GenerateBatchCode();
                //applicant.grnno = Generategrn();
                //applicant.pono = "-";
                //applicant.sono = "-";

                var ponodata1 = _context.inward.Where(a => a.pono == applicant.pono.Trim() && a.status.Trim() == "Pending").OrderBy(a => a.inward_id).FirstOrDefault();
                if (ponodata1 != null)
                {
                    //FIRSTLY INAWARDING DONE
                    if(ponodata1.flag == 1)
                    {
                        return Json(new { success = false, message = "You have already completed inwarding. Please proceed with outwarding!" });
                    }
                    //FIRSTLY OUTWARDING DONE
                    else
                    {

                    }
                }

                applicant.flag = 1;
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

                int smaxId = _context.Storage_Operation.Any() ? _context.Storage_Operation.Max(e => e.id) + 0 : 0;
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

                    a.flag = 1;
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

                int maxId = _context.inward.Any() ? _context.inward.Max(e => e.inward_id) + 1 : 1;
                applicant.inward_id = maxId;
                List<inwardPacket> packetsToRemove = new List<inwardPacket>();

                if (applicant.ordertype.Trim() == "Demo")
                {
                    //var sonoid = "DEMO/IN/" + DateTime.Now.ToString("ddMMyyyy/HHmmss");
                    foreach (var item in applicant.inwardPacket)
                    {
                        applicant.pono = applicant.pono;
                        applicant.sono = applicant.pono;
                        item.pono = applicant.pono;
                        item.sono = applicant.pono;
                        
                    }
                    if(applicant.vendername == "-------")
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
                        //ADD INTO DMRPRPR
                        foreach (var item in DMRPRRP_List)
                        {
                            int dmmaxid = _context.DMRPRRP.Any() ? _context.DMRPRRP.Max(e => e.id) + 1 : 1;
                            item.refno = applicant.pono;
                            item.inout = applicant.flag;
                            var typeget = applicant.inwardPacket.Where(a => a.productcode.Trim().ToUpper() == item.productcode.ToUpper().Trim()).Select(a => a.type).FirstOrDefault();
                            DMRPRRP dm = new DMRPRRP()
                            {
                                id = dmmaxid,
                                productcode = item.productcode,
                                grn = item.grn,
                                batch = item.batch,
                                boxno = item.boxno,
                                refno = item.refno,
                                ordertype = item.ordertype,
                                inout = item.inout,
                                pickflag = item.pickflag,
                                location = item.location,
                                type = item.type,
                                statusflag = item.statusflag,
                                date = DateTime.Now.ToString("yyyy-MM-dd"),
                                time = DateTime.Now.ToString("HH:mm:ss"),
                                returntype = applicant.typeofreturn,
                                partyname= applicant.partyname.Trim(),
                                condition = typeget,
                                from= applicant.vendername.Trim(),
                            };
                            _context.DMRPRRP.Add(dm);
                            _context.SaveChanges();

                            //FROM STOCK OUT FOR REPAIR AND COME BACK AS REPAIRED 
                            var foundstoarge = _context.Storage_Operation
                            .Where(a => a.boxno.Trim() == item.boxno.Trim() &&
                            a.batchcode.Trim() == item.batch.Trim() &&
                            a.grnno.Trim() == item.grn.Trim() &&
                            a.productcode.Trim() == item.productcode.Trim() &&
                            a.statusflag.Trim() == "DMOUT")
                            .FirstOrDefault();

                            if (foundstoarge != null)
                            {
                                foundstoarge.locationcode = "TMP";
                                foundstoarge.statusflag = "ST";
                                _context.Storage_Operation.Update(foundstoarge);
                                _context.SaveChanges();
                            }
                        }
                        //END
                    }
                }
                else if (applicant.ordertype.Trim() == "Repair")
                {
                    //var sonoid = "REPAIR/IN/" + DateTime.Now.ToString("ddMMyyyy/HHmmss");
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
                            if(packetdata.Count > 0)
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
                            int dmmaxid = _context.DMRPRRP.Any() ? _context.DMRPRRP.Max(e => e.id) + 1 : 1;
                            item.refno = applicant.pono;
                            item.inout = applicant.flag;
                            var typeget = applicant.inwardPacket.Where(a => a.productcode.Trim().ToUpper() == item.productcode.ToUpper().Trim()).Select(a => a.type).FirstOrDefault();
                            //if(typeget == "NONRP")
                            //{
                            //    item.statusflag = "NONRP";
                            //}
                            //if (typeget == "ST")
                            //{
                            //    item.statusflag = "ST";
                            //}
                            //if (typeget == "DMG")
                            //{
                            //    item.statusflag = "DMG";
                            //}
                            
                            DMRPRRP dm = new DMRPRRP()
                            {
                                id = dmmaxid,
                                productcode = item.productcode,
                                grn = item.grn,
                                batch = item.batch,
                                boxno = item.boxno,
                                refno = item.refno,
                                ordertype = item.ordertype,
                                inout = item.inout,
                                pickflag = item.pickflag,
                                location = item.location,
                                type = item.type,
                                statusflag = "ST",
                                date = DateTime.Now.ToString("yyyy-MM-dd"),
                                time = DateTime.Now.ToString("HH:mm:ss"),
                                returntype = applicant.typeofreturn,
                                partyname = applicant.partyname.Trim(),
                                condition = typeget,
                                from = applicant.vendername.Trim(),
                            };
                            _context.DMRPRRP.Add(dm);
                            _context.SaveChanges();

                            //FROM STOCK OUT FOR REPAIR AND COME BACK AS REPAIRED 
                            var foundstoarge = _context.Storage_Operation
                            .Where(a => a.boxno.Trim() == item.boxno.Trim() &&
                            a.batchcode.Trim() == item.batch.Trim() &&
                            a.grnno.Trim() == item.grn.Trim() &&
                            a.productcode.Trim() == item.productcode.Trim() && 
                            a.statusflag.Trim() == "RPROUT")
                            .FirstOrDefault();

                            if (foundstoarge != null)
                            {
                                var status = "-";
                                if(typeget == "NONRP")
                                {
                                    status = "NONRP";
                                }
                                if (typeget == "ST")
                                {
                                    status = "ST";
                                }
                                if (typeget == "DMG")
                                {
                                    status = "DMG";
                                }
                                foundstoarge.locationcode = "TMP";
                                foundstoarge.statusflag = status.Trim();
                                _context.Storage_Operation.Update(foundstoarge);
                                _context.SaveChanges();
                            }
                        }
                        
                        //END
                        //REMOVE THAT FROM STORAGE
                        if (Remove_StockInventory.Count > 0)
                        {
                            foreach(var item in Remove_StockInventory)
                            {
                                //IN CASE STOCK IS FOUND TO BE IN REPRI CONDITION 
                                var found = _context.Storage_Operation
                                         .Where(a =>a.id == item.id 
                                         && a.productcode.Trim() == item.productcode.Trim()
                                         && a.batchcode.Trim() == item.batchcode.Trim()
                                         && a.boxno.Trim() == item.boxno.Trim()).FirstOrDefault();

                                if (found != null)
                                {
                                    _context.Storage_Operation.Remove(found);
                                    _context.SaveChanges();
                                }
                            }
                        }
                        //END
                    }
                }
                else if (applicant.ordertype.Trim() == "Replacement")
                {
                    //var sonoid = "REPLACEMENT/IN/" + DateTime.Now.ToString("ddMMyyyy/HHmmss");
                    foreach (var item in applicant.inwardPacket)
                    {
                        applicant.pono = applicant.pono;
                        applicant.sono = applicant.pono;
                        item.pono = applicant.pono;
                        item.sono = applicant.pono;
                        //applicant.pono = sonoid;
                        //applicant.sono = sonoid;
                        //item.pono = sonoid;
                        //item.sono = sonoid;
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
                        //ADD INTO DMRPRPR
                        foreach (var item in DMRPRRP_List)
                        {
                            int dmmaxid = _context.DMRPRRP.Any() ? _context.DMRPRRP.Max(e => e.id) + 1 : 1;
                            item.refno = applicant.pono;
                            item.inout = applicant.flag;
                            var typeget = applicant.inwardPacket.Where(a => a.productcode.Trim().ToUpper() == item.productcode.ToUpper().Trim()).Select(a => a.type).FirstOrDefault();
                            DMRPRRP dm = new DMRPRRP()
                            {
                                id = dmmaxid,
                                productcode = item.productcode,
                                grn = item.grn,
                                batch = item.batch,
                                boxno = item.boxno,
                                refno = item.refno,
                                ordertype = item.ordertype,
                                inout = item.inout,
                                pickflag = item.pickflag,
                                location = item.location,
                                type = item.type,
                                statusflag = item.statusflag,
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
                        //EDN
                        //REMOVE FROM STORAGE
                        if (Remove_StockInventory.Count > 0)
                        {
                            foreach(var item in Remove_StockInventory)
                            {
                                //IN CASE STOCK IS FOUND TO BE IN REPRI CONDITION 
                                var found = _context.Storage_Operation
                                             .Where(a => a.id == item.id 
                                             &&  a.productcode.Trim() == item.productcode.Trim()
                                             && a.batchcode.Trim() == item.batchcode.Trim()
                                             && a.boxno.Trim() == item.boxno.Trim())
                                             .FirstOrDefault();

                                if (found != null)
                                {
                                    _context.Storage_Operation.Remove(found);
                                    _context.SaveChanges();
                                }
                            }
                        }
                        //END
                        
                    }
                }

                _context.Add(applicant);

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
                _context.SaveChanges();


                if (applicant.ordertype.Trim() == "Demo")
                {
                    List<inwardPacket> inpck = new List<inwardPacket>();
                    foreach (var item in applicant.inwardPacket)
                    {
                        if (!DMRPRRP_List.Any(a => a.productcode.Trim() == item.productcode.Trim()))
                        {
                            inpck.Add(item);
                        }

                        //var MAXID = _context.TransactionList.Any() ? _context.TransactionList.Max(e => e.id) + 1 : 1;
                        //TransactionList TransactionList = new TransactionList()
                        //{
                        //    id = MAXID,
                        //    refno = applicant.pono,
                        //    vendor = applicant.vendername,
                        //    partyname = applicant.partyname,
                        //    product = item.productcode,
                        //    qty = item.quantity.ToString(),
                        //    inout=applicant.flag,
                        //    status = applicant.status,
                        //    ordertype= applicant.ordertype,
                        //};
                        //_context.TransactionList.Add(TransactionList);
                        //_context.SaveChanges();
                    }

                    //FOR SSHIPPER WISE 
                    if (stickerDecision == 1)
                    {
                        //set sticker 
                        string printprn = "";
                        var path = $"{_webHostEnvironment.WebRootPath}\\Sticker\\ErosDEMOValue.prn";
                        var value = $"{_webHostEnvironment.WebRootPath}\\Sticker\\ErosDEMO.prn";
                        foreach (inwardPacket Packet in inpck)
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
                                                string value1 = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn";
                                                string fileContent = System.IO.File.ReadAllText(value1);
                                                fileContent = fileContent.Replace("<D001>", Packet.productcode.ToUpper().ToString().Trim());
                                                fileContent = fileContent.Replace("<D002>", Packet.description.ToUpper().ToString().Trim());
                                                fileContent = fileContent.Replace("<D003>", applicant.grnno.ToString().Trim());
                                                fileContent = fileContent.Replace("<D004>", q + "-" + i + "/" + Packet.noofpackets.ToString().Trim());
                                                fileContent = fileContent.Replace("<D005>", Packet.qtyperpkt.ToString().Trim() + "/" + Packet.setofsub_assemb.ToString().Trim());
                                                fileContent = fileContent.Replace("<D006>", applicant.batchcode.ToString().Trim());
                                                System.IO.File.WriteAllText(value, fileContent);
                                                string fileContent1 = System.IO.File.ReadAllText(value);
                                                printprn = printprn + fileContent1 + Environment.NewLine;

                                                //ADD
                                                var typeget = applicant.inwardPacket.Where(a => a.productcode.Trim().ToUpper() == Packet.productcode.ToUpper().Trim()).Select(a => a.type).FirstOrDefault();
                                                int dmmaxid = _context.DMRPRRP.Any() ? _context.DMRPRRP.Max(e => e.id) + 1 : 1;
                                                DMRPRRP dm = new DMRPRRP()
                                                {
                                                    id = dmmaxid,
                                                    productcode = Packet.productcode,
                                                    grn = applicant.grnno,
                                                    batch = applicant.batchcode,
                                                    boxno = q + "-" + i + "/" + Packet.noofpackets.ToString().Trim(),
                                                    refno = applicant.sono,
                                                    ordertype = applicant.ordertype,
                                                    inout = applicant.flag,
                                                    pickflag = 0,
                                                    location = "TMP",
                                                    type = "DM",
                                                    statusflag = "ST",
                                                    date = DateTime.Now.ToString("yyyy-MM-dd"),
                                                    time = DateTime.Now.ToString("HH:mm:ss"),
                                                    returntype = applicant.typeofreturn,
                                                    partyname = applicant.partyname.Trim(),
                                                    condition = typeget,
                                                    from = applicant.vendername.Trim(),
                                                };
                                                _context.DMRPRRP.Add(dm);
                                                _context.SaveChanges();
                                                //END

                                                //smaxId++;
                                                ////ADD INTO STORAGE OPERATION
                                                //Storage_Operation st = new Storage_Operation()
                                                //{
                                                //    id = smaxId,
                                                //    productcode = Packet.productcode,
                                                //    batchcode = applicant.batchcode,
                                                //    boxno = q + "-" + i + "/" + Packet.noofpackets,
                                                //    locationcode = "TMP",
                                                //    statusflag = "DM",
                                                //    pickflag = "0",
                                                //    grnno = applicant.grnno,
                                                //};
                                                //_context.Storage_Operation.Add(st);
                                                //_context.SaveChanges();
                                                ////END
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                }
                                //else
                                //{
                                //    int qty = Packet.quantity;
                                //    int qtypershp = Packet.noqtypershp;
                                //    float totalshipperCeil = (float)qty / qtypershp;
                                //    int totalshipper = (int)Math.Ceiling(totalshipperCeil);

                                //    int shp = 1;
                                //    for (int i = 1; i <= totalshipper; i++)
                                //    {
                                //        int remainingQty = qty - ((i - 1) * qtypershp); // Calculate remaining quantity for this shipper

                                //        // Determine the quantity for this shipper
                                //        int currentQty;
                                //        if (i < totalshipper)
                                //        {
                                //            currentQty = qtypershp;
                                //        }
                                //        else
                                //        {
                                //            currentQty = remainingQty;
                                //        }
                                //        string value1 = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn";
                                //        string fileContent = System.IO.File.ReadAllText(value1);
                                //        fileContent = fileContent.Replace("<D001>", Packet.productcode.ToString().ToUpper().Trim());
                                //        fileContent = fileContent.Replace("<D002>", Packet.description.ToString().ToUpper().Trim());
                                //        fileContent = fileContent.Replace("<D003>", applicant.grnno.ToString().Trim());
                                //        fileContent = fileContent.Replace("<D004>", $"{i}-{currentQty}/{shp.ToString().Trim()}");
                                //        fileContent = fileContent.Replace("<D005>", Packet.qtyperpkt.ToString().Trim() + "/" + Packet.setofsub_assemb.ToString().Trim());
                                //        fileContent = fileContent.Replace("<D006>", applicant.batchcode.ToString().Trim());
                                //        System.IO.File.WriteAllText(value, fileContent);

                                //        //  printprn = printprn + value + Environment.NewLine;

                                //        //------Replacement of above comment command ------ //
                                //        string fileContent1 = System.IO.File.ReadAllText(value);
                                //        printprn = printprn + fileContent1 + Environment.NewLine;
                                //        //------Replacement of above comment command ------ //
                                //    }
                                //}
                            }
                            else
                            {
                            }
                        }
                        byte[] byteArray = Encoding.ASCII.GetBytes(printprn);
                        MemoryStream stream = new MemoryStream(byteArray);
                        DMRPRRP_List.Clear();
                        Remove_StockInventory.Clear();
                        return File(stream, "text/plain", "example.prn");
                    }
                }
                else if (applicant.ordertype.Trim() == "Repair")
                {
                    List<inwardPacket> inpck = new List<inwardPacket>();
                    foreach (var item in applicant.inwardPacket)
                    {
                        if (!DMRPRRP_List.Any(a => a.productcode.Trim() == item.productcode.Trim()))
                        {
                            inpck.Add(item);
                        }
                    }

                    //FOR SSHIPPER WISE 
                    if (stickerDecision == 1)
                    {
                        //set sticker 
                        string printprn = "";
                        var path = $"{_webHostEnvironment.WebRootPath}\\Sticker\\ErosDEMOValue.prn";
                        var value = $"{_webHostEnvironment.WebRootPath}\\Sticker\\ErosDEMO.prn";
                        foreach (inwardPacket Packet in inpck)
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
                                                string value1 = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn";
                                                string fileContent = System.IO.File.ReadAllText(value1);
                                                fileContent = fileContent.Replace("<D001>", Packet.productcode.ToUpper().ToString().Trim());
                                                fileContent = fileContent.Replace("<D002>", Packet.description.ToUpper().ToString().Trim());
                                                fileContent = fileContent.Replace("<D003>", applicant.grnno.ToString().Trim());
                                                fileContent = fileContent.Replace("<D004>", q + "-" + i + "/" + Packet.noofpackets.ToString().Trim());
                                                fileContent = fileContent.Replace("<D005>", Packet.qtyperpkt.ToString().Trim() + "/" + Packet.setofsub_assemb.ToString().Trim());
                                                fileContent = fileContent.Replace("<D006>", applicant.batchcode.ToString().Trim());
                                                System.IO.File.WriteAllText(value, fileContent);
                                                string fileContent1 = System.IO.File.ReadAllText(value);
                                                printprn = printprn + fileContent1 + Environment.NewLine;

                                                //ADD INTO STORAGE ALSO IF FOUND 
                                                //item.refno = applicant.pono;
                                                //item.inout = applicant.flag;
                                                int dmmaxidst = _context.Storage_Operation.Any() ? _context.Storage_Operation.Max(e => e.id) + 1 : 1;
                                                var typeget = applicant.inwardPacket
                                                    .Where(a => a.productcode.Trim().ToUpper() == Packet.productcode.ToUpper().Trim())
                                                    .Select(a => a.type)
                                                    .FirstOrDefault();


                                                //ADD
                                                var status = "ST";
                                                int dmmaxid = _context.DMRPRRP.Any() ? _context.DMRPRRP.Max(e => e.id) + 1 : 1;
                                                DMRPRRP dm = new DMRPRRP()
                                                {
                                                    id = dmmaxid,
                                                    productcode = Packet.productcode,
                                                    grn = applicant.grnno,
                                                    batch = applicant.batchcode,
                                                    boxno = q + "-" + i + "/" + Packet.noofpackets.ToString().Trim(),
                                                    refno = applicant.sono,
                                                    ordertype = applicant.ordertype,
                                                    inout = applicant.flag,
                                                    pickflag = 0,
                                                    location = "TMP",
                                                    type = "RPR",
                                                    statusflag = status,
                                                    date = DateTime.Now.ToString("yyyy-MM-dd"),
                                                    time = DateTime.Now.ToString("HH:mm:ss"),
                                                    returntype = applicant.typeofreturn,
                                                    partyname = applicant.partyname.Trim(),
                                                    condition = status,
                                                    from = applicant.vendername.Trim(),
                                                };
                                                _context.DMRPRRP.Add(dm);
                                                _context.SaveChanges();
                                                //END

                                                if (applicant.typeofreturn.Trim() == "Returned-Replacement")
                                                {
                                                    
                                                    if (typeget == "NONRP")
                                                    {
                                                        status = "NONRP";
                                                    }
                                                    if (typeget == "ST")
                                                    {
                                                        status = "ST";
                                                    }
                                                    if (typeget == "DMG")
                                                    {
                                                        status = "DMG";
                                                    }
                                                    Storage_Operation st = new Storage_Operation()
                                                    {
                                                        id = dmmaxidst,
                                                        productcode = Packet.productcode.Trim(),
                                                        batchcode = applicant.batchcode.Trim(),
                                                        boxno = q + "-" + i + "/" + Packet.noofpackets.ToString().Trim(),
                                                        locationcode = "TMP",
                                                        statusflag = status,
                                                        pickflag = "0",
                                                        grnno = applicant.grnno,
                                                    };
                                                    _context.Storage_Operation.Add(st);
                                                    _context.SaveChanges();
                                                }


                                                //smaxId++;
                                                ////ADD INTO STORAGE OPERATION
                                                //Storage_Operation st = new Storage_Operation()
                                                //{
                                                //    id = smaxId,
                                                //    productcode = Packet.productcode,
                                                //    batchcode = applicant.batchcode,
                                                //    boxno = q + "-" + i + "/" + Packet.noofpackets,
                                                //    locationcode = "TMP",
                                                //    statusflag = "DM",
                                                //    pickflag = "0",
                                                //    grnno = applicant.grnno,
                                                //};
                                                //_context.Storage_Operation.Add(st);
                                                //_context.SaveChanges();
                                                ////END
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                }
                                
                            }
                            else
                            {
                            }
                        }
                        byte[] byteArray = Encoding.ASCII.GetBytes(printprn);
                        MemoryStream stream = new MemoryStream(byteArray);
                        DMRPRRP_List.Clear();
                        Remove_StockInventory.Clear();
                        return File(stream, "text/plain", "example.prn");
                    }
                }
                else if (applicant.ordertype.Trim() == "Replacement")
                {
                    List<inwardPacket> inpck = new List<inwardPacket>();
                    foreach (var item in applicant.inwardPacket)
                    {
                        if (!DMRPRRP_List.Any(a => a.productcode.Trim() == item.productcode.Trim()))
                        {
                            inpck.Add(item);
                        }
                    }

                    //FOR SSHIPPER WISE 
                    if (stickerDecision == 1)
                    {
                        //set sticker 
                        string printprn = "";
                        var path = $"{_webHostEnvironment.WebRootPath}\\Sticker\\ErosDEMOValue.prn";
                        var value = $"{_webHostEnvironment.WebRootPath}\\Sticker\\ErosDEMO.prn";
                        foreach (inwardPacket Packet in inpck)
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
                                                string value1 = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn";
                                                string fileContent = System.IO.File.ReadAllText(value1);
                                                fileContent = fileContent.Replace("<D001>", Packet.productcode.ToUpper().ToString().Trim());
                                                fileContent = fileContent.Replace("<D002>", Packet.description.ToUpper().ToString().Trim());
                                                fileContent = fileContent.Replace("<D003>", applicant.grnno.ToString().Trim());
                                                fileContent = fileContent.Replace("<D004>", q + "-" + i + "/" + Packet.noofpackets.ToString().Trim());
                                                fileContent = fileContent.Replace("<D005>", Packet.qtyperpkt.ToString().Trim() + "/" + Packet.setofsub_assemb.ToString().Trim());
                                                fileContent = fileContent.Replace("<D006>", applicant.batchcode.ToString().Trim());
                                                System.IO.File.WriteAllText(value, fileContent);
                                                string fileContent1 = System.IO.File.ReadAllText(value);
                                                printprn = printprn + fileContent1 + Environment.NewLine;

                                                //ADD
                                                var typeget = applicant.inwardPacket.Where(a => a.productcode.Trim().ToUpper() == Packet.productcode.ToUpper().Trim()).Select(a => a.type).FirstOrDefault();
                                                int dmmaxid = _context.DMRPRRP.Any() ? _context.DMRPRRP.Max(e => e.id) + 1 : 1;
                                                DMRPRRP dm = new DMRPRRP()
                                                {
                                                    id = dmmaxid,
                                                    productcode = Packet.productcode,
                                                    grn = applicant.grnno,
                                                    batch = applicant.batchcode,
                                                    boxno = q + "-" + i + "/" + Packet.noofpackets.ToString().Trim(),
                                                    refno = applicant.sono,
                                                    ordertype = applicant.ordertype,
                                                    inout = applicant.flag,
                                                    pickflag = 0,
                                                    location = "TMP",
                                                    type = "RP",
                                                    statusflag = typeget,
                                                    date = DateTime.Now.ToString("yyyy-MM-dd"),
                                                    time = DateTime.Now.ToString("HH:mm:ss"),
                                                    returntype = applicant.typeofreturn,
                                                    partyname = applicant.partyname.Trim(),
                                                    condition = typeget,
                                                    from = applicant.vendername.Trim(),
                                                };
                                                _context.DMRPRRP.Add(dm);
                                                _context.SaveChanges();
                                                //END

                                                //ADD INTO STORAGE OPERATION
                                                int Smmaxid = _context.Storage_Operation.Any() ? _context.Storage_Operation.Max(e => e.id) + 1 : 1;
                                                Storage_Operation st = new Storage_Operation()
                                                {
                                                    id = Smmaxid,
                                                    productcode = Packet.productcode,
                                                    batchcode = applicant.batchcode,
                                                    boxno = q + "-" + i + "/" + Packet.noofpackets,
                                                    locationcode = "TMP",
                                                    statusflag = typeget,
                                                    pickflag = "0",
                                                    grnno = applicant.grnno,
                                                };
                                                _context.Storage_Operation.Add(st);
                                                _context.SaveChanges();
                                                ////END
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                }
                                
                            }
                            else
                            {
                            }
                        }
                        byte[] byteArray = Encoding.ASCII.GetBytes(printprn);
                        MemoryStream stream = new MemoryStream(byteArray);
                        DMRPRRP_List.Clear();
                        Remove_StockInventory.Clear();
                        return File(stream, "text/plain", "example.prn");
                    }
                }

                
                //FOR SUBCOMPONENTWISE 2
                //return RedirectToAction("Index");
                return Json(new { success = true, message = "Demo Outward Process done successfully!" });
            }
            else if (applicant.ordertype == "Purchase")
            {
                int maxId = _context.inward.Any() ? _context.inward.Max(e => e.inward_id) + 1 : 1;
                applicant.inward_id = maxId;
                applicant.batchcode = applicant.batchcode.ToUpper();
                applicant.flag = 1;
                int flag = 0;
                applicant.typeofreturn = "-";
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
                
                List<inwardPacket> sss = new List<inwardPacket>();
                var exstpacket = new inwardPacket();
                var inwardSerialno = new List<inwardSerialno>();

                //purchase - subcom
                var inward_subcomponent = new List<inward_subcomponent>();
                var purchase1 = _context.purchase.FirstOrDefault(p => p.pono == applicant.pono);
                foreach (var product in applicant.inwardPacket)
                {
                    product.date = applicant.date;
                    product.flag = applicant.flag;
                    if (product.quantity > 0)
                    {
                        if (purchase1 != null)
                        {
                            // Fetch all Poproductdetail records associated with the found Porderid'
                            var poproductDetailsList = _context.purchase_subcomponent
                                .Where(pd => pd.purchaseproduct_id == purchase1.id && pd.sccode.StartsWith(product.productcode))
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
                                        tqty = item.tqty,
                                        sono = "-",
                                        pono = applicant.pono,
                                    };
                                    inward_subcomponent.Add(newProduct);
                                }
                            }
                        }

                    }
                }
                applicant.inward_subcomponent.AddRange(inward_subcomponent);


                var PO_Exist = _context.inward.Where(p => p.pono == applicant.pono).AsNoTracking().ToList(); //IF DATA NULL THEN UPDATE
                int total = 0;
                if (PO_Exist.Count > 0)
                {
                    var existingpacket = _context.inwardPacket.Where(a => a.pono == applicant.pono)
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
                            packets.sono = "-";
                            if ((packets.setofsub_assemb == null || packets.setofsub_assemb == "0") && (packets.qtyperpkt == null || packets.qtyperpkt == "0") && (packets.noofpackets == null || packets.noofpackets == "0" || packets.noqtypershp == null))
                            {
                                packets.noqtypershp = 0;
                                packets.quantity = 0;
                                packets.totalsubassmbly = 0;
                            }
                            if(packets.quantity == 0 ||  packets.quantity == null)
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

                            foreach (var packetQuantity in productDetails.Where(b => b.description == a.ProductName))
                            {
                                // Find the corresponding product detail for the packet
                                var productDetail = productDetails.FirstOrDefault(pd => pd.id == packetQuantity.id);
                                if (productDetail != null)
                                {
                                    // Compare the existing quantity with the newly entered quantity
                                    if (total == productDetail.quantity)
                                    {
                                        //to set complete if data already exist
                                        var exist = _context.inward.Where(inwardPacket => inwardPacket.pono == applicant.pono).ToList();
                                        foreach (var item in exist)
                                        {
                                            item.status = "Completed";
                                        }
                                        // Quantity is sufficient (Completed)
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
                    _context.Add(applicant);
                    //maintain logs
                    var user = HttpContext.Session.GetString("User");
                    var logs = new Logs();
                    logs.pagename = "Inward Operation";
                    //logs.task = "Create "+applicant.ordertype+" inward !";
                    logs.taskid = maxId;
                    logs.task = maxId.ToString() + " inward operation " + applicant.ordertype + " Create";
                    logs.action = "Create";
                    logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                    logs.time = DateTime.Now.ToString("HH:mm:ss");
                    logs.username = user;
                    _context.Add(logs);

                    _context.SaveChanges();
                   // _notyfService.Success("Order Create Successfully");
                    if (stickerDecision == 1)
                    {
                        //set sticker 
                        string printprn = "";
                        var path = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn";
                        var value = $"{_webHostEnvironment.WebRootPath}\\Sticker\\ErosVALUE.prn";
                        //foreach (inwardPacket Packet in applicant.inwardPacket)
                        //{
                        //    if (System.IO.File.Exists(path))
                        //    {
                        //        System.IO.File.Delete(value);
                        //        System.IO.File.Copy(path, value);

                        //        if (int.TryParse(Packet.noofpackets, out int noOfPackets) && noOfPackets > 0)
                        //        {
                        //            for (int q = 1; q <= Packet.quantity; q++)
                        //            {
                        //                for (int i = 1; i <= noOfPackets; i++)
                        //                {
                        //                    try
                        //                    {
                        //                        string value1 = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn";
                        //                        string fileContent = System.IO.File.ReadAllText(value1);
                        //                        fileContent = fileContent.Replace("<D001>", Packet.productcode.ToString().Trim());
                        //                        fileContent = fileContent.Replace("<D002>", Packet.description.ToString().Trim());
                        //                        fileContent = fileContent.Replace("<D003>", applicant.grnno.ToString().Trim());
                        //                        fileContent = fileContent.Replace("<D004>", $"{q}-{i}/{noOfPackets.ToString().Trim()}");
                        //                        fileContent = fileContent.Replace("<D005>", $"{Packet.qtyperpkt.ToString().Trim()}/{Packet.setofsub_assemb.ToString().Trim()}");
                        //                        fileContent = fileContent.Replace("<D006>", applicant.batchcode.ToString().Trim());
                        //                        System.IO.File.WriteAllText(value, fileContent);

                        //                        string fileContent1 = System.IO.File.ReadAllText(value);
                        //                        printprn = printprn + fileContent1 + Environment.NewLine;
                        //                    }
                        //                    catch
                        //                    {
                        //                        // Handle any exceptions
                        //                    }
                        //                }
                        //            }
                        //        }
                        //        else
                        //        {
                        //            // Handle the case where 'Packet.noofpackets' is not a valid integer or <= 0
                        //        }
                        //    }
                        //    else
                        //    {
                        //        // Handle the case where 'path' does not exist
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
                    List<inwardPacket> packetsToRemove = new List<inwardPacket>();
                    //check pending or complete
                    foreach (var a in applicant.inwardPacket)
                    {
                        a.date = applicant.date;
                        a.flag = applicant.flag;
                        a.sono = "-";
                        if ((a.setofsub_assemb == null || a.setofsub_assemb == "0") && (a.qtyperpkt == null || a.qtyperpkt == "0") && (a.noofpackets == null || a.noofpackets == "0" | a.noqtypershp == null))
                        {
                            a.quantity = 0;
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
                            foreach (var packetQuantity in productDetails.Where(b => b.description.Trim().ToUpper() == a.description.Trim().ToUpper() && b.productcode.Trim().ToUpper() == a.productcode.Trim().ToUpper()))
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
                                        flag = 1;
                                        applicant.status = "Pending";
                                    }
                                }
                            }
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


                    //maintain logs
                    var user = HttpContext.Session.GetString("User");
                    var logs = new Logs();
                    logs.pagename = "Inward Operation";
                    //logs.task = "Create " + applicant.ordertype+" Inward !";
                    logs.taskid = maxId;
                    logs.task = maxId.ToString() + " inward operation " + applicant.ordertype + " Create";
                    logs.action = "Create";
                    logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                    logs.time = DateTime.Now.ToString("HH:mm:ss");
                    logs.username = user;
                    _context.Add(logs);

                    _context.Update(purchase);
                    _context.Add(applicant);
                    _context.SaveChanges();
                    //_notyfService.Success("Order Create Successfully");
                    if (stickerDecision == 1)
                    {
                        string pattern = "[^a-zA-Z0-9]";
                        applicant.batchcode = Regex.Replace(applicant.batchcode.Trim(), pattern, "");
                        
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
                        //                    fileContent = fileContent.Replace("<D001>", Packet.productcode.ToString().Trim());
                        //                    fileContent = fileContent.Replace("<D002>", Packet.description.ToString().Trim());
                        //                    fileContent = fileContent.Replace("<D003>", applicant.grnno.ToString().Trim());
                        //                    fileContent = fileContent.Replace("<D004>", q + "-" + i + "/" + Packet.noofpackets.ToString().Trim());
                        //                    fileContent = fileContent.Replace("<D005>", Packet.qtyperpkt.ToString().Trim() + "/" + Packet.setofsub_assemb.ToString().Trim());
                        //                    fileContent = fileContent.Replace("<D006>", applicant.batchcode.ToString().Trim());

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
                                    if(SHPlist.Count > 0)
                                    {
                                        var data = SHPlist.Where(a => a.productcode.Trim() == Packet.productcode.Trim()).OrderBy(a=>a.noqtypershp).ToList();
                                        if(data.Count > 0)
                                        {
                                            foreach(var item in data)
                                            {
                                                string value1 = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn";
                                                string fileContent = System.IO.File.ReadAllText(value1);
                                                fileContent = fileContent.Replace("<D001>", Packet.productcode.ToString().Trim().ToUpper());
                                                fileContent = fileContent.Replace("<D002>", Packet.description.ToString().Trim().ToUpper());
                                                fileContent = fileContent.Replace("<D003>", applicant.grnno.ToString().Trim());
                                                fileContent = fileContent.Replace("<D004>", $"{item.noqtypershp}-{item.quantity}/{1}");
                                                fileContent = fileContent.Replace("<D005>", Packet.qtyperpkt.ToString().Trim() + "/" + Packet.setofsub_assemb.ToString().Trim());
                                                fileContent = fileContent.Replace("<D006>", applicant.batchcode.ToString().Trim());
                                                System.IO.File.WriteAllText(value, fileContent);

                                                //  printprn = printprn + value + Environment.NewLine;

                                                //ADD INTO TABLE
                                                int SHPList_maxId = _context.SHPList.Any() ? _context.SHPList.Max(e => e.Id) + 1 : 1;
                                                SHPList SHPList = new SHPList()
                                                {
                                                    Id = SHPList_maxId,
                                                    BoxCount = (item.noqtypershp).ToString(),
                                                    ProductCode = item.productcode.Trim(),
                                                    Qty = item.quantity,
                                                    RefNo = item.pono.Trim(),
                                                };
                                                _context.SHPList.Add(SHPList);
                                                _context.SaveChanges();
                                                //END

                                                //------Replacement of above comment command ------ //
                                                string fileContent1 = System.IO.File.ReadAllText(value);
                                                printprn = printprn + fileContent1 + Environment.NewLine;
                                            }
                                        }
                                    }

                                    //OLD PROCESS
                                    //int qty = Packet.quantity;
                                    //int qtypershp = Packet.noqtypershp;
                                    //float totalshipperCeil = (float)qty / qtypershp;
                                    //int totalshipper = (int)Math.Ceiling(totalshipperCeil);

                                    //int shp = 1;
                                    //for (int i = 1; i <= totalshipper; i++)
                                    //{
                                    //    int remainingQty = qty - ((i - 1) * qtypershp); // Calculate remaining quantity for this shipper

                                    //    // Determine the quantity for this shipper
                                    //    int currentQty;
                                    //    if (i < totalshipper)
                                    //    {
                                    //        currentQty = qtypershp;
                                    //    }
                                    //    else
                                    //    {
                                    //        currentQty = remainingQty;
                                    //    }
                                    //    string value1 = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn";
                                    //    string fileContent = System.IO.File.ReadAllText(value1);
                                    //    fileContent = fileContent.Replace("<D001>", Packet.productcode.ToString().Trim().ToUpper());
                                    //    fileContent = fileContent.Replace("<D002>", Packet.description.ToString().Trim().ToUpper());
                                    //    fileContent = fileContent.Replace("<D003>", applicant.grnno.ToString().Trim());
                                    //    fileContent = fileContent.Replace("<D004>", $"{i}-{currentQty}/{shp.ToString().Trim()}");
                                    //    fileContent = fileContent.Replace("<D005>", Packet.qtyperpkt.ToString().Trim() + "/" + Packet.setofsub_assemb.ToString().Trim());
                                    //    fileContent = fileContent.Replace("<D006>", applicant.batchcode.ToString().Trim());
                                    //    System.IO.File.WriteAllText(value, fileContent);

                                    //    //  printprn = printprn + value + Environment.NewLine;

                                    //    //------Replacement of above comment command ------ //
                                    //    string fileContent1 = System.IO.File.ReadAllText(value);
                                    //    printprn = printprn + fileContent1 + Environment.NewLine;
                                    //    //------Replacement of above comment command ------ //
                                    //}
                                    //OLD PROCESS END
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
                    return RedirectToAction("Index", "inwards");
                }
                //return RedirectToAction("Index");
                //return RedirectToAction("Index", "inwards");

            }
            else if (applicant.ordertype == "Salereturn")
            {
                applicant.batchcode = applicant.batchcode.ToUpper();
                int flag = 0;
                applicant.flag = 1;
                applicant.gstinno = "-";
                applicant.typeofreturn = "-";
                applicant.pono = "-";
                //condition for sale return
                applicant.typeofreturn = "-";
                applicant.pono = "-";
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
                if (!string.IsNullOrEmpty(applicant.grndate))
                {
                    applicant.grndate = applicant.grndate.ToUpper();
                }
                else
                {
                    applicant.grndate = applicant.date;
                }
                int maxId = _context.inward.Any() ? _context.inward.Max(e => e.inward_id) + 1 : 1;
                applicant.inward_id = maxId;

                // sales - subcom
                var inward_subcomponent = new List<inward_subcomponent>();
                var purchase1 = _context.so_inward.FirstOrDefault(p => p.sono == applicant.sono);
                foreach (var product in applicant.inwardPacket)
                {
                    product.date = applicant.date;
                    product.flag = applicant.flag;
                    if (applicant.grnno == null)
                    {
                        applicant.grnno = "-";
                    }
                    if (product.totalpacket == null)
                    {
                        product.totalpacket = "0";
                    }
                    if (purchase1 != null)
                    {
                        if (purchase1 != null)
                        {
                            // Fetch all Poproductdetail records associated with the found Porderid'

                            var poproductDetailsList = _context.purchase_subcomponent
                                .Where(pd => pd.purchaseproduct_id == purchase1.id && pd.sccode.StartsWith(product.productcode.ToUpper()))
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
                                    var newProduct = new inward_subcomponent
                                    {
                                        sccode = item.sccode,
                                        subcomponents = item.subcomponents,
                                        scqty = item.scqty,
                                        scuom = item.scuom,
                                        tqty = item.tqty
                                    };
                                    inward_subcomponent.Add(newProduct);
                                }
                            }
                        }

                    }
                }
                applicant.inward_subcomponent.AddRange(inward_subcomponent);

                List<inwardPacket> packetsToRemove = new List<inwardPacket>();

                //Existing data is updated into inward opration
                foreach (var a in applicant.inwardPacket)
                {
                    a.date = applicant.date;
                    a.flag = applicant.flag;
                    a.inwardId = applicant.inward_id;
                    a.pono = "-";
                    if ((a.setofsub_assemb == null || a.setofsub_assemb == "0") && (a.qtyperpkt == null || a.qtyperpkt == "0") && (a.noofpackets == null || a.noofpackets == "0" || a.noqtypershp == null))
                    {
                        //a.quantity = 0;
                        a.noqtypershp = 0;
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

                    a.sono = applicant.sono;
                    a.pono = "-";

                }
                //Existing data is updated into inward opration
                foreach (var a in applicant.inwardPacket)
                {
                    a.date = applicant.date;
                    a.flag = applicant.flag;
                    a.pono = "-";
                    a.sono = applicant.sono.Trim();
                }
                if (savepolist1.Count > 0)
                {
                    int maxIdPR = _context.PR_model.Any() ? _context.PR_model.Max(e => e.id) + 0 : 0;
                    foreach (var item in savepolist1)
                    {
                        var find = _context.Storage_Operation.Where(a => a.productcode.Trim() == item.productcode.Trim()
                        && a.batchcode.Trim() == item.batchcode.Trim()
                        //&& a.grnno.Trim() == item.grnno
                        && a.boxno.Trim() == item.boxno.Trim()).FirstOrDefault();
                        if (find != null)
                        {
                            find.locationcode = item.locationcode;
                            find.statusflag = item.statusflag;
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
                            statusflag = item.statusflag,
                            pickflag = "0",
                            grnno = item.grnno,
                            refno = applicant.sono,
                            date = DateTime.Now.ToString("dd-MM-yyyy")  // Corrected assignment
                        };
                        _context.PR_model.Add(PR_model);
                        _context.SaveChanges();
                        _context.Storage_Operation.Update(find);
                        _context.SaveChanges();
                    }
                }

                _context.Add(applicant); // add inward
                _context.SaveChanges();
                savepolist1.Clear();

                var pooredr = _context.so_inward.Where(a => a.sono.Trim() == applicant.sono.Trim()).FirstOrDefault();
                var pooredr1 = _context.so_inwardReturn.Where(a => a.sono.Trim().Contains(applicant.sono.Trim())).FirstOrDefault();
                if(pooredr != null && pooredr1 == null)
                {
                    pooredr.status = "Completed";
                    _context.so_inward.Update(pooredr);
                    _context.SaveChanges();
                }
                else if (pooredr != null && pooredr1!=null)
                {
                    pooredr.status = "Completed";
                    _context.so_inward.Update(pooredr);
                    _context.SaveChanges();

                    pooredr1.status = "Completed";
                    _context.so_inwardReturn.Update(pooredr1);
                    _context.SaveChanges();
                }

                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "Inward Operation";
                //logs.task = "Create "+applicant.ordertype+" Inward !";
                logs.taskid = maxId;
                logs.task = maxId.ToString()+" inward operation "+applicant.ordertype+ " Create";
                logs.action = "Create";
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.username = user;
                _context.Add(logs);

                _context.SaveChanges();
                //if (stickerDecision == 0)
                //{
                //    //set sticker 
                //    string printprn = "";
                //    var path = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn";
                //    var value = $"{_webHostEnvironment.WebRootPath}\\Sticker\\ErosVALUE.prn";

                //    foreach (inwardPacket Packet in applicant.inwardPacket.Where(f => f.check == "true"))

                //    {
                //        if (System.IO.File.Exists(path) == true)
                //        {
                //            System.IO.File.Delete(value);
                //            System.IO.File.Copy(path, value);

                //            int noofpkt = Convert.ToInt32(Packet.noofpackets);
                //            if (noofpkt > 0)
                //            {
                //                for (int q = 1; q <= int.Parse(Packet.quantity.ToString()); q++)
                //                {
                //                    for (int i = 1; i <= int.Parse(Packet.noofpackets); i++)
                //                    {
                //                        try
                //                        {
                //                            string value1 = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn"; // Replace with the actual file path

                //                            // Perform file manipulation operations
                //                            string fileContent = System.IO.File.ReadAllText(value1);
                //                            fileContent = fileContent.Replace("<D001>", Packet.productcode.ToString().Trim().ToUpper());
                //                            fileContent = fileContent.Replace("<D002>", Packet.description.ToString().Trim().ToUpper());
                //                            fileContent = fileContent.Replace("<D003>", applicant.grnno.ToString().Trim());
                //                            fileContent = fileContent.Replace("<D004>", q + "-" + i + "/" + Packet.noofpackets.ToString().Trim());
                //                            fileContent = fileContent.Replace("<D005>", Packet.qtyperpkt.ToString().Trim() + "/" + Packet.setofsub_assemb.ToString().Trim());
                //                            fileContent = fileContent.Replace("<D006>", applicant.batchcode.ToString().Trim());

                //                            System.IO.File.WriteAllText(value, fileContent);

                //                            //  printprn = printprn + value + Environment.NewLine;

                //                            //------Replacement of above comment command ------ //
                //                            string fileContent1 = System.IO.File.ReadAllText(value);
                //                            printprn = printprn + fileContent1 + Environment.NewLine;
                //                            //------Replacement of above comment command ------ //
                //                        }

                //                        catch
                //                        {
                //                            // Handle any exceptions
                //                        }
                //                    }
                //                }
                //            }
                //            else
                //            {
                //                int qty = Packet.quantity;
                //                int qtypershp = Packet.noqtypershp;
                //                float totalshipperCeil = (float)qty / qtypershp;
                //                int totalshipper = (int)Math.Ceiling(totalshipperCeil);

                //                int shp = 1;
                //                for (int i = 1; i <= totalshipper; i++)
                //                {
                //                    int remainingQty = qty - ((i - 1) * qtypershp); // Calculate remaining quantity for this shipper

                //                    // Determine the quantity for this shipper
                //                    int currentQty;
                //                    if (i < totalshipper)
                //                    {
                //                        currentQty = qtypershp;
                //                    }
                //                    else
                //                    {
                //                        currentQty = remainingQty;
                //                    }
                //                    string value1 = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn";
                //                    string fileContent = System.IO.File.ReadAllText(value1);
                //                    fileContent = fileContent.Replace("<D001>", Packet.productcode.ToString().Trim().ToUpper());
                //                    fileContent = fileContent.Replace("<D002>", Packet.description.ToString().Trim().ToUpper());
                //                    fileContent = fileContent.Replace("<D003>", applicant.grnno.ToString().Trim());
                //                    fileContent = fileContent.Replace("<D004>", $"{i}-{currentQty}/{shp.ToString().Trim()}");
                //                    fileContent = fileContent.Replace("<D005>", Packet.qtyperpkt.ToString().Trim() + "/" + Packet.setofsub_assemb.ToString().Trim());
                //                    fileContent = fileContent.Replace("<D006>", applicant.batchcode.ToString().Trim());
                //                    System.IO.File.WriteAllText(value, fileContent);

                //                    //  printprn = printprn + value + Environment.NewLine;

                //                    //------Replacement of above comment command ------ //
                //                    string fileContent1 = System.IO.File.ReadAllText(value);
                //                    printprn = printprn + fileContent1 + Environment.NewLine;
                //                    //------Replacement of above comment command ------ //
                //                }
                //            }


                //        }
                //        else
                //        {

                //        }
                //    }

                //    byte[] byteArray = Encoding.ASCII.GetBytes(printprn);
                //    MemoryStream stream = new MemoryStream(byteArray);
                //    return File(stream, "text/plain", "example.prn");
                //}
                //return RedirectToAction("Index");
                return Json(new { success = true, message = "Sale return process complete successfully , Please do storage of that stickers !" });
            }
            else
            {
                return Json(Ok());
            }
        }

        [HttpGet]
        public ActionResult EditStatus(int? id)
        {
            ViewBag.partyname = Getpartyname();
            ViewBag.pono = GetPONO();
            ViewBag.sono = GetSONO();
            ViewBag.description = Getdescription();

            if (id == null)
            {
                // If the id parameter is null, you can handle the error accordingly.
                return BadRequest();
            }

            var inward = _context.inward
                .Include(i => i.inwardPacket) // Include packets to avoid lazy loading
                .Where(po => po.inward_id == id)
                .FirstOrDefault();

            if (inward == null)
            {
                // If the record is not found, you can handle the error accordingly.
                return NotFound(); // or return RedirectToAction("Index");
            }

            // If there are no inwardPacket items, add a default one for display
            if (inward.inwardPacket.Count == 0)
            {
                inward.inwardPacket.Add(new inwardPacket { id = 1 });
            }

            // Assuming EditStatus.cshtml is your edit view
            return View("EditStatus", inward);
        }

        [HttpPost]
        //out or in both demo repl or repl
        public async Task<IActionResult> EditStatus(int id, inward inward)
        {
            var po_inward = _context.purchase.Where(a => a.id == id).Include(a => a.poProduct_details).FirstOrDefault();
            if(inward.batchcode == null)
            {
                inward.batchcode = "00A00";
            }
            if (inward.remarks == null)
            {
                inward.remarks = "-";
            }
            if (id != inward.inward_id)
            {
                return NotFound();
            }
            try
            {
                if (inward.ordertype == "Demo" || inward.ordertype == "Repair" || inward.ordertype == "Replacement")
                {
                    inward.sono = "-";
                    inward.pono = "-";
                    if (inward.typeofreturn == "Returnable")
                    {
                        inward.status = "Pending";
                    }
                    else if (inward.typeofreturn == "Non-Returnable")
                    {
                        inward.status = "Completed";
                    }
                    else if (inward.typeofreturn == "Billed")
                    {
                        inward.status = "Completed";
                    }
                    else if (inward.typeofreturn == "Returned")
                    {
                        inward.status = "Completed";
                    }
                    _context.Update(inward);

                    // Maintain logs
                    var user = HttpContext.Session.GetString("User");
                    var logs = new Logs()
                    {
                        pagename = "Transaction pending "+inward.ordertype +" Operation",
                        task = inward.inward_id + "$" + inward.pono + "$" + inward.sono + "$" + inward.vendername + "$" + inward.typeofreturn + "$" + inward.partyname + "$" + inward.gstinno + "$" + inward.contactno + "$" + inward.address + "$" + inward.dcno + "$" + inward.invoiceno + "$" + inward.dcdate + "$" + inward.invoicedate + "$" + inward.grnno + "$" + inward.grndate + "$" + inward.remarks + "$" + inward.batchcode + "$" + inward.date + "$" + inward.time + "$" + inward.ordertype + "$" + inward.status + "$" + inward.flag,
                        taskid = Convert.ToInt32(id),
                        //task = id.ToString() + " inward operation " + inward.ordertype + " edit status",
                        action = "Update",
                        date = DateTime.Now.ToString("dd/MM/yyyy"),
                        time = DateTime.Now.ToString("HH:mm:ss"),
                        username = user
                    };

                    _context.Add(logs);

                    await _context.SaveChangesAsync();
                    _notyfService.Success("Status Updated Succesfully");
                    return RedirectToAction("InwardPendingList", "inwards");

                }
                else if (inward.ordertype == "Purchase")
                {
                    inward.sono = "-";
                    inward.typeofreturn = "-";
                    if (inward.typeofreturn == "Returnable")
                    {
                        inward.status = "Pending";
                    }
                    else if (inward.typeofreturn == "Non-Returnable")
                    {
                        inward.status = "Completed";
                    }
                    else if (inward.typeofreturn == "Returned")
                    {
                        inward.status = "Completed";
                    }
                    _context.Update(inward);
                    // Maintain logs
                    var user = HttpContext.Session.GetString("User");
                    var logs = new Logs()
                    {
                        pagename = "Transaction pending " + inward.ordertype + " Operation",
                        task = inward.inward_id + "$" + inward.pono + "$" + inward.sono + "$" + inward.vendername + "$" + inward.typeofreturn + "$" + inward.partyname + "$" + inward.gstinno + "$" + inward.contactno + "$" + inward.address + "$" + inward.dcno + "$" + inward.invoiceno + "$" + inward.dcdate + "$" + inward.invoicedate + "$" + inward.grnno + "$" + inward.grndate + "$" + inward.remarks + "$" + inward.batchcode + "$" + inward.date + "$" + inward.time + "$" + inward.ordertype + "$" + inward.status + "$" + inward.flag,
                        taskid = Convert.ToInt32(id),
                        //task = id.ToString(),
                        action = "Update",
                        date = DateTime.Now.ToString("dd/MM/yyyy"),
                        time = DateTime.Now.ToString("HH:mm:ss"),
                        username = user,
                    };

                    _context.Add(logs);

                    await _context.SaveChangesAsync();
                    _notyfService.Success("Status Updated Succesfully");
                    return RedirectToAction("InwardPendingList", "inwards");
                }
                else if (inward.ordertype == "Salesreturn")
                {
                    inward.pono = "-";
                    inward.gstinno = "-";
                    if (inward.typeofreturn == "Returnable")
                    {
                        inward.status = "Pending";
                    }
                    else if (inward.typeofreturn == "Non-Returnable")
                    {
                        inward.status = "Completed";
                    }
                    else if (inward.typeofreturn == "Returned")
                    {
                        inward.status = "Completed";
                    }
                    _context.Update(inward);

                    // Maintain logs
                    var user = HttpContext.Session.GetString("User");
                    var logs = new Logs()
                    {
                        pagename = "Transaction pending " + inward.ordertype + " Operation",
                        task = inward.inward_id + "$" + inward.pono + "$" + inward.sono + "$" + inward.vendername + "$" + inward.typeofreturn + "$" + inward.partyname + "$" + inward.gstinno + "$" + inward.contactno + "$" + inward.address + "$" + inward.dcno + "$" + inward.invoiceno + "$" + inward.dcdate + "$" + inward.invoicedate + "$" + inward.grnno + "$" + inward.grndate + "$" + inward.remarks + "$" + inward.batchcode + "$" + inward.date + "$" + inward.time + "$" + inward.ordertype + "$" + inward.status + "$" + inward.flag,
                        taskid = Convert.ToInt32(id),
                        action = "Update",
                        date = DateTime.Now.ToString("dd/MM/yyyy"),
                        time = DateTime.Now.ToString("HH:mm:ss"),
                        username = user
                    };

                    _context.Add(logs);

                    await _context.SaveChangesAsync();
                    _notyfService.Success("Status Updated Succesfully");
                    return RedirectToAction("InwardPendingList", "inwards");
                }
                else
                {
                    return Ok();
                }

                //return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!inward_exist(inward.inward_id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
            return View(inward);
        }

        [HttpGet]
        public ActionResult EditStatuss(int? id)
        {
            ViewBag.partyname = Getpartyname();
            ViewBag.pono = GetPONO();
            ViewBag.sono = GetSONO();
            ViewBag.description = Getdescription();

            if (id == null)
            {
                // If the id parameter is null, you can handle the error accordingly.
                return BadRequest();
            }

            var inward = _context.inward
                .Include(i => i.inwardPacket) // Include packets to avoid lazy loading
                .Where(po => po.inward_id == id)
                .FirstOrDefault();

            if (inward == null)
            {
                // If the record is not found, you can handle the error accordingly.
                return NotFound(); // or return RedirectToAction("Index");
            }

            // If there are no inwardPacket items, add a default one for display
            if (inward.inwardPacket.Count == 0)
            {
                inward.inwardPacket.Add(new inwardPacket { id = 1 });
            }

            // Assuming EditStatuss.cshtml is your edit view
            return View("EditStatuss", inward);
        }

        [HttpPost]
        public async Task<IActionResult> EditStatuss(int id, inward inward)
        {
            if (id != inward.inward_id)
            {
                return NotFound();
            }

            try
            {
                List<inwardPacket> inwardPacket = _context.inwardPacket.Where(d => d.inwardId == inward.inward_id).ToList();
                _context.inwardPacket.RemoveRange(inwardPacket);
                //inward.date = DateTime.Now.ToUniversalTime();
                //inward.date = DateTime.Now.ToString("yyyy-MM-dd");

                // Set warranty to "No Warranty" for each product based on the status
                foreach (var product in inward.inwardPacket)
                {
                    if (inward.status == "Cancel")
                    {
                        //product.Warranty = "No";
                    }
                }

                if (inward.ordertype == "Purchase")
                {
                    inward.sono = "-";
                    inward.typeofreturn = "-";
                    if (inward.typeofreturn == "Returnable")
                    {
                        inward.status = "Pending";
                    }
                    else if (inward.typeofreturn == "Non-Returnable" || inward.typeofreturn == "Returned")
                    {
                        inward.status = "Completed";
                    }

                    _context.Update(inward);

                    // Maintain logs
                    var user = HttpContext.Session.GetString("User");
                    var logs = new Logs()
                    {
                        pagename = "In-Out " + inward.ordertype + " Operation",
                        task = inward.inward_id + "$" + inward.pono + "$" + inward.sono + "$" + inward.vendername + "$" + inward.typeofreturn + "$" + inward.partyname + "$" + inward.gstinno + "$" + inward.contactno + "$" + inward.address + "$" + inward.dcno + "$" + inward.invoiceno + "$" + inward.dcdate + "$" + inward.invoicedate + "$" + inward.grnno + "$" + inward.grndate + "$" + inward.remarks + "$" + inward.batchcode + "$" + inward.date + "$" + inward.time + "$" + inward.ordertype + "$" + inward.status + "$" + inward.flag,
                        taskid = Convert.ToInt32(id),
                        action = "Update",
                        date = DateTime.Now.ToString("dd/MM/yyyy"),
                        time = DateTime.Now.ToString("HH:mm:ss"),
                        username = user
                    };

                    _context.Add(logs);

                    await _context.SaveChangesAsync();

                    return RedirectToAction("StockMovementList", "inwards");
                    _notyfService.Success("Record status update Successfully");
                }
                else if (inward.ordertype == "Salesreturn")
                {
                    inward.pono = "-";
                    inward.gstinno = "-";
                    if (inward.typeofreturn == "Returnable")
                    {
                        inward.status = "Pending";
                    }
                    else if (inward.typeofreturn == "Non-Returnable" || inward.typeofreturn == "Returned")
                    {
                        inward.status = "Completed";
                    }

                    _context.Update(inward);

                    // Maintain logs
                    var user = HttpContext.Session.GetString("User");
                    var logs = new Logs()
                    {
                        pagename = "In-Out " + inward.ordertype + " Operation",
                        task = inward.inward_id + "$" + inward.pono + "$" + inward.sono + "$" + inward.vendername + "$" + inward.typeofreturn + "$" + inward.partyname + "$" + inward.gstinno + "$" + inward.contactno + "$" + inward.address + "$" + inward.dcno + "$" + inward.invoiceno + "$" + inward.dcdate + "$" + inward.invoicedate + "$" + inward.grnno + "$" + inward.grndate + "$" + inward.remarks + "$" + inward.batchcode + "$" + inward.date + "$" + inward.time + "$" + inward.ordertype + "$" + inward.status + "$" + inward.flag,
                        taskid = Convert.ToInt32(id),
                        action = "Update",
                        date = DateTime.Now.ToString("dd/MM/yyyy"),
                        time = DateTime.Now.ToString("HH:mm:ss"),
                        username = user
                    };

                    _context.Add(logs);

                    await _context.SaveChangesAsync();

                    return RedirectToAction("StockMovementList", "inwards");
                    //_notyfService.Success("Record status update Successfully");
                }
                else
                {
                    return Ok();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!inward_exist(inward.inward_id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // The following line is not needed
            // return View(inward);
        }

        public IActionResult GetStoreOperations(string productCode, string pono, string batch)
        {
            var ponoin = _context.inward.Where(a => a.pono.Trim() == pono.Trim()).FirstOrDefault();
            var grn = ponoin.grnno;

            List<Storage_Operation> stlist = new List<Storage_Operation>();

            var storeOperations = _context.Storage_Operation
                                .Where(a => a.productcode.Trim().ToUpper() == productCode.Trim().ToUpper()
                                    && a.batchcode.Trim() == batch.Trim()
                                    && a.statusflag == "LD")
                                .ToList();

            if (storeOperations.Count == 0)
            {
                var seachInLoading = _context.Loading_Dispatch_Operation
                    .Where(a => a.productcode.Trim().ToUpper() == productCode.Trim()
                    && a.batchcode.Trim() == batch.Trim())
                    .ToList();
                if(seachInLoading.Count > 0)
                {
                   
                    foreach(var item in seachInLoading)
                    {
                        
                        var st = new Storage_Operation()
                        {
                            locationcode = item.location,
                            batchcode = item.batchcode,
                            boxno = item.boxno,
                            productcode = item.productcode,
                        };
                        var location = _context.Picking_Operation.Where(a => a.productcode.Trim() == productCode.Trim() && a.batchcode.Trim() == batch.Trim()).FirstOrDefault();
                        if (location != null)
                        {
                            st.locationcode = location.location;
                        }
                        else
                        {
                            st.locationcode = "-";
                        }
                        stlist.Add(st);
                    }
                }
            }
            else
            {
                stlist.AddRange(storeOperations);
            }
            return Json(stlist);
        }

        [HttpGet]
        //public IActionResult Reprinting(string referenceNo)
        //{
        //    ViewBag.partyname = Getpartyname();
        //    ViewBag.pono = GetPONO();
        //    ViewBag.sono = GetSONO();
        //    ViewBag.description = Getdescription();

        //    if (string.IsNullOrWhiteSpace(referenceNo))
        //    {
        //        return BadRequest("Reference number is required.");
        //    }

        //    var inward = _context.inward
        //        .Include(i => i.inwardPacket)
        //        .FirstOrDefault(po => po.pono.Trim() == referenceNo.Trim());

        //    if (inward == null)
        //    {
        //        var sonodata = _context.so_inward.FirstOrDefault(so => so.sono.Trim() == referenceNo.Trim());

        //        if (sonodata != null)
        //        {
        //            // Log to ensure we're entering this block
        //            Console.WriteLine("SONO data found, rendering Reprinting1");

        //            var loadingList = _context.Loading_Dispatch_Operation
        //                .Where(ldo => ldo.sono.Trim() == referenceNo.Trim())
        //                .ToList();

        //            foreach (var item in loadingList)
        //            {
        //                if (item.customer == null)
        //                {
        //                    item.customer = sonodata.customername.Trim();
        //                }
        //            }

        //            var viewModel = new InwardDispatchViewModel
        //            {
        //                SoinwardList = new List<so_inward>
        //        {
        //            new so_inward
        //            {
        //                sono = sonodata.sono,
        //                customername = sonodata.customername,
        //                contactno = sonodata.contactno,
        //                address = sonodata.address
        //            }
        //        },
        //                LoadingDispatchList = loadingList.Select(ld => new Loading_Dispatch_Operation
        //                {
        //                    productcode = ld.productcode,
        //                    boxno = ld.boxno,
        //                    batchcode = ld.batchcode,
        //                    location = ld.location
        //                }).ToList()
        //            };
        //            //return View("inwards/Reprinting1", viewModel);
        //            //return RedirectToAction("Reprinting1", "Inwards");
        //            return View("Reprinting1", viewModel); // Pass the view model to the view
        //        }
        //        else
        //        {
        //            // Log that SONO data wasn't found
        //            Console.WriteLine("SONO data not found");
        //        }
        //    }

        //    // Log to ensure we're rendering Reprinting view
        //    Console.WriteLine("Rendering Reprinting view");
        //    return View("Reprinting", inward);
        //}
        //public IActionResult Reprinting(string referenceNo)
        //{
        //    if (string.IsNullOrWhiteSpace(referenceNo))
        //    {
        //        return BadRequest("Reference number is required.");
        //    }

        //    var inward = _context.inward
        //        .Include(i => i.inwardPacket)
        //        .FirstOrDefault(po => po.pono.Trim() == referenceNo.Trim());

        //    //convert the grn date formate into dd-mm-yyyy from yyyy-mm-dd

        //    if (inward != null)
        //    {
        //        return View("Reprinting", inward);

        //    }
        //    // Log to ensure we're rendering Reprinting view
        //    return View("Reprinting", inward);
        //}
        public IActionResult Reprinting(string referenceNo)
        {
            if (string.IsNullOrWhiteSpace(referenceNo))
            {
                return BadRequest("Reference number is required.");
            }

            var inward = _context.inward
                .Include(i => i.inwardPacket)
                .FirstOrDefault(po => po.pono.Trim() == referenceNo.Trim());

            if (inward != null)
            {
                // Assuming inward has a field grnDate which is a string
                DateTime grnDateParsed;
                if (DateTime.TryParse(inward.grndate, out grnDateParsed))
                {
                    // Format the grnDate to dd-MM-yyyy
                    inward.grndate = grnDateParsed.ToString("dd-MM-yyyy");
                }

                return View("Reprinting", inward);
            }

            // Log to ensure we're rendering Reprinting view
            return View("Reprinting", inward);
        }

        [HttpGet]
        public IActionResult Reprinting2(string referenceNo)
        {
            if (string.IsNullOrWhiteSpace(referenceNo))
            {
                return BadRequest("Reference number is required.");
            }

            var sonodata = _context.so_inward.FirstOrDefault(so => so.sono.Trim() == referenceNo.Trim());

            if (sonodata != null)
            {
                var loadingList = _context.Loading_Dispatch_Operation
                    .Where(ldo => ldo.sono.Trim() == referenceNo.Trim())
                    .ToList();

                foreach (var item in loadingList)
                {
                    if (item.customer == null)
                    {
                        item.customer = sonodata.customername.Trim();
                    }
                    var found = _context.Picking_Operation.Where(a => a.sono.Trim() == item.sono.Trim() && a.productcode.Trim() == item.productcode.Trim() && a.boxno.Trim() == item.boxno.Trim() && a.batchcode.Trim() == item.batchcode.Trim()).FirstOrDefault();
                    if (found != null)
                    {
                        item.location = found.location.Trim();
                    }
                }

                var viewModel = new InwardDispatchViewModel
                {
                    //var getloc=_context.pickstorage.Where(a=>a.batchcode.Trim() == )

                    SoinwardList = new List<so_inward>
                    {
                        new so_inward
                        {
                            sono = sonodata.sono,
                            customername = sonodata.customername,
                            contactno = sonodata.contactno,
                            address = sonodata.address
                        }
                    },
                    
                    LoadingDispatchList = loadingList.Select(ld => new Loading_Dispatch_Operation
                    {
                        productcode = ld.productcode,
                        boxno = ld.boxno,
                        batchcode = ld.batchcode,
                        location = ld.location
                    }).ToList()
                };
                return View(viewModel);
            }
            else
            {
            }
            return View();
        }
        public class SelectedItem
        {
            public string ProductCode { get; set; }
            public string BoxNo { get; set; }
            public string BatchCode { get; set; }
            public string Location { get; set; }
        }

        [HttpPost]
        public IActionResult Reprinting2(string sono, List<SelectedItem> selectedItems)
        {
            try
            {
                //set sticker 
                var batch = "-";
                var grn = "-";
                var qtyperset = "-";
                var pro = "-";
                var des = "-";
                var box = "-";
                var qtyperpkt = "-";

                List<Loading_Dispatch_Operation> ldlist = new List<Loading_Dispatch_Operation>();
                //set sticker 
                foreach (var Packet in selectedItems)
                {
                    var data = _context.inward
                                .Where(a => a.batchcode.Trim() == Packet.BatchCode.Trim())
                                .FirstOrDefault();

                    if (data != null && !string.IsNullOrEmpty(data.grnno))
                    {
                        grn = data.grnno.Trim();
                    }
                    else
                    {
                        grn = "-";
                    }
                    var found1 = _context.Product_Master.Where(a => a.productcode.Trim() == Packet.ProductCode.Trim()).Select(a => a.productdescription.Trim()).FirstOrDefault();
                    if (found1 != null)
                    {
                        des = found1;
                    }
                    else
                    {
                        des = "-";
                    }
                    var check = _context.inward
                                .Where(a => a.batchcode.Trim() == Packet.BatchCode.Trim()).ToList();
                    List<inwardPacket> inlist = new List<inwardPacket>();
                    foreach(var i in check)
                    {
                        var found2 = _context.inwardPacket.Where(a => a.inwardId == i.inward_id).ToList();
                        if (found2.Count > 0)
                        {
                            foreach(var b in found2)
                            {
                                inlist.Add(b);
                            }
                        }
                    }
                    if (inlist.Count > 0)
                    {
                        var get = inlist.Where(a => a.productcode.Trim() == Packet.ProductCode.Trim()).FirstOrDefault();
                        qtyperpkt = get.qtyperpkt.ToString().Trim() + "/"+ get.setofsub_assemb.ToString();
                    }
                    else
                    {
                        qtyperpkt = "-";
                    }
                    //Packet.qtyperpkt.ToString().Trim() + "/" + Packet.setofsub_assemb
                    Loading_Dispatch_Operation ld = new Loading_Dispatch_Operation()
                    {
                        sono = sono,
                        productcode = Packet.ProductCode.Trim(),
                        batchcode = Packet.BatchCode.Trim(),
                        grn = grn,
                        productname = des,
                        qtyperset = qtyperpkt,
                        boxno = Packet.BoxNo,
                    };
                    ldlist.Add(ld);
                }

                //FOR INWARD
                string printprn = "";
                var path = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn";
                var value = $"{_webHostEnvironment.WebRootPath}\\Sticker\\ErosVALUE.prn";
                if (System.IO.File.Exists(path) == true)
                {
                    System.IO.File.Delete(value);
                    System.IO.File.Copy(path, value);
                    foreach(var Packet in ldlist)
                    {
                        try
                        {
                            string value1 = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn"; // Replace with the actual file path
                            string fileContent = System.IO.File.ReadAllText(value1);
                            fileContent = fileContent.Replace("<D001>", Packet.productcode.ToString().Trim().ToUpper());
                            fileContent = fileContent.Replace("<D002>", Packet.productname.ToString().Trim().ToUpper());
                            fileContent = fileContent.Replace("<D003>", Packet.grn.ToString().Trim());
                            fileContent = fileContent.Replace("<D004>", Packet.boxno.Trim());
                            fileContent = fileContent.Replace("<D005>", Packet.qtyperset.Trim());
                            fileContent = fileContent.Replace("<D006>", Packet.batchcode.ToString().Trim());
                            System.IO.File.WriteAllText(value, fileContent);
                            string fileContent1 = System.IO.File.ReadAllText(value);
                            printprn = printprn + fileContent1 + Environment.NewLine;
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                }
                byte[] byteArray = Encoding.ASCII.GetBytes(printprn);
                MemoryStream stream = new MemoryStream(byteArray);
                //SELECTED ROWS

                //ADD REPRINTING DATA
                foreach(var item in ldlist)
                {
                    var maxIdq = _context.ReprintingRemark.Any() ? _context.ReprintingRemark.Max(e => e.id) + 1 : 1;
                    ReprintingRemark ReprintingRemark = new ReprintingRemark
                    {
                        id = maxIdq,
                        sono = sono.Trim(),
                        remark = "Return Process !",
                        batch = item.batchcode.Trim(),
                        grn = item.grn,
                        date = DateTime.Now.ToString("dd/MM/yyyy"),
                        time = DateTime.Now.ToString("HH:mm:ss"),
                    };
                    _context.ReprintingRemark.Add(ReprintingRemark);
                    _context.SaveChanges();
                }
                
                return File(stream, "text/plain", "example.prn");
                // return Json(new { success = true, fileContent = Convert.ToBase64String(byteArray) });
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }
        //{
        //    ViewBag.partyname = Getpartyname();
        //    ViewBag.pono = GetPONO();
        //    ViewBag.sono = GetSONO();
        //    ViewBag.description = Getdescription();

        //    if (referenceNo == null)
        //    {
        //        // If the id parameter is null, you can handle the error accordingly.
        //        return BadRequest();
        //    }

        //    var inward = _context.inward
        //        .Include(i => i.inwardPacket) // Include packets to avoid lazy loading
        //        .Where(po => po.pono.Trim() == referenceNo.Trim())
        //        .FirstOrDefault();

        //    if (inward == null)
        //    {
        //        // If the record is not found, you can handle the error accordingly.
        //        return NotFound(); // or return RedirectToAction("Index");
        //    }

        //    // If there are no inwardPacket items, add a default one for display
        //    if (inward.inwardPacket.Count == 0)
        //    {
        //        inward.inwardPacket.Add(new inwardPacket { id = 1 });
        //    }

        //    // Assuming EditStatuss.cshtml is your edit view
        //    return View("Reprinting", inward);
        //    _notyfService.Success(" Reprint Successfull");
        //}


        public class SelectedRowData
        {
            public string ProductCode { get; set; }
            public string BoxNo { get; set; }
            public string LocationCode { get; set; }
            public string BatchCode { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Reprinting(int id, inward inward, List<SelectedRowData> selectedRows)
        {
           

            try
            {
                //set sticker 
                var batch = "-";
                var grn = "-";
                var qtyperset = "-";
                var pro = "-";
                var des = "-";
                var box = "-";

                //set sticker 
                foreach (inwardPacket Packet in inward.inwardPacket)
                {
                   
                    int noofpkt = Convert.ToInt32(Packet.noofpackets);
                    if (noofpkt > 0)
                    {
                        for (int q = 1; q <= int.Parse(Packet.quantity.ToString()); q++)
                        {
                            for (int i = 1; i <= int.Parse(Packet.noofpackets); i++)
                            {
                                try
                                {
                                    pro = Packet.productcode;
                                    des = Packet.description;
                                    grn = inward.grnno.ToString().Trim();
                                    box = q + "-" + i + "/" + Packet.noofpackets;
                                    qtyperset = Packet.qtyperpkt.ToString().Trim() + "/" + Packet.setofsub_assemb;
                                    batch = inward.batchcode;
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                }

                //FOR INWARD
                string printprn = "";
                var path = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn";
                var value = $"{_webHostEnvironment.WebRootPath}\\Sticker\\ErosVALUE.prn";
                foreach (inwardPacket Packet in inward.inwardPacket)
                //foreach (inwardPacket Packet in inward.inwardPacket)
                {
                    if (Packet.check == "true")
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
                                                string fileContent = System.IO.File.ReadAllText(value1);
                                                fileContent = fileContent.Replace("<D001>", Packet.productcode.ToString().Trim().ToUpper());
                                                fileContent = fileContent.Replace("<D002>", Packet.description.ToString().Trim().ToUpper());
                                                fileContent = fileContent.Replace("<D003>", inward.grnno.ToString().Trim());
                                                fileContent = fileContent.Replace("<D004>", q + "-" + i + "/" + Packet.noofpackets.ToString().Trim());
                                                fileContent = fileContent.Replace("<D005>", Packet.qtyperpkt.ToString().Trim() + "/" + Packet.setofsub_assemb.ToString().Trim());
                                                fileContent = fileContent.Replace("<D006>", inward.batchcode.ToString().Trim());
                                                System.IO.File.WriteAllText(value, fileContent);
                                                string fileContent1 = System.IO.File.ReadAllText(value);
                                                printprn = printprn + fileContent1 + Environment.NewLine;
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (SHPlist.Count > 0)
                                    {
                                        var data = SHPlist.Where(a => a.productcode.Trim() == Packet.productcode.Trim()).OrderBy(a => a.noqtypershp).ToList();
                                        if (data.Count > 0)
                                        {
                                            foreach (var item in data)
                                            {
                                                string value1 = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn";
                                                string fileContent = System.IO.File.ReadAllText(value1);
                                                fileContent = fileContent.Replace("<D001>", Packet.productcode.ToString().Trim().ToUpper());
                                                fileContent = fileContent.Replace("<D002>", Packet.description.ToString().Trim().ToUpper());
                                                fileContent = fileContent.Replace("<D003>", inward.grnno.ToString().Trim());
                                                fileContent = fileContent.Replace("<D004>", $"{item.noqtypershp}-{item.quantity}/{1}");
                                                fileContent = fileContent.Replace("<D005>", Packet.qtyperpkt.ToString().Trim() + "/" + Packet.setofsub_assemb.ToString().Trim());
                                                fileContent = fileContent.Replace("<D006>", inward.batchcode.ToString().Trim());
                                                System.IO.File.WriteAllText(value, fileContent);

                                                //  printprn = printprn + value + Environment.NewLine;

                                                //------Replacement of above comment command ------ //
                                                string fileContent1 = System.IO.File.ReadAllText(value);
                                                printprn = printprn + fileContent1 + Environment.NewLine;
                                            }
                                        }
                                    }

                                }
                        }
                        else
                            {
                            }
                }

                if (selectedRows.Count > 0)
                    {
                        if (System.IO.File.Exists(path) == true)
                        {
                            System.IO.File.Delete(value);
                            System.IO.File.Copy(path, value);

                            foreach (var item in selectedRows)
                            {
                                try
                                {
                                    string value1 = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Eros.prn"; // Replace with the actual file path
                                    string fileContent = System.IO.File.ReadAllText(value1);
                                    fileContent = fileContent.Replace("<D001>", pro.ToString().Trim().ToUpper());
                                    fileContent = fileContent.Replace("<D002>", des.ToString().Trim().ToUpper());
                                    fileContent = fileContent.Replace("<D003>", grn.ToString().Trim());
                                    fileContent = fileContent.Replace("<D004>", item.BoxNo.ToString().Trim());
                                    fileContent = fileContent.Replace("<D005>", qtyperset.ToString().Trim());
                                    fileContent = fileContent.Replace("<D006>", batch.ToString().Trim());
                                    System.IO.File.WriteAllText(value, fileContent);
                                    string fileContent1 = System.IO.File.ReadAllText(value);
                                    printprn = printprn + fileContent1 + Environment.NewLine;
                                }
                                catch
                                {
                                }

                            }
                        }
                        else
                        {
                        }
                    }
                }
                byte[] byteArray = Encoding.ASCII.GetBytes(printprn);
                MemoryStream stream = new MemoryStream(byteArray);

                //SELECTED ROWS
                

                //ADD REPRINTING DATA
                int maxIdS2 = _context.ReprintingRemark.Any() ? _context.ReprintingRemark.Max(e => e.id) : 0;
                maxIdS2++;
                ReprintingRemark ReprintingRemark = new ReprintingRemark
                {
                    id = maxIdS2,
                    sono = inward.pono.Trim(),
                    remark = inward.remark.Trim(),
                    batch = inward.batchcode.Trim(),
                    grn = inward.grnno,
                    date = DateTime.Now.ToString("dd/MM/yyyy"),
                    time = DateTime.Now.ToString("HH:mm:ss"),
                };
                _context.ReprintingRemark.Add(ReprintingRemark);
                _context.SaveChanges();

                var user = HttpContext.Session.GetString("User");
                var logs = new Logs
                {
                    pagename = "Inward Operation",
                    task = id.ToString() + "id,  Reprinting Operation done with remark " + inward.remark,
                    taskid = id, // Assuming maxId is already defined elsewhere
                    date = DateTime.Now.ToString("dd/MM/yyyy"),
                    time = DateTime.Now.ToString("HH:mm:ss"),
                    action = "View",
                    username = user
                };

                // Add logs to the context
                _context.Add(logs);
                _context.SaveChanges();
                return File(stream, "text/plain", "example.prn");
                // return Json(new { success = true, fileContent = Convert.ToBase64String(byteArray) });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!inward_exist(inward.inward_id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // The following line is not needed
            // return View(inward);
        }

        // Class to deserialize JSON data sent from client
        public class CombinedData
    {
        public Dictionary<string, string> FormDataObject { get; set; }
        public List<SelectedRow> SelectedRows { get; set; }
    }

    public class SelectedRow
    {
        public string ProductCode { get; set; }
        public string BoxNo { get; set; }
        public string LocationCode { get; set; }
        public string BatchCode { get; set; }
    }

    private bool inward_exist(int id)
        {
            return (_context.inward?.Any(e => e.inward_id == id)).GetValueOrDefault();
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            // Maintain logs
            //if (user == null)
            //{
            //    // Redirect to login page or handle unauthorized access
            //    return RedirectToAction("Login", "Account");
            //}
            var user = HttpContext.Session.GetString("User");

            var logs = new Logs
            {
                pagename = "Inward Operation",
                task = id.ToString()+ " Inward Operation view",
                //task = "View Inward Operation",
                taskid = id, // Assuming maxId is already defined elsewhere
                date = DateTime.Now.ToString("dd/MM/yyyy"),
                time = DateTime.Now.ToString("HH:mm:ss"),
                action = "View",
                username = user
            };

            // Add logs to the context
            _context.Add(logs);
            _context.SaveChanges();

            var inwardSupplier_Master = _context.inward
                .Include(e => e.inwardPacket)
                .FirstOrDefault(a => a.inward_id == id);

            if (inwardSupplier_Master == null)
            {
                return NotFound();
            }

            return View(inwardSupplier_Master);
        }
        [HttpGet]
        public IActionResult PendingDetails(int id)
        {
            var user = HttpContext.Session.GetString("User");

            var logs = new Logs
            {
                pagename = "Inward Operation",
                //task = "View Inward PendingDetail",
                task = id.ToString() + " Inward Operation view",
                taskid = id, // Assuming maxId is already defined elsewhere
                date = DateTime.Now.ToString("dd/MM/yyyy"),
                time = DateTime.Now.ToString("HH:mm:ss"),
                action = "View",
                username = user
            };

            // Add logs to the context
            _context.Add(logs);
            _context.SaveChanges();

            inward inward = _context.inward.Include(e => e.inwardPacket).Where(a => a.inward_id == id).FirstOrDefault();
            return View(inward);
        }
        [HttpGet]
        public IActionResult PendingDetails1(int id)
        {
            var user = HttpContext.Session.GetString("User");

            var logs = new Logs
            {
                pagename = "Inward Operation",
                //task = "View Inward PendingDetail",
                task = id.ToString() + " Inward Operation view",
                taskid = id, // Assuming maxId is already defined elsewhere
                date = DateTime.Now.ToString("dd/MM/yyyy"),
                time = DateTime.Now.ToString("HH:mm:ss"),
                action = "View",
                username = user
            };

            // Add logs to the context
            _context.Add(logs);
            _context.SaveChanges();

            inward inward = _context.inward.Include(e => e.inwardPacket).Where(a => a.inward_id == id).FirstOrDefault();
            return View(inward);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            var data = await _context.inward.Include(e => e.inwardPacket).FirstOrDefaultAsync(a => a.inward_id == id);

            if (data == null)
            {
                return NotFound();
            }

            var scdata = _context.inward_subcomponent.Where(a => a.inwardpacket_id == data.inward_id).ToList();

            _context.inward_subcomponent.RemoveRange(scdata);

            var indata = _context.inwardPacket.Where(a => a.inwardId == data.inward_id).ToList();

            var purchasefound = _context.purchase.Where(a => a.pono == data.pono).FirstOrDefault();
            if (purchasefound != null)
            {
                if (purchasefound.status == "Completed")
                {
                    purchasefound.status = "Pending";
                    _context.Update(purchasefound);
                }
            }
            //maintain logs
            var user = HttpContext.Session.GetString("User");
            var logs = new Logs();
            logs.pagename = "Inward Operation";
            logs.task = data.inward_id + "$" +data.pono + "$" +data.sono + "$" +data.vendername + "$" +data.typeofreturn + "$" +data.partyname + "$" +data.gstinno + "$" +data.contactno + "$" +data.address + "$" +data.dcno + "$" +data.invoiceno + "$" +data.dcdate + "$" +data.invoicedate + "$" +data.grnno + "$" +data.grndate + "$" +data.remarks + "$" +data.batchcode + "$" +data.date + "$" +data.time + "$" +data.ordertype + "$" +data.status + "$" +data.flag ;
            logs.taskid = Convert.ToInt32(id);
            //logs.task=id.ToString();
            logs.date = DateTime.Now.ToString("dd/MM/yyyy");
            logs.time = DateTime.Now.ToString("HH:mm:ss");
            logs.action = "Delete";
            logs.username = user;
            _context.Add(logs);

            _context.inwardPacket.RemoveRange(indata);

            _context.inward.Remove(data);

            _context.SaveChanges();

            _notyfService.Error("Record Deleted Successfully");
            return RedirectToAction(nameof(Index));
            //if (_context.inward == null)
            //{
            //    return Problem("Entity set 'AuthDbContext.Supplier_Master'  is null.");
            //}
            //var data = _context.inward.Include(e => e.inwardPacket).Where(a => a.inward_id == id).FirstOrDefault();
            //var scdata = _context.inward_subcomponent.Where(a => a.inwardpacket_id == data.inward_id).ToList();

            //if (data != null)
            //{
            //    _context.inward.Remove(data);
            //}

            //_context.SaveChanges();
            //_notyfService.Error("Record Deleted Succesfully");
            //return RedirectToAction(nameof(Index));
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var data = await _context.inward.Include(e => e.inwardPacket).FirstOrDefaultAsync(a => a.inward_id == id);

            if (data == null)
            {
                return NotFound();
            }

            var scdata = _context.inward_subcomponent.Where(a => a.inwardpacket_id == data.inward_id).ToList();

            _context.inward_subcomponent.RemoveRange(scdata);

            var indata = _context.inwardPacket.Where(a => a.inwardId == data.inward_id).ToList();

            var purchasefound = _context.purchase.Where(a=>a.pono ==  data.pono).FirstOrDefault();
            if (purchasefound != null)
            {
                if(purchasefound.status == "Completed")
                {
                    purchasefound.status = "Pending";
                    _context.Update(purchasefound);
                }
            }
            

            _context.inwardPacket.RemoveRange(indata);

            _context.inward.Remove(data);

            _context.SaveChanges();
            
            _notyfService.Error("Record Deleted Successfully");


            return RedirectToAction(nameof(Index));
        }
        private bool so_inwardExists(int id)
        {
            return (_context.so_inward?.Any(e => e.id == id)).GetValueOrDefault();
        }

    }
}
