using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AuthSystem.Data;
using eros.Models;
using AspNetCoreHero.ToastNotification.Notyf;
using AspNetCoreHero.ToastNotification.Abstractions;
using static eros.Controllers.Picking_OperationController;
using System.Text.RegularExpressions;
using Nest;
using Microsoft.Identity.Client.Extensions.Msal;
using AspNetCore;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Office2010.Excel;
using iText.StyledXmlParser.Css.Page;
using Syncfusion.EJ2.Linq;
using System.Collections.Immutable;

namespace eros.Controllers
{
    public class Storage_OperationController : Controller
    {
        private readonly ErosDbContext _context;
        public INotyfService _notifyService { get; }
        public Storage_OperationController(ErosDbContext context, INotyfService notyfService)
        {
            _context = context;
            _notifyService = notyfService;
        }
        private static List<Storage_Operation> Storage_OperationAdd = new List<Storage_Operation>();
        [HttpPost]
        public IActionResult FilterOrdersData(string orderType)
        {
            List<Storage_Operation> storagelist = new List<Storage_Operation>();
            if (orderType == null)
            {
                var list = _context.Storage_Operation.Where(a => a.statusflag.Trim() == "ST").OrderByDescending(a=>a.id).ToList();
                foreach (var item in list)
                {
                    storagelist.Add(item);
                }
            }
            else if (orderType.Trim() == "DM")
            {
                var list = _context.DMRPRRP.Where(a => a.statusflag.Trim() == "ST" && a.pickflag == 0 && a.type.Trim() == orderType.Trim()).ToList();
                foreach (var item in list)
                {
                    Storage_Operation st = new Storage_Operation()
                    {
                        productcode = item.productcode,
                        boxno = item.boxno,
                        grnno = item.grn,
                        batchcode = item.batch,
                        locationcode = item.location,
                    };
                    storagelist.Add(st);
                }
            }
            else if (orderType.Trim() == "DMG")
            {
                var list = _context.Storage_Operation.Where(a => a.statusflag.Trim() == "DMG" && a.pickflag.Trim() == "0" ).ToList();
                foreach (var item in list)
                {
                    Storage_Operation st = new Storage_Operation()
                    {
                        productcode = item.productcode,
                        boxno = item.boxno,
                        grnno = item.grnno,
                        batchcode = item.batchcode,
                        locationcode = item.locationcode,
                    };
                    storagelist.Add(st);
                }
            }
            //else if (orderType.Trim() == "SR")
            //{
            //    var list = _context.PR_model.Where(a => a.statusflag.Trim() == "ST" && a.ordertype.Trim() == orderType.Trim()).ToList();
            //    //var list2 = _context.Storage_Operation.Where(a => a.statusflag.Trim() == "ST" && a.pickflag.Trim() == "0").ToList();
            //    foreach (var item in list)
            //    {
            //        var found  = _context.Storage_Operation.Where(a=>a.productcode.Trim() == item.productcode.Trim() )
            //        Storage_Operation st = new Storage_Operation()
            //        {
            //            productcode = item.productcode,
            //            boxno = item.boxno,
            //            grnno = item.grnno,
            //            batchcode = item.batchcode,
            //            locationcode = item.locationcode,
            //        };
            //        storagelist.Add(st);
            //    }
            //}
            else if (orderType.Trim() == "RPR")
            {
                var list = _context.DMRPRRP.Where(a => a.statusflag.Trim() == "ST" && a.pickflag == 0 && a.type.Trim() == orderType.Trim()).ToList();
                foreach (var item in list)
                {
                    Storage_Operation st = new Storage_Operation()
                    {
                        productcode = item.productcode,
                        boxno = item.boxno,
                        grnno = item.grn,
                        batchcode = item.batch,
                        locationcode = item.location,
                    };
                    storagelist.Add(st);
                }
            }
            else if (orderType.Trim() == "RP")
            {
                var list = _context.DMRPRRP.Where(a => a.statusflag.Trim() == "DMG" && a.pickflag == 0 && a.type.Trim() == orderType.Trim()).ToList();
                foreach (var item in list)
                {
                    Storage_Operation st = new Storage_Operation()
                    {
                        productcode = item.productcode,
                        boxno = item.boxno,
                        grnno = item.grn,
                        batchcode = item.batch,
                        locationcode = item.location,
                    };
                    storagelist.Add(st);
                }
            }
            else if (orderType.Trim() == "NONRPR")
            {
                var list = _context.DMRPRRP.Where(a => a.statusflag.Trim() == "NONRPR" && a.pickflag == 0).ToList();
                foreach (var item in list)
                {
                    Storage_Operation st = new Storage_Operation()
                    {
                        productcode = item.productcode,
                        boxno = item.boxno,
                        grnno = item.grn,
                        batchcode = item.batch,
                        locationcode = item.location,
                    };
                    storagelist.Add(st);
                }
            }
            else
            {
                var list = _context.Storage_Operation.Where(a => a.statusflag.Trim() == "ST").ToList();
                foreach (var item in list)
                {
                    storagelist.Add(item);
                }
            }

            return Json(new { success = true, data = storagelist });
        }
        [HttpPost]
        public IActionResult DeleteFromList_DMG(string productCode, string batchCode, string box, string grnno)
        {
            var splitbox1 = box.Split("-");
            var firstpart = splitbox1[0];
            var secondpart = splitbox1[1];
            var splitsecand = splitbox1[1].Split("/");
            var numerator = splitsecand[0];
            var denominator = splitsecand[1];

            if (Convert.ToInt64(numerator) > Convert.ToInt64(denominator))
            {
                return Json(new { success = false, message = "Functionality Not defined !" });
            }
            else
            {
                var found = Storage_OperationAdd
                        .Where(a => a.productcode.Trim().ToUpper() == productCode.Trim().ToUpper()
                        && a.grnno.Trim() == grnno.Trim()
                        && a.batchcode.Trim() == batchCode.Trim()
                        && a.boxno.Trim() == box.Trim())
                        .FirstOrDefault();

                if (found != null)
                {
                    Storage_OperationAdd.Remove(found);
                    return Json(new { success = true, data = found });
                }
                else
                {
                    return Json(new { success = false, message = "Shipper Not Found !" });
                }
            }

        }
        [HttpPost]
        public IActionResult DeleteFromList(string productCode,string batchCode,string box,string grnno)
        {
            var splitbox1 = box.Split("-");
            var firstpart = splitbox1[0];
            var secondpart = splitbox1[1];
            var splitsecand = splitbox1[1].Split("/");
            var numerator = splitsecand[0];
            var denominator = splitsecand[1];

            if (Convert.ToInt64(numerator) > Convert.ToInt64(denominator))
            {
                return Json(new { success = false, message = "Functionality Not defined !" });
            }
            else
            {
                var found = Storage_OperationAdd
                        .Where(a => a.productcode.Trim().ToUpper() == productCode.Trim().ToUpper()
                        && a.grnno.Trim() == grnno.Trim()
                        && a.batchcode.Trim() == batchCode.Trim()
                        && a.boxno.Trim() == box.Trim())
                        .FirstOrDefault();

                if (found != null)
                {
                    Storage_OperationAdd.Remove(found);
                    return Json(new { success = true, data = found });
                }
                else
                {
                    return Json(new { success = false, message = "Shipper Not Found !" });
                }
            }
                
        }
        [HttpPost]
        public IActionResult saveInSTList(string batch, string grn, string box, string loc, string pro)
        {
            var splitbox = box.Split("-");
            var firstpart = splitbox[0];
            var secondpart = splitbox[1];
            var splitsecand = splitbox[1].Split("/");
            var numerator = splitsecand[0];
            var denominator = splitsecand[1];

            if(Convert.ToInt64(numerator) > Convert.ToInt32(denominator))
            {
                var foundlist1 = Storage_OperationAdd
                .Where(a => a.productcode.Trim() == pro.Trim()
                            && a.batchcode.Trim() == batch.Trim()
                            && a.grnno.Trim() == grn.Trim()
                            && a.boxno.Trim() == box.Trim())
                .FirstOrDefault();
                if (foundlist1 != null)
                {
                    return Json(new { success = false, message = "Already Scanned !" });
                }
                else
                {
                    Storage_Operation st = new Storage_Operation()
                    {
                        productcode = pro,   // Assign product code
                        batchcode = batch,   // Assign batch code
                        grnno = grn,         // Assign GRN number
                        boxno = box,         // Assign box number
                    };
                    Storage_OperationAdd.Add(st);
                    return Json(new { success = true });
                }
                
            }

            var found = _context.Storage_Operation
                .Where(a => a.productcode.Trim() == pro.Trim()
                            && a.batchcode.Trim() == batch.Trim()
                            && a.grnno.Trim() == grn.Trim()
                            && a.boxno.Trim() == box.Trim()
                            && (a.statusflag.Trim() == "ST" || a.statusflag.Trim() == "PI" || a.statusflag.Trim() == "LD"))
                .FirstOrDefault();
            
            var foundlist = Storage_OperationAdd
                .Where(a => a.productcode.Trim() == pro.Trim()
                            && a.batchcode.Trim() == batch.Trim()
                            && a.grnno.Trim() == grn.Trim()
                            && a.boxno.Trim() == box.Trim())
                .FirstOrDefault();

            if(foundlist != null)
            {
                return Json(new { success = false, message = "Already Scanned !" });
            }

            if (found != null)
            {
                //check for retun products
                var returnproduct = _context.PR_model
                    .Where(a => a.productcode.Trim() == pro.Trim()
                    && a.batchcode.Trim() == batch.Trim()
                    //&& a.grnno.Trim() == grn.Trim() 
                    && a.boxno.Trim() == box.Trim())
                    .FirstOrDefault();

                if (returnproduct != null)
                {
                    Storage_Operation st = new Storage_Operation()
                    {
                        productcode = pro,   // Assign product code
                        batchcode = batch,   // Assign batch code
                        grnno = grn,         // Assign GRN number
                        boxno = box,         // Assign box number
                    };
                    Storage_OperationAdd.Add(st);
                    return Json(new { success = true });
                }
                else
                {
                    var checkin = _context.DMRPRRP.Where(a => a.productcode.Trim() == pro.Trim() && a.batch.Trim() == batch.Trim() && a.grn.Trim() == grn.Trim() && a.boxno.Trim() == box.Trim() && a.statusflag.Trim() == "ST").FirstOrDefault();
                    if (checkin != null && found != null)
                    {
                        Storage_Operation st = new Storage_Operation()
                        {
                            productcode = pro,   // Assign product code
                            batchcode = batch,   // Assign batch code
                            grnno = grn,         // Assign GRN number
                            boxno = box,         // Assign box number
                        };
                        Storage_OperationAdd.Add(st);
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Duplicate entry found in stock!" });
                    }
                    
                }
            }
            else
            {
                var foundIN = _context.inward
                    .FirstOrDefault(a => a.flag == 1 && a.batchcode.Trim() == batch.Trim() &&
                                         (a.ordertype == "Demo" || a.ordertype == "Repair" || a.ordertype == "Replacement"));

                if (foundIN != null)
                {
                    var check = _context.Storage_Operation
                            .Where(a => a.batchcode.Trim() == batch.Trim()
                            && a.productcode.Trim() == pro.Trim()
                            && a.grnno.Trim() == grn.Trim()
                            && a.boxno.Trim() == box.Trim())
                            .FirstOrDefault();

                    if (check != null)
                    {
                        if (check.statusflag.Trim() == "DM")
                        {
                            var foundIN1 = _context.inward
                                            .FirstOrDefault(a => a.flag == 1 && a.batchcode.Trim() == batch.Trim() &&
                                             a.ordertype == "Demo");

                            if (foundIN1 != null)
                            {
                                if (check == null)
                                {
                                    Storage_Operation st1 = new Storage_Operation()
                                    {
                                        productcode = pro,   // Assign product code
                                        batchcode = batch,   // Assign batch code
                                        grnno = grn,         // Assign GRN number
                                        boxno = box,         // Assign box number
                                    };
                                    Storage_OperationAdd.Add(st1);
                                    return Json(new { success = true });

                                }
                                else
                                {
                                    return Json(new { success = false, message = "Duplicate entry found of ordertype " + foundIN.ordertype + " !" });
                                }
                            }
                        }
                        else if (check.statusflag.Trim() == "RPR")
                        {
                            var foundIN2 = _context.inward
                                         .FirstOrDefault(a => a.flag == 1 && a.batchcode.Trim() == batch.Trim() &&
                                         a.ordertype == "Repair");

                            if (foundIN2 != null)
                            {
                                if (check != null)
                                {
                                    if (check.statusflag.Trim() == "RPR")
                                    {
                                        Storage_Operation st2 = new Storage_Operation()
                                        {
                                            productcode = pro,   // Assign product code
                                            batchcode = batch,   // Assign batch code
                                            grnno = grn,         // Assign GRN number
                                            boxno = box,         // Assign box number
                                        };
                                        Storage_OperationAdd.Add(st2);
                                        return Json(new { success = true });
                                    }
                                    else
                                    {
                                        return Json(new { success = false, message = "Duplicate entry found of ordertype " + foundIN.ordertype + " !" });
                                    }
                                }

                            }

                        }
                        else if (check.statusflag.Trim() == "RP")
                        {
                            var foundIN2 = _context.inward
                                         .FirstOrDefault(a => a.flag == 1 && a.batchcode.Trim() == batch.Trim() &&
                                         a.ordertype == "Replacement");

                            if (foundIN2 != null)
                            {
                                if (check != null)
                                {
                                    if (check.statusflag.Trim() == "RP")
                                    {
                                        Storage_Operation st3 = new Storage_Operation()
                                        {
                                            productcode = pro,   // Assign product code
                                            batchcode = batch,   // Assign batch code
                                            grnno = grn,         // Assign GRN number
                                            boxno = box,         // Assign box number
                                        };
                                        Storage_OperationAdd.Add(st3);
                                        return Json(new { success = true });
                                    }
                                    else
                                    {
                                        return Json(new { success = false, message = "Duplicate entry found of ordertype " + foundIN.ordertype + " !" });
                                    }
                                }

                            }

                        }
                    }
                }
                Storage_Operation st = new Storage_Operation()
                {
                    productcode = pro,   // Assign product code
                    batchcode = batch,   // Assign batch code
                    grnno = grn,         // Assign GRN number
                    boxno = box,         // Assign box number
                };
                Storage_OperationAdd.Add(st);
                return Json(new { success = true });
            }
        }

        [HttpPost]
        public IActionResult saveInSTList_DMG(string batch, string grn, string box, string loc, string pro)
        {
            var found = _context.Storage_Operation
                .Where(a => a.productcode.Trim() == pro.Trim()
                            && a.batchcode.Trim() == batch.Trim()
                            && a.grnno.Trim() == grn.Trim()
                            && a.boxno.Trim() == box.Trim()
                            && a.statusflag.Trim() == "ST")
                .FirstOrDefault();

            var foundd = _context.Storage_Operation
                .Where(a => a.productcode.Trim() == pro.Trim()
                            && a.batchcode.Trim() == batch.Trim()
                            && a.grnno.Trim() == grn.Trim()
                            && a.boxno.Trim() == box.Trim()
                            && a.statusflag.Trim() == "DMG")
                .FirstOrDefault();

            if (found == null)
            {
                return Json(new { success = false, message = "Scanned item is not in storage before !" });
            }
            else
            {
                var found1 = Storage_OperationAdd
                .Where(a => a.productcode.Trim() == pro.Trim()
                            && a.batchcode.Trim() == batch.Trim()
                            && a.grnno.Trim() == grn.Trim()
                            && a.boxno.Trim() == box.Trim())
                .FirstOrDefault();

                if (found1 != null)
                {
                    return Json(new { success = false, message = "Already scanned !" });
                }
                else
                {
                    found.statusflag = "DMG";
                    Storage_OperationAdd.Add(found);
                    return Json(new { success = true });
                }
            }
        }

        [HttpPost]
        public IActionResult saveInSTListDMRPRRP(string batch, string grn, string box, string loc, string pro)
        {
            var found = _context.Storage_Operation
                .Where(a => a.productcode.Trim() == pro.Trim()
                            && a.batchcode.Trim() == batch.Trim()
                            && a.grnno.Trim() == grn.Trim()
                            && a.boxno.Trim() == box.Trim()
                            && (a.statusflag.Trim() == "DM" || a.statusflag.Trim() == "RPR" || a.statusflag.Trim() == "RP"))
                .FirstOrDefault();

            if (found != null)
            {
                return Json(new { success = true });
                //return Json(new { success = false, message = "Please scan correct shipper !" });
            }
            else
            {
                //return Json(new { success = true });
                return Json(new { success = false, message = "Please scan correct shipper !" });
            }
        }


        [HttpPost]
        public ActionResult CreateStockIn(List<Storage_Operation> storage)
        {

            foreach (var item in storage)
            {
                var found = _context.Storage_Operation
                    .Where(a => a.productcode.Trim() == item.productcode.Trim()
                    && a.batchcode.Trim() == item.batchcode.Trim()
                    && a.grnno.Trim() == item.grnno.Trim()
                    && a.pickflag == item.pickflag
                    && a.boxno.Trim() == item.boxno.Trim()
                    && (a.statusflag.Trim() == "RPR" || a.statusflag == "DM" || a.statusflag == "RP"))
                    .FirstOrDefault();

                if (found != null)
                {
                    found.locationcode = item.locationcode;
                    found.statusflag = "ST";
                    _context.Storage_Operation.Update(found);
                    _context.SaveChanges();
                }
            }
            return Json(new { success = true, message = "Stock Update successfully done !" });
        }
        [HttpGet]
        public async Task<IActionResult> CreateStockTransfer()
        {
            var listdata = await _context.DMRPRRP.OrderByDescending(a=>a.id).ToListAsync(); // Fetch data asynchronously
            return View(listdata); // Pass the list to the view
        }
        [HttpGet]
        //var listdata = await _context.DMRPRRP
        //                             .OrderByDescending(a => a.id)
        //                             .ToListAsync();

        //foreach (var item in listdata)
        //{
        //    var found = _context.inward.Where(a => a.pono.Trim() == item.refno.Trim()).OrderByDescending(a => a.inward_id).FirstOrDefault();
        //    if (found != null)
        //    {
        //        item.Status = found.status.Trim();
        //    }
        //}

        //return View(listdata); // Pass the list to the view
        public async Task<IActionResult> CreateStockTransferHome()
        {
            var listdata = new List<DMRPRRP>();  // Start fresh with an empty list
            var getdistinct = _context.inward
                .Where(a => a.ordertype.Trim() == "Demo" || a.ordertype.Trim() == "Repair" || a.ordertype.Trim() == "Replacement")
                
                .ToList();

            var GETlst = getdistinct.DistinctBy(a => a.pono.Trim()).ToList();

            if(GETlst.Count > 0)
            {
                foreach (var item in GETlst)
                {
                    // Get the first inward record for this PONO
                    var find = _context.inward.Where(a => a.pono.Trim() == item.pono.Trim()).OrderBy(a => a.inward_id).FirstOrDefault();

                    if (find != null)
                    {
                        item.flag = find.flag;
                        item.vendername = find.vendername;
                        item.partyname = find.partyname;
                        item.status = find.status;
                        item.date = find.date;

                        // Get all packets related to this inward record
                        var findpacket = _context.inwardPacket.Where(a => a.inwardId == find.inward_id).ToList();
                        item.qty = findpacket.Sum(a => a.quantity);

                        // Determine pending quantity based on flag (Inwarding or Outwarding done first)
                        var fond2 = _context.inwardPacket
                            .Where(a => a.pono.Trim() == item.pono.Trim() && a.flag == (find.flag == 1 ? 2 : 1))
                            .ToList();
                        var total = fond2.Sum(a => a.quantity);

                        item.pqty = Math.Abs(total - findpacket.Sum(a => a.quantity));

                        // Create DMRPRRP object and add to list
                        DMRPRRP dm = new DMRPRRP()
                        {
                            ordertype = find.ordertype,
                            refno = item.pono,
                            inout = item.flag,
                            from = item.vendername,
                            partyname = item.partyname,
                            qty = item.qty.ToString(),
                            pqty = item.pqty.ToString(),
                            Status = item.status,
                            date = item.date,
                        };
                        listdata.Add(dm);
                    }
                }

            }

            //List<inwardPacket> ip = new List<inwardPacket>();
            //List<DMRPRRP> DMRPRRPlist = new List<DMRPRRP>();
            //var list = _context.inward.Where(a => a.ordertype.Trim() == "Demo" || a.ordertype.Trim() == "Repair" || a.ordertype.Trim() == "Replacement").ToList();
            //if(list.Count > 0)
            //{
            //    foreach (var item in list)
            //    {
            //        var found = _context.inwardPacket.Where(a => a.inwardId == item.inward_id).ToList();
            //        if (found.Count > 0)
            //        {
            //            foreach (var newitem in found)
            //            {
            //                newitem.vendor = item.vendername;
            //                newitem.partyname = item.partyname;
            //                newitem.inout = item.flag;
            //                newitem.status = item.status;
            //                newitem.date = item.date;
            //                newitem.ordertype = item.ordertype;
            //            }
            //            ip.AddRange(found);
            //        }
            //    }
            //    listdata.Clear();
            //    foreach (var item in ip)
            //    {
            //        var cond = "ST";
            //        var found = _context.DMRPRRP.Where(a => a.productcode.ToUpper().Trim() == item.productcode.Trim().ToUpper()
            //        && a.ordertype.Trim() == item.ordertype.Trim() && a.from.Trim() == item.vendor.Trim() && a.refno.Trim() == item.pono.Trim()).Select(a => a.condition).FirstOrDefault();
            //        if (found != null)
            //        {
            //            cond = found;
            //        }

            //        DMRPRRP DMRPRRP = new DMRPRRP()
            //        {
            //            id = item.id,
            //            from = item.vendor,
            //            partyname = item.partyname,
            //            inout = Convert.ToInt32(item.flag),
            //            statusflag = item.status,
            //            date = item.date,
            //            ordertype = item.ordertype,
            //            refno = item.pono,
            //            productcode = item.productcode,
            //            qty = (item.quantity).ToString(),
            //            condition = cond,
            //            Status = item.status,
            //        };
            //        listdata.Add(DMRPRRP);
            //    }
            //    //var orderedList = listdata.OrderByDescending(x => x.id).ToList();
            //}

            return View(listdata);
        }


        // GET: Storage_Operation
        public async Task<IActionResult> Index()
        {
            if (_context.Storage_Operation == null)
            {
                return Problem("Entity set 'AuthDbContext.do_allotment' is null.");
            }
            var storages = _context.Storage_Operation.Where(a => a.pickflag == "0" && a.statusflag == "ST" ).OrderByDescending(a => a.id).ToList();
            return View(storages);
        }
        public IActionResult CreateStockMovement()
        {
            return View();
        }

        public ActionResult InsertStockMovement(List<Storage_Operation> storage)
        {
            if (storage != null && storage.Any())
            {
                foreach (var customer in storage)
                {
                    customer.batchcode = customer.batchcode?.Replace("\u001e", "") ?? "";
                    customer.productcode = customer.productcode?.Replace("\u001e", "") ?? "";
                    customer.grnno = customer.grnno?.Replace("\u001e", "") ?? "";
                    customer.boxno = customer.boxno?.Replace("\u001e", "") ?? "";

                    var splitbox1 = customer.boxno.Split("-");
                    var firstpart = splitbox1[0];
                    var secondpart = splitbox1[1];
                    var splitsecand = splitbox1[1].Split("/");
                    var numerator = splitsecand[0];
                    var denominator = splitsecand[1];

                    if (Convert.ToInt64(numerator) > Convert.ToInt64(denominator))
                    {
                        var found = _context.Storage_Operation
                            .Where(a=>a.productcode.Trim() == customer.productcode.Trim() 
                                && a.grnno.Trim()==customer.grnno.Trim() 
                                && a.batchcode.Trim() == customer.batchcode.Trim()).ToList();

                        List<Storage_Operation> changelist = new List<Storage_Operation>();

                        if (found.Count > 0)
                        {
                            foreach(var item in found.Take(Convert.ToInt32(numerator)))
                            {
                                changelist.Add(item);
                            }

                            foreach(var item in changelist)
                            {
                                item.locationcode = customer.locationcode;
                                _context.Storage_Operation.Update(item);
                                _context.SaveChanges();
                            }
                        }
                    }
                    else
                    {
                        var existdata = _context.Storage_Operation
                        .Where(e => e.productcode.Trim().ToUpper() == customer.productcode.Trim().ToUpper()
                            && e.boxno.Trim() == customer.boxno.Trim()
                            && e.batchcode.Trim() == customer.batchcode.Trim()
                            && e.grnno.Trim() == customer.grnno.Trim()
                            && e.locationcode.Trim() != customer.locationcode.Trim()
                            && e.statusflag.Trim() == "ST")
                        .ToList();

                        var Pickndata = _context.pickstorage
                            .Where(a => a.productcode.Trim().ToUpper() == customer.productcode.Trim().ToUpper()
                                && a.boxno.Trim() == customer.boxno.Trim()
                                && a.batchcode.Trim() == customer.batchcode.Trim()
                                && a.location.Trim() != customer.locationcode.Trim()
                                && a.flag == 0)
                            .ToList();

                        if (existdata.Any())
                        {
                            foreach (var kk in existdata)
                            {
                                if (Pickndata.Any())
                                {
                                    foreach (var pp in Pickndata.Where(a => a.productcode.Trim().ToUpper() == kk.productcode.Trim().ToUpper() && a.boxno.Trim() == kk.boxno.Trim() && a.batchcode.Trim() == kk.batchcode.Trim() && a.location.Trim() == kk.locationcode.Trim()))
                                    {
                                        pp.location = customer.locationcode;
                                        _context.pickstorage.Update(pp);
                                    }
                                }
                                //maintain logs
                                var user = HttpContext.Session.GetString("User");
                                var logs = new Logs();
                                logs.pagename = "Storage Operation";
                                logs.task = "Internal Stock Movement";
                                logs.taskid = kk.id;
                                logs.action = "Create";
                                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                                logs.time = DateTime.Now.ToString("HH:mm:ss");
                                logs.username = user;
                                _context.Add(logs);

                                kk.locationcode = customer.locationcode; // Assuming Location is the property that stores the location data
                                _context.Storage_Operation.Update(kk);
                            }

                            _context.SaveChanges();

                            //var splitbox = customer.boxno.Split('-')[1];

                            //var pickstorage = _context.pickstorage.FirstOrDefault(a => a.productcode.Trim().ToUpper() == customer.productcode.Trim().ToUpper() && a.boxno.Contains(splitbox.Trim()) && a.flag == 0);
                            //var storagedata = _context.Storage_Operation.FirstOrDefault(a => a.productcode.Trim().ToUpper() == customer.productcode.Trim().ToUpper() && a.boxno.Contains(splitbox.Trim()) && a.statusflag == "DMG");

                            //if (pickstorage != null && storagedata != null && pickstorage.batchcode == storagedata.batchcode && pickstorage.productcode.ToUpper() == storagedata.productcode.ToUpper() && customer.productcode.ToUpper() == pickstorage.productcode.ToUpper())
                            //{
                            //    pickstorage.productcode = customer.productcode.Trim().ToUpper();
                            //    pickstorage.location = customer.locationcode.Trim();
                            //    pickstorage.batchcode = customer.batchcode.Trim();
                            //    pickstorage.boxno = customer.boxno.Trim();

                            //    _context.pickstorage.Update(pickstorage);
                            //    _context.SaveChanges();

                            //    return Json(new { success = true, message = "Picking alloted Damage " + storagedata.boxno + " is replaced with new shipper " + customer.boxno + "!" });
                            //}
                        }
                        else
                        {
                            return Json(new { success = false, message = "In storage, shippers are not available to perform internal stock movement." });
                        }
                    }


                        
                }
            }
            return Json(new { success = true, message = "Internal stock movement successfully completed!" });
        }

        public async Task<IActionResult> Damage()
        {

            if (_context.Storage_Operation == null)
            {
                return Problem("Entity set 'AuthDbContext.do_allotment' is null.");
            }
            var storages = _context.Storage_Operation.Where(a => a.statusflag == "DMG").ToList();

            return View(storages);
        }
        public IActionResult DamageList()
        {
            var damagedData = _context.Storage_Operation.Where(x => x.statusflag == "DMG").ToList();
            return View(damagedData);
        }
        [HttpPost]
        public IActionResult CheckStorage(string pro,string batch ,string  box,string grn,string loc)
        {
            var splitbox = box.Split("-");
            var firstpart = splitbox[0];
            var secondpart = splitbox[1];
            var splitsecand = splitbox[1].Split("/");
            var numerator = splitsecand[0];
            var denominator = splitsecand[1];

            if (Convert.ToInt64(numerator) > Convert.ToInt32(denominator))
            {
                var checlist = _context.Storage_Operation
                    .Where(a => a.productcode.Trim() == pro.Trim()
                    && a.batchcode.Trim() == batch.Trim()
                    && a.pickflag == "0"
                    && a.statusflag == "ST").ToList();
                if(checlist.Count > 0)
                {
                    return Json(new { success = true, message = "Storage Data Found Successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Entered Productcode " + pro + " of shipper " + box + " is already found in storage with same location !" });
                }
            }
            else
            {
                var storagedata = _context.Storage_Operation
                .Where(a => a.productcode.Trim().ToUpper() == pro.Trim().ToUpper() && a.boxno.Trim() == box.Trim() && a.batchcode.Trim() == batch.Trim() 
                && a.locationcode.Trim() != loc.Trim() && a.statusflag == "ST")
                .FirstOrDefault();

                var storagedataEXIT = _context.Storage_Operation
                    .Where(a => a.productcode.Trim().ToUpper() == pro.Trim().ToUpper() && a.boxno.Trim() == box.Trim() && a.batchcode.Trim() == batch.Trim() 
                    && a.locationcode.Trim() == loc.Trim() && a.statusflag == "ST")
                    .FirstOrDefault();

                if (storagedata != null && storagedataEXIT == null)
                {
                    return Json(new { success = true, message = "Storage Data Found Successfully." });
                }
                else if (storagedataEXIT != null)
                {
                    return Json(new { success = false, message = "Entered Productcode " + pro + " of shipper " + box + " is already found in storage with same location !" });
                }
                else if (storagedata == null && storagedataEXIT == null)
                {
                    return Json(new { success = false, message = "Entered Productcode " + pro + " of shipper " + box + " is not found in storage !" });
                }
                else
                {
                    return Ok();
                }
            }

            
        }
        [HttpPost]
        public IActionResult CheckStorageDMG([FromBody] StorageOperationData data)
        {
            //found in strage
            var storagedataEXIT = _context.Storage_Operation
                .FirstOrDefault(a => a.productcode.Trim().ToUpper() == data.ProductCode.Trim().ToUpper() &&
                                     a.boxno.Trim() == data.BoxNo.Trim() &&
                                     a.batchcode.Trim() == data.BatchCode.Trim() &&
                                     a.locationcode.Trim() == data.LocationCode.Trim() &&
                                     a.statusflag == "ST");
            //found but location mismatch
            var storagedataEXITl = _context.Storage_Operation
                .FirstOrDefault(a => a.productcode.Trim().ToUpper() == data.ProductCode.Trim().ToUpper() &&
                                     a.boxno.Trim() == data.BoxNo.Trim() &&
                                     a.batchcode.Trim() == data.BatchCode.Trim() &&
                                     a.locationcode.Trim() != data.LocationCode.Trim() &&
                                     a.statusflag == "ST");
            //found but as dmg
            var storagedataEXITDMG = _context.Storage_Operation
                .FirstOrDefault(a => a.productcode.Trim().ToUpper() == data.ProductCode.Trim().ToUpper() &&
                                     a.boxno.Trim() == data.BoxNo.Trim() &&
                                     a.batchcode.Trim() == data.BatchCode.Trim() &&
                                     a.locationcode.Trim() == data.LocationCode.Trim() &&
                                     a.statusflag == "DMG");

            if (storagedataEXIT != null && storagedataEXITl == null && storagedataEXITDMG == null)
            {
                return Json(new { success = true, message = "Storage Data Found Successfully." });
            }
            else if (storagedataEXITDMG == null && storagedataEXIT == null && storagedataEXITl == null)
            {
                return Json(new { success = true, message = "Storage Data Found Successfully." });
            }
            if (storagedataEXITl != null && storagedataEXIT == null && storagedataEXITDMG == null)
            {
                return Json(new { success = false, message = "Entered Productcode " + data.ProductCode + " of shipper " + data.BoxNo + " is not found on the enter location in storage!" });
            }
            else if (storagedataEXITDMG != null && storagedataEXITl == null && storagedataEXIT == null)
            {
                return Json(new { success = false, message = "Entered Productcode " + data.ProductCode + " of shipper " + data.BoxNo + " is already found as damaged product in storage!" });
            }
            else
            {
                return Ok();
            }
        }

        //public IActionResult CheckStorageDMG([FromBody] StorageOperationData data)
        //{

        //    var storagedataEXIT = _context.Storage_Operation
        //        .Where(a => a.productcode.Trim() == data.ProductCode.Trim() && a.boxno.Trim() == data.BoxNo.Trim() && a.batchcode.Trim() == data.BatchCode.Trim() && a.locationcode.Trim() == data.LocationCode.Trim() && a.statusflag == "ST").FirstOrDefault();
        //    var storagedataEXITDMG = _context.Storage_Operation
        //        .Where(a => a.productcode.Trim() == data.ProductCode.Trim() && a.boxno.Trim() == data.BoxNo.Trim() && a.batchcode.Trim() == data.BatchCode.Trim() && a.locationcode.Trim() == data.LocationCode.Trim() && a.statusflag == "DMG").FirstOrDefault();

        //    if (storagedataEXIT != null)
        //    {
        //        return Json(new { success = true, message = "Storage Data Found Successfully." });
        //    }
        //    else if(storagedataEXIT == null)
        //    {
        //        return Json(new { success = false, message = "Entered Productcode " + data.ProductCode + " of shipper " + data.BoxNo + " is not found in storage on the entered location !" });

        //    }else if(storagedataEXITDMG != null )
        //    {
        //        return Json(new { success = false, message = "Entered Productcode " + data.ProductCode + " of shipper " + data.BoxNo + " is alredy found as damage product !" });
        //    }
        //    else
        //    {
        //        return Ok();
        //    }
        //}

        public class StorageOperationData
        {
            public string ProductCode { get; set; }
            public string BoxNo { get; set; }
            public string BatchCode { get; set; }
            public string LocationCode { get; set; }
            // public string Grnno { get; set; }
        }

        //[HttpPost]
        //public JsonResult insertsite(List<Storage_Operation> storage_Operations)
        //{

        //    if (storage_Operations == null)
        //    {
        //        storage_Operations = new List<Storage_Operation>();
        //    }

        //    try
        //    {
        //        using (var context = new ErosDbContext()) // Replace 'YourDbContext' with your actual database context class name
        //        {
        //            context.Storage_Operation.AddRange(storage_Operations);
        //            int insertedRecords = context.SaveChanges();
        //            return Json(insertedRecords);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle any potential exceptions that occurred during the database operation
        //        // You can log the exception or return an error message as needed
        //        return Json("An error occurred while saving the records: " + ex.Message);
        //    }
        //    //_context.do_allotment.AddRange(do_Allotments);
        //    //int insertedrecords = _context.SaveChanges();
        //    //return Json(insertedrecords);
        //}

        // GET: Storage_Operation/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Storage_Operation == null)
            {
                return NotFound();
            }

            var storage_Operation = await _context.Storage_Operation
                .FirstOrDefaultAsync(m => m.id == id);
            if (storage_Operation == null)
            {
                return NotFound();
            }

            return View(storage_Operation);
        }

        // GET: Storage_Operation/Create
        public IActionResult Create()    //1
        {
            if (_context.Storage_Operation == null)
            {
                return Problem("Entity set 'AuthDbContext.Storage_Operation' is null.");
            }
            return View();
        }
        public IActionResult CreateDMG()    //1
        {
            if (_context.Storage_Operation == null)
            {
                return Problem("Entity set 'AuthDbContext.Storage_Operation' is null.");
            }
            return View();
        }

        // POST: Storage_Operation/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        //[ValidateAntiForgeryToken]
        //public IActionResult Create(Storage_Operation storage_Operation)  //2
        //{
        //    //int maxId = _context.Storage_Operation.Any() ? _context.Storage_Operation.Max(e => e.id) + 1 : 1;
        //    //storage_Operation.id = maxId;

        //        _context.Add(storage_Operation);
        //        _context.SaveChangesAsync();
        //        //return RedirectToAction(nameof(Index));
        //        _notyfService.Success("Record Saved Successfully");
        //        return RedirectToAction("Index");

        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string optionvalue)
        {
            return View();
        }

        //public ActionResult InsertCustomers(List<Storage_Operation> storage)
        //{
        //    List<string> duplicateRecords = new List<string>();

        //    if (storage != null)
        //    {
        //        foreach(var customer in storage)
        //        {
        //            var found = _context.Storage_Operation
        //                .Where(a => a.productcode.Trim() == customer.productcode.Trim()
        //                && a.grnno.Trim() == customer.grnno.Trim()
        //                && a.batchcode.Trim() == customer.batchcode.Trim()
        //                && a.boxno.Trim() == customer.boxno.Trim()
        //                && (a.statusflag.Trim() == "DM" || a.statusflag.Trim() == "RPR" || a.statusflag.Trim() == "RP"))
        //                .FirstOrDefault();

        //            if (found != null)
        //            {
        //                found.locationcode = customer.locationcode; 
        //                _context.Storage_Operation.Update(found);
        //                _context.SaveChanges();
        //            }
        //        }
        //        return Json(new { success = true, message = "Storage Operation Done Successfully !" });

        //        foreach (Storage_Operation customer in storage)
        //        {


        //            if (customer.productcode.ToUpper() == null || customer.batchcode == null 
        //                && customer.boxno == null || customer.locationcode == null || customer.grnno == null)
        //            {
        //                return Json(new { success = false, message = "Some values found Null !" });
        //            }
        //            else
        //            {
        //                var SRProduct = _context.Storage_Operation.Where(a => a.productcode.Trim().ToUpper() == customer.productcode.Trim().ToUpper()
        //                           && a.boxno.Trim() == customer.boxno.Trim()
        //                           && a.batchcode.Trim() == customer.batchcode.Trim()
        //                           && a.grnno.Trim() == customer.grnno.Trim()
        //                           && a.statusflag == "SR" 
        //                           && a.locationcode == "TMP").FirstOrDefault();
        //                var PRProduct = _context.Storage_Operation.Where(a => a.productcode.Trim().ToUpper() == customer.productcode.Trim().ToUpper()
        //                           && a.boxno.Trim() == customer.boxno.Trim()
        //                           && a.batchcode.Trim() == customer.batchcode.Trim()
        //                           && a.grnno.Trim() == customer.grnno.Trim()
        //                           && a.statusflag == "PR"
        //                           && a.locationcode == "TMP").FirstOrDefault();
        //                if (SRProduct != null && PRProduct == null)
        //                {
        //                    SRProduct.locationcode = customer.locationcode;
        //                    SRProduct.statusflag = "ST";
        //                    _context.Storage_Operation.Update(SRProduct);
        //                    _context.SaveChanges();
        //                }else if (SRProduct == null && PRProduct != null)
        //                {
        //                    return Json(new { success = false, message = "Scanned products are found to be purchase return data !" });
        //                }
        //                else if(SRProduct == null && PRProduct == null)
        //                {
        //                    bool recordExists = _context.Storage_Operation
        //                    .Any(e => e.productcode.Trim().ToUpper() == customer.productcode.Trim().ToUpper()
        //                           && e.boxno.Trim() == customer.boxno.Trim()
        //                           && e.batchcode.Trim() == customer.batchcode.Trim()
        //                           && e.grnno.Trim() == customer.grnno.Trim()
        //                           && e.statusflag.Trim() == "ST");

        //                    bool recordExists1 = _context.Storage_Operation
        //                   .Any(e => e.productcode.Trim().ToUpper() == customer.productcode.Trim().ToUpper()
        //                          && e.boxno.Trim() == customer.boxno.Trim()
        //                          && e.batchcode.Trim() == customer.batchcode.Trim()
        //                          && e.grnno.Trim() == customer.grnno.Trim()
        //                          && (e.statusflag.Trim() == "LD" || e.statusflag.Trim() == "PI"));

        //                    if (recordExists1)
        //                    {
        //                        return Json(new { success = false, message = "Scanned products are found to be Loaded !" });
        //                    }
        //                    if (!recordExists)
        //                    {
        //                        int maxId = _context.Storage_Operation.Any() ? _context.Storage_Operation.Max(e => e.id) + 1 : 1;

        //                        var doallot = new Storage_Operation();
        //                        doallot.id = maxId;
        //                        if (customer.productcode.Contains(''))
        //                        {
        //                            doallot.productcode = customer.productcode.Replace("\u001e", "").ToUpper();
        //                        }
        //                        else
        //                        {
        //                            doallot.productcode = customer.productcode.Trim().ToUpper();
        //                        }
        //                        doallot.boxno = customer.boxno.ToString().Trim();
        //                        if (customer.batchcode.Contains(''))
        //                        {
        //                            doallot.batchcode = customer.batchcode.Replace("\u001e", "");
        //                        }
        //                        else
        //                        {
        //                            doallot.batchcode = customer.batchcode;
        //                        }

        //                        doallot.locationcode = customer.locationcode.ToString().Trim();
        //                        doallot.grnno = customer.grnno.ToString().Trim();
        //                        doallot.statusflag = "ST";

        //                        _context.Add(doallot);

        //                        //maintain logs
        //                        var user = HttpContext.Session.GetString("User");
        //                        var logs = new Logs();
        //                        logs.pagename = "Storage Operation";
        //                        logs.task = "Add to Storage";
        //                        logs.taskid = maxId;
        //                        logs.action = "Create";
        //                        logs.date = DateTime.Now.ToString("dd/MM/yyyy");
        //                        logs.time = DateTime.Now.ToString("HH:mm:ss");
        //                        logs.username = user;
        //                        _context.Add(logs);
        //                        _context.SaveChanges();

        //                        var splitbox = customer.boxno.Split('-')[1];

        //                        //logic if found damage product box in picking 
        //                        var pickstorage = _context.pickstorage
        //                            .Where(a => a.productcode.Trim().ToUpper() == customer.productcode.Trim().ToUpper() 
        //                            && a.boxno.Contains(splitbox.Trim()) && a.flag == 0)
        //                            .FirstOrDefault();

        //                        var storagedata = _context.Storage_Operation
        //                            .Where(a => a.productcode.Trim().ToUpper() == customer.productcode.Trim().ToUpper() 
        //                            && a.boxno.Contains(splitbox.Trim()) 
        //                            && a.statusflag == "DMG")
        //                            .FirstOrDefault();

        //                        if (pickstorage != null 
        //                            && storagedata != null 
        //                            && pickstorage.batchcode == storagedata.batchcode 
        //                            && pickstorage.productcode.ToUpper() == storagedata.productcode.ToUpper() 
        //                            && customer.productcode.ToUpper() == pickstorage.productcode.ToUpper())
        //                        {
        //                            // Update pickstorage with values from customer
        //                            pickstorage.productcode = customer.productcode.Trim().ToUpper();
        //                            pickstorage.location = customer.locationcode.Trim();
        //                            pickstorage.batchcode = customer.batchcode.Trim();
        //                            pickstorage.boxno = customer.boxno.Trim();
        //                            _context.pickstorage.Update(pickstorage);
        //                            _context.SaveChanges();
        //                        }
        //                    }
        //                    else
        //                    {
        //                        duplicateRecords.Add($"Product Code: {customer.productcode.Trim().ToUpper()}, Box No: {customer.boxno.Trim()}, Batch Code: {customer.batchcode.Trim()},");
        //                    }
        //                }

        //            }
        //        }
        //    }

        //    if (duplicateRecords.Count > 0)
        //    {
        //        string errorMessage = "Non-duplicate data saved, but some records were found to be duplicates. Details:\n" + string.Join("\n", duplicateRecords);
        //        return Json(new { success = false, message = errorMessage });
        //    }

        //    return Json(new { success = true, message = "Storage Operation Done Successfully !" });

        //}

        //public ActionResult InsertCustomers(List<Storage_Operation> storage)
        //{
        //    if (storage != null)
        //    {

        //        var doallot = new Storage_Operation();

        //        foreach (Storage_Operation customer in storage)
        //        {
        //            var check = _context.Storage_Operation
        //                .Where(a => a.batchcode.Trim() == customer.batchcode.Trim()
        //                && a.productcode.Trim() == customer.productcode.Trim()
        //                && a.grnno.Trim() == customer.grnno.Trim()
        //                && a.boxno.Trim() == customer.boxno.Trim())
        //                .FirstOrDefault();

        //            var foundIN1 = _context.inward
        //                                  .FirstOrDefault(a => a.flag == 1 && a.batchcode.Trim() == customer.batchcode.Trim() && a.ordertype.Trim() == "Demo");
        //            var foundIN2 = _context.inward
        //                                  .FirstOrDefault(a => a.flag == 1 && a.batchcode.Trim() == customer.batchcode.Trim() && a.ordertype.Trim() == "Repair");
        //            var foundIN3 = _context.inward
        //                                  .FirstOrDefault(a => a.flag == 1 && a.batchcode.Trim() == customer.batchcode.Trim() && a.ordertype.Trim() == "Replacement");

        //                if (check == null && foundIN1 != null)
        //                {
        //                    if (foundIN1.ordertype.Trim() == "Demo")
        //                    {
        //                        customer.statusflag = "DM";
        //                    }
        //                }
        //                else if(check == null && foundIN1 == null)
        //                {
        //                    customer.statusflag = "ST";
        //                }
        //                else if (check != null && foundIN2 != null )
        //                {
        //                    if(check.statusflag.Trim() == "RPR")
        //                    {
        //                       /// UPDATE STORAGE OPERATION
        //                       customer.statusflag = "RPR";
        //                       //IF CONDITION TRUW THEN CHECK DATA , LOCATON SHOULD ONLY NEED TO UPDATE NOT ADD
        //                    }
        //                    else if(check.statusflag.Trim() == "RP")
        //                    {
        //                        /// UPDATE STORAGE OPERATION
        //                        customer.statusflag = "RP";
        //                        //IF CONDITION TRUW THEN CHECK DATA , LOCATON SHOULD ONLY NEED TO UPDATE NOT ADD
        //                    }
        //                }

        //            if (customer.productcode.ToUpper() == null || customer.batchcode == null
        //                && customer.boxno == null || customer.locationcode == null || customer.grnno == null)
        //            {
        //                return Json(new { success = false, message = "Some values found Null !" });
        //            }
        //            else
        //            {
        //                var SRProduct = _context.Storage_Operation.Where(a => a.productcode.Trim().ToUpper() == customer.productcode.Trim().ToUpper()
        //                           && a.boxno.Trim() == customer.boxno.Trim()
        //                           && a.batchcode.Trim() == customer.batchcode.Trim()
        //                           && a.grnno.Trim() == customer.grnno.Trim()
        //                           && a.statusflag == "SR"
        //                           && a.locationcode == "TMP").FirstOrDefault();
        //                var PRProduct = _context.Storage_Operation.Where(a => a.productcode.Trim().ToUpper() == customer.productcode.Trim().ToUpper()
        //                           && a.boxno.Trim() == customer.boxno.Trim()
        //                           && a.batchcode.Trim() == customer.batchcode.Trim()
        //                           && a.grnno.Trim() == customer.grnno.Trim()
        //                           && a.statusflag == "PR"
        //                           && a.locationcode == "TMP").FirstOrDefault();

        //                if (SRProduct != null && PRProduct == null)
        //                {
        //                    SRProduct.locationcode = customer.locationcode;
        //                    SRProduct.statusflag = "ST";
        //                    _context.Storage_Operation.Update(SRProduct);
        //                    _context.SaveChanges();
        //                }
        //                else if (SRProduct == null && PRProduct != null)
        //                {
        //                    return Json(new { success = false, message = "Scanned products are found to be purchase return data !" });
        //                }
        //                else if (SRProduct == null && PRProduct == null)
        //                {
        //                    bool recordExists = _context.Storage_Operation
        //                    .Any(e => e.productcode.Trim().ToUpper() == customer.productcode.Trim().ToUpper()
        //                           && e.boxno.Trim() == customer.boxno.Trim()
        //                           && e.batchcode.Trim() == customer.batchcode.Trim()
        //                           && e.grnno.Trim() == customer.grnno.Trim()
        //                           && e.statusflag.Trim() == "ST");

        //                    bool recordExists1 = _context.Storage_Operation
        //                   .Any(e => e.productcode.Trim().ToUpper() == customer.productcode.Trim().ToUpper()
        //                          && e.boxno.Trim() == customer.boxno.Trim()
        //                          && e.batchcode.Trim() == customer.batchcode.Trim()
        //                          && e.grnno.Trim() == customer.grnno.Trim()
        //                          && (e.statusflag.Trim() == "LD" || e.statusflag.Trim() == "PI"));

        //                    if (recordExists1)
        //                    {
        //                        return Json(new { success = false, message = "Scanned products are found to be Loaded !" });
        //                    }
        //                    if (!recordExists)
        //                    {
        //                        if (check == null && foundIN2 == null)
        //                        {
        //                            var found = _context.Storage_Operation
        //                                .Where(a => a.productcode.Trim() == customer.productcode.Trim() 
        //                                && a.boxno.Trim() == customer.boxno.Trim() && a.batchcode.Trim() == customer.batchcode.Trim() 
        //                                && a.statusflag.Trim() == "LD")
        //                                .FirstOrDefault();

        //                            var SRData = _context.PR_model
        //                                .Where(a => a.productcode.Trim() == customer.productcode.Trim() 
        //                                && a.boxno.Trim() == customer.boxno.Trim() && a.batchcode.Trim() == customer.batchcode.Trim() 
        //                                && a.statusflag.Trim() == "LD")
        //                                .FirstOrDefault();

        //                            if(found != null && SRData != null)
        //                            {
        //                                found.locationcode = customer.locationcode;
        //                                found.statusflag = "ST";
        //                                _context.Storage_Operation.Update(found);
        //                                _context.SaveChanges();
        //                                SRData.locationcode = customer.locationcode;
        //                                SRData.statusflag = "ST";
        //                                _context.Storage_Operation.Update(found);
        //                                _context.SaveChanges();
        //                            }
        //                            else
        //                            {
        //                                int maxId = _context.Storage_Operation.Any() ? _context.Storage_Operation.Max(e => e.id) + 1 : 1;

        //                                doallot.id = maxId;
        //                                if (customer.productcode.Contains(''))
        //                                {
        //                                    doallot.productcode = customer.productcode.Replace("\u001e", "").ToUpper();
        //                                }
        //                                else
        //                                {
        //                                    doallot.productcode = customer.productcode.Trim().ToUpper();
        //                                }
        //                                doallot.boxno = customer.boxno.ToString().Trim();
        //                                if (customer.batchcode.Contains(''))
        //                                {
        //                                    doallot.batchcode = customer.batchcode.Replace("\u001e", "");
        //                                }
        //                                else
        //                                {
        //                                    doallot.batchcode = customer.batchcode;
        //                                }
        //                                doallot.locationcode = customer.locationcode.ToString().Trim();
        //                                doallot.grnno = customer.grnno.ToString().Trim();
        //                                doallot.statusflag = customer.statusflag.Trim();

        //                                _context.Add(doallot);
        //                                _context.SaveChanges();
        //                            }
        //                        }
        //                        else if (check != null && foundIN2 != null)
        //                        {
        //                            check.locationcode = customer.locationcode;
        //                            check.statusflag = customer.statusflag;
        //                            _context.Storage_Operation.Update(check);
        //                            _context.SaveChanges();
        //                        }

        //                        var splitbox = customer.boxno.Split('-')[1];

        //                        //logic if found damage product box in picking 
        //                        var pickstorage = _context.pickstorage
        //                            .Where(a => a.productcode.Trim().ToUpper() == customer.productcode.Trim().ToUpper()
        //                            && a.boxno.Contains(splitbox.Trim()) && a.flag == 0)
        //                            .FirstOrDefault();

        //                        var storagedata = _context.Storage_Operation
        //                            .Where(a => a.productcode.Trim().ToUpper() == customer.productcode.Trim().ToUpper()
        //                            && a.boxno.Contains(splitbox.Trim())
        //                            && a.statusflag == "DMG")
        //                            .FirstOrDefault();

        //                        if (pickstorage != null
        //                            && storagedata != null
        //                            && pickstorage.batchcode == storagedata.batchcode
        //                            && pickstorage.productcode.ToUpper() == storagedata.productcode.ToUpper()
        //                            && customer.productcode.ToUpper() == pickstorage.productcode.ToUpper())
        //                        {
        //                            // Update pickstorage with values from customer
        //                            pickstorage.productcode = customer.productcode.Trim().ToUpper();
        //                            pickstorage.location = customer.locationcode.Trim();
        //                            pickstorage.batchcode = customer.batchcode.Trim();
        //                            pickstorage.boxno = customer.boxno.Trim();

        //                            _context.pickstorage.Update(pickstorage);
        //                            _context.SaveChanges();
        //                        }
        //                    }

        //                }

        //            }
        //        }
        //    }
        //    return Json(new { success = true, message = "Storage Operation Done Successfully !" });

        //}


        // Function to correct productcode if overlap is found
        private string CorrectProductCode(string productCode, string batch, string grn, List<Storage_Operation> storage)
        {
            string correctedCode = productCode;
            int counter = 1;

            int counter1 = 0; // Counts how many product codes start with the given productCode
            int counter2 = 0; // Counts how many product codes end with the given productCode
            int counter3 = 0; // Counts how many product codes contain the given productCode

            // Loop through the storage to check the product codes
            foreach (var pro in storage)
            {
                // Check if productCode starts with pro.productcode
                if (correctedCode.StartsWith(pro.productcode, StringComparison.OrdinalIgnoreCase))
                {
                    counter1++;
                }
                // Check if productCode ends with pro.productcode
                if (correctedCode.EndsWith(pro.productcode, StringComparison.OrdinalIgnoreCase))
                {
                    counter2++;
                }
                // Check if productCode contains pro.productcode
                if (correctedCode.IndexOf(pro.productcode, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    counter3++;
                }
            }

            // Find the maximum counter value and determine which counter it corresponds to
            int maxCounter = Math.Max(counter1, Math.Max(counter2, counter3));
            string maxCounterType = "";

            if (maxCounter == counter1)
            {
                maxCounterType = "StartsWith";
            }
            else if (maxCounter == counter2)
            {
                maxCounterType = "EndsWith";
            }
            else if (maxCounter == counter3)
            {
                maxCounterType = "Contains";
            }
            // Modify correctedCode based on the counts
            // Optionally, update the correctedCode if overlaps were found
            if (maxCounter > 0)
            {
                correctedCode = $"{productCode} ({maxCounter})"; // Example of modifying the product code
            }

            return correctedCode;
        }

        public ActionResult InsertCustomers(List<Storage_Operation> storage)
        {
            try
            {
                if (storage != null)
                {
                    //foreach(var item in storage)
                    //{
                    //    item.batchcode = item.batchcode?.Replace("\u001e", "") ?? "";
                    //    item.productcode = item.productcode?.Replace("\u001e", "") ?? "";
                    //    item.grnno = item.grnno?.Replace("\u001e", "") ?? "";
                    //    item.boxno = item.boxno?.Replace("\u001e", "") ?? "";
                    //}

                    var doallot = new Storage_Operation();

                    foreach (Storage_Operation customer in storage)
                    {
                        customer.batchcode = customer.batchcode?.Replace("\u001e", "") ?? "";
                        customer.productcode = customer.productcode?.Replace("\u001e", "") ?? "";
                        customer.grnno = customer.grnno?.Replace("\u001e", "") ?? "";
                        customer.boxno = customer.boxno?.Replace("\u001e", "") ?? "";

                        //CONDITION FOR STRING OVERLAP
                        //customer.productcode = CorrectProductCode(customer.productcode,customer.grnno, customer.batchcode, storage);
                        //END CONDITION FOR STRING OVERLAP

                        if (customer.productcode.ToUpper() == null || customer.batchcode == null
                            && customer.boxno == null || customer.locationcode == null || customer.grnno == null)
                        {
                            return Json(new { success = false, message = "Some values found Null !" });
                        }
                        else
                        {
                            //IF PRODUCT IS SALE RETURN PRODUCT
                            var SRData = _context.PR_model
                                .Where(a => a.productcode.Trim() == customer.productcode.Trim()
                                && a.boxno.Trim() == customer.boxno.Trim() && a.batchcode.Trim() == customer.batchcode.Trim()
                                && a.statusflag.Trim() == "LD")
                                .FirstOrDefault();

                            //CHECK IN DM_RPR_RP TABEL
                            var DMRPRRP_Data = _context.DMRPRRP
                                .Where(a => a.productcode.Trim() == customer.productcode.Trim()
                                && a.boxno.Trim() == customer.boxno.Trim()
                                && a.batch.Trim() == customer.batchcode.Trim()
                                && a.pickflag == 0
                                && (a.statusflag.Trim() == "ST" || a.statusflag.Trim() == "DMG" || a.statusflag.Trim() == "NONRPR") )
                                .FirstOrDefault();
                            //END

                            //IN CASE OF SALE RETURN
                            if (SRData != null)
                            {
                                SRData.statusflag = "ST";
                                _context.PR_model.Update(SRData);
                                _context.SaveChanges();

                                customer.statusflag = "ST";

                                int maxId = _context.Storage_Operation.Any() ? _context.Storage_Operation.Max(e => e.id) + 1 : 1;

                                doallot.id = maxId;
                                if (customer.productcode.Contains(''))
                                {
                                    doallot.productcode = customer.productcode.Replace("\u001e", "").ToUpper();
                                }
                                else
                                {
                                    doallot.productcode = customer.productcode.Trim().ToUpper();
                                }
                                doallot.boxno = customer.boxno.ToString().Trim();
                                if (customer.batchcode.Contains(''))
                                {
                                    doallot.batchcode = customer.batchcode.Replace("\u001e", "");
                                }
                                else
                                {
                                    doallot.batchcode = customer.batchcode;
                                }
                                doallot.locationcode = customer.locationcode.ToString().Trim();
                                doallot.grnno = customer.grnno.ToString().Trim();
                                doallot.statusflag = customer.statusflag.Trim();

                                _context.Add(doallot);
                                _context.SaveChanges();
                            }
                            //IN CASE OD DM/RPR/RP
                            else if (DMRPRRP_Data != null)
                            {
                                DMRPRRP_Data.location = customer.locationcode;
                                _context.DMRPRRP.Update(DMRPRRP_Data);
                                _context.SaveChanges();
                                if(DMRPRRP_Data.statusflag.Trim() == "DMG" || DMRPRRP_Data.statusflag.Trim() == "NONRPR" || DMRPRRP_Data.statusflag.Trim() == "ST")
                                {
                                    var found = _context.Storage_Operation
                                        .Where(a => a.productcode.Trim() == customer.productcode.Trim() 
                                        && a.boxno.Trim() == customer.boxno.Trim() 
                                        && a.batchcode.Trim() == customer.batchcode.Trim() 
                                        && (a.statusflag.Trim() == "DMG" || a.statusflag.Trim() == "NONRPR" || a.statusflag.Trim() == "ST"))
                                        .FirstOrDefault();
                                    if(found != null)
                                    {
                                        found.locationcode = customer.locationcode.Trim();
                                        _context.Storage_Operation.Update(found);
                                        _context.SaveChanges();
                                    }

                                    if(DMRPRRP_Data.ordertype.Trim() == "Replacement" && found == null)
                                    {
                                        
                                        int maxId = _context.Storage_Operation.Any() ? _context.Storage_Operation.Max(e => e.id) + 1 : 1;
                                        customer.statusflag = DMRPRRP_Data.statusflag.Trim();
                                        doallot.id = maxId;
                                        doallot.productcode = customer.productcode.Trim().ToUpper();
                                        doallot.boxno = customer.boxno.ToString().Trim();
                                        doallot.batchcode = customer.batchcode;
                                        doallot.locationcode = customer.locationcode.ToString().Trim();
                                        doallot.grnno = customer.grnno.ToString().Trim();
                                        doallot.statusflag = customer.statusflag.Trim();
                                        doallot.pickflag = "0";
                                        _context.Add(doallot);
                                        _context.SaveChanges();
                                    }
                                }
                            }
                            //IN CASE STOCK
                            else
                            {
                                var splitbox1 = customer.boxno.Split("-");
                                var firstpart = splitbox1[0];
                                var secondpart = splitbox1[1];
                                var splitsecand = splitbox1[1].Split("/");
                                var numerator = splitsecand[0];
                                var denominator = splitsecand[1];

                                if(Convert.ToInt64(numerator) > Convert.ToInt64(denominator))
                                {
                                    int counter = 1;
                                    var chekkout = _context.Storage_Operation
                                        .Where(a => a.productcode.Trim() == customer.productcode.Trim()
                                                    && a.batchcode.Trim() == customer.batchcode.Trim())
                                        .ToList();

                                    List<string> list = new List<string>();
                                    var qty = 1;
                                    var box = 1;

                                    if (chekkout.Count > 0)
                                    {
                                        counter = chekkout.Count;
                                        for (int i = 1; i <= Convert.ToInt64(numerator); i++)
                                        {
                                            counter++;
                                            // Create the data string based on loop index, quantity, and box number
                                            var data = counter.ToString() + "-" + qty + "/" + box;
                                            list.Add(data);
                                        }
                                    }
                                    else
                                    {
                                        for (int i = counter; i <= Convert.ToInt64(numerator); i++)
                                        {
                                            // Create the data string based on loop index, quantity, and box number
                                            var data = i.ToString() + "-" + qty + "/" + box;
                                            list.Add(data);
                                        }
                                    }

                                    

                                    int count = 0;
                                    foreach(var boxes in list)
                                    {
                                        count++;
                                        customer.statusflag = "ST";

                                        int maxId = _context.Storage_Operation.Any() ? _context.Storage_Operation.Max(e => e.id) + 1 : 1;

                                        doallot.id = maxId;
                                        doallot.productcode = customer.productcode.Trim().ToUpper();
                                        doallot.batchcode = customer.batchcode;
                                        //var splitBox = boxes.Split('-');
                                        //splitBox[0] = count.ToString();
                                        doallot.boxno = boxes;
                                        //doallot.boxno = string.Join("-", splitBox).Trim();
                                        doallot.locationcode = customer.locationcode.ToString().Trim();
                                        doallot.grnno = customer.grnno.ToString().Trim();
                                        doallot.statusflag = customer.statusflag.Trim();

                                        _context.Add(doallot);
                                        _context.SaveChanges();
                                    }
                                }
                                else
                                {
                                    customer.statusflag = "ST";

                                    int maxId = _context.Storage_Operation.Any() ? _context.Storage_Operation.Max(e => e.id) + 1 : 1;

                                    doallot.id = maxId;
                                    if (customer.productcode.Contains(''))
                                    {
                                        doallot.productcode = customer.productcode.Replace("\u001e", "").ToUpper();
                                    }
                                    else
                                    {
                                        doallot.productcode = customer.productcode.Trim().ToUpper();
                                    }
                                    doallot.boxno = customer.boxno.ToString().Trim();
                                    if (customer.batchcode.Contains(''))
                                    {
                                        doallot.batchcode = customer.batchcode.Replace("\u001e", "");
                                    }
                                    else
                                    {
                                        doallot.batchcode = customer.batchcode;
                                    }
                                    doallot.locationcode = customer.locationcode.ToString().Trim();
                                    doallot.grnno = customer.grnno.ToString().Trim();
                                    doallot.statusflag = customer.statusflag.Trim();

                                    _context.Add(doallot);
                                    _context.SaveChanges();
                                }

                                
                            }


                            var splitbox = customer.boxno.Split('-')[1];

                            //logic if found damage product box in picking 
                            var pickstorage = _context.pickstorage
                                .Where(a => a.productcode.Trim().ToUpper() == customer.productcode.Trim().ToUpper()
                                && a.boxno.Contains(splitbox.Trim()) && a.flag == 2)
                                .FirstOrDefault();

                            var storagedata = _context.Storage_Operation
                                .Where(a => a.productcode.Trim().ToUpper() == customer.productcode.Trim().ToUpper()
                                && a.boxno.Contains(splitbox.Trim())
                                && a.statusflag == "DMG")
                                .FirstOrDefault();

                            if (pickstorage != null && storagedata != null)
                            {
                                // Update pickstorage with values from customer
                                pickstorage.productcode = customer.productcode.Trim().ToUpper();
                                pickstorage.location = customer.locationcode.Trim();
                                pickstorage.batchcode = customer.batchcode.Trim();
                                pickstorage.boxno = customer.boxno.Trim();
                                pickstorage.flag = 0;
                                _context.pickstorage.Update(pickstorage);
                                _context.SaveChanges();
                                //doallot.pickflag = "1";
                                //_context.Storage_Operation.Update(doallot);
                                //_context.SaveChanges();
                            }
                        }
                    }
                }
                Storage_OperationAdd.Clear();
                return Json(new { success = true, message = "Storage Operation Done Successfully !" });
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }

        public ActionResult InsertCustomersDMG(List<Storage_Operation> storage)
        {
            try
            {
                if (storage != null)
                {

                    var doallot = new Storage_Operation();

                    foreach (Storage_Operation customer in storage)
                    {

                        customer.batchcode = customer.batchcode?.Replace("\u001e", "") ?? "";
                        customer.productcode = customer.productcode?.Replace("\u001e", "") ?? "";
                        customer.grnno = customer.grnno?.Replace("\u001e", "") ?? "";
                        customer.boxno = customer.boxno?.Replace("\u001e", "") ?? "";

                        if (customer.productcode.ToUpper() == null || customer.batchcode == null
                            && customer.boxno == null || customer.locationcode == null || customer.grnno == null)
                        {
                            return Json(new { success = false, message = "Some values found Null !" });
                        }
                        else
                        {
                            //IN CASE STOCK
                           
                            customer.statusflag = "DMG";

                            var found = _context.Storage_Operation
                            .Where(a => a.productcode.Trim().ToUpper() == customer.productcode.Trim().ToUpper()
                            && a.boxno.Trim() == customer.boxno.Trim()
                            && a.batchcode.Trim() == customer.batchcode.Trim()
                            && a.grnno.Trim() == customer.grnno.Trim()).FirstOrDefault();

                            if (found != null)
                            {
                                found.locationcode = customer.locationcode;
                                found.statusflag = customer.statusflag;
                                _context.Storage_Operation.Update(found);
                                _context.SaveChanges();
                            }

                        }
                    }
                }
                Storage_OperationAdd.Clear();
                return Json(new { success = true, message = "Damage stock Storage Operation Done Successfully !" });
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }

            //if (storage != null)
            //{
            //    foreach (Storage_Operation customer in storage)
            //    {
            //        customer.productcode = customer.productcode.ToUpper();
            //        customer.batchcode = customer.batchcode.ToUpper();

            //        //string pattern = @"[^a-zA-Z0-9\-\/$ ]";
            //        //customer.batchcode = Regex.Replace(customer.batchcode?.Trim(), pattern, "");

            //        bool recordExistsWithDMGStatus = _context.Storage_Operation
            //            .Any(e => e.productcode.Trim().ToUpper() == customer.productcode.Trim().ToUpper()
            //                && e.boxno.Trim() == customer.boxno.Trim()
            //                && e.batchcode.Trim() == customer.batchcode.Trim()
            //                && e.grnno.Trim() == customer.grnno.Trim()
            //                && e.statusflag == "DMG");

            //        if (!recordExistsWithDMGStatus)
            //        {
            //            bool recordExists = _context.Storage_Operation
            //                .Any(e => e.productcode.Trim().ToUpper() == customer.productcode.Trim().ToUpper()
            //                    && e.boxno.Trim() == customer.boxno.Trim()
            //                    && e.batchcode.Trim() == customer.batchcode.Trim()
            //                    && e.grnno.Trim() == customer.grnno.Trim()
            //                    );

            //            if (recordExists)
            //            {
            //                // Replace the existing data with status flag "DMG"
            //                var existingData = _context.Storage_Operation
            //                    .FirstOrDefault(e => e.productcode.Trim().ToUpper() == customer.productcode.Trim().ToUpper()
            //                        && e.boxno.Trim() == customer.boxno.Trim()
            //                        && e.batchcode.Trim() == customer.batchcode.Trim()
            //                        && e.grnno.Trim() == customer.grnno.Trim());

            //                existingData.statusflag = "DMG";
            //                //existingData.balanceqty = "-";

            //                _context.SaveChanges();
            //            }
            //            else
            //            {
            //                // Add new data with status flag "DMG"
            //                int maxId = _context.Storage_Operation.Any() ? _context.Storage_Operation.Max(e => e.id) + 1 : 1;

            //                var newData = new Storage_Operation
            //                {
            //                    id = maxId,
            //                    productcode = customer.productcode?.Trim().ToUpper(),
            //                    boxno = customer.boxno?.Trim(),
            //                    grnno = customer.grnno?.Trim(),
            //                    batchcode = customer.batchcode?.Trim(),
            //                    locationcode = customer.locationcode?.Trim(),
            //                    statusflag = "DMG",
            //                    //balanceqty = "-"
            //                };

            //                _context.Add(newData);
            //                _context.SaveChanges();
            //                _notifyService.Success("Damage Product Added Successfully !");
            //            }
            //        }
            //        else
            //        {
            //            // Record with the same criteria already exists with status flag "DMG", so skip insertion
            //            // You can handle this scenario as needed
            //        }
            //    }
            //}

            //return Json(new { success = true });
        }

        // GET: Storage_Operation/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Storage_Operation == null)
            {
                return NotFound();
            }

            var storage_Operation = await _context.Storage_Operation.FindAsync(id);
            if (storage_Operation == null)
            {
                return NotFound();
            }
            return View(storage_Operation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Storage_Operation storage_Operation)
        {
            if (id != storage_Operation.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(storage_Operation);
                    //maintain logs
                    var user = HttpContext.Session.GetString("User");
                    var logs = new Logs();
                    logs.pagename = "Storage Operation";
                    logs.task = "Edit";
                    logs.taskid = id;
                    logs.action = "Update";
                    logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                    logs.time = DateTime.Now.ToString("HH:mm:ss");
                    logs.username = user;
                    _context.Add(logs);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Storage_OperationExists(storage_Operation.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(storage_Operation);
        }

        public ActionResult damageStockQty(string productcode, string boxno, string batchcode, string locationcode, string grnno)
        {
            try
            {
                // Assuming you're using Entity Framework or any other ORM
                var storageOperation = _context.Storage_Operation.FirstOrDefault(s => s.productcode.ToUpper() == productcode.ToUpper() && s.boxno == boxno && s.batchcode == batchcode && s.locationcode == locationcode && s.statusflag == "ST");

                if (storageOperation != null)
                {
                    storageOperation.statusflag = "DMG";
                    _context.SaveChanges();

                    return Json(new { success = true, message = "Box number " + boxno + " of product " + productcode + " is marked as damaged." });
                }
                else
                {
                    return Json(new { success = false, message = "That Box of product Not found in Storage record ." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating the storage operation record." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (_context.Storage_Operation == null)
            {
                return Problem("Entity set 'AuthDbContext.storage_Operation'  is null.");
            }
            var supplier = _context.Storage_Operation.Where(a => a.id == id).FirstOrDefault(); ;
            if (supplier != null)
            {
                _context.Storage_Operation.Remove(supplier);
            }

            await _context.SaveChangesAsync();
            _notifyService.Error("Record Deleted Succesfully");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult saverow(string optionValue)
        {
            // Perform any server-side processing here (e.g., save to database)
            // For simplicity, let's just return the received textData as JSON
            return Json(new { success = true, data = optionValue });
        }

        //------------------------------ 29th July 2023 ----------- /
        //public IActionResult grnno(string selectedValue)
        //{
        //    inward Inward = new inward();
        //    Inward.inward_id = _context
        //    var dee = _context.inward.Where(e => e.grnno == selectedValue).FirstOrDefault();

        //    var value1 = _context.Packets.Where();
        //    // Perform any server-side processing here (e.g., save to database)
        //    // For simplicity, let's just return the received textData as JSON
        // return Json(dee);
        //}

        private bool Storage_OperationExists(int id)
        {
            return (_context.Storage_Operation?.Any(e => e.id == id)).GetValueOrDefault();
        }
        public IActionResult Cleared()
        {
            try
            {
                List<Storage_Operation> pst = _context.Storage_Operation.ToList();
                if (pst.Count > 0)
                {
                    _context.Storage_Operation.RemoveRange(_context.Storage_Operation);
                    _context.SaveChanges();
                    return Json(new { success = true, message = "All Cleared" });
                }
                else
                {
                    return Json(new { success = false, message = "Data is Already Empty" });
                }

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred" });
            }

        }
    }
}
