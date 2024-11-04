using AspNetCore;
using AuthSystem.Data;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using Elasticsearch.Net;
using eros.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.Web.CodeGeneration.DotNet;
using Nest;
using Newtonsoft.Json;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Text.Json;

using static eros.Controllers.Loading_Dispatch_OperationController;

namespace eros.Controllers
{
    public class Loading_Dispatch_OperationController : Controller
    {
        private readonly ErosDbContext _context;

        public Loading_Dispatch_OperationController(ErosDbContext context)
        {
            _context = context;
        }


        public IActionResult TMPDataDetails(string sono)
        {
            List<Picking_Operation> picklistDataList = new List<Picking_Operation>();
            var groupedData = _context.Picking_Operation.Where(a=>a.flag== 0 && a.sono.Trim() == sono.Trim())
                .ToList();


            if (groupedData.Count > 0)
            {
                foreach (var item in groupedData)
                {
                    //var adjustedQty = item.sumqty == 0 ? 1 : item.sumqty;

                    var data = new Picking_Operation
                    {
                        productcode = item.productcode,
                        location = item.location,
                        batchcode = item.batchcode,
                        boxno = item.boxno,
                        //instockqty = adjustedQty.ToString(),
                    };

                    // Add the data object to the list
                    picklistDataList.Add(data);
                }
            }

            if (picklistDataList.Count > 0)
            {
                return Json(new { success = true, data = picklistDataList });
            }
            else
            {
                return Json(new { success = false, message = "No Data Found to pick agaisnt order no " + sono + " !" });
            }
        }

        public IActionResult Suspend(string sono)
        {
            var get_pgdata = _context.Picking_Operation.Where(a => a.sono.Trim() == sono && a.flag == 1).ToList();
            //var get_pg = _context.Picklist_Generation.Where(a => a.sono.Trim() == sono.Trim()).ToList();
            var get_podata = _context.Load_Dispatchtable.Where(a => a.sono.Trim() == sono).ToList();

            if (get_pgdata.Count > 0)
            {
                foreach (var pgData in get_pgdata)
                {
                    pgData.flag = 0;
                    _context.Update(pgData);
                }
            }
            else
            {
                return Json(new { success = false, message = "Please Add data !" });
            }

            //if (get_podata != null)
            //{
            //    _context.Picking_Operation.RemoveRange(get_podata);
            //}
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        public class SonoStatusViewModel
        {
            public int Id { get; set; } // Add this property if it doesn't exist
            public string Sono { get; set; }
            public string Status { get; set; }
            public bool HasResume { get; set; }
            public bool HasSuspend { get; set; }
            public string Date { get; set; } // Define the Date property

        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var distinctSonoList = _context.Load_Dispatchtable
                                            .Select(p => p.sono)
                                            .Distinct()
                                            .ToList();

                List<SonoStatusViewModel> sonoStatusList = new List<SonoStatusViewModel>();

                foreach (var sono in distinctSonoList)
                {
                    var collectSoData = _context.Picking_Operation.Where(a => a.sono == sono).ToList();
                    var pocollectionData = _context.Load_Dispatchtable.Where(a => a.sono == sono).ToList();
                    var loaddata = _context.Loading_Dispatch_Operation.Where(a => a.sono == sono).ToList();

                    bool hasSuspend = pocollectionData.Any(data => data.statusflag == "0" || data.statusflag == "1");
                    bool isExit = pocollectionData.All(data => data.statusflag == "1");

                    string status;

                    if (loaddata.Count == 0 && pocollectionData.Count != collectSoData.Count)
                    {
                        status = "Suspend";
                    }

                    else if (collectSoData.Count == pocollectionData.Count && loaddata.Count == collectSoData.Count && pocollectionData.Count == loaddata.Count)
                    {
                        status = "Completed";
                    }
                    else
                    {
                        continue;
                    }

                    // Fetch Id based on your logic, assuming you have some Id associated with sono
                    var id = _context.Loading_Dispatch_Operation.FirstOrDefault(x => x.sono == sono)?.id;
                    var date = _context.Loading_Dispatch_Operation.Where(x => x.sono == sono).Select(x => x.currentdate).FirstOrDefault();


                    sonoStatusList.Add(new SonoStatusViewModel
                    {
                        Id = id ?? 0, // Assign the fetched Id here; use a default value if Id is nullable
                        Sono = sono,
                        Status = status,
                        Date = date,
                        // Format the date as "dd-MM-yyyy"
                        HasSuspend = hasSuspend
                    });
                }
                // Order sonoStatusList by date in descending order
                sonoStatusList = sonoStatusList.OrderByDescending(x => x.Date).ToList();
                return View(sonoStatusList);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return View(error);
            }
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Loading_Dispatch_Operation == null)
            {
                return NotFound();
            }

            var Loading_Dispatch_Operation = await _context.Loading_Dispatch_Operation
                .FirstOrDefaultAsync(m => m.id == id);
            if (Loading_Dispatch_Operation == null)
            {
                return NotFound();
            }

            return View(Loading_Dispatch_Operation);
        }
        public IActionResult Create()
        {
            try
            {
                Loading_Dispatch_Operation applicant = new Loading_Dispatch_Operation();

                ViewBag.data1 = SetData1InViewBag();
                ViewBag.datanew = SetDataNewInViewBag();
                ViewBag.couriername = SetCourierNameInViewBag();
                ViewBag.transportname = SetTransportNameInViewBag();
                ViewBag.loaddata = GetWbridgeItems();

                return View();
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return View(error);
            }
        }
        //private List<SelectListItem> GetWbridgeItems()
        //{

        //    LoadData_table applicant1 = new LoadData_table();
        //    List<SelectListItem> wbridge1 = _context.LoadData_table
        //                                    .Where(m => m.complete_flag == "0")
        //                                    .AsNoTracking().OrderBy(n => n.loadingsheetno)
        //                                    .Select(n => new SelectListItem
        //                                    {
        //                                        Value = n.loadingsheetno.ToString(),
        //                                        Text = n.loadingsheetno.ToString(),

        //                                    })
        //                                    .Distinct()
        //                                    .ToList();
        //    var defItem1 = new SelectListItem()
        //    {
        //        Value = "",
        //        Text = "--Select Shipment ID--"
        //    };

        //    wbridge1.Insert(0, defItem1);
        //    return wbridge1;
        //}

        private List<SelectListItem> GetWbridgeItems()
        {
            var wbridgeItems = _context.LoadData_table
                .Where(m => m.complete_flag == "0")
                .AsNoTracking()
                .OrderBy(n => n.loadingsheetno)
                .Select(n => new SelectListItem
                {
                    Value = n.loadingsheetno.ToString(),
                    Text = n.loadingsheetno.ToString()
                })
                .Distinct()
                .ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "--Select Shipment ID--"
            };

            wbridgeItems.Insert(0, defItem);

            return wbridgeItems;
        }

        private List<SelectListItem> SetData1InViewBag()
        {
            var uniqueSonoValues11 = _context.Picking_Operation
                .Where(n => n.flag == 0)
                .Select(n => n.sono)
                .Distinct()
                .ToList();
            var uniqueSonoValues = _context.Picking_Operation
                .Select(n => n.sono)
                .Distinct()
                .ToList();
            List<SelectListItem> wbridge = new List<SelectListItem>();
            foreach (var sono in uniqueSonoValues)
            {
                var collectSoData = _context.pickstorage.Where(a => a.sono.Trim() == sono.Trim()).ToList();
                var pocollectionData = _context.Picking_Operation.Where(a => a.sono.Trim() == sono.Trim()).ToList();
                var loadingdata = _context.Load_Dispatchtable.Where(a => a.sono.Trim() == sono.Trim()).ToList();
                var loading = _context.Loading_Dispatch_Operation.Where(a => a.sono.Trim() == sono.Trim()).ToList();
                var soinward = _context.so_inward.FirstOrDefault(a => a.sono.Trim() == sono.Trim() && a.status == "Pending");

                if (collectSoData.Count == pocollectionData.Count && soinward != null && loading.Count <= pocollectionData.Count)
                    //&& loading.Count <= pocollectionData.Count
                {
                    //if(loadingdata.Count == 0  || loadingdata.Count < pocollectionData.Count)
                    //{
                    wbridge.Add(new SelectListItem
                    {
                        Value = sono,
                        Text = sono.ToString()
                    });
                    //}

                }
            }
            var defItem = new SelectListItem
            {
                Value = "",
                Text = "---Select Order No. ---"
            };
            wbridge.Insert(0, defItem);
            return wbridge;
        }
        private List<SelectListItem> SetDataNewInViewBag()
        {
            var uniqueSonoValues = _context.Picking_Operation
                //.Where(n => n.flag == 0)
                .Select(n => n.sono)
                .Distinct()
                .ToList();
            List<SelectListItem> wbridge2 = new List<SelectListItem>();
            foreach (var sono in uniqueSonoValues)
            {
                //var LoadData_table = _context.LoadData_table.Where(a => a.sono == sono && a.complete_flag == "0").ToList();
                var collectSoData = _context.pickstorage.Where(a => a.sono.Trim() == sono.Trim()).ToList();
                var pocollectionData = _context.Picking_Operation.Where(a => a.sono.Trim() == sono.Trim()).ToList();
                var loadingdata = _context.Load_Dispatchtable.Where(a => a.sono.Trim() == sono.Trim()).ToList();
                var loading = _context.Loading_Dispatch_Operation.Where(a => a.sono.Trim() == sono.Trim()).ToList();
                var soinward = _context.so_inward.FirstOrDefault(a => a.sono.Trim() == sono.Trim() && a.status == "Pending");

                if (collectSoData.Count == pocollectionData.Count 
                    && soinward != null && loading.Count <= pocollectionData.Count)
                    //&& loading.Count <= pocollectionData.Count)
                    //&& (LoadData_table == null || LoadData_table.Count > 0))
                {
                    wbridge2.Add(new SelectListItem
                    {
                        Value = sono,
                        Text = sono.ToString()
                    });
                }
            }
            return wbridge2;
        }
        private List<SelectListItem> SetCourierNameInViewBag()
        {
            var lstProducts = _context.Courier_Master.AsNoTracking().Select(n =>
                new SelectListItem
                {
                    Value = n.couriername,
                    Text = n.couriername
                }).ToList();

            lstProducts.Insert(0, new SelectListItem { Text = "--Select Courier--", Value = "0" });
            return lstProducts;
        }
        private List<SelectListItem> SetTransportNameInViewBag()
        {
            var lstProducts1 = _context.Transport_Master.AsNoTracking().Select(n =>
                new SelectListItem
                {
                    Value = n.transportname,
                    Text = n.transportname
                }).ToList();

            lstProducts1.Insert(0, new SelectListItem { Text = "--Select Transport--", Value = "0" });
            return lstProducts1;
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Loading_Dispatch_Operation == null)
            {
                return NotFound();
            }

            var Loading_Dispatch_Operation = await _context.Loading_Dispatch_Operation.FindAsync(id);
            if (Loading_Dispatch_Operation == null)
            {
                return NotFound();
            }
            return View(Loading_Dispatch_Operation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,rackname,shelves,bin")] Loading_Dispatch_Operation Loading_Dispatch_Operation)
        {
            if (id != Loading_Dispatch_Operation.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(Loading_Dispatch_Operation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!rack_masterExists(Loading_Dispatch_Operation.id))
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
            return View(Loading_Dispatch_Operation);
        }

        // GET: Loading_Dispatch_Operation/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Loading_Dispatch_Operation == null)
            {
                return NotFound();
            }

            var Loading_Dispatch_Operation = await _context.Loading_Dispatch_Operation
                .FirstOrDefaultAsync(m => m.id == id);
            if (Loading_Dispatch_Operation == null)
            {
                return NotFound();
            }

            return View(Loading_Dispatch_Operation);
        }

        // POST: Loading_Dispatch_Operation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Loading_Dispatch_Operation == null)
            {
                return Problem("Entity set 'ErosDbContext.Loading_Dispatch_Operation'  is null.");
            }
            var Loading_Dispatch_Operation = await _context.Loading_Dispatch_Operation.FindAsync(id);
            if (Loading_Dispatch_Operation != null)
            {
                _context.Loading_Dispatch_Operation.Remove(Loading_Dispatch_Operation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool rack_masterExists(int id)
        {
            return (_context.Loading_Dispatch_Operation?.Any(e => e.id == id)).GetValueOrDefault();
        }

        [HttpPost]
        public IActionResult Getdispatch(string optionValue)
        {
            List<Loading_Dispatch_Operation> picklistDataList = new List<Loading_Dispatch_Operation>();
            try
            {
                var remainingScanCounter = 0;
                var scannedShipperCounter = 0;
                var totalShipperCounter = 0;

                var remaining = _context.Picking_Operation.Count(a => a.sono == optionValue && a.flag == 0);
                var scanned = _context.Picking_Operation.Count(a => a.sono == optionValue && a.flag == 1);
                var totalcount = remaining + scanned;

                var saleorder = _context.Picking_Operation
                    .Where(m => m.sono == optionValue && m.flag == 0)
                    .ToList();
                foreach (var mat in saleorder)
                {
                    var data = new Loading_Dispatch_Operation
                    {
                        sono = mat.sono,
                        productcode = mat.productcode.ToUpper(),
                        boxno = mat.boxno.ToString(),
                        batchcode = mat.batchcode.ToString(),
                        location = mat.location.ToString(),
                    };
                    picklistDataList.Add(data);
                }
                var exist = _context.Picking_Operation
                    .Where(m => m.sono == optionValue && m.flag == 1)
                    .ToList();
                var data1 = picklistDataList
                    .Where(p => !exist.Any(e => e.boxno == p.boxno
                     && e.batchcode == p.batchcode
                     && e.productcode.ToUpper() == p.productcode.ToUpper() && e.flag == 1))
                    .ToList();
                var result = new
                {
                    data2 = exist,
                    data1 = picklistDataList,
                    remaining = remaining,
                    scanned = scanned,
                    totalcount = totalcount,
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            return Json(picklistDataList);
        }

        [HttpGet]
        //public ActionResult Getload(string selectedValue, string selectedValue1, string selectedValue2, string sono)
        //{
        //    var remainingScanCounter = 0;
        //    var scannedShipperCounter = 0;
        //    var totalShipperCounter = 0;

        //    var remaining = _context.Picking_Operation.Where(a => a.sono == sono && a.flag == 0).ToList();
        //    var scanned = _context.Picking_Operation.Where(a => a.sono == sono && a.flag == 1).ToList();
        //    var totalcount = Convert.ToInt32(remaining) + Convert.ToInt32(scanned);


        //    var table = _context.Picking_Operation.Where(x => x.boxno.Trim() == selectedValue.Trim() && x.productcode.Trim() == selectedValue1.Trim() && x.batchcode.Trim() == selectedValue2.Trim()).OrderByDescending(x => x.batchcode).FirstOrDefault();

        //    if (table != null)
        //    {
        //        var result = new
        //        {
        //            boxno = table.boxno,
        //            productcode = table.productcode,
        //            batchcode = table.batchcode,
        //            remaining = remaining,
        //            scanned = scanned,
        //            totalcount = totalcount,
        //        };

        //        var loadlable = _context.Picking_Operation.Where(x => x.boxno.Trim() == selectedValue.Trim() && x.productcode.Trim() == selectedValue1.Trim() && x.batchcode.Trim() == selectedValue2.Trim()).OrderByDescending(x => x.batchcode).FirstOrDefault();
        //        if (loadlable != null)
        //        {
        //            loadlable.flag = 1;
        //        }
        //        _context.Picking_Operation.Update(loadlable);
        //        _context.SaveChanges();

        //        Load_Dispatchtable Load_Dispatchtable = new Load_Dispatchtable()
        //        {
        //            sono = table.sono,

        //            productcode = table.productcode,

        //            productname = "-",

        //            location = table.location,

        //            batchcode = table.batchcode,
        //            statusflag = "0",
        //            boxno = table.boxno,
        //            // date = DateTime.Now.ToString("dd/MM/yyyy"),
        //            date = DateTime.Now.ToUniversalTime(),
        //            time = DateTime.Now.ToString("hh:ss:tt")


        //        };
        //        _context.Load_Dispatchtable.Add(Load_Dispatchtable);
        //        _context.SaveChanges();
        //        return Json(result);
        //    }
        //    else
        //    {
        //        // Handle the case where the record is not found
        //        return Json(new { error = "Record not found" });
        //    }
        //}
        public ActionResult Getload(string boxno, string procode, string batch, string sono, int remainingScanCounter, int scannedShipperCounter, int totalShipperCounter)
        {

            boxno = boxno?.Replace("\u001e", "") ?? "";
            procode = procode?.Replace("\u001e", "") ?? "";
            batch = batch?.Replace("\u001e", "") ?? "";
            sono = sono?.Replace("\u001e", "") ?? "";

            var remaining = _context.Picking_Operation.Count(a => a.sono == sono && a.flag == 0);
            var scanned = _context.Picking_Operation.Count(a => a.sono == sono && a.flag == 1);
            var totalcount = remaining + scanned;
            List<Load_Dispatchtable> loadpicklist = new List<Load_Dispatchtable>();
            var splitbox = boxno.Trim().Split("-")[1];
            var splitbox1 = splitbox.Split("/");
            var num = splitbox1[0];
            var den = splitbox1[1];
            if(Convert.ToInt32(num) > Convert.ToInt32(den))
            {
                var correct_or_not = _context.Picking_Operation
                    .Where(a=>a.boxpicked.Trim() ==  boxno 
                    && a.flag == 0 
                    && a.sono.Trim() == sono.Trim())
                    .ToList();

                if(correct_or_not.Count > 0)
                {
                    var picking = _context.Picking_Operation
                    .Where(a => a.sono.Trim() == sono.Trim()
                    && a.productcode.Trim() == procode.Trim())
                    .ToList();

                    var loading = _context.Load_Dispatchtable
                        .Where(a => a.sono.Trim() == sono.Trim()
                        && a.productcode.Trim() == procode.Trim())
                        .ToList();

                    if (picking.Count == loading.Count)
                    {
                        return Json(new { success = false, message = "You have already scan all shipper of that product !" });
                    }
                    else
                    {
                        var found_picking = _context.Picking_Operation
                        .Where(a => a.productcode.Trim().ToUpper() == procode.Trim().ToUpper()
                        && a.batchcode.Trim() == batch.Trim()
                        && a.boxpicked.Trim() == boxno.Trim()
                        && a.flag == 0
                        && a.sono.Trim() == sono.Trim())
                        .ToList();
                        if (found_picking.Count > 0)
                        {
                            int loopLimit = Math.Min(found_picking.Count, Convert.ToInt32(num));
                            foreach (var item in found_picking)
                            {
                                Load_Dispatchtable Load_Dispatchtable = new Load_Dispatchtable()
                                {
                                    sono = sono,
                                    productcode = item.productcode,
                                    productname = "-",
                                    location = item.location,
                                    batchcode = item.batchcode,
                                    statusflag = "0",
                                    boxno = item.boxno,
                                };

                                _context.Load_Dispatchtable.Add(Load_Dispatchtable);
                                _context.SaveChanges();
                                loadpicklist.Add(Load_Dispatchtable);

                                item.flag = 1;
                                _context.Picking_Operation.Update(item);
                                _context.SaveChanges();

                            }

                            var foundtotalcount = _context.Picking_Operation.Where(a => a.sono.Trim() == sono.Trim()).ToList();
                            var foundremcount = _context.Picking_Operation.Where(a => a.sono.Trim() == sono.Trim() && a.flag == 0).ToList();
                            var foundscancount = _context.Picking_Operation.Where(a => a.sono.Trim() == sono.Trim() && a.flag == 1).ToList();

                            return Json(new
                            {
                                success = true,
                                loadpicklist,
                                flag = 1,
                                remaining = foundremcount,
                                scanned = foundscancount,
                                totalcount = foundtotalcount,
                            });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Please scan correct shipper !" });
                        }
                    }

                }
                else
                {
                    return Json(new { success = false, message = "Please scan correct boxno !" });
                }

            }
            else
            {
                var check = _context.Picking_Operation
                               .Where(x => x.boxno.Trim() == boxno.Trim()
                               && x.productcode.Trim().ToUpper() == procode.Trim().ToUpper()
                               && x.batchcode.Trim() == batch.Trim()
                               && x.sono.Trim() == sono.Trim()
                               && x.flag == 1)
                               .FirstOrDefault();

                if (check != null)
                {
                    return Json(new { success = false, message = "Already scanned !" });
                }
                else
                {
                    var table = _context.Picking_Operation
                   .Where(x => x.boxno.Trim() == boxno.Trim()
                   && x.productcode.Trim().ToUpper() == procode.Trim().ToUpper()
                   && x.batchcode.Trim() == batch.Trim()
                   && x.sono.Trim() == sono.Trim()
                   && x.flag == 0)
                   .FirstOrDefault();

                    if (table != null)
                    {
                        table.flag = 1;
                        _context.Picking_Operation.Update(table);
                        _context.SaveChanges();

                        var result = new
                        {
                            flag = 0,
                            boxno = table.boxno,
                            productcode = table.productcode,
                            batchcode = table.batchcode,
                            remaining = remainingScanCounter - 1,
                            scanned = scannedShipperCounter + 1,
                            totalcount = totalShipperCounter,
                        };

                        Load_Dispatchtable Load_Dispatchtable = new Load_Dispatchtable()
                        {
                            sono = table.sono,
                            productcode = table.productcode,
                            productname = "-",
                            location = table.location,
                            batchcode = table.batchcode,
                            statusflag = "0",
                            boxno = table.boxno,
                        };

                        _context.Load_Dispatchtable.Add(Load_Dispatchtable);
                        _context.SaveChanges();

                        //maintain logs
                        var user = HttpContext.Session.GetString("User");
                        var logs = new Logs();
                        logs.pagename = "Loading Dispatch";
                        logs.task = "Scan Product loaded to dispatch";
                        logs.taskid = Load_Dispatchtable.Id;
                        logs.action = "Create";
                        logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                        logs.time = DateTime.Now.ToString("HH:mm:ss");
                        logs.username = user;

                        _context.Add(logs);
                        _context.SaveChanges();


                        return Json(new {success = true, result, flag = 0 });
                    }
                    else
                    {
                        // Handle the case where the record is not found
                        return Json(new { error = "Record not found" });
                    }
                }

            }

        }


        [HttpPost]
        //,int remainingScanCounter, int scannedShipperCounter, int totalShipperCounter
        public IActionResult GetMultiple(string optionValue, int remainingScanCounter, int scannedShipperCounter, int totalShipperCounter)
        {
            List<Loading_Dispatch_Operation> picklistDataList = new List<Loading_Dispatch_Operation>();
            List<Load_Dispatchtable> DataList = new List<Load_Dispatchtable>();

            try
            {
                
                //Use a single query to retrieve the necessary data
                var saleorder = _context.LoadData_table
                    .Where(mat => mat.loadingsheetno == Convert.ToInt32(optionValue))
                    .ToList();
                
                int totalRemaining = 0; // Initialize total remaining count
                int totalScanned = 0; // Initialize total scanned count
                int totalsumcount = 0; // Initialize total scanned count

                var exist = new List<Load_Dispatchtable>();
                foreach (var mat in saleorder)
                {
                    var remaining = _context.Picking_Operation.Count(a => a.sono == mat.sono && a.flag == 0);
                    var scanned = _context.Picking_Operation.Count(a => a.sono == mat.sono && a.flag == 1);
                    var totalcount = remaining + scanned;

                    totalRemaining += remaining; // Accumulate remaining count
                    totalScanned += scanned; // Accumulate scanned count
                    totalsumcount += totalcount;

                    var loaddata = _context.Picking_Operation.Where(x => x.sono == mat.sono && x.flag == 0).ToList();

                    var disdata = _context.Picking_Operation.Where(x => x.sono == mat.sono && x.flag == 1).ToList();
                    if (disdata.Any())
                    {
                        exist = _context.Load_Dispatchtable
                            .Where(x => x.sono == mat.sono).ToList();
                        // Create a new list for data1 excluding items from exist
                        var data4 = picklistDataList
                            .Where(p => !exist.Any(e => e.boxno == p.boxno && e.batchcode == p.batchcode && e.productcode.ToUpper() == p.productcode.ToUpper())).ToList();
                        DataList.AddRange(exist);
                    }

                    if (loaddata.Any())
                    {
                        foreach (var pickdata in loaddata)
                        {
                            var data = new Loading_Dispatch_Operation
                            {
                                sono = pickdata.sono,
                                productcode = pickdata.productcode,
                                boxno = pickdata.boxno.ToString(),
                                batchcode = pickdata.batchcode.ToString(),
                                location = pickdata.location.ToString(),
                            };
                            picklistDataList.Add(data);
                        }
                        
                    }

                }
                var result = new
                {
                    //data3 = DataList,
                    //data4 = picklistDataList,
                    //data5 = saleorder.Select(x => x.sono).ToList(),
                    data3 = DataList,
                    data4 = picklistDataList,
                    data5 = saleorder.Select(x => x.sono).ToList(),
                    remaining = remainingScanCounter + totalRemaining, // Add total remaining to remainingScanCounter
                    scanned = scannedShipperCounter + totalScanned, // Add total scanned to scannedShipperCounter
                    totalcount = totalShipperCounter + totalsumcount,
                };
                return Ok(result);
            }

            catch (Exception ex)
            {
                //Log or handle the exception appropriately
                string msg = ex.Message;
            }

            return Json(picklistDataList);
        }

        [HttpPost]
        public IActionResult SaveDataToDatabase([FromBody] List<LoadData_table> data)
        {
            try
            {

                // Assuming LoadData_table is your DbSet in the DbContext
                var maxLoadingSheetNo = _context.LoadData_table.Max(a => (int?)a.loadingsheetno) ?? 0;
                maxLoadingSheetNo++;


                // Save data to the database using Entity Framework
                foreach (var item in data)
                {
                    var existingData = _context.LoadData_table.Where(a => a.sono == item.sono).ToList();
                    if (existingData.Any())
                    {
                        _context.LoadData_table.RemoveRange(existingData); // Remove existing data for the given sono
                    }
                    var entity = new LoadData_table
                    {
                        sono = item.sono,
                        loadingsheetno = maxLoadingSheetNo,
                        unloadsequence = item.unloadsequence,
                        complete_flag = "0",
                        shipment_date = DateTime.Now.ToString("yyyy-MM-dd"),
                        shipment_month = DateTime.Now.ToString("MM"),
                        shipment_year = DateTime.Now.ToString("yy"),
                        shipment_time = DateTime.Now.ToString("hh:ss:tt"),
                    };
                    _context.LoadData_table.Add(entity);

                }

                _context.SaveChanges();

                LoadData_table applicant1 = new LoadData_table();
                //applicant.Picking_Packet.Add(new Picking_Packet() { pick_id = 1 });
                List<SelectListItem> wbridge1 = _context.LoadData_table.AsNoTracking().OrderBy(n => n.loadingsheetno).Select(n => new SelectListItem
                {

                    Value = n.loadingsheetno.ToString(),
                    Text = n.loadingsheetno.ToString(),

                }).Distinct().ToList();
                var defItem1 = new SelectListItem()
                {
                    Value = "",
                    Text = "-Select Shipment-"
                };

                wbridge1.Insert(0, defItem1);


                return Json(wbridge1);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult EditDataToDatabase([FromBody] List<LoadData_table> data)
        {
            try
            {

                // Assuming LoadData_table is your DbSet in the DbContext
                //var maxLoadingSheetNo = _context.LoadData_table.Max(a => (int?)a.loadingsheetno) ?? 0;
                //maxLoadingSheetNo++;


                // Save data to the database using Entity Framework
                foreach (var item in data)
                {
                    var getload = _context.LoadData_table.Where(x => x.sono == item.sono && x.loadingsheetno == item.loadingsheetno).FirstOrDefault();
                    if (getload != null)
                    {
                        getload.unloadsequence = item.unloadsequence;
                        _context.Update(getload);
                        _context.SaveChanges();
                    }
                    //var entity = new LoadData_table
                    //{
                    //    sono = item.sono,
                    //    loadingsheetno = item.loadingsheetno,
                    //    unloadsequence = item.unloadsequence,
                    //    shipment_date = DateTime.Now.ToString("dd/MM/yyyy"),
                    //    shipment_month = DateTime.Now.ToString("MM"),
                    //    shipment_year = DateTime.Now.ToString("yy"),
                    //    shipment_time = DateTime.Now.ToString("hh:ss:tt"),
                    //};

                    //_context.LoadData_table.Add(entity);
                }

                //_context.SaveChanges();

                LoadData_table applicant1 = new LoadData_table();
                //applicant.Picking_Packet.Add(new Picking_Packet() { pick_id = 1 });
                List<SelectListItem> wbridge1 = _context.LoadData_table.AsNoTracking().OrderBy(n => n.loadingsheetno).Select(n => new SelectListItem
                {

                    Value = n.loadingsheetno.ToString(),
                    Text = n.loadingsheetno.ToString(),

                }).Distinct().ToList();
                var defItem1 = new SelectListItem()
                {
                    Value = "",
                    Text = "----Select Shipment----"
                };

                wbridge1.Insert(0, defItem1);


                return Json(wbridge1);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        //public ActionResult GetData(string selectedValue, string selectedValue1, string selectedValue2, string selectedValue3, string sono, int remainingScanCounter, int scannedShipperCounter, int totalShipperCounter)
        //{
        //    var remaining = 0;
        //    var scanned = 0;
        //    var totalcount = 0;
        //    var sonumbersFromLoad = _context.LoadData_table
        //                    .Where(a => a.loadingsheetno == Convert.ToInt32(selectedValue3))
        //                    .Select(a => a.sono)
        //                    .ToList();

        //    var pickingData = _context.Picking_Operation
        //                     .Where(a => sonumbersFromLoad.Contains(a.sono))
        //                     .ToList();

        //     remaining = _context.Picking_Operation.Where(a => a.sono == sonumbersFromLoad.Contains(a.sono) && a.flag == 0).ToList();
        //     scanned = _context.Picking_Operation.Where(a => a.sono == sonumbersFromLoad.Contains(a.sono) && a.flag == 1).ToList();
        //     totalcount = Convert.ToInt32(remaining) + Convert.ToInt32(scanned);

        //    var pick = _context.Picking_Operation
        //        .Where(x => x.boxno.Trim() == selectedValue.Trim() 
        //                 && x.productcode.Trim() == selectedValue1.Trim() 
        //                 && x.batchcode.Trim() == selectedValue2.Trim() 
        //                 && x.flag == 0)
        //        .FirstOrDefault();

        //    if (pick != null)
        //    {
        //        var result = new
        //        {
        //            boxno = pick.boxno,
        //            productcode = pick.productcode,
        //            batchcode = pick.batchcode,
        //            remaining = remaining,
        //            scanned = scanned,
        //            totalcount = totalcount,
        //        };

        //        var lable = _context.Picking_Operation
        //                    .Where(x => x.boxno.Trim() == selectedValue.Trim() 
        //                        && x.productcode.Trim() == selectedValue1.Trim() 
        //                        && x.batchcode.Trim() == selectedValue2.Trim())
        //                    .OrderByDescending(x => x.batchcode)
        //                    .FirstOrDefault();

        //        if (lable != null)
        //        {
        //            lable.flag = 1;
        //        }

        //        _context.Picking_Operation.Update(lable);
        //        _context.SaveChanges();

        //        Load_Dispatchtable Load_Dispatchtable = new Load_Dispatchtable()
        //        {
        //            sono = pick.sono,
        //            productcode = pick.productcode,
        //            productname = "-",
        //            location = pick.location,
        //            batchcode = pick.batchcode,
        //            statusflag = "0",
        //            boxno = pick.boxno,
        //            //date = DateTime.Now,
        //            //time = DateTime.Now.ToString("hh:ss:tt")
        //        };
        //        _context.Load_Dispatchtable.Add(Load_Dispatchtable);
        //        _context.SaveChanges();

        //        var count = _context.Picking_Operation.Where(picking => picking.sono == pick.sono && picking.flag == 0 && !_context.Loading_Dispatch_Operation.Any(ldo => ldo.boxno == picking.boxno && ldo.productcode == picking.productcode && ldo.batchcode == picking.batchcode)).Count();

        //        if (count == 0)
        //        {
        //            //checkseq.complete_flag = "1";
        //            //_context.LoadData_table.Update(checkseq);
        //            _context.SaveChanges();
        //        }

        //        return Json(result);

        //        return Json(new { error = "Sale order is Loaded completed." });

        //    }
        //    return Json(new { error = "Invalid sale order" });
        //}

        public ActionResult GetData(string selectedValue, string selectedValue1, string selectedValue2, string selectedValue3, string sono, int remainingScanCounter, int scannedShipperCounter, int totalShipperCounter)
        {
            var sonumbersFromLoad = _context.LoadData_table
                                             .Where(a => a.loadingsheetno == Convert.ToInt32(selectedValue3))
                                             .Select(a => a.sono)
                                             .ToList();

            var pickingData = _context.Picking_Operation
                                   .Where(a => sonumbersFromLoad.Contains(a.sono))
                                   .ToList();

            var remaining = _context.Picking_Operation
                                 .Count(a => sonumbersFromLoad.Contains(a.sono) && a.flag == 0);

            var scanned = _context.Picking_Operation
                               .Count(a => sonumbersFromLoad.Contains(a.sono) && a.flag == 1);


            //var sonumbersFromLoad = _context.LoadData_table
            //               .Where(a => a.loadingsheetno == Convert.ToInt32(selectedValue3))
            //               .OrderByDescending(a => a.unloadsequence)
            //               .Select(a => a.sono)  // Order by unloadignsequnce in descending order
            //               .FirstOrDefault();

            var totalcount = remaining + scanned;

            var pick = _context.Picking_Operation
                             .Where(x => x.boxno.Trim() == selectedValue.Trim()
                                       && x.productcode.Trim().ToUpper() == selectedValue1.Trim().ToUpper()
                                       && x.batchcode.Trim() == selectedValue2.Trim()
                                       && x.flag == 0
                                       && x.sono.Trim() == sono.Trim())
                             .FirstOrDefault();

            if (pick != null)
            {
                var result = new
                {
                    boxno = pick.boxno,
                    productcode = pick.productcode,
                    batchcode = pick.batchcode,
                    remaining = remaining-1,
                    scanned = scanned+1,
                    totalcount = totalcount,
                };

                var label = _context.Picking_Operation
                              .Where(x => x.boxno.Trim() == selectedValue.Trim()
                                          && x.productcode.Trim().ToUpper() == selectedValue1.Trim().ToUpper()
                                          && x.batchcode.Trim() == selectedValue2.Trim()
                                          && x.sono.Trim() == sono.Trim())
                              .OrderByDescending(x => x.batchcode)
                              .FirstOrDefault();

                if (label != null)
                {
                    label.flag = 1;
                    _context.Picking_Operation.Update(label);
                    _context.SaveChanges();

                    var load_Dispatchtable = new Load_Dispatchtable()
                    {
                        sono = pick.sono,
                        productcode = pick.productcode,
                        productname = "-",
                        location = pick.location,
                        batchcode = pick.batchcode,
                        statusflag = "0",
                        boxno = pick.boxno,
                        //date = DateTime.Now,
                        //time = DateTime.Now.ToString("hh:ss:tt")
                    };

                    _context.Load_Dispatchtable.Add(load_Dispatchtable);
                    _context.SaveChanges();

                    var count = _context.Picking_Operation
                                     .Count(picking => picking.sono == pick.sono
                                                    && picking.flag == 0
                                                    && !_context.Loading_Dispatch_Operation
                                                                .Any(ldo => ldo.boxno == picking.boxno
                                                                        && ldo.productcode.ToUpper() == picking.productcode.ToUpper()
                                                                        && ldo.batchcode == picking.batchcode));

                    if (count == 0)
                    {
                        //checkseq.complete_flag = "1";
                        //_context.LoadData_table.Update(checkseq);
                        _context.SaveChanges();
                    }

                    return Json(result);
                }

                return Json(new { error = "Sale order is Loaded completed." });
            }

            return Json(new { error = "Invalid sale order" });
        }


        [HttpGet]
        public ActionResult Getseq(string optionValue)
        {
            List<LoadData_table> picklistDataList1 = new List<LoadData_table>();

            try
            {
                // Use a single query to retrieve the necessary data
                var sale = _context.LoadData_table
           .Where(mat => mat.loadingsheetno == Convert.ToInt32(optionValue))
           .OrderByDescending(mat => mat.unloadsequence) // Order by unloadsequence
           .Select(mat => new { mat.loadingsheetno, mat.sono, mat.unloadsequence }) // Select only the necessary columns
           .Distinct()
           .ToList();

                //foreach (var sat in sale)
                //{

                //    var loadorder = _context.LoadData_table.Where(x => x.loadingsheetno == sat.loadingsheetno);

                //    if (loadorder.Any())
                //    {
                foreach (var pickdata in sale)
                {
                    var datatable = new LoadData_table
                    {
                        sono = pickdata.sono,
                        unloadsequence = pickdata.unloadsequence,
                        //productname = pickdata.productname,

                    };

                    picklistDataList1.Add(datatable);
                }
                //}
                //}
            }
            catch (Exception ex)
            {
                // Log or handle the exception appropriately
                string msg = ex.Message;
            }

            return Json(picklistDataList1);
        }
        [HttpPost]
        public IActionResult SaveData([FromBody] JsonElement postData)
        {
            try
            {
                var formData = JsonConvert.DeserializeObject<Loading_Dispatch_Operation>(postData.GetProperty("Loading_Dispatch_Operation").ToString());

                formData.dcno = formData.dcno.ToUpper();
                formData.invoiceno = formData.invoiceno.ToUpper();

                var tableData = JsonConvert.DeserializeObject<List<Loading_Dispatch_Operation>>(postData.GetProperty("TableData").ToString());

                var existingItems = _context.Loading_Dispatch_Operation.Where(ldo => ldo.sono == formData.sono).ToList();
                _context.Loading_Dispatch_Operation.RemoveRange(existingItems);

                var pickingcount = _context.Picking_Operation.Where(a => a.sono == formData.sono).ToList();
                var loadingcount = _context.Load_Dispatchtable.Where(a => a.sono == formData.sono).ToList();
                
                if (pickingcount.Count == loadingcount.Count)
                {
                    if (tableData.Count > 0)
                    {

                        foreach (var item in tableData)
                        {
                            item.dispatchtype = formData.dispatchtype;
                            item.currentdate = DateTime.Now.ToString("yyyy-MM-dd");
                            item.sono = formData.sono;
                            item.sonosequence = "-";
                            item.location = "-";
                            item.productname = "-";
                            item.unloadingsequence = "-";
                            item.dc_flag = "S";
                            item.partial_flag = "1";
                            item.dcno = formData.dcno.ToUpper();
                            item.invoiceno = formData.invoiceno.ToUpper();
                            item.dcdate = formData.dcdate.ToUpper();
                            item.invoicedate = formData.invoicedate.ToUpper();

                            if (item.dispatchtype == "Courier")
                            {
                                item.dispatchdate = DateTime.Now.ToString("yyyy-MM-dd");
                                item.docketno = formData.docketno;
                                item.couriername = formData.couriername;
                                item.truckno = "-";
                                item.contactno = "-";
                                item.drivername = "-";
                                item.transportname = "-";
                                item.dcno = formData.dcno.ToUpper();
                                item.invoiceno = formData.dcno.ToUpper();
                                item.dcdate = formData.dcdate.ToUpper();
                                item.invoicedate = formData.invoicedate.ToUpper();
                            }
                            else if (item.dispatchtype == "Transport")
                            {
                                item.dispatchdate = DateTime.Now.ToString("yyyy-MM-dd");
                                item.truckno = formData.truckno;
                                item.contactno = formData.contactno;
                                item.drivername = formData.drivername;
                                item.transportname = formData.transportname;
                                item.docketno = "-";
                                item.couriername = "-";
                                item.dcno = formData.dcno.ToUpper();
                                item.invoiceno = formData.dcno.ToUpper();
                                item.dcdate = formData.dcdate.ToUpper();
                                item.invoicedate = formData.invoicedate.ToUpper();
                            }

                            var loaddispatch = _context.Load_Dispatchtable
                                .Where(a => a.sono.Trim() == formData.sono.Trim() 
                                && a.productcode.Trim().ToUpper() == item.productcode.Trim().ToUpper() 
                                && a.boxno.Trim() == item.boxno.Trim() 
                                && a.batchcode.Trim() == item.batchcode.Trim()
                                && a.statusflag == "0")
                                .FirstOrDefault();

                            var StorageData = _context.Storage_Operation
                                .Where(a => a.productcode.Trim().ToUpper() == item.productcode.Trim().ToUpper() 
                                && a.boxno.Trim() == item.boxno.Trim() 
                                && a.batchcode.Trim() == item.batchcode.Trim()
                                && a.statusflag.Trim() == "PI")
                                .FirstOrDefault();

                            if (loaddispatch != null && StorageData != null)
                            {
                                StorageData.statusflag = "LD";
                                loaddispatch.statusflag = "1";
                                item.location = loaddispatch.location;
                                _context.Loading_Dispatch_Operation.Add(item);
                                //maintain logs
                                var user = HttpContext.Session.GetString("User");
                                var logs = new Logs();
                                logs.pagename = "Loading Dispatch";
                                logs.taskid = item.id;
                                logs.task = item.id.ToString();
                                logs.action = "Create";
                                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                                logs.time = DateTime.Now.ToString("HH:mm:ss");
                                logs.username = user;
                                _context.Add(logs);

                                _context.Storage_Operation.Update(StorageData);
                                _context.Load_Dispatchtable.Update(loaddispatch);
                                _context.SaveChanges();
                            }
                            else
                            {
                                return Json(new { success = false, message = item.boxno + "of " + item.productcode + "product not found in storage & loading !" });
                            }
                        }

                    }
                    else
                    {
                        return Json(new { success = false, message = "Plaese scan all the boxes against sale order number " + formData.sono + " !" });
                    }

                }
                else
                {
                    return Json(new { success = false, message = "Please scan all the boxes against sale order number " + formData.sono + " !" });
                }   

                var picklist = _context.Picklist_Generation
                                        .Where(a => a.sono.Trim() == formData.sono.Trim())
                                        .Include(a => a.pickstorages)
                                        .ToList();

                var soinward = _context.so_inward
                                   .Where(a => a.sono.Trim() == formData.sono.Trim() && a.status == "Pending")
                                   .Include(a => a.soProduct_details)
                                   .FirstOrDefault();
                
                if (picklist.Count > 0)
                {
                    var product = _context.Picklist_Generation.Where(a => a.sono == formData.sono.Trim()).ToList();
                    var flag = 0;
                    var item = product.Where(a => Convert.ToInt32(a.soqty) == Convert.ToInt32(a.pickingqty)).ToList();
                    if (product.Count == item.Count)
                    {
                        flag = 1;
                    }
                    else
                    {
                        flag = 0;
                    }

                    if (flag != 0)
                    {
                        var picking = _context.Picking_Operation
                            .Where(a => a.sono.Trim() == formData.sono 
                            && a.flag == 1)
                            .ToList();

                        var loadingg = _context.Loading_Dispatch_Operation
                            .Where(a => a.sono == formData.sono)
                            .ToList();

                        if (picking.Count == loadingg.Count)
                        {
                            soinward.status = "Completed";
                            _context.so_inward.Update(soinward);
                            _context.SaveChanges();
                        }
                    }
                    else
                    {
                        _context.SaveChanges();
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Data not found in picklist Generation !" });
                }
                _context.SaveChanges();
                return Json(new { success = true, message = "Dispatch Process complete Successfully !" });
            }
            catch (Exception ex)
            {
                // Log and return error
                Console.WriteLine("An error occurred while saving data: " + ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult MultipleSave([FromBody] JsonElement postData)
        {
            try
            {
                var swlectsono = "-";
                var formData = JsonConvert.DeserializeObject<Loading_Dispatch_Operation>(postData.GetProperty("Loading_Dispatch_Operation").ToString());
                var tableData = JsonConvert.DeserializeObject<List<Loading_Dispatch_Operation>>(postData.GetProperty("TableData").ToString());
                var sonolist = formData.sono.Split(',');

                var loadingsheetno = _context.LoadData_table
                                             .Where(item => sonolist.Contains(item.sono))
                                             .Select(item => item.loadingsheetno)
                                             .Distinct()
                                             .ToList();

                if (loadingsheetno.Count == 1)
                {
                    foreach (var sitem in sonolist)
                    {
                        var pickingcount = _context.Picking_Operation.Where(a => a.sono == sitem).ToList();
                        var loadingcount = _context.Load_Dispatchtable.Where(a => a.sono == sitem).ToList();

                        if (pickingcount.Count == loadingcount.Count)
                        {
                            foreach (var tableItem in tableData)
                            {
                                tableItem.dispatchtype = formData.dispatchtype;
                                tableItem.currentdate = DateTime.Now.ToString("yyyy-MM-dd");
                                tableItem.productname = "-";
                                tableItem.location = "-";
                                tableItem.sonosequence = "-";
                                tableItem.unloadingsequence = "-";
                                tableItem.dc_flag = "M";
                                tableItem.partial_flag = "2";

                                if (tableItem.dispatchtype == "Courier")
                                {
                                    tableItem.dispatchdate = DateTime.Now.ToString("yyyy-MM-dd");
                                    tableItem.docketno = formData.docketno;
                                    tableItem.couriername = formData.couriername;
                                    tableItem.truckno = "-";
                                    tableItem.contactno = "-";
                                    tableItem.drivername = "-";
                                    tableItem.transportname = "-";
                                }
                                else if (tableItem.dispatchtype == "Transport")
                                {
                                    tableItem.dispatchdate = DateTime.Now.ToString("yyyy-MM-dd");
                                    tableItem.truckno = formData.truckno;
                                    tableItem.contactno = formData.contactno;
                                    tableItem.drivername = formData.drivername;
                                    tableItem.transportname = formData.transportname;
                                    tableItem.docketno = "-";
                                    tableItem.couriername = "-";
                                }

                                var loaddispatch = _context.Load_Dispatchtable
                                                       .Where(a => a.productcode.Trim().ToUpper() == tableItem.productcode.Trim().ToUpper()
                                                                && a.boxno.Trim() == tableItem.boxno.Trim()
                                                                && a.batchcode.Trim() == tableItem.batchcode.Trim()
                                                                && a.statusflag == "0")
                                                       .FirstOrDefault();


                                if (loaddispatch != null)
                                {
                                    swlectsono = loaddispatch.sono.Trim();

                                    var productExists = _context.Picking_Operation
                                                            .Where(p => p.sono.Trim() == swlectsono
                                                                     && p.boxno.Trim() == tableItem.boxno.Trim()
                                                                     && p.productcode.Trim().ToUpper() == tableItem.productcode.Trim().ToUpper()
                                                                     && p.batchcode.Trim() == tableItem.batchcode.Trim()
                                                                     && p.flag == 1)
                                                            .FirstOrDefault();

                                    var storageData = _context.Storage_Operation
                                                          .Where(a => a.productcode.Trim().ToUpper() == tableItem.productcode.Trim().ToUpper()
                                                                   && a.boxno.Trim() == tableItem.boxno.Trim()
                                                                   && a.batchcode.Trim() == tableItem.batchcode.Trim()
                                                                   && a.statusflag == "PI")
                                                          .FirstOrDefault();

                                    var chk = _context.Loading_Dispatch_Operation
                                                  .Where(x => x.batchcode == tableItem.batchcode
                                                           && x.boxno == tableItem.boxno
                                                           && x.productcode.ToUpper() == tableItem.productcode.ToUpper())
                                                  .FirstOrDefault();

                                    if (productExists != null && storageData != null && chk == null)
                                    {
                                        tableItem.sono = swlectsono;
                                        storageData.statusflag = "LD";
                                        loaddispatch.statusflag = "1";
                                        _context.Loading_Dispatch_Operation.Add(tableItem);

                                        //maintain logs
                                        var user = HttpContext.Session.GetString("User");
                                        var logs = new Logs();
                                        logs.pagename = "Loading Dispatch";
                                        logs.task = tableItem.id.ToString();
                                        //logs.task = "Scan Products dispatched in multiple ";
                                        logs.taskid = tableItem.id;
                                        logs.action = "Create";
                                        logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                                        logs.time = DateTime.Now.ToString("HH:mm:ss");
                                        logs.username = user;
                                        _context.Add(logs);

                                        _context.Storage_Operation.Update(storageData);
                                        _context.Load_Dispatchtable.Update(loaddispatch);
                                        _context.SaveChanges();
                                    }
                                    else
                                    {
                                        return Json(new { success = false, message = $"{tableItem.boxno} of {tableItem.productcode} product not found in storage & loading!" });
                                    }
                                }
                                
                            }
                        }
                        else
                        {
                            return Json(new { success = false, message = $"Please scan all the boxes against sale order number {sitem}!" });
                        }
                    }

                    foreach(var sitem in sonolist)
                    {

                        var picklist = _context.Picklist_Generation
                                              .Where(a => a.sono.Trim() == sitem.Trim())
                                              .Include(a => a.pickstorages)
                                              .ToList();

                        var soinward = _context.so_inward
                                             .Where(a => a.sono.Trim() == sitem.Trim() && a.status == "Pending")
                                             .Include(a => a.soProduct_details)
                                             .FirstOrDefault();
                        
                        var check = _context.Loading_Dispatch_Operation.Where(a => a.sono == sitem).ToList();
                        
                        var check1 = _context.Load_Dispatchtable.Where(a => a.sono == sitem && a.statusflag == "1").ToList();
                        
                        var shipment = _context.LoadData_table
                                          .Where(e => e.sono.Trim() == sitem.Trim() && e.complete_flag == "0")
                                          .FirstOrDefault();

                        if (picklist.Count > 0 && shipment != null)
                        {
                            //for loading dispatch status
                           if(check.Count == check1.Count)
                            {
                                shipment.complete_flag = "1";
                                _context.LoadData_table.Update(shipment);
                                _context.SaveChanges();
                            }
                            
                            //for picklist genration
                            var product = _context.Picklist_Generation.Where(a => a.sono == sitem.Trim()).ToList();
                            var flag = 0;
                            var item = product.Where(a => Convert.ToInt32(a.soqty) == Convert.ToInt32(a.pickingqty)).ToList();
                            if (product.Count == item.Count)
                            {
                                flag = 1;
                            }
                            else
                            {
                                flag = 0;
                            }
                            if (flag != 0)
                            {
                                var picking = _context.Picking_Operation.Where(a => a.sono.Trim() == sitem && a.flag == 1).ToList();
                                var loadingg = _context.Loading_Dispatch_Operation.Where(a => a.sono == sitem).ToList();
                                if (picking.Count == loadingg.Count)
                                {
                                    soinward.status = "Completed";
                                    _context.so_inward.Update(soinward);
                                    _context.SaveChanges();
                                }
                            }
                            else
                            {
                                _context.SaveChanges();
                            }

                        }
                        
                    }
                    return Json(new { success = true, message = "Dispatch Process Completed Successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Loading sheetno found not same!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        //public IActionResult MultipleSave([FromBody] JsonElement postData)
        //{
        //    try
        //    {
        //        var formData = JsonConvert.DeserializeObject<Loading_Dispatch_Operation>(postData.GetProperty("Loading_Dispatch_Operation").ToString());

        //        var tableData = JsonConvert.DeserializeObject<List<Loading_Dispatch_Operation>>(postData.GetProperty("TableData").ToString());

        //        var sonolist = formData.sono.Split(',');

        //        var productList = new List<Picking_Operation>();

        //        var loadingsheetno = _context.LoadData_table
        //                             .Where(item => sonolist.Contains(item.sono))
        //                             .Select(item => item.loadingsheetno)
        //                             .Distinct()
        //                             .ToList();


        //        if (loadingsheetno.Count == 1)
        //        {
        //            foreach (var sitem in sonolist)
        //            {
        //                var pickingcount = _context.Picking_Operation.Where(a => a.sono == sitem).ToList();
        //                var loadingcount = _context.Load_Dispatchtable.Where(a => a.sono == sitem).ToList();

        //                if (pickingcount.Count == loadingcount.Count)
        //                {
        //                    foreach (var item in tableData)
        //                    {
        //                        item.dispatchtype = formData.dispatchtype;
        //                        item.currentdate =
        //                        .Now.ToString("dd/MM/yyyy");
        //                        item.productname = "-";
        //                        item.location = "-";
        //                        item.sonosequence = "-";
        //                        item.unloadingsequence = "-";
        //                        item.dc_flag = "M";
        //                        item.partial_flag = "2";

        //                        if (item.dispatchtype == "Courier")
        //                        {
        //                            item.dispatchdate = DateTime.Now.ToString("dd/MM/yyyy");
        //                            item.docketno = formData.docketno;
        //                            item.couriername = formData.couriername;
        //                            item.truckno = "-";
        //                            item.contactno = "-";
        //                            item.drivername = "-";
        //                            item.transportname = "-";
        //                        }
        //                        else if (item.dispatchtype == "Transport")
        //                        {
        //                            item.dispatchdate = DateTime.Now.ToString("dd/MM/yyyy");
        //                            item.truckno = formData.truckno;
        //                            item.contactno = formData.contactno;
        //                            item.drivername = formData.drivername;
        //                            item.transportname = formData.transportname;
        //                            item.docketno = "-";
        //                            item.couriername = "-";

        //                        }
        //                        //picking data check 

        //                        var loaddispatch = _context.Load_Dispatchtable
        //                                           .Where(a => a.productcode.Trim() == item.productcode.Trim()
        //                                           && a.boxno.Trim() == item.boxno.Trim()
        //                                           && a.batchcode.Trim() == item.batchcode.Trim() && a.statusflag == "0")
        //                                           .FirstOrDefault();

        //                        var swlectsono = loaddispatch.sono.Trim();

        //                        var productExists = _context.Picking_Operation
        //                                            .Where(p => p.sono.Trim() == swlectsono &&
        //                                            p.boxno.Trim() == item.boxno.Trim() &&
        //                                            p.productcode.Trim() == item.productcode.Trim() &&
        //                                            p.batchcode.Trim() == item.batchcode.Trim() && p.flag == 1)
        //                                            .FirstOrDefault();

        //                        var StorageData = _context.Storage_Operation
        //                                          .Where(a => a.productcode.Trim() == item.productcode.Trim()
        //                                           && a.boxno.Trim() == item.boxno.Trim()
        //                                           && a.batchcode.Trim() == item.batchcode.Trim() && a.statusflag == "PI")
        //                                           .FirstOrDefault();


        //                        var chk = _context.Loading_Dispatch_Operation
        //                                  .Where(x => x.batchcode == item.batchcode
        //                                   && x.boxno == item.boxno
        //                                   && x.productcode == item.productcode)
        //                                  .FirstOrDefault();

        //                        if (productExists != null)
        //                        {
        //                            if (loaddispatch != null && StorageData != null && chk == null)
        //                            {
        //                                item.sono = swlectsono;
        //                                StorageData.statusflag = "LD";
        //                                loaddispatch.statusflag = "1";
        //                                _context.Loading_Dispatch_Operation.Add(item);
        //                                _context.Storage_Operation.Update(StorageData);
        //                                _context.Load_Dispatchtable.Update(loaddispatch);
        //                                _context.SaveChanges();
        //                            }
        //                            else
        //                            {
        //                                return Json(new { success = false, message = item.boxno + "of " + item.productcode + "product not found in storage & loading !" });
        //                            }
        //                        }
        //                        else
        //                        {
        //                            return Json(new { success = false, message = item.boxno + "of " + item.productcode + "product not Picked in picking operation !" });
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    return Json(new { success = false, message = "Plaese scann all the boxes against sale order number " + sitem + " !" });
        //                }

        //                var picklist = _context.Picklist_Generation
        //                                    .Where(a => a.sono.Trim() == sitem.Trim())
        //                                    .Include(a => a.pickstorages)
        //                                    .ToList();
        //                var soinward = _context.so_inward
        //                                   .Where(a => a.sono.Trim() == sitem.Trim() && a.status == "Pending")
        //                                   .Include(a => a.soProduct_details)
        //                                   .FirstOrDefault();

        //                if (picklist.Count > 0)
        //                {
        //                    var product = _context.Picklist_Generation.Where(a => a.sono == sitem.Trim()).ToList();
        //                    var flag = 0;
        //                    var item = product.Where(a => a.soqty == a.pickingqty).ToList();
        //                    if (product.Count == item.Count)
        //                    {
        //                        flag = 1;
        //                    }
        //                    else
        //                    {
        //                        flag = 0;
        //                    }
        //                    if (flag != 0)
        //                    {
        //                        var picking = _context.Picking_Operation.Where(a => a.sono.Trim() == sitem.Trim() && a.flag == 1).ToList();
        //                        var loadingg = _context.Loading_Dispatch_Operation.Where(a => a.sono.Trim() == sitem.Trim()).ToList();
        //                        if (picking.Count == loadingg.Count)
        //                        {
        //                            soinward.status = "Completed";
        //                            _context.so_inward.Update(soinward);
        //                            _context.SaveChanges();
        //                        }
        //                    }
        //                    else
        //                    {
        //                        _context.SaveChanges();
        //                    }
        //                }
        //                else
        //                {
        //                    return Json(new { success = false, message = "Data not found in picklist geertaion !" });
        //                }

        //                var shipment = _context.LoadData_table
        //                  .Where(item => sonolist.Contains(item.sono))
        //                  .ToList();

        //                if (shipment.All(item => item.complete_flag == "0"))
        //                {
        //                    foreach (var item in shipment)
        //                    {
        //                        item.complete_flag = "1";
        //                        _context.LoadData_table.Update(shipment);
        //                    }
        //                    _context.SaveChanges();
        //                }

        //            }
        //        }
        //        else
        //        {
        //            return Json(new { success = false, message = "Loading sheetno found not same !" });
        //        }
        //        _context.SaveChanges();
        //        return Json(new { success = true, message = "Dispatch Process Completed Successfully !" });
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, error = ex.Message });
        //    }

        //}
    }

}












