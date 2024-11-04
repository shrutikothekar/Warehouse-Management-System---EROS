//pickgenration new
using AspNetCoreHero.ToastNotification.Abstractions;
using AspNetCoreHero.ToastNotification.Helpers;
using AuthSystem.Data;
using DocumentFormat.OpenXml.Drawing.Diagrams;

//using DocumentFormat.OpenXml.Drawing.Diagrams;
using eros.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Nest;
using System.Globalization;
using System.Linq;

namespace eros.Controllers
{
    public class Picklist_GenerationController : Controller
    {
        private readonly ErosDbContext _context;
        public INotyfService _notyfService { get; }
       
        public Picklist_GenerationController(ErosDbContext context,INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;

        }

        //_notyfService.Success("inward Order Updated Succesfully");

        // GET: Picklist_Generation
        //public async Task<IActionResult> Index()
        //{
        //    return _context.Picklist_Generation != null ?
        //                View(await _context.Picklist_Generation.ToListAsync()) :
        //                Problem("Entity set 'ErosDbContext.Picklist_Generation'  is null.");
        //}

        public async Task<IActionResult> Index()
        {
            var listshow = await _context.Picklist_Generation.Where(a=>a.flagstatus == 0).OrderByDescending(a=>a.gen_id).ToListAsync();

            foreach (var item in listshow)
            {
                var allFlagsSetToOne = _context.pickstorage
                    .Where(a => a.genid == item.gen_id)
                    .All(a => a.flag == 1);

                item.status = allFlagsSetToOne ? "Completed" : "Incomplete";
            }

            return View(listshow);
        }

        // GET: Picklist_Generation/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Picklist_Generation == null)
            {
                return NotFound();
            }
            var Picklist_Generation = await _context.Picklist_Generation
                .FirstOrDefaultAsync(m => m.gen_id == id);
            if (Picklist_Generation == null)
            {
                return NotFound();
            }

            return View(Picklist_Generation);
        }

        //public IActionResult Create()
        //{
        //    //var distinctSono = _context.Picklist_Generation
        //    //                .GroupBy(a => a.sono)
        //    //                .Select(g => new
        //    //                {
        //    //                    SONO = g.Key,
        //    //                    TotalSOQty = g.Sum(x => Convert.ToInt32(x.soqty)),
        //    //                    TotalPickingQty = g.Sum(x => Convert.ToInt32(x.pickingqty))
        //    //                })
        //    //                .ToList();

        //    var distinctSoNos = _context.Picklist_Generation
        //        .Where(a => a.flagstatus == 1)
        //        .GroupBy(a => a.sono)
        //        .Where(g => g.Any(x => x.pickingqty == x.soqty)) // Filter out sono with different pickqty and soqty
        //        .Select(g => g.Key)
        //        .ToList();

        //    List<SelectListItem> wbridge = _context.so_inward.AsNoTracking()
        //        .Select(n => new SelectListItem
        //        {
        //            Value = n.sono,
        //            Text = n.sono.ToString()
        //        })
        //        .Where(item => !distinctSoNos.Contains(item.Value))  // Exclude sonos present in distinctSoNos
        //        .OrderBy(item => item.Value)
        //            .Distinct() // Ensure distinct values
        //        .ToList();

        //    var defItem = new SelectListItem()
        //    {
        //        Value = "",
        //        Text = "----Select SONO----"
        //    };

        //    wbridge.Insert(0, defItem);
        //    ViewBag.data1 = wbridge;

        //    return View();
        //}

        public int GetDenominatorCount(string boxNumber)
        {
            // Assuming the box number format is "x/y"
            string[] parts = boxNumber.Split('/');

            if (parts.Length == 2 && int.TryParse(parts[1], out int denominator))
            {
                return denominator;
            }
            else
            {
                // If the box number format is invalid or the denominator cannot be parsed, return -1 or throw an exception
                // You may adjust this behavior based on your requirements
                return -1;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(NewPickcs NewPickcs)
        {
            try
            {
                bool allZero = true;
                foreach (var item in NewPickcs.Picklist_Generation1)
                {
                    if (item.pickingqty != null && Convert.ToInt32(item.pickingqty) != 0)
                    {
                        allZero = false;
                        break;
                    }
                }
                if (allZero)
                {
                    return Json(new { success = false, message = "All picking quantities are zero. Please enter the pick quantity before submitting." });
                }


                var sono = _context.so_inward.Where(a => a.sono == NewPickcs.Picklist_Generation.sono).FirstOrDefault();
                if (sono.status == "Cancel")
                {
                    sono.status = "Pending";
                    _context.so_inward.Update(sono);
                    _context.SaveChanges();
                }

                var existdata = _context.Picklist_Generation
                                .Where(a => a.sono == NewPickcs.Picklist_Generation.sono && a.flagstatus == 0)
                                .ToList();
                List<pickstorage> pickStorageList = new List<pickstorage>();
                Dictionary<string, int> groupCounts = new Dictionary<string, int>();
                if (existdata.Any())
                {
                    foreach (var extitem in existdata)
                    {
                        foreach (var item in NewPickcs.Picklist_Generation1.Where(a => a.prdcode.Trim() == extitem.prdcode.Trim()))
                        {
                            var allotstock = _context.pickstorage.Where(a => a.productcode.Trim().ToUpper() == item.prdcode.Trim().ToUpper() && a.flag == 0).ToList();

                            var allotstorage = _context.Storage_Operation
                                        .Where(a => a.productcode.Trim().ToUpper() == item.prdcode.Trim().ToUpper() && a.statusflag == "ST" && a.pickflag == "0")
                                        .OrderBy(a => a.batchcode.Trim()).ThenBy(a => a.locationcode.Trim())
                                        .ToList();
                            
                            var alreadyExit = _context.pickstorage
                                             .Where(a => a.sono.Trim() == NewPickcs.Picklist_Generation.sono &&
                                                         a.productcode.Trim().ToUpper() == item.prdcode.Trim().ToUpper() &&
                                                         a.flag == 0)
                                             .ToList();

                            if (alreadyExit.Count > 0)
                            {
                                string alertMessage = "Already some product quantity is picked, Please perform picking operation first of that products !";
                                return Json(new { success = false, message = alertMessage });
                            }

                            var remaining = Math.Abs(allotstock.Count() - allotstorage.Count());

                            item.sono = NewPickcs.Picklist_Generation.sono;
                            int pickedqnty = Convert.ToInt16(item.pickingqty);

                            if (item.prdcode != null)
                            {
                                if (allotstorage.Count > 0 && Convert.ToInt32(item.pickingqty) > 0)
                                {
                                    var groupedData = allotstorage//(1/2, 1/1 )
                                        .GroupBy(q => new { SecondDigit = GetSecondDigit(q.boxno) });

                                    int denominatorValue = GetDenominatorCount(allotstorage.First().boxno); // Assuming all box numbers have the same denominator
                                    int sumOfCounts = groupedData.Count();

                                    if (sumOfCounts < denominatorValue)
                                    {
                                        string alertMessage = "Box Quantity is missing !";
                                        return Json(new { success = false, message = alertMessage });
                                    }

                                    int minCount = int.MaxValue; // Start with a very large value


                                    foreach (var eachgroupdata in groupedData)//(1/2, 1/1 )
                                    {
                                        var output = allotstorage.Where(a => a.boxno.Split("-")[1].Trim() == eachgroupdata.Key.SecondDigit.Trim()).Take(pickedqnty).ToList();
                                        groupCounts[eachgroupdata.Key.SecondDigit] = output.Count;
                                        minCount = Math.Min(minCount, output.Count); // Update the minimum count

                                        foreach (var eachbox in output)
                                        {
                                            pickstorage pickstorage = new pickstorage
                                            {
                                                genid = extitem.gen_id,
                                                sono = item.sono.Trim(),
                                                productcode = eachbox.productcode.Trim().ToUpper(),
                                                location = eachbox.locationcode.Trim(),
                                                batchcode = eachbox.batchcode.Trim(),
                                                boxno = eachbox.boxno.Trim()
                                            };
                                            eachbox.pickflag = "1";
                                            pickStorageList.Add(pickstorage);
                                        }
                                    }
                                    int totalQuantity1 = groupCounts.Values.Min();

                                    if (totalQuantity1 != Convert.ToInt32(item.pickingqty))
                                    {
                                        return Json(new { success = false, message = $"The quantity of " + item.prdcode + " is not available. Minimum Quantity is " + minCount });
                                    }
                                    if (Convert.ToInt32(extitem.pickingqty) + pickedqnty <= Convert.ToInt32(item.soqty))
                                    {
                                        int sumPickingQty = Convert.ToInt32(extitem.pickingqty) + pickedqnty;

                                        extitem.pickingqty = sumPickingQty.ToString();
                                        extitem.pickstorages = pickStorageList;
                                        _context.Update(extitem);
                                        //maintain logs
                                        var user = HttpContext.Session.GetString("User");
                                        var logs = new Logs();
                                        logs.pagename = "Picklist Generation";
                                        logs.task = "Picklist Add & Update";
                                        logs.taskid = extitem.gen_id;
                                        logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                                        logs.time = DateTime.Now.ToString("HH:mm:ss");
                                        logs.username = user;
                                        logs.action = "Create";
                                        _context.Add(logs);
                                        _context.SaveChanges();

                                    }
                                    else
                                    {
                                        return Json(new { success = false, message = "The picking quantity " + item.pickingqty + " exceeds the sales order quantity " + item.soqty + ", and the already filled quantity is " + Convert.ToInt32(extitem.pickingqty) + "." });

                                    }
                                }
                                else
                                {
                                    if (Convert.ToInt32(item.pickingqty) > 0)
                                    {
                                        return Json(new { success = false, message = $"The quantity of " + item.prdcode + " is not available. in stoarge ! " });
                                    }
                                    else
                                    {
                                        //return Ok();
                                    }
                                }
                                    
                                
                            }
                        }
                    }
                }
                else
                {
                    foreach (var item in NewPickcs.Picklist_Generation1)
                    {
                        var allotstock = _context.pickstorage.Where(a => a.productcode.Trim() == item.prdcode.Trim().ToUpper() && a.flag == 0).ToList();

                        var allotstorage = _context.Storage_Operation
                                    .Where(a => a.productcode.Trim().ToUpper() == item.prdcode.Trim().ToUpper() && a.statusflag == "ST" && a.pickflag == "0")
                                    .OrderBy(a => a.batchcode.Trim()).ThenBy(a => a.locationcode.Trim())
                                    .ToList();
                        
                        var remaining = Math.Abs(allotstock.Count() - allotstorage.Count());

                        item.sono = NewPickcs.Picklist_Generation.sono;
                        if (item.prdcode != null)
                        {
                            int pickedqnty = Convert.ToInt16(item.pickingqty);

                            if(allotstorage.Count > 0 && Convert.ToInt32(item.pickingqty) > 0)
                            {
                                var groupedData = allotstorage//(1/2, 1/1 )
                               .GroupBy(q => new { SecondDigit = GetSecondDigit(q.boxno) });
                                int denominatorValue = GetDenominatorCount(allotstorage.First().boxno); // Assuming all box numbers have the same denominator
                                int sumOfCounts = groupedData.Count();

                                if (sumOfCounts < denominatorValue)
                                {
                                    string alertMessage = "Box Quantity is missing !";
                                    return Json(new { success = false, message = alertMessage });
                                }
                                int minCount = int.MaxValue; // Start with a very large value
                                

                                foreach (var eachgroupdata in groupedData)//(1/2, 1/1 )
                                {

                                    var output = allotstorage.Where(a => a.boxno.Split("-")[1].Trim() == eachgroupdata.Key.SecondDigit.Trim()).Take(pickedqnty).ToList();

                                    groupCounts[eachgroupdata.Key.SecondDigit] = output.Count;

                                    minCount = Math.Min(minCount, output.Count); // Update the minimum count

                                    foreach (var eachbox in output)
                                    {
                                        pickstorage pickstorage = new pickstorage
                                        {
                                            sono = item.sono.Trim(),
                                            productcode = eachbox.productcode.Trim().ToUpper(),
                                            location = eachbox.locationcode.Trim(),
                                            batchcode = eachbox.batchcode.Trim(),
                                            boxno = eachbox.boxno.Trim()
                                        };
                                        eachbox.pickflag = "1";
                                        pickStorageList.Add(pickstorage);
                                    }
                                }

                                //int totalQuantity1 = groupCounts.Values.Min();

                                //if (totalQuantity1 != Convert.ToInt32(item.pickingqty))
                                //{
                                //    return Json(new { success = false, message = $"The quantity of " + item.prdcode + " is not available. Minimum Quantity is " + minCount });
                                //}
                            }
                            else
                            {
                                if(Convert.ToInt32(item.pickingqty) > 0)
                                {
                                    return Json(new { success = false, message = $"The quantity of " + item.prdcode + " is not available. in stoarge ! " });
                                }
                                else
                                {
                                    //return Ok();
                                }
                            }

                            
                                Picklist_Generation picklist = new Picklist_Generation
                                {
                                    prdcode = item.prdcode,
                                    prdname = item.prdname.ToString().Trim(),
                                    soqty = item.soqty.ToString().Trim(),
                                    pickingqty = item.pickingqty.ToString().Trim(),
                                    sono = item.sono.Trim(),
                                    flagstatus = item.flagstatus,
                                    pickstorages = pickStorageList
                                };
                                _context.Picklist_Generation.Add(picklist);

                                var user = HttpContext.Session.GetString("User");
                                var logs = new Logs();
                                logs.pagename = "Picklist Genrate";
                                logs.task = "Picklist Genrate";
                                logs.action = "Create";
                                logs.taskid = picklist.gen_id;
                                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                                logs.time = DateTime.Now.ToString("HH:mm:ss");
                                logs.username = user;
                                _context.Add(logs);

                            
                            
                        }
                    }
                }
                _context.SaveChanges();
                return Json(new { success = true, message = "Picklist Generated Successfully !" });
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                return Json(new { success = false, message = ex.Message });

            }
        }

        //public async Task<IActionResult> Create(NewPickcs NewPickcs)
        //{
        //    try
        //    {
        //        var sono = _context.so_inward.Where(a => a.sono == NewPickcs.Picklist_Generation.sono).FirstOrDefault();
        //        if (sono.status == "Cancel")
        //        {
        //            sono.status = "Pending";
        //            _context.so_inward.Update(sono);
        //            _context.SaveChanges();
        //        }
        //        var existdata = _context.Picklist_Generation
        //                        .Where(a => a.sono == NewPickcs.Picklist_Generation.sono && a.flagstatus == 0)
        //                        .ToList();

        //        if (existdata.Any())
        //        {
        //            var exitpick = _context.pickstorage
        //                             .Where(a => a.sono == NewPickcs.Picklist_Generation.sono && a.flag == 0)
        //                             .ToList();

        //            if (exitpick.Any())
        //            {
        //                // Retrieve corresponding data from Storage_Operation and remove it
        //                foreach (var entry in exitpick)
        //                {
        //                    var allotstorage = _context.Storage_Operation
        //                                    .Where(a => a.productcode.Trim() == item.prdcode.Trim() && a.statusflag == "ST")
        //                                    .OrderBy(a => a.batchcode.Trim()).ThenBy(a => a.locationcode.Trim())
        //                                    .ToList();

        //                    var storedataToRemove = _context.Storage_Operation
        //                        .Where(s => s.productcode.Trim() == entry.productcode.Trim() && s.batchcode.Trim() == entry.batchcode.Trim() && s.boxno == entry.boxno.Trim() && s.locationcode == entry.location.Trim())
        //                        .ToList();

        //                    storedataToRemove.RemoveRange(entry);
        //                }
        //                foreach (var extitem in existdata)
        //                {
        //                }

        //            }
        //            else
        //            {
        //                foreach (var extitem in existdata)
        //                {
        //                    foreach (var item in NewPickcs.Picklist_Generation1.Where(a => a.prdcode.Trim() == extitem.prdcode.Trim()))
        //                    {
        //                        var allotstock = _context.pickstorage.Where(a => a.productcode.Trim() == item.prdcode.Trim() && a.flag == 0).ToList();

        //                        var allotstorage = _context.Storage_Operation
        //                                    .Where(a => a.productcode.Trim() == item.prdcode.Trim() && a.statusflag == "ST")
        //                                    .OrderBy(a => a.batchcode.Trim()).ThenBy(a => a.locationcode.Trim())
        //                                    .ToList();

        //                        foreach (var rr in allotstock)
        //                        {
        //                            var newitem = allotstorage.Where(r => r.productcode.Trim() == rr.productcode.Trim() && r.batchcode.Trim() == rr.batchcode.Trim() && r.boxno.Trim() == rr.boxno.Trim() && r.locationcode.Trim() == rr.location.Trim()).FirstOrDefault();
        //                            if (newitem != null)
        //                            {
        //                                allotstorage.Remove(newitem);
        //                            }
        //                        }
        //                        var remaining = Math.Abs(allotstock.Count() - allotstorage.Count());


        //                        item.sono = NewPickcs.Picklist_Generation.sono;
        //                        int pickedqnty = Convert.ToInt16(item.pickingqty);

        //                        if (item.prdcode != null)
        //                        {
        //                            if (allotstorage.Count >= pickedqnty && allotstorage.Count > 0)
        //                            {

        //                                var groupedData = allotstorage//(1/2, 1/1 )
        //                                    .GroupBy(q => new { SecondDigit = GetSecondDigit(q.boxno) });

        //                                List<pickstorage> pickStorageList = new List<pickstorage>();

        //                                foreach (var eachgroupdata in groupedData)//(1/2, 1/1 )
        //                                {
        //                                    foreach (var eachbox in allotstorage//times takes - pickedqnty
        //                                        .Where(a => a.boxno.Split("-")[1].Trim() == eachgroupdata.Key.SecondDigit.Trim()).Take(pickedqnty))
        //                                    {
        //                                        pickstorage pickstorage = new pickstorage
        //                                        {
        //                                            genid = extitem.gen_id,
        //                                            sono = item.sono.Trim(),
        //                                            productcode = eachbox.productcode.Trim(),
        //                                            location = eachbox.locationcode.Trim(),
        //                                            batchcode = eachbox.batchcode.Trim(),
        //                                            boxno = eachbox.boxno.Trim()
        //                                        };

        //                                        pickStorageList.Add(pickstorage);
        //                                    }
        //                                }
        //                                if (Convert.ToInt32(extitem.pickingqty) + pickedqnty <= Convert.ToInt32(item.soqty))
        //                                {
        //                                    int sumPickingQty = Convert.ToInt32(extitem.pickingqty) + pickedqnty;

        //                                    extitem.pickingqty = sumPickingQty.ToString();
        //                                    extitem.pickstorages = pickStorageList;
        //                                    _context.Update(extitem);
        //                                    //maintain logs
        //                                    var user = HttpContext.Session.GetString("User");
        //                                    var logs = new Logs();
        //                                    logs.pagename = "Picklist Generation";
        //                                    logs.task = "Picklist Add & Update";
        //                                    logs.taskid = extitem.gen_id;
        //                                    logs.date = DateTime.Now.ToString("dd/MM/yyyy");
        //                                    logs.time = DateTime.Now.ToString("HH:mm:ss");
        //                                    logs.username = user;
        //                                    logs.action = "Create";
        //                                    _context.Add(logs);
        //                                    _context.SaveChanges();

        //                                }
        //                                else
        //                                {
        //                                    throw new Exception("The sum of picking quantities exceeds the sales order quantity.");
        //                                }
        //                            }
        //                            else
        //                            {
        //                                var availableQuantity = allotstorage.Count();

        //                                var message = availableQuantity + " units of stock are available, but some have already been allocated. Please create a Purchase Order.";

        //                                return Json(new { success = false, message = message });
        //                            }


        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            foreach (var item in NewPickcs.Picklist_Generation1)
        //            {
        //                var allotstock = _context.pickstorage.Where(a => a.productcode.Trim() == item.prdcode.Trim() && a.flag == 0).ToList();

        //                var allotstorage = _context.Storage_Operation
        //                            .Where(a => a.productcode.Trim() == item.prdcode.Trim() && a.statusflag == "ST")
        //                            .OrderBy(a => a.batchcode.Trim()).ThenBy(a => a.locationcode.Trim())
        //                            .ToList();

        //                foreach (var rr in allotstock)
        //                {
        //                    var newitem = allotstorage.Where(r => r.productcode.Trim() == rr.productcode.Trim() && r.batchcode.Trim() == rr.batchcode.Trim() && r.boxno.Trim() == rr.boxno.Trim() && r.locationcode.Trim() == rr.location.Trim()).FirstOrDefault();
        //                    if (newitem != null)
        //                    {
        //                        allotstorage.Remove(newitem);
        //                    }
        //                }
        //                var remaining = Math.Abs(allotstock.Count() - allotstorage.Count());

        //                item.sono = NewPickcs.Picklist_Generation.sono;
        //                if (item.prdcode != null)
        //                {
        //                    int pickedqnty = Convert.ToInt16(item.pickingqty);

        //                    if (allotstorage.Count >= pickedqnty && allotstorage.Count > 0)
        //                    {
        //                        var groupedData = allotstorage//(1/2, 1/1 )
        //                        .GroupBy(q => new { SecondDigit = GetSecondDigit(q.boxno) });

        //                        List<pickstorage> pickStorageList = new List<pickstorage>();

        //                        foreach (var eachgroupdata in groupedData)//(1/2, 1/1 )
        //                        {
        //                            foreach (var eachbox in allotstorage//times takes - pickedqnty
        //                                .Where(a => a.boxno.Split("-")[1].Trim() == eachgroupdata.Key.SecondDigit.Trim()).Take(pickedqnty))
        //                            {
        //                                pickstorage pickstorage = new pickstorage
        //                                {
        //                                    sono = item.sono.Trim(),
        //                                    productcode = eachbox.productcode.Trim(),
        //                                    location = eachbox.locationcode.Trim(),
        //                                    batchcode = eachbox.batchcode.Trim(),
        //                                    boxno = eachbox.boxno.Trim()
        //                                };

        //                                pickStorageList.Add(pickstorage);
        //                            }
        //                        }
        //                        Picklist_Generation picklist = new Picklist_Generation
        //                        {
        //                            prdcode = item.prdcode,
        //                            prdname = item.prdname.ToString().Trim(),
        //                            soqty = item.soqty.ToString().Trim(),
        //                            pickingqty = item.pickingqty.ToString().Trim(),
        //                            sono = item.sono.Trim(),
        //                            flagstatus = item.flagstatus,
        //                            pickstorages = pickStorageList
        //                        };
        //                        _context.Picklist_Generation.Add(picklist);

        //                        //maintain logs
        //                        var user = HttpContext.Session.GetString("User");
        //                        var logs = new Logs();
        //                        logs.pagename = "Picklist Genrate";
        //                        logs.task = "Picklist Genrate";
        //                        logs.action = "Create";
        //                        logs.taskid = picklist.gen_id;
        //                        logs.date = DateTime.Now.ToString("dd/MM/yyyy");
        //                        logs.time = DateTime.Now.ToString("HH:mm:ss");
        //                        logs.username = user;
        //                        _context.Add(logs);

        //                    }
        //                    else
        //                    {
        //                        var availableQuantity = allotstorage.Count();

        //                        var message = availableQuantity + " units of stock are available, but some have already been allocated. Please create a Purchase Order.";

        //                        return Json(new { success = false, message = message });
        //                    }
        //                }
        //            }
        //        }
        //        _context.SaveChanges();
        //        return Json(new { success = true, message = "Picklist Generate Successfully !" });
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (Exception ex)
        //    {
        //        //Console.WriteLine(ex.Message);
        //        return Json(new { success = false, message = ex.Message });

        //    }
        //}

        public IActionResult Create()
        {
            var soinward_sono = _context.so_inward.Where(a => a.status == "Pending").Select(a => a.sono).Distinct().ToList();

            var distinctSono = _context.Picklist_Generation
                .Where(a => soinward_sono.Contains(a.sono)) // Filter by SONOs present in so_inward
                .GroupBy(a => a.sono)
                .Select(g => new
                {
                    SONO = g.Key,
                    TotalSOQty = g.Sum(x => Convert.ToInt32(x.soqty)),
                    TotalPickingQty = g.Sum(x => Convert.ToInt32(x.pickingqty))
                })
                .Where(x => x.TotalSOQty == x.TotalPickingQty)
                .Select(x => x.SONO) // Select only SONO values
                .Distinct()
                .ToList();

            var all_soinward_sono = soinward_sono.Except(distinctSono).ToList(); // SONOs where sumquantity match

            List<SelectListItem> wbridge = all_soinward_sono
                .Select(x => new SelectListItem
                {
                    Value = x,
                    Text = x
                })
                .ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select SONO----"
            };

            wbridge.Insert(0, defItem);
            ViewBag.data1 = wbridge;

            return View();
        }

        private string GetSecondDigit(string boxno)
        {
            string[] parts = boxno.Split('-');
            return parts.Length == 2 ? parts[1].Trim() : string.Empty;
        }

        // GET: Picklist_Generation/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Picklist_Generation == null)
            {
                return NotFound();
            }

            var Picklist_Generation = await _context.Picklist_Generation.FindAsync(id);
            if (Picklist_Generation == null)
            {
                return NotFound();
            }
            return View(Picklist_Generation);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,rackname,shelves,bin")] Picklist_Generation Picklist_Generation)
        {
            if (id != Picklist_Generation.gen_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(Picklist_Generation);
                    await _context.SaveChangesAsync();
                    _notyfService.Success("Picklist updated Succesfully");

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!rack_masterExists(Picklist_Generation.gen_id))
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
            return View(Picklist_Generation);
        }

        // GET: Picklist_Generation/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Picklist_Generation == null)
            {
                return NotFound();
            }

            var Picklist_Generation = await _context.Picklist_Generation
                .FirstOrDefaultAsync(m => m.gen_id == id);
            if (Picklist_Generation == null)
            {
                return NotFound();
            }

            return View(Picklist_Generation);
        }

        // POST: Picklist_Generation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Picklist_Generation == null)
            {
                return Problem("Entity set 'ErosDbContext.Picklist_Generation'  is null.");
            }
            var Picklist_Generation = await _context.Picklist_Generation.FindAsync(id);
            if (Picklist_Generation != null)
            {
                _context.Picklist_Generation.Remove(Picklist_Generation);
            }

            _context.SaveChanges();
            _notyfService.Success("Picklist delete Succesfully");

            return RedirectToAction(nameof(Index));
        }

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
        //            .GroupBy(q => GetSecondDigit(q.boxno))
        //            .ToDictionary(group => group.Key, group => group.Count());


        //        // Group by PI
        //        var storedata1 = _context.Storage_Operation
        //            .Where(a => a.productcode == b && a.statusflag == "PI")
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
        //        };

        //        inStockQuantities.Add(instcok);
        //    }

        //    return View(inStockQuantities);
        //}

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

        private List<string> GetPossibleBoxes(int totalBoxes)
        {
            var boxes = new List<string>();
            for (int i = 1; i <= totalBoxes; i++)
            {
                boxes.Add($"{i}/{totalBoxes}");
            }
            return boxes;
        }
        public ActionResult InStockQty(string productcode)
        {
            List<InStockQty> inStockQuantities = new List<InStockQty>();

            if (productcode == null)
            {
                return Json(new { success = false, message = "Please select the product first !" });
            }
            else
            {
                var indata = _context.inward
                .Where(a => a.flag == 1)
                .Select(a => a.inward_id)
                .ToList();

                var inPacketData = _context.inwardPacket
                    .Where(a => indata.Contains(a.inwardId))
                    .ToList();

                var productdata = _context.inwardPacket
                    .Select(a => new { a.productcode, a.noofpackets })
                    .Distinct()
                    .ToList();

                var productcodes = _context.Storage_Operation.Where(a => a.productcode.Trim().ToUpper() == productcode.Trim().ToUpper()).Select(a => a.productcode.Trim().ToUpper()).Distinct().ToList();

                foreach (var b in productcodes)
                {

                    var result = new List<KeyValuePair<string, int>>();

                    var storedata = _context.Storage_Operation
                        .Where(a => a.productcode.Trim().ToUpper() == b && a.statusflag ==    "ST")// && a.statusflag == "ST" && a.statusflag == "PI"
                        .ToList();
                    var groupedData = storedata
                        .GroupBy(q => GetSecondDigit(q.boxno))
                        .ToDictionary(group => group.Key, group => group.Count());

                    var box = storedata.Select(a => a.boxno.Trim()).FirstOrDefault();
                    var splitbox = GetSpliBox(box);

                    var allotstock = _context.Storage_Operation.Where(a => a.productcode.Trim().ToUpper() == productcode.Trim().ToUpper() && a.pickflag == "1").ToList();
                    var groupedDataAllot = allotstock
                        .GroupBy(q => GetSecondDigit(q.boxno))
                        .ToDictionary(group => group.Key, group => group.Count());

                    // Group by PI
                    var storedata1 = _context.Storage_Operation
                        .Where(a => a.productcode.Trim().ToUpper() == b && a.statusflag == "PI")
                        .ToList();
                    var groupedData1 = storedata1
                        .GroupBy(q => GetSecondDigit(q.boxno))
                        .ToDictionary(group => group.Key, group => group.Count());


                    int secondDigitCount = int.MaxValue;
                    int secondDigitCount1 = int.MaxValue;
                    int secondDigitCount3 = int.MaxValue;

                    var possibleBoxes = new List<string>();

                    //FIRST
                    if (groupedData.Count > 0)
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
                        secondDigitCount = result.Min(kvp => kvp.Value);
                    }
                    else
                    {
                        secondDigitCount = 0;
                    }

                    //SECAND
                    if (groupedData1.Count > 0)
                    {
                        possibleBoxes = GetPossibleBoxes(splitbox);
                    }
                    foreach (var item in possibleBoxes)
                    {
                        int count = 0;
                        foreach (var kvp in groupedData1)
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
                        secondDigitCount1 = result.Min(kvp => kvp.Value);
                    }
                    else
                    {
                        secondDigitCount1 = 0;
                    }

                    //THRID
                    if (groupedDataAllot.Count > 0)
                    {
                        possibleBoxes = GetPossibleBoxes(splitbox);
                    }
                    foreach (var item in possibleBoxes)
                    {
                        int count = 0;
                        foreach (var kvp in groupedDataAllot)
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
                        secondDigitCount3 = result.Min(kvp => kvp.Value);
                    }
                    else
                    {
                        secondDigitCount3 = 0;
                    }
                    
                    var instcok = new InStockQty
                    {
                        productcode = b,
                        stcokallocate = secondDigitCount1,
                        currentqty = secondDigitCount,
                        allotstock = secondDigitCount3,
                    };

                    inStockQuantities.Add(instcok);
                }
            }
            ViewBag.roles= HttpContext.Session.GetString("Role");
            return View(inStockQuantities);
        }
        public ActionResult CheckStock(string productcode, int quantity)
        {
            List<InStockQty> inStockQuantities = new List<InStockQty>();

            // ... Your existing code ...
            var indata = _context.inward
                .Where(a => a.flag == 1)
                .Select(a => a.inward_id)
                .ToList();

            var inPacketData = _context.inwardPacket
                .Where(a => indata.Contains(a.inwardId))
                .ToList();

            var productdata = _context.inwardPacket
                .Select(a => new { a.productcode, a.noofpackets })
                .Distinct()
                .ToList();

            //var productcodes = _context.Storage_Operation.Where(v => v.productcode.Trim() == productcode.Trim()).Select(a => a.productcode.Trim()).Distinct().ToList();
            var productcodes = _context.Storage_Operation.Where(a => a.productcode.ToUpper() == productcode.ToUpper()).Select(a => a.productcode.ToUpper()).Distinct().ToList();

            if(productcodes.Count > 0)
            {
                foreach (var b in productcodes)
                {
                    // Group by ST
                    var storedata = _context.Storage_Operation
                        .Where(a => a.productcode.ToUpper() == b)// && a.statusflag == "ST" && a.statusflag == "PI"
                        .ToList();
                    var groupedData = storedata
                        .GroupBy(q => GetSecondDigit(q.boxno))
                        .ToDictionary(group => group.Key, group => group.Count());


                    // Group by PI
                    var storedata1 = _context.Storage_Operation
                        .Where(a => a.productcode.ToUpper() == b && a.statusflag == "PI")
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
                    };

                    inStockQuantities.Add(instcok);
                }
            }
            else
            {
                return Json(new { success = false, message = "Insufficinet storage !" });
            }

            return View(inStockQuantities);
        }

        //public IActionResult InStockQty(string productcode)
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

        //    var productcodes = _context.Storage_Operation.Where(v => v.productcode == productcode).Select(a => a.productcode).Distinct().ToList();


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
        //private string GetSecondDigit(string boxno)
        //{
        //    string[] parts = boxno.Split('-');
        //    return parts.Length == 2 ? parts[1].Trim() : string.Empty;
        //}

        private bool rack_masterExists(int id)
        {
            return (_context.Picklist_Generation?.Any(e => e.gen_id == id)).GetValueOrDefault();
        }

        [HttpPost]
        public IActionResult GetSono(string optionValue)
        {
            
            Picklist_Generation picklist = new Picklist_Generation();
            List<Picklist_Generation> picklistDataList = new List<Picklist_Generation>();
           
            var getdatapg = _context.Picklist_Generation
                            .Where(a => a.sono == optionValue && a.flagstatus == 0)
                            .ToList();

            if (getdatapg.Count > 0)
            {
                
                int i = 1; // Assuming 'i' is initialized somewhere in your code
               
                foreach (var data in getdatapg)
                {
                    var soqty = data.soqty;
                    var pickqty = data.pickingqty;
                    Picklist_Generation data1; // Declare data1 outside the if-else blocks

                    if (Convert.ToInt32(soqty) > 0 && Convert.ToInt32(pickqty) == 0)
                    {
                        data1 = new Picklist_Generation
                        {
                            gen_id = i,
                            prdcode = data.prdcode,
                            prdname = data.prdname,
                            soqty = data.soqty.ToString(),
                            pickingqty = Convert.ToInt32(pickqty).ToString(),
                        };
                    }
                    else
                    {
                        data1 = new Picklist_Generation
                        {
                            gen_id = i,
                            prdcode = data.prdcode,
                            prdname = data.prdname,
                            soqty = data.soqty.ToString(),
                            pickingqty = (Convert.ToInt32(soqty) - Convert.ToInt32(pickqty)).ToString(),
                        };
                    }

                    picklistDataList.Add(data1);
                    i++;
                }
            }

            else
            {
                //var saleorder = _context.so_inward
                //.Where(a => a.sono.Trim() == optionValue.Trim())
                //.Include(a => a.soProduct_details)
                //.FirstOrDefault();
                var saleorder = _context.so_inward
                .Where(a => a.sono.Trim() == optionValue.Trim())
                .Include(a => a.soProduct_details)
                .FirstOrDefault();

                var product = _context.so_product.Where(a => a.orderid == saleorder.id).ToList();

                int i = 0;
                foreach (var mat in product)
                {

                    //var EXIST = _context.Picking_Operation.Where(a => a.sono.Trim() == optionValue.Trim() && a.productcode.Trim() == mat.productcode.Trim()).ToList();
                    //if (EXIST.Count != 0)
                    //{
                    //    foreach (var item in EXIST)
                    //    {
                    //    }
                    //}
                    //else
                    //{
                    i++;
                    var data = new Picklist_Generation
                    {
                        gen_id = i,
                        prdcode = mat.productcode,
                        prdname = mat.description,
                        soqty = mat.quantity.ToString(),
                    };
                    picklistDataList.Add(data); // Add the data to the list
                                                //}
                }
            }
            
            return Json(picklistDataList); // Return the list of Picklist_Generation data
        }

        //[HttpPost]
        //public IActionResult GetSono(string optionValue, string productcode)
        //{
        //    List<Picklist_Generation> picklistDataList = new List<Picklist_Generation>();

        //    var saleorder = _context.so_inward
        //        .Where(a => a.sono == optionValue)
        //        .Include(a => a.soProduct_details)
        //        .FirstOrDefault();

        //    if (saleorder != null)
        //    {
        //        var product = _context.so_product
        //            .Where(a => a.orderid == saleorder.id)
        //            .ToList();

        //        foreach (var mat in product)
        //        {
        //            var storageOperationData = _context.Storage_Operation
        //                .Where(a => a.productcode.Trim() == mat.productcode.Trim())
        //                .OrderBy(a => a.batchcode.Trim())  // Order by batchcode in ascending order (FIFO)
        //                .FirstOrDefault();

        //            if (mat.productcode != null)
        //            {
        //                var storedata = _context.Storage_Operation.Where(a => a.productcode.Trim() == mat.productcode.Trim()).ToList();
        //                var groupedData = storedata
        //                    .GroupBy(q => new { SecondDigit = GetSecondDigit(q.boxno), LocationCode = q.locationcode })
        //                    .ToDictionary(group => $"{group.Key.SecondDigit}*{group.Key.LocationCode}", group => group.Count());
        //                //var qtypershipper = _context
        //                foreach (var kvp in groupedData)
        //                {
        //                    var secondDigit = kvp.Key.Split('*')[0].Trim();
        //                    var locationCode = kvp.Key.Split('*')[1].Trim();
        //                    int secondDigitCount = kvp.Value;

        //                    // You can now use 'secondDigit', 'locationCode', and 'secondDigitCount' as needed

        //                    var data = new Picklist_Generation
        //                    {
        //                        prdcode = mat.productcode,
        //                        prdname = mat.description,
        //                        soqty = mat.quantity.ToString(),
        //                        boxno = secondDigit,
        //                        location = locationCode,
        //                        batchcode = storageOperationData.batchcode.Trim(),
        //                        instockqty = secondDigitCount.ToString(),
        //                    };

        //                    picklistDataList.Add(data);
        //                }
        //            }
        //        }

        //        if (picklistDataList.Any())
        //        {
        //            // Add the data to the list
        //            return Json(picklistDataList);
        //        }
        //        else
        //        {
        //            // Product code doesn't exist in storage
        //            return View("ProductNotExists");
        //        }
        //    }

        //    return View("ProductNotExists");
        //}

        //public IActionResult GetSono(string optionValue, string productcode)
        //{
        //    List<Picklist_Generation> picklistDataList = new List<Picklist_Generation>();

        //    var saleorder = _context.so_inward
        //        .Where(a => a.sono == optionValue)
        //        .Include(a => a.soProduct_details)
        //        .FirstOrDefault();

        //    if (saleorder != null)
        //    {
        //        var product = _context.so_product
        //            .Where(a => a.orderid == saleorder.id)
        //            .ToList();

        //        int i = 0;
        //        foreach (var mat in product)
        //        {
        //            i++;

        //            var storageOperationData = _context.Storage_Operation
        //                .Where(a => a.productcode.Trim() == mat.productcode.Trim())
        //                .OrderBy(a => a.batchcode.Trim())  // Order by batchcode in ascending order (FIFO)
        //                .FirstOrDefault();

        //            if (mat.productcode != null)
        //            {
        //                var storedata = _context.Storage_Operation.Where(a => a.productcode.Trim() == mat.productcode.Trim()).ToList();
        //                // Use HashSet to store unique boxno values
        //                var groupedData = storedata
        //                        .GroupBy(q => new { SecondDigit = GetSecondDigit(q.boxno), q.locationcode })
        //                        .ToDictionary(group => $"{group.Key.SecondDigit}-{group.Key.locationcode}", group => group.Count());

        //                //var groupedData = storedata
        //                //    .GroupBy(q => GetSecondDigit(q.boxno))
        //                //    .ToDictionary(group => group.Key, group => group.Count());
        //                int secondDigitCount = 0;
        //                foreach (var kvp in groupedData)
        //                {
        //                    var secondDigit = kvp.Key.Trim();
        //                    secondDigitCount = kvp.Value;
        //                }

        //                var uniqueBoxNos = new HashSet<string>();

        //                foreach (var a in storedata)
        //                {
        //                    string boxno = a.boxno.Trim();

        //                    var boxParts = boxno.Split('-');
        //                    if (boxParts.Length == 2 && boxParts[1] == "2")
        //                    {
        //                        string boxQuantity = boxParts[0];
        //                        string second = boxParts[1];
        //                    }
        //                    string third = boxParts[1];
        //                    int duplicate = 0;
        //                    // Check if the boxno is unique before adding to the list
        //                    if (uniqueBoxNos.Add(third))
        //                    {
        //                        //// Check if product code exists in PickingOperation
        //                        //if (_context.Picking_Operation.Any(po => po.productcode.Trim() == mat.productcode.Trim() && po.boxno.EndsWith(third) && po.sono.Trim() == optionValue.Trim()))
        //                        //{
        //                        //    duplicate++;
        //                        //}
        //                        var data = new Picklist_Generation
        //                        {
        //                            //gen_id = i,
        //                            prdcode = mat.productcode,
        //                            prdname = mat.description,
        //                            soqty = mat.quantity.ToString(),
        //                            boxno = third.ToString(),
        //                            batchcode = storageOperationData.batchcode.Trim(), // Use the original batch code
        //                            location = storageOperationData.locationcode.ToString(),
        //                            instockqty = secondDigitCount.ToString(), // Use FirstOrDefault to get a single value
        //                        };
        //                        picklistDataList.Add(data);
        //                    }
        //                }
        //            }
        //        }

        //        if (picklistDataList.Any())
        //        {
        //            // Add the data to the list
        //            return Json(picklistDataList);
        //        }
        //        else
        //        {
        //            // Product code doesn't exist in storage
        //            return View("ProductNotExists");
        //        }
        //    }

        //    return View("ProductNotExists");
        //}


        //[HttpPost]
        //public IActionResult GetSono(string optionValue, string productcode)
        //{
        //    List<Picklist_Generation> picklistDataList = new List<Picklist_Generation>();

        //    var saleorder = _context.so_inward
        //        .Where(a => a.sono == optionValue)
        //        .Include(a => a.soProduct_details)
        //        .FirstOrDefault();

        //    if (saleorder != null)
        //    {
        //        var product = _context.so_product
        //            .Where(a => a.orderid == saleorder.id)
        //            .ToList();

        //        int i = 0;
        //        foreach (var mat in product)
        //        {
        //            i++;

        //            var storageOperationData = _context.Storage_Operation
        //       .Where(a => a.productcode == mat.productcode)
        //       .OrderBy(a => a.batchcode)  // Order by batchcode in ascending order (FIFO)
        //       .FirstOrDefault();

        //            if (mat.productcode != null)
        //            {

        //                var storedata = _context.Storage_Operation.Where(a=>a.productcode == mat.productcode).ToList();

        //                foreach (var a in storedata)
        //                {
        //                    string boxno = a.boxno;

        //                    var boxParts = boxno.Split('-');
        //                    if (boxParts.Length == 2 && boxParts[1] == "2")
        //                    {
        //                        string boxQuantity = boxParts[0];
        //                        string second = boxParts[1];
        //                    }
        //                    string third = boxParts[1];
        //                    var data = new Picklist_Generation
        //                    {
        //                        gen_id = i,
        //                        prdcode = mat.productcode,
        //                        prdname = mat.description,
        //                        soqty = mat.quantity.ToString(),
        //                        boxno = third.ToString(),
        //                        batchcode = storageOperationData.batchcode.Trim(), // Use the original batch code
        //                        location = storageOperationData.locationcode.ToString(),
        //                        instockqty = "-", //boxNoCount.ToString(),
        //                    };
        //                    picklistDataList.Add(data);

        //                }
        //            }

        //        }

        //        if (picklistDataList.Any())
        //        {
        //            // Add the data to the list
        //            return Json(picklistDataList);
        //        }
        //        else
        //        {
        //            // Product code doesn't exist in storage
        //            return View("ProductNotExists");
        //        }
        //    }

        //    return View("ProductNotExists");
        //}

    }
    //public List<SelectList> wbridge()
    //{

    //}
    // GET: Picklist_Generation/Create
    //public IActionResult Create()
    //{
    //    Picklist_Generation applicant = new Picklist_Generation();

    //    var listso = _context.Picklist_Generation
    //           .Where(a => a.flagstatus == 0)
    //           .Select(a => a.sono)
    //           .Distinct()
    //           .ToList();

    //    List<SelectListItem> wbridge = _context.so_inward.AsNoTracking()
    //        .OrderBy(n => n.sono)
    //        .Select(n => new SelectListItem
    //        {

    //            Value = n.sono,
    //            Text = n.sono.ToString()
    //        }).ToList();
    //    var defItem = new SelectListItem()
    //    {
    //        Value = "",
    //        Text = "----Select Segement----"
    //    };

    //    wbridge.Insert(0, defItem);
    //    ViewBag.data1 = wbridge;

    //    return View();
    //}

    //public IActionResult Create()
    //{
    //    Picklist_Generation applicant = new Picklist_Generation();

    //    var listso = _context.Picklist_Generation
    //        .Where(a => a.flagstatus == 0)
    //        .Select(a => a.sono)
    //        .Distinct()
    //        .ToList();

    //    // once if both of soqty and pickqty both values of Picklist_Generation are if found same then dont show that sono in viewbag

    //    List<SelectListItem> wbridge = _context.so_inward.AsNoTracking()
    //        .Select(n => new SelectListItem
    //        {
    //            Value = n.sono,
    //            Text = n.sono.ToString()
    //        })
    //        .ToList();

    //    // Filter wbridge based on listso
    //    wbridge = wbridge
    //        .Where(item => listso.Contains(item.Value) || listso.Any())  // Include items not present in listso
    //        .OrderBy(item => item.Value)
    //        .ToList();

    //    var defItem = new SelectListItem()
    //    {
    //        Value = "",
    //        Text = "----Select Segement----"
    //    };

    //    wbridge.Insert(0, defItem);
    //    ViewBag.data1 = wbridge;

    //    return View();
    //}

}





