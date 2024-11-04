//pick opration
using AspNetCore;
using AspNetCoreHero.ToastNotification.Abstractions;
using AuthSystem.Data;
using DocumentFormat.OpenXml.Drawing.Diagrams;

//using DocumentFormat.OpenXml.Drawing;
//using DocumentFormat.OpenXml.Drawing.Diagrams;
//using DocumentFormat.OpenXml.Office.CustomUI;
using eros.Models;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
//using Nest;
using Newtonsoft.Json;
using System.Linq;
using System.Text.RegularExpressions;
using static eros.Controllers.Picking_OperationController;
//using static Nest.JoinField;

namespace eros.Controllers
{
    public class Picking_OperationController : Controller
    {
        private readonly ErosDbContext _context;
        public INotyfService _notyfService { get; }

        public Picking_OperationController(ErosDbContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;

        }
        // Define a model to represent the split datajson
        public class SplitDataModel
        {
            public string ProductCode { get; set; }
            public string BatchCode { get; set; }
            public string BoxNo { get; set; }
            public string sono { get; set; }
        }

        //public IActionResult DamageStockQty([FromBody] SplitDataModel splitData)
        //{
        //    //Damage Data found in storage 
        //    var storage = _context.Storage_Operation
        //        .Where(a => a.productcode.Trim() == splitData.ProductCode.Trim()
        //            && a.boxno.Trim() == splitData.BoxNo.Trim()
        //            && a.batchcode.Trim() == splitData.BatchCode.Trim()
        //            && a.statusflag == "ST")
        //        .FirstOrDefault();

        //    var storageDM = _context.Storage_Operation
        //        .Where(a => a.productcode.Trim() == splitData.ProductCode.Trim()
        //            && a.boxno.Trim() == splitData.BoxNo.Trim()
        //            && a.batchcode.Trim() == splitData.BatchCode.Trim()
        //            && a.statusflag == "DMG")
        //        .FirstOrDefault();

        //    var storagePI = _context.Storage_Operation
        //        .Where(a => a.productcode.Trim() == splitData.ProductCode.Trim()
        //            && a.boxno.Trim() == splitData.BoxNo.Trim()
        //            && a.batchcode.Trim() == splitData.BatchCode.Trim()
        //            && a.statusflag == "PI")
        //        .FirstOrDefault();

        //    var pickfound = _context.pickstorage.Where(a => a.sono.Trim() == splitData.sono.Trim() && a.productcode.Trim() == splitData.ProductCode.Trim() && a.boxno.Trim() == splitData.BoxNo.Trim() && a.batchcode.Trim() == splitData.BatchCode.Trim() && a.flag == 0).FirstOrDefault();


        //    if (storage != null)
        //    {
        //        storage.statusflag = "DMG";
        //        _context.Storage_Operation.Update(storage);
        //        _context.SaveChanges();
        //    }
        //    else
        //    {
        //        if (storageDM != null)
        //        {
        //            return Json(new { success = false, message = "Product found Damage in storage !" });
        //        }
        //        else if (storagePI != null)
        //        {
        //            return Json(new { success = false, message = "Product already Picked while picking Parform !" });
        //        }
        //        else
        //        {
        //            return Json(new { success = false, message = "Product not found in storage !" });
        //        }
        //    }
        //    var boxnosplit = splitData.BoxNo.Split('-')[1];

        //    //find pickstorage against picklist genration
        //    var picklist = _context.Picklist_Generation.Where(a => a.sono.Trim() == splitData.sono.Trim() && a.prdcode.Trim() == splitData.ProductCode.Trim()).FirstOrDefault();
        //    var pickstoage = _context.pickstorage.Where(a => a.sono.Trim() == splitData.sono.Trim() &&
        //                                                  a.productcode.Trim() == splitData.ProductCode.Trim() &&
        //                                                  a.batchcode.Trim() == splitData.BatchCode.Trim() &&
        //                                                  a.boxno.Contains(boxnosplit.Trim()) &&
        //                                                  a.flag == 0 &&
        //                                                  a.genid == picklist.gen_id).FirstOrDefault();

        //    //find new data from storage to insert
        //    var pickinsert = _context.Storage_Operation.Where(a => a.productcode.Trim() == splitData.ProductCode.Trim() && a.boxno.Contains(boxnosplit.Trim()) && a.statusflag == "ST").OrderBy(d => d.batchcode).FirstOrDefault();

        //    if (pickstoage != null)
        //    {
        //        if (pickinsert != null && pickinsert.batchcode == pickstoage.batchcode)
        //        {
        //            // Update pickstorage with data from pickinsert
        //            pickstoage.productcode = pickinsert.productcode;
        //            pickstoage.batchcode = pickinsert.batchcode;
        //            pickstoage.boxno = pickinsert.boxno;
        //            pickstoage.location = pickinsert.locationcode;

        //            _context.pickstorage.Update(pickstoage);
        //        }
        //        else if (pickinsert != null && pickinsert.batchcode != pickstoage.batchcode)
        //        {
        //            // Find the next batch data ordered by batchcode from Storage_Operation

        //            if (pickinsert != null)
        //            {
        //                // Update pickstorage with data from pickinsert
        //                pickstoage.productcode = pickinsert.productcode;
        //                pickstoage.batchcode = pickinsert.batchcode;
        //                pickstoage.boxno = pickinsert.boxno;
        //                pickstoage.location = pickinsert.locationcode;

        //                _context.pickstorage.Update(pickstoage);
        //            }
        //            else
        //            {
        //                return Json(new { success = false, message = "Next Batch Product not found in storage !" });

        //                //return Json(new { success = false, message = "Asking to Genrate purchase order of that product: " + pickinsert.productcode + "Box No : " + boxnosplit + " Incomplete Picking done !" });
        //            }
        //            _context.SaveChanges();
        //        }
        //        else
        //        {
        //            return Json(new { success = true, message = "Please generate the purchase order for " + pickstoage.productcode + ", but the picking process is suspended temporarily!" });
        //        }
        //        // Update pickstorage with data from pickinsert
        //    }
        //    else
        //    {
        //        //if no data found against productcode, batch,boxno
        //        return Json(new { success = true, message = "Damage Product Update successfully" });
        //        //no need to find data in pickstoage agaianst enter productcode TO INSERT IN PICKTSTORAGE
        //    }

        //    return Json(new { success = true, message = "Damage Product Update successfully" });
        //}

        [HttpPost]
        public IActionResult DamageStockQty([FromBody] SplitDataModel splitData)
        {
            var boxnosplit = splitData.BoxNo.Split('-')[1];

            //Damage Data found in storage 
            var storage = _context.Storage_Operation
                .Where(a => a.productcode.Trim().ToUpper() == splitData.ProductCode.Trim().ToUpper()
                    && a.boxno.Trim() == splitData.BoxNo.Trim()
                    && a.batchcode.Trim() == splitData.BatchCode.Trim()
                    && a.statusflag == "ST")
                .FirstOrDefault();

            //Damage data found in pickstorage
            var pickfound = _context.pickstorage
                            .Where(a => a.sono.Trim() == splitData.sono.Trim() 
                            && a.productcode.Trim().ToUpper() == splitData.ProductCode.Trim().ToUpper() 
                            && a.boxno.Trim() == splitData.BoxNo.Trim() && a.batchcode.Trim() == splitData.BatchCode.Trim()
                            && a.flag == 0)
                            .FirstOrDefault();

            if(pickfound!=null && storage != null)
            {
                storage.statusflag = "DMG";
                _context.Storage_Operation.Update(storage);
                _context.SaveChanges();
                //IN CASE FOUND DMG IN PICKED DATA AND STORAGE 
                var storagelist = _context.Storage_Operation
                            .Where(a => a.productcode.Trim() == splitData.ProductCode.ToUpper().Trim() &&
                                        a.boxno.Contains(boxnosplit.Trim()) &&
                                        a.statusflag == "ST" &&
                                        a.pickflag == "0")
                            .OrderBy(d => d.batchcode)
                            .ToList();

                if(storagelist.Count > 0)
                {
                    var getfirst = storagelist.FirstOrDefault();
                    //REPLACE WITH THE PICKED DATA
                    pickfound.boxno = getfirst.boxno;
                    pickfound.batchcode = getfirst.batchcode;
                    pickfound.location = getfirst.locationcode;

                    //UPDATE PICKDATA
                    _context.pickstorage.Update(pickfound);
                    _context.SaveChanges();

                    //UPDATE STORAGE
                    getfirst.pickflag = "1";
                    _context.Storage_Operation.Update(getfirst);
                    _context.SaveChanges();
                }
                else
                {
                    //UPDATE PICKDATA
                    pickfound.flag = 2;
                    _context.pickstorage.Update(pickfound);
                    _context.SaveChanges();
                }
            }
            else if(pickfound == null && storage != null)
            {
                //IN CASE FOUND DMG IN STORAGE ONLY
                storage.statusflag = "DMG";
                _context.Storage_Operation.Update(storage);
                _context.SaveChanges();
            }

            var picked = _context.pickstorage.Where(a => a.flag == 1 && a.sono.Trim() == splitData.sono.Trim()).ToList();
            var balced = _context.pickstorage.Where(a => a.flag == 0 && a.sono.Trim() == splitData.sono.Trim()).ToList();
            return Json(new { success = true, message = "Damage Product Update successfully" , qtypicked = picked.Count, balpicked = balced.Count });

        }

        public IActionResult Suspend(string sono)
        {
            var get_pgdata = _context.pickstorage.Where(a => a.sono.Trim() == sono && a.flag == 1).ToList();
            var get_pg = _context.Picklist_Generation.Where(a => a.sono.Trim() == sono.Trim()).ToList();
            var get_podata = _context.Picking_Operation.Where(a => a.sono.Trim() == sono).ToList();

            if (get_pgdata.Count > 0)
            {
                foreach (var pgData in get_pgdata)
                {
                    var b1 = pgData.boxno.Trim().Split('-');
                    var get_storagedata = _context.Storage_Operation
                        .Where(a => a.statusflag.Trim() == "PI"
                                    && a.productcode.Trim().ToUpper() == pgData.productcode.Trim().ToUpper()
                                    && a.locationcode.Trim() == pgData.location.Trim()
                                    && a.batchcode.Trim() == pgData.batchcode.Trim()
                                    && a.boxno.Trim().EndsWith(b1.Last())) // Compare the last element
                        .ToList();

                    foreach (var storageData in get_storagedata)
                    {
                        storageData.statusflag = "ST"; // Use double quotes for string assignment
                        _context.Update(storageData);
                    }

                    pgData.flag = 0;
                    _context.Update(pgData);
                }

            }
            else
            {
                return Json(new { success = false, message = "Please Add data !" });
            }

            if (get_podata != null)
            {
                _context.Picking_Operation.RemoveRange(get_podata);
            }
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: Picking_Process
        public class SonoStatusViewModel
        {
            public int Id { get; set; } // Add this property if it doesn't exist
            public string Sono { get; set; }
            public string Status { get; set; }
            public bool HasResume { get; set; }
            public bool HasSuspend { get; set; }
            public string date { get; set; }
        }

        //public async Task<IActionResult> Index()
        //{
        //    var distinctSonoList = _context.Picking_Operation
        //                                    .Select(p => p.sono)
        //                                    .Distinct()
        //                                    .ToList();

        //    List<SonoStatusViewModel> sonoStatusList = new List<SonoStatusViewModel>();

        //    foreach (var sono in distinctSonoList)
        //    {
        //        var collectSoData = _context.pickstorage.Where(a => a.sono == sono).ToList();
        //        var pocollectionData = _context.Picking_Operation.Where(a => a.sono == sono).ToList();

        //        bool hasSuspend = collectSoData.Any(data => data.flag == 1 && data.flag == 0);
        //        bool isExit = collectSoData.All(data => data.flag == 1);

        //        string status;

        //        if (collectSoData.Count != pocollectionData.Count)
        //        {
        //            status = "Suspend";
        //        }
        //        //else if (pocollectionData.Count == 0)
        //        //{
        //        //    status = "Start Picking";
        //        //}
        //        else if (collectSoData.Count == pocollectionData.Count)
        //        {
        //            status = "Exit";
        //        }
        //        else
        //        {
        //            continue;
        //        }

        //        sonoStatusList.Add(new SonoStatusViewModel
        //        {

        //            Sono = sono,
        //            Status = status,
        //            HasSuspend = hasSuspend
        //        });
        //    }


        //    return View(sonoStatusList);
        //}

        public async Task<IActionResult> Index()
        {
            var distinctSonoList = _context.Picking_Operation
                                            .Select(p => p.sono)
                                            .Distinct()
                                            .ToList();

            List<SonoStatusViewModel> sonoStatusList = new List<SonoStatusViewModel>();

            foreach (var sono in distinctSonoList)
            {
                var collectSoData = _context.pickstorage.Where(a => a.sono == sono).ToList();
                var pocollectionData = _context.Picking_Operation.Where(a => a.sono == sono).ToList();

                bool hasSuspend = collectSoData.Any(data => data.flag == 1 || data.flag == 0);
                bool isExit = collectSoData.All(data => data.flag == 1);

                string status;

                if (collectSoData.Count != pocollectionData.Count)
                {
                    status = "Suspend";
                }

                else if (collectSoData.Count == pocollectionData.Count)
                {
                    status = "Exit";
                }
                else
                {
                    continue;
                }

                // Fetch Id based on your logic, assuming you have some Id associated with sono
                var id = _context.Picking_Operation.FirstOrDefault(x => x.sono == sono)?.pick_id;
                var date = _context.Picking_Operation.Where(x => x.sono == sono).Select(x => x.date).FirstOrDefault();


                sonoStatusList.Add(new SonoStatusViewModel
                {
                    Id = id ?? 0, // Assign the fetched Id here; use a default value if Id is nullable
                    Sono = sono,
                    date = date, // Assign the fetched date here
                    Status = status,
                    HasSuspend = hasSuspend,
                });
            }
            // Order sonoStatusList by date in descending order
            sonoStatusList = sonoStatusList.OrderByDescending(x => x.date).ToList();
            return View(sonoStatusList);
        }

        //GET: Picking_Process/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Picking_Operation == null)
            {
                return NotFound();
            }

            var Picking_Operation = await _context.Picking_Operation
                .FirstOrDefaultAsync(m => m.pick_id == id);
            if (Picking_Operation == null)
            {
                return NotFound();
            }

            return View(Picking_Operation);
        }

        //public List<SelectListItem> GetWbridgeList()
        //{
        //    List<SelectListItem> wbridgeList = _context.pickstorage
        //        .Where(n => _context.pickstorage.Any(p => p.sono == n.sono && n.flag == 0))
        //        .AsNoTracking()
        //        .OrderBy(n => n.sono)
        //        .Select(n => new SelectListItem
        //        {
        //            Value = n.sono,
        //            Text = n.sono.ToString()
        //        })
        //        .Distinct()
        //        .ToList();

        //    var defaultItem = new SelectListItem()
        //    {
        //        Value = "",
        //        Text = "----Select SONO----"
        //    };

        //    wbridgeList.Insert(0, defaultItem);
        //    return wbridgeList;
        //}

        public List<SelectListItem> GetWbridgeList()
        {
            var pglist = _context.Picklist_Generation
                                .Select(a => a.sono)
                                .Distinct()
                                .ToList();

            var filteredSonoList = new List<string>();

            foreach (var sono in pglist)
            {
                var exist = _context.Picking_Operation
                                    .FirstOrDefault(a => a.sono == sono);

                if (exist == null)
                {
                    filteredSonoList.Add(sono);
                }
            }

            // Convert filtered sono values to SelectListItem objects
            List<SelectListItem> wbridgeList = filteredSonoList
                .Select(sono => new SelectListItem
                {
                    Value = sono,
                    Text = sono
                })
                .ToList();

            // Insert default item at the beginning of the list
            var defaultItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select SONO----"
            };
            wbridgeList.Insert(0, defaultItem);

            return wbridgeList;
        }

        // GET: Picking_Process/Create
        public IActionResult Create()
        {
            Picking_Operation applicant = new Picking_Operation();

            ViewBag.data1 = GetWbridgeList();
            return View();

        }

        //public IActionResult Create()
        //{
        //    // Retrieve the list of sono values that already exist in Picking_Operation
        //    var existingSonoValues = _context.Picking_Operation.Select(p => p.sono).ToList();
        //    if (existingSonoValues.Any())
        //    {
        //        var existingenartion = _context.Picklist_Generation
        //                                .Where(a => existingSonoValues.Contains(a.sono))
        //                                .GroupBy(a => Convert.ToInt32(a.soqty)) // Group by soqty
        //                                .Select(group => new
        //                                {
        //                                    SoQty = group.Key,
        //                                    SumSoQty = group.Sum(item => Convert.ToInt32(item.soqty))
        //                                })
        //                                .ToList();

        //        if (existingSonoValues.Count != existingenartion.FirstOrDefault()?.SumSoQty)
        //        {
        //            // If counts do not match, add sono values to ViewBag
        //            List<SelectListItem> wbridge = _context.so_inward
        //                .AsNoTracking()
        //                .OrderBy(n => n.sono)
        //                .Where(n => !existingSonoValues.Contains(n.sono))
        //                .Select(n => new SelectListItem
        //                {
        //                    Value = n.sono,
        //                    Text = n.sono.ToString()
        //                })
        //                .ToList();

        //            var defItem = new SelectListItem()
        //            {
        //                Value = "",
        //                Text = "----Select Segment----"
        //            };

        //            wbridge.Insert(0, defItem);
        //            ViewBag.data1 = wbridge;
        //        }
        //        else
        //        {
        //            // If counts match, you might want to handle this case
        //            // You can choose to add a different set of sono values or take another action
        //        }
        //    }
        //    else
        //    {
        //        // If there are no existing sono values, add all sono values to ViewBag
        //        List<SelectListItem> wbridge = _context.so_inward
        //            .AsNoTracking()
        //            .OrderBy(n => n.sono)
        //            .Select(n => new SelectListItem
        //            {
        //                Value = n.sono,
        //                Text = n.sono.ToString()
        //            })
        //            .ToList();

        //        var defItem = new SelectListItem()
        //        {
        //            Value = "",
        //            Text = "----Select Segment----"
        //        };

        //        wbridge.Insert(0, defItem);
        //        ViewBag.data1 = wbridge;
        //    }

        //    return View();
        //}

        public class PickedListData
        {
            public string Location { get; set; }
            public string BoxNo { get; set; }
            public int qtypicked { get; set; }
            public int balanceqty { get; set; }
            public string sono { get; set; }
            //public List<TableDataModel> dataList { get; set; }
            public string batchno { get; set; }
            public string productcode { get; set; }

        }
        // Define a model for your table data
        public class TableDataModel
        {
            public string Location { get; set; }
            public string BoxNo { get; set; }
            public string prdcode { get; set; }
            public string batchcode { get; set; }
            public int instockqty { get; set; }
            //public string prdname { get; set; }
            //public int pickingqty { get; set; }
        }
        private static List<Picking_Operation> picking = new List<Picking_Operation>();

        [HttpPost]
        //public IActionResult SavePickingData([FromBody] PickedListData pickedListData)
        //{
        //    try
        //    {
        //        int maxId = _context.Picking_Operation.Any() ? _context.Picking_Operation.Max(e => e.pick_id) + 1 : 1;
        //        List<Picking_Operation> savepolist = new List<Picking_Operation>();
        //        List<Picking_Operation> loadpicklist = new List<Picking_Operation>();
        //        int i = 1;

        //        savepolist = _context.Picking_Operation.Where(a => a.sono == pickedListData.sono).ToList();

        //        string sono = pickedListData.sono;
        //        var box = pickedListData.BoxNo.Trim();
        //        var enterL1 = pickedListData.Location.Trim();
        //        var enterB1 = pickedListData.BoxNo.Trim();
        //        var enterbatchno = pickedListData.batchno.Trim();
        //        var enterproductcode = pickedListData.productcode.Trim().ToUpper();
        //        var qtypicked = pickedListData.qtypicked;
        //        var balanceqty = pickedListData.balanceqty;

        //        if (balanceqty == 0)
        //        {
        //            return Json(new { success = false, message = "You have scanned all the boxes  against order no " + pickedListData.sono.Trim() + " , Balannce scan box count is "+balanceqty+" !" });
        //        }
        //        else
        //        {
        //            var splitboxdata = enterB1.Split("-");
        //            var getsecandpart = splitboxdata[1];
        //            var split_num = getsecandpart.Split("/")[0];
        //            var split_den = getsecandpart.Split("/")[1];

        //            if (Convert.ToInt32(split_num) > Convert.ToInt64(split_den))
        //            {
        //                var savepolist_pickstore = _context.pickstorage
        //                                            .Where(a => a.sono == pickedListData.sono
        //                                            && a.productcode.Trim() == pickedListData.productcode.Trim()
        //                                            && a.batchcode.Trim() == pickedListData.batchno.Trim()
        //                                            && a.flag == 0)
        //                                            .ToList();

        //                var savepolist_picking = _context.Picking_Operation
        //                                            .Where(a => a.sono == pickedListData.sono
        //                                            && a.productcode.Trim() == pickedListData.productcode.Trim()
        //                                            && a.batchcode.Trim() == pickedListData.batchno.Trim()
        //                                            && a.flag == 0)
        //                                            .ToList();


        //                if (savepolist_picking.Count <= savepolist_pickstore.Count)
        //                {
        //                    var data = savepolist_pickstore
        //                                .Where(a => a.location.Trim() == pickedListData.Location.Trim()
        //                                && a.batchcode.Trim() == pickedListData.batchno.Trim()
        //                                && a.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper())
        //                                .ToList();

        //                    if(data.Count > 0)
        //                    {
        //                        foreach (var item in data)
        //                        {
        //                            var consider_box = "1/1";
        //                            var checkinstorage = _context.Storage_Operation
        //                                .Where(a => a.locationcode.Trim() == enterL1.Trim() &&
        //                                            a.productcode.Trim().ToUpper() == enterproductcode.Trim().ToUpper() &&
        //                                            a.batchcode.Trim() == enterbatchno.Trim() &&
        //                                            a.statusflag.Trim() == "ST" &&
        //                                            a.boxno.Trim().EndsWith(consider_box.Trim()))
        //                                .ToList();

        //                            var pickcount = _context.pickstorage.Where(a => a.sono.Trim() == pickedListData.sono.Trim()
        //                                            && a.productcode.Trim() == pickedListData.productcode.Trim() 
        //                                            && a.flag == 0)
        //                                            .ToList();

        //                            // Calculate the minimum between pickcount.Count and split_num
        //                            int loopLimit = Math.Min(pickcount.Count, Convert.ToInt32(split_num));

        //                            if (checkinstorage.Count > 0 && checkinstorage.Count >= Convert.ToInt32(split_num))
        //                            {
        //                                foreach (var stitem in checkinstorage.Take(Convert.ToInt32(loopLimit)))
        //                                {
        //                                    //ADD TO PICKING OPERATION
        //                                    var pickingdata = new Picking_Operation
        //                                    {
        //                                        pick_id = maxId,
        //                                        balanceqty = balanceqty,
        //                                        batchcode = item.batchcode.Trim(),
        //                                        boxpicked = stitem.boxno.Trim(),
        //                                        locationpicked = enterL1,
        //                                        instockqty = "-",
        //                                        pickingqty = "-",
        //                                        productcode = item.productcode.Trim(),
        //                                        qtypicked = qtypicked,
        //                                        location = enterL1,
        //                                        boxno = enterB1,
        //                                        sono = pickedListData.sono.Trim(),
        //                                        //date = DateTime.Now.ToUniversalTime(),
        //                                    };
        //                                    loadpicklist.Add(pickingdata);

        //                                    //UPDATE IN PICKSTORAGE
        //                                    var pickstoredata = _context.pickstorage
        //                                                        .Where(a => a.sono.Trim() == item.sono.Trim()
        //                                                        && a.productcode.Trim() == item.productcode.Trim()
        //                                                        && a.batchcode.Trim() == item.batchcode.Trim()
        //                                                        && a.boxno.Trim().EndsWith(consider_box.Trim())
        //                                                        && a.location.Trim() == item.location.Trim()
        //                                                        && a.flag == 0)
        //                                                        .FirstOrDefault();
        //                                    if (pickstoredata != null)
        //                                    {
        //                                        //ADD PICKING
        //                                        _context.Picking_Operation.Add(pickingdata);
        //                                        _context.SaveChanges();

        //                                        //UPDATE PICKSTORAGE
        //                                        pickstoredata.flag = 1;
        //                                        _context.pickstorage.Update(pickstoredata);
        //                                        _context.SaveChanges();

        //                                        //UPDATE STORAGE
        //                                        stitem.statusflag = "PI";
        //                                        stitem.pickflag = "1";
        //                                        _context.Storage_Operation.Update(stitem);
        //                                        _context.SaveChanges();
        //                                    }

        //                                }

        //                                //UPDATE PICKLIST
        //                                var pgdata = _context.Picklist_Generation
        //                                            .Where(a => a.sono == pickedListData.sono && a.prdcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper())
        //                                            .GroupBy(a => a.prdcode)
        //                                            .Select(group => new
        //                                            {
        //                                                ProductCode = group.Key,
        //                                                SoQty = group.FirstOrDefault().soqty,
        //                                                PickingQty = group.FirstOrDefault().pickingqty
        //                                            })
        //                                            .ToList();

        //                                foreach (var groupData in pgdata)
        //                                {

        //                                    int soQty = Convert.ToInt32(groupData.SoQty.Trim());
        //                                    int pickingQty = Convert.ToInt32(groupData.PickingQty.Trim());

        //                                    if (soQty == pickingQty)
        //                                    {
        //                                        var gopickstorage = _context.pickstorage
        //                                            .Where(j => j.sono.Trim() == pickedListData.sono.Trim()
        //                                                && j.productcode.Trim().ToUpper() == groupData.ProductCode.Trim().ToUpper())
        //                                            .ToList();

        //                                        bool allFlagsAreOne = gopickstorage.All(j => j.flag == 1);

        //                                        if (allFlagsAreOne)
        //                                        {
        //                                            var pickGenerationRecords = _context.Picklist_Generation
        //                                                .Where(a => a.sono.Trim() == pickedListData.sono.Trim()
        //                                                && a.prdcode.Trim() == groupData.ProductCode.Trim()
        //                                                && a.flagstatus == 0)
        //                                                .FirstOrDefault();

        //                                            List<pickstorage> pickstore = new List<pickstorage>();

        //                                            if (pickGenerationRecords != null)
        //                                            {
        //                                                var pickingboxes = _context.Picking_Operation
        //                                                                    .Where(a => a.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper()
        //                                                                    && a.sono.Trim() == pickedListData.sono.Trim()
        //                                                                    && a.flag == 0)
        //                                                                    .ToList();

        //                                                var selectpickstorage = _context.pickstorage
        //                                                                        .Where(a => a.sono.Trim() == pickedListData.sono.Trim() &&
        //                                                                        a.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper() &&
        //                                                                        a.flag == 1)
        //                                                                        .ToList();

        //                                                if (pickingboxes.Count == selectpickstorage.Count)
        //                                                {
        //                                                    pickGenerationRecords.flagstatus = 1;
        //                                                    _context.Picklist_Generation.Update(pickGenerationRecords);
        //                                                    _context.SaveChanges();
        //                                                }
        //                                            }
        //                                        }
        //                                    }
        //                                }


        //                            }
        //                            else
        //                            {
        //                                return Json(new { success = false, message = "Scanned Box!" + enterB1 + " of " + enterproductcode + " product is not Found in storage !" });
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    return Json(new { success = false, message = "You have already scan the picked product quantities of product " + pickedListData.productcode.Trim() + " !" });
        //                }

        //                var pickqtyyyy = _context.pickstorage.Where(a => a.sono.Trim() == pickedListData.sono.Trim() && a.flag == 1).ToList();
        //                var balcqtyyyy = _context.pickstorage.Where(a => a.sono.Trim() == pickedListData.sono.Trim() && a.flag == 0).ToList();

        //                //RETURN FLAF 1
        //                return Json(new
        //                {
        //                    success = true,
        //                    message = "Data successfully processed.",
        //                    data = loadpicklist,
        //                    qtypicked = pickqtyyyy.Count(),
        //                    balanceqty = balcqtyyyy.Count(),
        //                    flag = 1,
        //                });
        //            }
        //            else
        //            {
        //                var checkin_first = _context.pickstorage
        //                                         .Where(a => a.productcode.Trim() == pickedListData.productcode.Trim()
        //                                            && a.flag == 0
        //                                            && a.boxno.Trim().Contains(getsecandpart.Trim())
        //                                            && a.batchcode.Trim() == pickedListData.batchno.Trim()
        //                                            && a.location.Trim() == pickedListData.Location.Trim())
        //                                         .FirstOrDefault();

        //                if (checkin_first == null)
        //                {
        //                    return Json(new { success = false, message =  "Please Scan correct one !" });
        //                }

        //                var checkpickstore = _context.pickstorage
        //                                        .Where(a => a.sono.Trim() == pickedListData.sono.Trim()
        //                                        && a.productcode.Trim() == pickedListData.productcode.Trim()
        //                                        && a.flag == 0)
        //                                        .ToList();

        //                var checkpickingop = _context.Picking_Operation
        //                                        .Where(a => a.sono.Trim() == pickedListData.sono.Trim() 
        //                                        && a.productcode.Trim() == pickedListData.productcode.Trim()
        //                                        && a.flag == 0)
        //                                        .ToList();

        //                if(checkpickstore.Count >= checkpickingop.Count)
        //                {
        //                    var exist = _context.Picking_Operation
        //                    .Where(a => a.sono.Trim() == pickedListData.sono.Trim() 
        //                    && a.location.Trim() == pickedListData.Location.Trim()
        //                    && a.boxno.Trim() == pickedListData.BoxNo.Trim()
        //                    && a.batchcode.Trim() == pickedListData.batchno.Trim()
        //                    && a.productcode.ToUpper().Trim() == enterproductcode.ToUpper().Trim())
        //                    .FirstOrDefault();

        //                    if(exist != null)
        //                    {
        //                        return Json(new { success = false, message = "Already Scanned " + pickedListData.productcode + " product shipper , against order no " + pickedListData.sono.Trim() + " !" });
        //                    }
        //                    else
        //                    {
        //                        var secondPartEnterB1 = enterB1.Split('-')[1];

        //                        var existdata = _context.pickstorage
        //                                        .Where(a => a.sono.Trim() == pickedListData.sono.Trim()
        //                                        && a.location.Trim() == pickedListData.Location.Trim()
        //                                        && a.boxno.Trim().Contains(secondPartEnterB1.Trim())
        //                                        && a.batchcode.Trim() == pickedListData.batchno.Trim()
        //                                        && a.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper())
        //                                        .FirstOrDefault();

        //                        if(existdata!=null && existdata.flag == 0)
        //                        {
        //                            if (existdata != null)
        //                            {
        //                                var checkinstorage = _context.Storage_Operation
        //                                    .Where(a => a.locationcode.Trim() == pickedListData.Location.Trim() &&
        //                                                a.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper() &&
        //                                                a.batchcode.Trim() == pickedListData.batchno.Trim() &&
        //                                                a.statusflag.Trim() == "ST" &&
        //                                                a.boxno.Trim() == pickedListData.BoxNo.Trim())
        //                                    .FirstOrDefault();

        //                                if (checkinstorage != null)
        //                                {
        //                                    pickedListData.qtypicked = (pickedListData.qtypicked + i);
        //                                    qtypicked = pickedListData.qtypicked;

        //                                    pickedListData.balanceqty = (pickedListData.balanceqty - i);
        //                                    balanceqty = pickedListData.balanceqty;

        //                                    var pickingdata = new Picking_Operation
        //                                    {
        //                                        pick_id = maxId,
        //                                        balanceqty = balanceqty,
        //                                        batchcode = pickedListData.batchno.Trim(),
        //                                        boxpicked = pickedListData.BoxNo.Trim(),
        //                                        locationpicked = pickedListData.Location.Trim(),
        //                                        instockqty = "-",
        //                                        pickingqty = "-",
        //                                        productcode = pickedListData.productcode.Trim(),
        //                                        qtypicked = qtypicked,
        //                                        location = pickedListData.Location.Trim(),
        //                                        boxno = pickedListData.BoxNo.Trim(),
        //                                        sono = pickedListData.sono.Trim(),
        //                                    };

        //                                    var loc = pickedListData.BoxNo.Trim().Split('-');

        //                                    //CECK STORAGE (UPDATE), PICKSTORAGE (UPDATE), AND PICKING (ADD)
        //                                    if (checkinstorage != null && existdata != null && pickingdata != null)
        //                                    {
        //                                        //UPDATE STORAGE
        //                                        checkinstorage.statusflag = "PI";
        //                                        checkinstorage.pickflag = "1";
        //                                        _context.Storage_Operation.Update(checkinstorage);
        //                                        _context.SaveChanges();

        //                                        //UPDATE PICKSTORAGE
        //                                        existdata.flag = 1;
        //                                        _context.pickstorage.Update(existdata);
        //                                        _context.SaveChanges();

        //                                        //ADD TO PICKING 
        //                                        _context.Picking_Operation.Add(pickingdata);
        //                                        _context.SaveChanges();
        //                                    }

        //                                    var pgdata = _context.Picklist_Generation
        //                                                .Where(a => a.sono == pickedListData.sono)
        //                                                .GroupBy(a => a.prdcode)
        //                                                .Select(group => new
        //                                                {
        //                                                    ProductCode = group.Key,
        //                                                    SoQty = group.FirstOrDefault().soqty,
        //                                                    PickingQty = group.FirstOrDefault().pickingqty
        //                                                })
        //                                                .ToList();

        //                                    //UPDATE STORAGE , IN CASE OF NOT SCAN BOXES AND PICKSTORAGE HAS THAT BOXES
        //                                    foreach (var groupData in pgdata.Where(h => h.ProductCode.Trim().ToUpper() == enterproductcode.ToUpper()))
        //                                    {

        //                                        int soQty = Convert.ToInt32(groupData.SoQty.Trim());
        //                                        int pickingQty = Convert.ToInt32(groupData.PickingQty.Trim());

        //                                        if (soQty == pickingQty)
        //                                        {
        //                                            var gopickstorage = _context.pickstorage
        //                                                .Where(j => j.sono.Trim() == pickedListData.sono.Trim()
        //                                                    && j.productcode.Trim().ToUpper() == groupData.ProductCode.Trim().ToUpper())
        //                                                .ToList();

        //                                            bool allFlagsAreOne = gopickstorage.All(j => j.flag == 1);

        //                                            if (allFlagsAreOne)
        //                                            {
        //                                                var pickGenerationRecords = _context.Picklist_Generation
        //                                                    .Where(a => a.sono.Trim() == pickedListData.sono.Trim()
        //                                                    && a.prdcode.Trim() == groupData.ProductCode.Trim())
        //                                                    .FirstOrDefault();

        //                                                List<pickstorage> pickstore = new List<pickstorage>();

        //                                                if (pickGenerationRecords != null)
        //                                                {
        //                                                    //UPDATE BOXES IN PICKSTORAE WITH PICKING BOXES
        //                                                    var pickingboxes = _context.Picking_Operation
        //                                                                    .Where(a => a.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper() &&
        //                                                                    a.sono.Trim() == pickedListData.sono.Trim() && a.flag == 0)
        //                                                                    .ToList();
        //                                                    var selectpickstorage = _context.pickstorage
        //                                                                            .Where(a => a.sono.Trim() == pickedListData.sono.Trim() &&
        //                                                                            a.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper() &&
        //                                                                            a.flag == 1)
        //                                                                            .ToList();

        //                                                    foreach (var neww in pickingboxes)
        //                                                    {
        //                                                        var unmatchedBox = selectpickstorage
        //                                                            .Where(a => a.boxno.Trim() != neww.boxno.Trim() &&
        //                                                                        !pickstore.Any(p => p.boxno.Trim() == a.boxno.Trim()))
        //                                                            .FirstOrDefault();

        //                                                        if (unmatchedBox != null)
        //                                                        {
        //                                                            pickstore.Add(unmatchedBox);
        //                                                        }
        //                                                    }

        //                                                    //UPDATE THAT BOXES WHICH IS NOT MATCH UPDATE THEIR FLAG AS 0 
        //                                                    foreach (var tt in pickstore)
        //                                                    {
        //                                                        var checjkk = _context.Storage_Operation
        //                                                            .Where(a => a.productcode.Trim().ToUpper() == tt.productcode.Trim().ToUpper() &&
        //                                                            a.boxno.Trim() == tt.boxno.Trim() && a.batchcode.Trim() == tt.batchcode.Trim() &&
        //                                                            a.locationcode.Trim() == tt.location.Trim())
        //                                                            .FirstOrDefault();

        //                                                        if (checjkk != null)
        //                                                        {
        //                                                            checjkk.pickflag = "0";
        //                                                            _context.Storage_Operation.Update(checjkk);
        //                                                            _context.SaveChanges();
        //                                                        }
        //                                                    }
        //                                                    //END

        //                                                    pickGenerationRecords.flagstatus = 1;
        //                                                    _context.Update(pickGenerationRecords);
        //                                                    _context.SaveChanges();
        //                                                    pickstore.Clear();
        //                                                }
        //                                            }
        //                                        }
        //                                    }

        //                                    //UPDATE PICKLIST
        //                                    foreach (var groupData in pgdata.Where(h => h.ProductCode.Trim().ToUpper() == enterproductcode.ToUpper()))
        //                                    {

        //                                        int soQty = Convert.ToInt32(groupData.SoQty.Trim());
        //                                        int pickingQty = Convert.ToInt32(groupData.PickingQty.Trim());

        //                                        if (soQty == pickingQty)
        //                                        {
        //                                            var gopickstorage = _context.pickstorage
        //                                                .Where(j => j.sono.Trim() == pickedListData.sono.Trim()
        //                                                    && j.productcode.Trim().ToUpper() == groupData.ProductCode.Trim().ToUpper())
        //                                                .ToList();

        //                                            bool allFlagsAreOne = gopickstorage.All(j => j.flag == 1);

        //                                            if (allFlagsAreOne)
        //                                            {
        //                                                var pickGenerationRecords = _context.Picklist_Generation
        //                                                    .Where(a => a.sono.Trim() == pickedListData.sono.Trim()
        //                                                    && a.prdcode.Trim() == groupData.ProductCode.Trim()
        //                                                    && a.flagstatus == 0)
        //                                                    .FirstOrDefault();

        //                                                List<pickstorage> pickstore = new List<pickstorage>();

        //                                                if (pickGenerationRecords != null)
        //                                                {
        //                                                    var pickingboxes = _context.Picking_Operation
        //                                                                        .Where(a => a.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper() &&
        //                                                                        a.sono.Trim() == pickedListData.sono.Trim() &&
        //                                                                        a.flag == 0)
        //                                                                        .ToList();
        //                                                    var selectpickstorage = _context.pickstorage
        //                                                                            .Where(a => a.sono.Trim() == pickedListData.sono.Trim() &&
        //                                                                            a.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper() &&
        //                                                                            a.flag == 1)
        //                                                                            .ToList();

        //                                                    if (pickingboxes.Count == selectpickstorage.Count)
        //                                                    {
        //                                                        pickGenerationRecords.flagstatus = 1;
        //                                                        _context.Picklist_Generation.Update(pickGenerationRecords);
        //                                                        _context.SaveChanges();
        //                                                    }
        //                                                }
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    return Json(new { success = false, message = "Scanned Box " + enterB1 + " of " + enterproductcode + " product is not found in stock !" });
        //                                }
        //                            }
        //                            else
        //                            {
        //                                return Json(new { success = false, message = "You have alredy picked all the shippers of that product " + pickedListData.productcode + " , against order no " + pickedListData.sono.Trim() + " !" });
        //                            }
        //                        }
        //                        else if(existdata!=null && existdata.flag == 1)
        //                        {
        //                            //alredy picked
        //                            return Json(new { success = false, message = "You have alredy picked that shippers of that product " + pickedListData.productcode + " , against order no " + pickedListData.sono.Trim() + " !" });
        //                        }
        //                        else
        //                        {
        //                            return Json(new { success = false, message = "Please scan correct shipper !" });
        //                        }
        //                    }

        //                    // RETURN FALG 1
        //                    return Json(new
        //                    {
        //                        success = true,
        //                        message = "Data successfully processed.",
        //                        location1 = pickedListData.Location,
        //                        boxno1 = pickedListData.BoxNo,
        //                        batch1 = pickedListData.batchno,
        //                        qtypicked = pickedListData.qtypicked,
        //                        balanceqty = pickedListData.balanceqty,
        //                        productcode1 = pickedListData.productcode,
        //                        flag = 0,
        //                    });
        //                }
        //                else
        //                {
        //                    return Json(new { success = false , message = "You have alredy picked all the shippers of that product "+pickedListData.productcode+ " , against order no "+pickedListData.sono.Trim()+" !"}); 
        //                }

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Catch Exception : " + ex.Message });
        //    }
        //}
        public IActionResult SavePickingData([FromBody] PickedListData pickedListData)
        {
            try
            {

                pickedListData.batchno = pickedListData.batchno?.Replace("\u001e", "") ?? "";
                pickedListData.productcode = pickedListData.productcode?.Replace("\u001e", "") ?? "";
                pickedListData.Location = pickedListData.Location?.Replace("\u001e", "") ?? "";
                pickedListData.BoxNo = pickedListData.BoxNo?.Replace("\u001e", "") ?? "";

                int maxId = _context.Picking_Operation.Any() ? _context.Picking_Operation.Max(e => e.pick_id) + 1 : 1;
                List<Picking_Operation> savepolist = new List<Picking_Operation>();
                List<Picking_Operation> loadpicklist = new List<Picking_Operation>();
                int i = 1;

                savepolist = _context.Picking_Operation.Where(a => a.sono == pickedListData.sono).ToList();

                string sono = pickedListData.sono;
                var box = pickedListData.BoxNo.Trim();
                var enterL1 = pickedListData.Location.Trim();
                var enterB1 = pickedListData.BoxNo.Trim();
                var enterbatchno = pickedListData.batchno.Trim();
                var enterproductcode = pickedListData.productcode.Trim().ToUpper();
                var qtypicked = pickedListData.qtypicked;
                var balanceqty = pickedListData.balanceqty;

                if (balanceqty == 0)
                {
                    return Json(new { success = false, message = "You have scanned all the boxes  against order no " + pickedListData.sono.Trim() + " , Balannce scan box count is " + balanceqty + " !" });
                }
                else
                {
                    var splitboxdata = enterB1.Split("-");
                    var getsecandpart = splitboxdata[1];
                    var split_num = getsecandpart.Split("/")[0];
                    var split_den = getsecandpart.Split("/")[1];

                    if (Convert.ToInt32(split_num) > Convert.ToInt64(split_den))
                    {
                        // Fetch pickstore and picking lists based on conditions
                        var savePickStoreList = _context.pickstorage
                            .Where(a => a.sono == pickedListData.sono
                                    && a.productcode.Trim().ToUpper() == pickedListData.productcode.ToUpper().Trim()
                                    && a.batchcode.Trim() == pickedListData.batchno.Trim()
                                    && a.flag == 0)
                            .ToList();

                        var savePickingList = _context.Picking_Operation
                            .Where(a => a.sono == pickedListData.sono
                                    && a.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper()
                                    && a.batchcode.Trim() == pickedListData.batchno.Trim()
                                    && a.flag == 0)
                            .ToList();

                        // Proceed if picking list count is less than or equal to pickstore list count
                        if (savePickStoreList.Count > 0)
                        {
                            var locationData = savePickStoreList
                                .Where(a => a.location.Trim() == pickedListData.Location.Trim()
                                        && a.batchcode.Trim() == pickedListData.batchno.Trim()
                                        && a.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper())
                                .ToList();

                            if (locationData.Any())
                            {
                                //foreach (var item in locationData)
                                //{
                                    var considerBox = "1/1";
                                    var checkInStorage = _context.Storage_Operation
                                        .Where(a => a.locationcode.Trim() == enterL1.Trim()
                                                    && a.productcode.Trim().ToUpper() == enterproductcode.Trim().ToUpper()
                                                    && a.batchcode.Trim() == enterbatchno.Trim()
                                                    && a.statusflag.Trim() == "ST"
                                                    && a.boxno.Trim().EndsWith(considerBox))
                                        .ToList();

                                    var pickCount = _context.pickstorage
                                        .Where(a => a.sono.Trim() == pickedListData.sono.Trim()
                                                    && a.productcode.Trim() == pickedListData.productcode.Trim()
                                                    && a.flag == 0)
                                        .ToList();

                                    int loopLimit = Math.Min(pickCount.Count, Convert.ToInt32(split_num));

                                    if (checkInStorage.Any() && checkInStorage.Count >= loopLimit)
                                    {
                                        foreach (var storageItem in checkInStorage.Take(loopLimit))
                                        {
                                            int maxId1 = _context.Picking_Operation.Any() ? _context.Picking_Operation.Max(e => e.pick_id) + 1 : 1;
                                            // Add to Picking_Operation
                                            var pickingData = new Picking_Operation
                                            {
                                                pick_id = maxId1,
                                                balanceqty = balanceqty,
                                                batchcode = storageItem.batchcode.Trim(),
                                                boxpicked = enterB1,
                                                locationpicked = enterL1,
                                                instockqty = "-",
                                                pickingqty = "-",
                                                productcode = storageItem.productcode.Trim(),
                                                qtypicked = qtypicked,
                                                location = enterL1,
                                                boxno = storageItem.boxno.Trim(),
                                                sono = pickedListData.sono.Trim()
                                            };
                                            loadpicklist.Add(pickingData);

                                            // Update pickstorage and storage operation
                                            var pickstoreData = _context.pickstorage
                                                .FirstOrDefault(a => a.sono.Trim() == pickedListData.sono.Trim()
                                                    && a.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper()
                                                    && a.batchcode.Trim() == pickedListData.batchno.Trim()
                                                    && a.boxno.Trim().EndsWith(considerBox)
                                                    && a.location.Trim() == pickedListData.Location.Trim()
                                                    && a.flag == 0);

                                            if (pickstoreData != null)
                                            {
                                                _context.Picking_Operation.Add(pickingData);
                                                pickstoreData.flag = 1;
                                                _context.pickstorage.Update(pickstoreData);
                                                _context.SaveChanges();

                                                storageItem.statusflag = "PI";
                                                //storageItem.pickflag = "1";
                                                _context.Storage_Operation.Update(storageItem);
                                                _context.SaveChanges();
                                            }
                                        }

                                        // Update Picklist_Generation if all flags are set
                                        var picklistGeneration = _context.Picklist_Generation
                                            .Where(a => a.sono == pickedListData.sono && a.prdcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper())
                                            .GroupBy(a => a.prdcode)
                                            .Select(group => new
                                            {
                                                ProductCode = group.Key,
                                                SoQty = group.FirstOrDefault().soqty,
                                                PickingQty = group.FirstOrDefault().pickingqty
                                            })
                                            .ToList();

                                        foreach (var pgItem in picklistGeneration)
                                        {
                                            if (Convert.ToInt32(pgItem.SoQty.Trim()) == Convert.ToInt32(pgItem.PickingQty.Trim()))
                                            {
                                                var pickStorageList = _context.pickstorage
                                                    .Where(j => j.sono.Trim() == pickedListData.sono.Trim()
                                                        && j.productcode.Trim().ToUpper() == pgItem.ProductCode.Trim().ToUpper())
                                                    .ToList();

                                                bool allFlagsAreOne = pickStorageList.All(j => j.flag == 1);

                                                if (allFlagsAreOne)
                                                {
                                                    var pickGenerationRecord = _context.Picklist_Generation
                                                        .FirstOrDefault(a => a.sono.Trim() == pickedListData.sono.Trim()
                                                            && a.prdcode.Trim() == pgItem.ProductCode.Trim()
                                                            && a.flagstatus == 0);

                                                    if (pickGenerationRecord != null)
                                                    {
                                                        var pickingBoxes = _context.Picking_Operation
                                                            .Where(a => a.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper()
                                                                && a.sono.Trim() == pickedListData.sono.Trim()
                                                                && a.flag == 0)
                                                            .ToList();

                                                        var selectedPickStorage = _context.pickstorage
                                                            .Where(a => a.sono.Trim() == pickedListData.sono.Trim()
                                                                && a.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper()
                                                                && a.flag == 1)
                                                            .ToList();

                                                        //UPDATE UNMATCH BOXES
                                                        List<pickstorage> unmatchedBoxes = selectedPickStorage
                                                            .Where(pb => !pickingBoxes.Any(ps => ps.boxno.Trim() == pb.boxno.Trim()))
                                                            .ToList();

                                                            foreach (var tt in unmatchedBoxes)
                                                            {
                                                                var checjkk = _context.Storage_Operation
                                                                    .Where(a => a.productcode.Trim().ToUpper() == tt.productcode.Trim().ToUpper() 
                                                                    && a.boxno.Trim() == tt.boxno.Trim() 
                                                                    && a.batchcode.Trim() == tt.batchcode.Trim() )
                                                                    .FirstOrDefault();

                                                                if (checjkk != null)
                                                                {
                                                                    checjkk.pickflag = "0";
                                                                    _context.Storage_Operation.Update(checjkk);
                                                                    _context.SaveChanges();
                                                                }
                                                            }
                                                            //END
                                                            if (pickingBoxes.Count == selectedPickStorage.Count)
                                                            {
                                                                pickGenerationRecord.flagstatus = 1;
                                                                _context.Picklist_Generation.Update(pickGenerationRecord);
                                                                _context.SaveChanges();
                                                            }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        return Json(new { success = false, message = $"Scanned Box {enterB1} of {enterproductcode} product not found in storage!" });
                                    }
                                //}
                            }
                        }
                        else
                        {
                            return Json(new { success = false, message = $"Already scanned the picked product quantities of {pickedListData.productcode.Trim()}!" });
                        }

                        // Return flag and counts
                        var pickedQty = _context.pickstorage.Where(a => a.sono.Trim() == pickedListData.sono.Trim() && a.flag == 1).Count();
                        var balanceQty = _context.pickstorage.Where(a => a.sono.Trim() == pickedListData.sono.Trim() && a.flag == 0).Count();

                        return Json(new
                        {
                            success = true,
                            message = "Data successfully processed.",
                            data = loadpicklist,
                            qtypicked = pickedQty,
                            balanceqty = balanceQty,
                            flag = 1
                        });

                    }
                    else
                    {
                        //CHECK ALREDY SCAN OR NOT
                        var checkpicking = _context.Picking_Operation
                                        .Where(a => a.sono.Trim() == pickedListData.sono.Trim() 
                                        && a.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper()
                                        && a.boxno.Trim() == pickedListData.BoxNo.Trim() 
                                        && a.batchcode.Trim() == pickedListData.batchno.Trim() 
                                        &&  a.flag == 0)
                                        .FirstOrDefault();
                        if (checkpicking != null)
                        {
                            return Json(new { success = false, message = "Already scanned !" });
                        }
                        //END

                        //ADDED
                        var secondPartEnterB1 = pickedListData.BoxNo.Split('-')[1];
                        var check = _context.pickstorage
                            .Where(a => a.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper()
                            && a.batchcode.Trim() == pickedListData.batchno.Trim()
                            && a.sono.Trim() == pickedListData.sono.Trim()
                            && a.boxno.Trim().Contains(secondPartEnterB1))
                            .ToList();
                        if (check.Count > 0)
                        {
                            var gett = check.Where(a=>a.flag == 0).ToList();
                            if(gett.Count == 0)
                            {
                                return Json(new { success = false, message = "You have already scanned all shipper's "+pickedListData.productcode+" of product against that "+pickedListData.sono+" order no  !" });
                            }
                            else
                            {
                                var getone = gett.Where(a => a.location.Trim() == pickedListData.Location.Trim()).FirstOrDefault();
                                    if (getone!=null)
                                    {
                                        //UPDATE STORAGE
                                        var storedata = _context.Storage_Operation
                                        .Where(a => a.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper()
                                        && a.boxno.Trim() == pickedListData.BoxNo.Trim()
                                        && a.batchcode.Trim() == pickedListData.batchno.Trim()
                                        && a.locationcode.Trim() == pickedListData.Location.Trim()
                                        && a.statusflag.Trim() == "ST").FirstOrDefault();
                                        if (storedata != null)
                                        {
                                            //storedata.pickflag = "1";
                                            storedata.statusflag = "PI";
                                            _context.Storage_Operation.Update(storedata);
                                            _context.SaveChanges();

                                            //PROCCEDD
                                            pickedListData.qtypicked = (pickedListData.qtypicked + i);
                                            qtypicked = pickedListData.qtypicked;

                                            pickedListData.balanceqty = (pickedListData.balanceqty - i);
                                            balanceqty = pickedListData.balanceqty;

                                            //ADD TO PICKING
                                            var pickingdata = new Picking_Operation
                                            {
                                                pick_id = maxId,
                                                balanceqty = balanceqty,
                                                batchcode = pickedListData.batchno.Trim(),
                                                boxpicked = pickedListData.BoxNo.Trim(),
                                                locationpicked = pickedListData.Location.Trim(),
                                                instockqty = "-",
                                                pickingqty = "-",
                                                productcode = pickedListData.productcode.Trim().ToUpper(),
                                                qtypicked = qtypicked,
                                                location = pickedListData.Location.Trim(),
                                                boxno = pickedListData.BoxNo.Trim(),
                                                sono = pickedListData.sono.Trim(),
                                            };
                                            _context.Picking_Operation.Add(pickingdata);
                                            _context.SaveChanges();

                                            //UPDATE PICKSTORAGE
                                            getone.flag = 1;
                                            _context.pickstorage.Update(getone);
                                            _context.SaveChanges();

                                            var checkpickstore = _context.pickstorage
                                                    .Where(a => a.sono.Trim() == pickedListData.sono.Trim()
                                                    && a.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper()
                                                    && a.flag == 1)
                                                    .ToList();

                                            var checkpickingop = _context.Picking_Operation
                                                    .Where(a => a.sono.Trim() == pickedListData.sono.Trim()
                                                    && a.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper()
                                                    && a.flag == 0)
                                                    .ToList();

                                            if (checkpickstore.Count == checkpickingop.Count)
                                            {
                                                var pgdata = _context.Picklist_Generation
                                                                .Where(a => a.sono == pickedListData.sono)
                                                                .GroupBy(a => a.prdcode)
                                                                .Select(group => new
                                                                {
                                                                    ProductCode = group.Key,
                                                                    SoQty = group.FirstOrDefault().soqty,
                                                                    PickingQty = group.FirstOrDefault().pickingqty
                                                                })
                                                                .ToList();

                                                //UPDATE STORAGE , IN CASE OF NOT SCAN BOXES AND PICKSTORAGE HAS THAT BOXES
                                                foreach (var groupData in pgdata.Where(h => h.ProductCode.Trim().ToUpper() == enterproductcode.ToUpper()))
                                                {

                                                    int soQty = Convert.ToInt32(groupData.SoQty.Trim());
                                                    int pickingQty = Convert.ToInt32(groupData.PickingQty.Trim());

                                                    if (soQty == pickingQty)
                                                    {
                                                        var gopickstorage = _context.pickstorage
                                                            .Where(j => j.sono.Trim() == pickedListData.sono.Trim()
                                                                && j.productcode.Trim().ToUpper() == groupData.ProductCode.Trim().ToUpper())
                                                            .ToList();

                                                        bool allFlagsAreOne = gopickstorage.All(j => j.flag == 1);

                                                        if (allFlagsAreOne)
                                                        {
                                                            var pickGenerationRecords = _context.Picklist_Generation
                                                                .Where(a => a.sono.Trim() == pickedListData.sono.Trim()
                                                                && a.prdcode.Trim() == groupData.ProductCode.Trim())
                                                                .FirstOrDefault();

                                                            List<pickstorage> pickstore = new List<pickstorage>();

                                                            if (pickGenerationRecords != null)
                                                            {
                                                                //UPDATE BOXES IN PICKSTORAE WITH PICKING BOXES
                                                                var pickingboxes = _context.Picking_Operation
                                                                                    .Where(a => a.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper() &&
                                                                                    a.sono.Trim() == pickedListData.sono.Trim() && a.flag == 0)
                                                                                    .ToList();

                                                                var selectpickstorage = _context.pickstorage
                                                                                        .Where(a => a.sono.Trim() == pickedListData.sono.Trim() &&
                                                                                        a.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper() &&
                                                                                        a.flag == 1)
                                                                                        .ToList();

                                                                foreach (var neww in pickingboxes)
                                                                {
                                                                    var unmatchedBox = selectpickstorage
                                                                        .Where(a => a.boxno.Trim() != neww.boxno.Trim() &&
                                                                                    !pickstore.Any(p => p.boxno.Trim() == a.boxno.Trim()))
                                                                        .FirstOrDefault();

                                                                    if (unmatchedBox != null)
                                                                    {
                                                                        pickstore.Add(unmatchedBox);
                                                                    }
                                                                }

                                                                //UPDATE THAT BOXES WHICH IS NOT MATCH UPDATE THEIR FLAG AS 0 
                                                                foreach (var tt in pickstore)
                                                                {
                                                                    var checjkk = _context.Storage_Operation
                                                                        .Where(a => a.productcode.Trim().ToUpper() == tt.productcode.Trim().ToUpper() &&
                                                                        a.boxno.Trim() == tt.boxno.Trim() && a.batchcode.Trim() == tt.batchcode.Trim() &&
                                                                        a.locationcode.Trim() == tt.location.Trim())
                                                                        .FirstOrDefault();

                                                                    if (checjkk != null)
                                                                    {
                                                                        checjkk.pickflag = "0";
                                                                        _context.Storage_Operation.Update(checjkk);
                                                                        _context.SaveChanges();
                                                                    }
                                                                }
                                                                //END

                                                                pickGenerationRecords.flagstatus = 1;
                                                                _context.Picklist_Generation.Update(pickGenerationRecords);
                                                                _context.SaveChanges();
                                                                pickstore.Clear();
                                                            }
                                                        }
                                                    }
                                                }
                                            };
                                        }
                                        else
                                        {
                                                var storedata11 = _context.Storage_Operation
                                                          .Where(a => a.productcode.Trim().ToUpper() == pickedListData.productcode.Trim().ToUpper()
                                                          && a.boxno.Trim() == pickedListData.BoxNo.Trim()
                                                          && a.batchcode.Trim() == pickedListData.batchno.Trim()
                                                          && a.locationcode.Trim() == pickedListData.Location.Trim()
                                                          && a.statusflag.Trim() == "DMG").FirstOrDefault();

                                                if (storedata11 != null)
                                                {
                                                    return Json(new { success = false, message = "Scanned product is found Damage in stock !" });
                                                }
                                                else
                                                {
                                                    return Json(new { success = false, message = "Stock is not found in storage !" });
                                                }
                                        }
                                    }
                                    else
                                    {
                                        return Json(new { success = false, message = "Please enter correct location of that shipper !!" });
                                    };

                                
                            };
                            
                        }
                        else
                        {
                            return Json(new { success = false, message = "There is no shipper found for picking !" });
                        };
                        // RETURN FALG 1
                        return Json(new
                        {
                            success = true,
                            message = "Data successfully processed.",
                            location1 = pickedListData.Location,
                            boxno1 = pickedListData.BoxNo,
                            batch1 = pickedListData.batchno,
                            qtypicked = pickedListData.qtypicked,
                            balanceqty = pickedListData.balanceqty,
                            productcode1 = pickedListData.productcode.ToUpper(),
                            flag = 0,
                        });
                        //ENDED
                    };
                };
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Catch Exception : " + ex.Message });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Picking_Operation Picking_Operation)
        {

            int maxId = _context.Picking_Operation.Any() ? _context.Picking_Operation.Max(e => e.pick_id) + 1 : 1;
            Picking_Operation.pick_id = maxId;
            List<Picking_Operation> packetsToRemove = new List<Picking_Operation>();
            _context.Add(Picking_Operation);
            await _context.SaveChangesAsync();
            _notyfService.Success("Picking Opration Succesfully completed !");
            return RedirectToAction(nameof(Index));
            return View(Picking_Operation);
        }

        // GET: Picking_Process/Edit/5
        public async Task<IActionResult> Edit(string sono, int? Id)
        {
            ViewBag.data11 = GetWbridgeList1(sono); // Pass sono as the selected value
            Picking_Operation applicant = new Picking_Operation();
            List<Picking_Operation> picklistDataList = new List<Picking_Operation>();

            var pglist = _context.Picklist_Generation
                .Where(a => a.sono == sono)
                .ToList();

            foreach (var pg in pglist)
            {
                var pdlist = _context.pickstorage.Where(a => a.genid == pg.gen_id && a.flag == 0)
                    .OrderBy(a => a.location) // Order by location
                    .ToList();

                var distinctLocationsAndBoxNos = pdlist
                    .GroupBy(a => new { a.location, BoxNoPart = a.boxno.Split('-')[1], a.batchcode, a.productcode })
                    .Select(group => new
                    {
                        productcode = group.Key.productcode,
                        batchcode = group.Key.batchcode,
                        Location = group.Key.location,
                        BoxNo = group.Key.BoxNoPart,
                        Count = group.Count()
                    });

                foreach (var item in distinctLocationsAndBoxNos)
                {
                    var data = new Picking_Operation
                    {
                        productcode = item.productcode,
                        location = item.Location,
                        batchcode = item.batchcode,
                        boxno = item.BoxNo,
                        instockqty = item.Count.ToString()
                    };
                    picklistDataList.Add(data);
                }
            }

            var exist = _context.Picking_Operation.Where(a => a.sono == sono).ToList();
            var data1 = picklistDataList
                .Where(p => !exist.Any(e => e.location == p.location
                                           && e.boxno == p.boxno
                                           && e.batchcode == p.batchcode
                                           && e.productcode.ToUpper() == p.productcode.ToUpper()))
                .ToList();

            int data3 = data1.Sum(a => Convert.ToInt32(a.instockqty));
            int data4 = exist.Count();
            ViewBag.data1 = data1;
            ViewBag.data2 = exist;
            ViewBag.data3 = data3;
            ViewBag.data4 = data4;
            ViewBag.sono = sono;

            var result = new
            {
                data1 = data1,
                data2 = exist,
                data3 = data3,
                data4 = data4,
            };

            return View();
        }
        //private static List<Picking_Operation> pickingopration_list = new List<Picking_Operation>();

        public List<SelectListItem> GetWbridgeList1(string selectedSono = "")
        {
            List<SelectListItem> wbridgeList = _context.pickstorage
                .Where(n => _context.pickstorage.Any(p => p.sono == n.sono && n.flag == 0))
                .AsNoTracking()
                .OrderBy(n => n.sono)
                .Select(n => new SelectListItem
                {
                    Value = n.sono,
                    Text = n.sono.ToString(),
                    Selected = n.sono == selectedSono // Set selected based on the provided value
                })
                .Distinct()
                .ToList();

            var defaultItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select SONO----"
            };

            wbridgeList.Insert(0, defaultItem);
            return wbridgeList;
        }

        //[HttpPost]

        public string ConvertToStandardDate(string customDate)
        {
            DateTime dateTime = DateTime.Now;

            if (string.IsNullOrEmpty(customDate) || customDate.Length < 5)
            {
                // Handle invalid input, return a default date, throw an exception, etc.
                throw new ArgumentException("Invalid custom date format");
            }

            // Extract year and month components
            string yearPart = customDate.Substring(0, 2);
            string monthPart = customDate.Substring(2, 1).ToUpper();
            string date = customDate.Substring(3, 2);
            // Map alphabetic month to numeric month
            int numericMonth = monthPart switch
            {
                "A" => 1,
                "B" => 2,
                "C" => 3,
                "D" => 4,
                "E" => 5,
                "F" => 6,
                "G" => 7,
                "H" => 8,
                "I" => 9,
                "J" => 10,
                "K" => 11,
                "L" => 12,
                _ => throw new ArgumentException("Invalid month character"),
            };

            // Combine and parse to a DateTime object
            string standardDate = $"{yearPart}-{numericMonth:D2}-01";
            return DateTime.Parse(standardDate).ToString("dd-MM-yy");

            //return dateTime;


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,rackname,shelves,bin")] Picking_Operation Picking_Operation)
        {
            if (id != Picking_Operation.pick_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(Picking_Operation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!rack_masterExists(Picking_Operation.pick_id))
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
            return View(Picking_Operation);
        }

        // GET: Picking_Process/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            //if (id == null || _context.Picking_Operation == null)
            //{
            //    return NotFound();
            //}

            //var Picking_Operation = await _context.Picking_Operation
            //    .FirstOrDefaultAsync(m => m.pick_id == id);
            //if (Picking_Operation == null)
            //{
            //    return NotFound();
            //}

            //return View(Picking_Operation);
            var getdata = _context.Picking_Operation.Where(a => a.pick_id == id).FirstOrDefault();
            if (getdata != null)
            {
                var sono = getdata.sono;
                var location = getdata.locationpicked;
                var boxno = getdata.boxno;
                var boxnosplit = getdata.boxpicked.Split('-');
                var box = boxnosplit[1];
                var batchcode = getdata.batchcode;
                var productcode = getdata.productcode.ToUpper();

                var pickstorage = _context.pickstorage
                    .Where(a => a.sono.Trim() == sono.Trim()
                             && a.productcode.Trim().ToUpper() == productcode.Trim().ToUpper()
                             && a.location.Trim() == location.Trim()
                             && a.batchcode.Trim() == batchcode.Trim()
                             && a.boxno.Trim() != null
                             && a.boxno.Trim().Contains('-'))
                    .FirstOrDefault();

                if (pickstorage != null)
                {
                    var boxParts = pickstorage.boxno.Split('-');

                    if (boxParts.Length > 1 && boxParts[1] == box)
                    {
                        pickstorage.flag = 0;
                        _context.pickstorage.Update(pickstorage);
                        _context.SaveChanges();
                    }
                    else
                    {
                        return Problem("Data not found in Pickstorage.");
                    }
                }
                var storagedata = _context.Storage_Operation
                    .Where(a => a.locationcode.Trim() == location.Trim()
                             && a.batchcode.Trim() == batchcode.Trim()
                             && a.boxno.Trim() == box.Trim()
                             && a.productcode.Trim().ToUpper() == productcode.Trim().ToUpper())
                    .FirstOrDefault();

                if (storagedata != null)
                {
                    storagedata.statusflag = "ST"; // Assuming statusflag is a string, use double quotes
                    _context.Storage_Operation.Update(storagedata);
                    _context.SaveChanges();
                }
                else
                {
                    return Problem("Data not found in stoarge.");
                }
            }
            if (_context.Picking_Operation == null)
            {
                return Problem("Entity set 'ErosDbContext.Picking_Operation' is null.");
            }
            var Picking_Operation = await _context.Picking_Operation.FindAsync(id);
            if (Picking_Operation != null)
            {
                _context.Picking_Operation.Remove(Picking_Operation);
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // POST:  Picking_Operation/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var getdata = _context.Picking_Operation.Where(a => a.pick_id == id).FirstOrDefault();
            if (getdata != null)
            {
                var sono = getdata.sono;
                var location = getdata.locationpicked;
                var boxnosplit = getdata.boxpicked.Split();
                var box = boxnosplit.Length > 1 ? boxnosplit[1] : null;
                var batchcode = getdata.batchcode;
                var productcode = getdata.productcode.ToUpper();

                var pickstorage = _context.pickstorage
                    .Where(a => a.sono == sono
                             && a.productcode.ToUpper() == productcode.ToUpper()
                             && a.location == location
                             && a.batchcode == batchcode
                             && a.boxno != null
                             && a.boxno.Contains('-'))
                    .FirstOrDefault();

                if (pickstorage != null)
                {
                    var boxParts = pickstorage.boxno.Split('-');

                    if (boxParts.Length > 1 && boxParts[1] == box)
                    {
                        pickstorage.flag = 0;
                        _context.pickstorage.Update(pickstorage);
                        _context.SaveChanges();
                    }
                    else
                    {
                        return Problem("Data not found in Pickstorage.");
                    }
                }
                var storagedata = _context.Storage_Operation
                    .Where(a => a.locationcode == location
                             && a.batchcode == batchcode
                             && a.boxno == box
                             && a.productcode.ToUpper() == productcode.ToUpper())
                    .FirstOrDefault();

                if (storagedata != null)
                {
                    storagedata.statusflag = "ST"; // Assuming statusflag is a string, use double quotes
                    _context.Storage_Operation.Update(storagedata);
                    _context.SaveChanges();
                }
                else
                {
                    return Problem("Data not found in stoarge.");
                }
            }
            if (_context.Picking_Operation == null)
            {
                return Problem("Entity set 'ErosDbContext.Picking_Operation' is null.");
            }
            var Picking_Operation = await _context.Picking_Operation.FindAsync(id);
            if (Picking_Operation != null)
            {
                _context.Picking_Operation.Remove(Picking_Operation);
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        private bool rack_masterExists(int id)
        {
            return (_context.Picking_Operation?.Any(e => e.pick_id == id)).GetValueOrDefault();
        }

        [HttpPost]
        public IActionResult Getlist1(string optionValue)
        {
            var exist = _context.Picking_Operation.Where(a => a.sono.Trim() == optionValue.Trim()).ToList();

            List<Picking_Operation> picklistDataList = new List<Picking_Operation>();

            var pglist = _context.Picklist_Generation
                .Where(a => a.sono.Trim() == optionValue.Trim())
                .ToList();

            foreach (var pg in pglist)
            {
                var pdlist = _context.pickstorage.Where(a => a.genid == pg.gen_id && a.flag == 0)
                    .OrderBy(a => a.location.Trim()) // Order by location
                    .ToList();

                var distinctLocationsAndBoxNos = pdlist
                   .GroupBy(a => new { a.location, BoxNoPart = a.boxno.Split('-')[1], a.batchcode, a.productcode })
                   .Select(group => new
                   {
                       productcode = group.Key.productcode.ToUpper(),
                       batchcode = group.Key.batchcode,
                       Location = group.Key.location,
                       BoxNo = group.Key.BoxNoPart,
                       Count = group.Count()
                   });
                foreach (var item in distinctLocationsAndBoxNos)
                {
                    var data = new Picking_Operation
                    {
                        productcode = item.productcode.ToUpper(), // You need to decide how to set this value since it's not present in your current 
                        location = item.Location,
                        batchcode = item.batchcode,
                        boxno = item.BoxNo,
                        instockqty = item.Count.ToString()
                    };
                    picklistDataList.Add(data);
                }
            }

            int data3 = exist.Count() > 0 ? picklistDataList.Sum(a => Convert.ToInt32(a.instockqty)) - exist.Count() : picklistDataList.Sum(a => Convert.ToInt32(a.instockqty));
            //int data3 = picklistDataList.Count();
            int data4 = exist.Count();
            var result = new
            {
                data1 = picklistDataList,
                data3 = data3,
                data4 = data4,
            };
            return Ok(result);
        }


        private string GetSecondDigit(string boxno)
        {
            string[] parts = boxno.Split('-');
            return parts.Length == 2 ? parts[1].Trim() : string.Empty;
        }

        [HttpGet]
        public ActionResult GetPick(string selectedValue, string selectedValue1)
        {
            var table = _context.Storage_Operation.Where(x => x.locationcode == selectedValue1 && x.boxno == selectedValue).FirstOrDefault();
            if (table != null)
            {
                var result = new
                {
                    boxno1 = table.boxno,
                    location1 = table.locationcode,
                    batch1 = table.batchcode,
                    //boxno1 = table.boxno,
                    // meters1 = roll.meters
                };
                return Json(result);
            }
            else
            {
                // Handle the case where one or both records are not found
                return Json(new { error = "One or more records not found" });
            }
        }


        //public ActionResult getpick(List<Picking_Operation> picking)
        //{
        //    if (picking != null)
        //    {
        //        foreach (Picking_Operation customer in picking)
        //        {
        //            int maxId = _context.Picking_Operation.Any() ? _context.Picking_Operation.Max(e => e.pick_id) + 1 : 1;
        //            //do_allotment.id = maxId;
        //            var doallot = new Picking_Operation();
        //            doallot.pick_id = maxId;
        //            doallot.boxno = customer.boxno;
        //            doallot.batchcode = customer.batchcode;
        //            doallot.location = customer.location;

        //            _context.Add(doallot);
        //            _context.SaveChanges();
        //        }
        //    }

        //    return RedirectToAction(nameof(Index));
        //}
        //[HttpPost]
        //public IActionResult SaveTableData(string tableData)
        //{
        //    try
        //    {
        //        List<Picking_Operation> data = JsonConvert.DeserializeObject<List<Picking_Operation>>(tableData);

        //        foreach (var item in data)
        //        {
        //            item.qtypicked = "0";
        //            item.balanceqty = "-";
        //            item.field3 = "-";
        //            item.field4 = "-";
        //            item.field5 = "-";
        //            item.productcode = "-";
        //            item.productname = "-";
        //            item.soqty = "-";
        //            item.pickingqty = "-";
        //            item.batchwiseqty = "-";
        //            item.instockqty = "-";

        //            item.sono = "-";

        //            item.date = DateTime.Now.ToString("dd/MM/yyyy");
        //            item.month = DateTime.Now.ToString("MM");
        //            item.year = DateTime.Now.ToString("yy");
        //            item.time = DateTime.Now.ToString("hh:ss:tt");

        //            // Add the data to the context
        //            _context.Picking_Operation.Add(item);
        //        }

        //        // Save changes to the database
        //        _context.SaveChanges();

        //        return Json(new { success = true });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception or handle it appropriately
        //        // For demonstration purposes, I'm just returning an error response

        //        return Json(new { success = false, error = ex.Message });
        //    }
        //}
        [HttpPost]
        public IActionResult SaveTableData(string tableData)
        {
            //try
            //{
            //    // Deserialize the first parameter (tableData)
            //    List<Picking_Operation> data = JsonConvert.DeserializeObject<List<Picking_Operation>>(tableData);

            //    foreach (var item in data)
            //    {
            //        // Process the data from the first table as needed
            //        // You can update properties of item or add to context, depending on your requirements
            //        //item.qtypicked = "0";
            //        item.balanceqty = "-";
            //        //item.field3 = "-";
            //        //item.field4 = "-";
            //        //item.field5 = "-";
            //        item.productcode = "-";
            //        item.productname = "-";
            //        item.soqty = "-";
            //        item.pickingqty = item.pickingqty;
            //        item.batchwiseqty = "-";
            //        item.instockqty = "-";
            //        //item.batchcode = "-";
            //        item.sono = "-";

            //        item.date = DateTime.Now.ToString("dd/MM/yyyy");
            //        item.month = DateTime.Now.ToString("MM");
            //        item.year = DateTime.Now.ToString("yy");
            //        item.time = DateTime.Now.ToString("hh:ss:tt");

            //        // Add the data to the context
            //        _context.Picking_Operation.Add(item);
            //    }

            //    // Deserialize the second parameter (table1Data)


            //    // Save changes to the database
            //    _context.SaveChanges();

            return Json(new { success = true });
            //}
            //catch (Exception ex)
            //{
            //    // Log the exception or handle it appropriately
            //    // For demonstration purposes, I'm just returning an error response
            //    return Json(new { success = false, error = ex.Message });
            //}
        }


        //public ActionResult setpick(string selectedValue1, string selectedValue2)
        //{

        //        var getroll = _context.Storage_Operation.Where(x => x.boxno == selectedValue2).FirstOrDefault();
        //        if (getroll != null)
        //        {
        //            getroll.locationcode = selectedValue1;

        //            getroll.statusflag = "PI";
        //            //getroll.checkflag = "1";
        //            _context.Add(getroll);
        //            _context.SaveChanges();

        //            return Json("Success");
        //        }
        //        else
        //        {
        //            return Json("error");
        //        }


        //}
        //[HttpGet]
        //public ActionResult getbal(string selectedValue1)
        //{
        //    // Assuming _context is your Entity Framework DbContext
        //    var loc = _context.Storage_Operation.FirstOrDefault(n => n.balanceqty == selectedValue1);


        //    return Json(loc);
        //}


        //public IActionResult SavePickingData([FromBody] PickedListData pickedListData)
        //{
        //    try
        //    {
        //        string sono = pickedListData.sono;
        //        var box = pickedListData.BoxNo.Trim();
        //        int i = 1;
        //        var enterL1 = pickedListData.Location.Trim();
        //        var enterB1 = pickedListData.BoxNo.Trim();
        //        var enterbatchno = pickedListData.batchno.Trim();   
        //        var enterproductcode = pickedListData.productcode.Trim();   
        //        var qtypicked = pickedListData.qtypicked;
        //        var balanceqty = pickedListData.balanceqty;

        //        if (balanceqty == 0)
        //        {
        //            //_context.Picking_Operation.AddRange(savepolist);
        //            //_context.SaveChanges();
        //            //savepolist.Clear();
        //            return Ok();
        //        }
        //        else
        //        {
        //            var secondPartEnterB1 = enterB1.Split('-')[1];
        //            foreach (var item in pickedListData.dataList
        //                .Where(a => a.Location.Trim() == enterL1.Trim() && a.BoxNo.Trim() == secondPartEnterB1 && a.batchcode.Trim() == enterbatchno))
        //            {
        //                // Check if enterB1 and enterL1 already exist in savepolist
        //                if (savepolist.Any(a => a.boxno.Trim() == enterB1 && a.location.Trim() == enterL1 && a.batchcode.Trim() == enterbatchno && a.productcode == item.prdcode && a.sono.Trim() == pickedListData.sono))
        //                {
        //                    return Json(new { success = false, message = "Already Scanned!" });
        //                }
        //                // Check how many items in savepolist meet the criteria
        //                int exist = savepolist
        //                    .Count(a => a.location.Trim() == pickedListData.Location.Trim() &&
        //                     a.productcode.Trim() == item.prdcode.Trim() &&
        //                     a.boxno.Trim() == secondPartEnterB1 &&
        //                     a.sono.Trim() == pickedListData.sono.Trim() &&
        //                     a.batchcode.Trim() == pickedListData.batchno.Trim());

        //                if (exist < item.instockqty)
        //                {
        //                    //if (item.BoxNo.EndsWith(SecandDigit))
        //                    if (item.BoxNo.Contains(secondPartEnterB1))
        //                    {
        //                        var batchcode = ConvertToStandardDate(item.batchcode.Trim());

        //                        var checkinstorage = _context.Storage_Operation
        //                        .Where(a => a.locationcode.Trim() == enterL1.Trim() &&
        //                                    a.productcode.Trim() == item.prdcode.Trim() &&
        //                                    //a.batchcode.Trim() == item.batchcode.Trim() &&
        //                                    a.batchcode.Trim() == enterbatchno.Trim() &&
        //                                    a.statusflag.Trim() == "ST" &&
        //                                    a.boxno.Trim() == enterB1.Trim())
        //                        //.OrderBy(a => a.batchcode) // Order by batchcode
        //                        .ToList();


        //                        if (checkinstorage.Count > 0)
        //                        {
        //                            pickedListData.qtypicked = (pickedListData.qtypicked + i);
        //                            qtypicked = pickedListData.qtypicked;

        //                            pickedListData.balanceqty = (pickedListData.balanceqty - i);
        //                            balanceqty = pickedListData.balanceqty;

        //                            var pickingdata = new Picking_Operation
        //                            {
        //                                balanceqty = balanceqty,
        //                                batchcode = item.batchcode,
        //                                boxpicked = enterB1,
        //                                locationpicked = enterL1,
        //                                instockqty = item.instockqty.ToString(),
        //                                pickingqty = "-",
        //                                productcode = item.prdcode,
        //                                //productname = item.prdname,
        //                                qtypicked = qtypicked,
        //                                location = enterL1,
        //                                boxno = enterB1,
        //                               // date = DateTime.Now.ToUniversalTime(),
        //                                sono = pickedListData.sono,
        //                            };


        //                            var recordToRemove = checkinstorage.Where(a => a.boxno.Trim() == box ).FirstOrDefault();
        //                            savepolist.Add(pickingdata);
        //                            if (balanceqty == 0)
        //                            {
        //                                // Assuming the count of matching records is stored in the variable 'matchingRecordsCount'
        //                                var storedata = _context.Storage_Operation;
        //                                int matchingRecordsCount = 0;

        //                                foreach (var storageRecord in storedata)
        //                                {
        //                                    if (savepolist.Any(p =>
        //                                        p.location.Trim() == storageRecord.locationcode.Trim() &&
        //                                        p.boxno.Trim() == storageRecord.boxno.Trim() &&
        //                                        p.batchcode.Trim() == storageRecord.batchcode.Trim() &&
        //                                        p.productcode.Trim() == storageRecord.productcode.Trim()))
        //                                    {
        //                                        storageRecord.statusflag = "PI";
        //                                        matchingRecordsCount++;
        //                                    }
        //                                }


        //                                foreach (var item1 in savepolist)
        //                                {
        //                                    var loc = item1.boxpicked.Trim().Split('-');
        //                                    var existdata = _context.pickstorage
        //                                        .Where(a => a.sono.Trim() == item1.sono.Trim() && a.location.Trim() == item1.locationpicked.Trim() && a.boxno.Trim().Contains(loc[1]) && a.batchcode.Trim() == item1.batchcode.Trim() && a.productcode.Trim() == item1.productcode.Trim()).FirstOrDefault();
        //                                    if (existdata != null)
        //                                    {
        //                                        existdata.flag = 1;
        //                                    }
        //                                    _context.Update(existdata);
        //                                    _context.SaveChanges();
        //                                }

        //                                //var pickgendata = _context.Picklist_Generation.Where(a => a.sono == pickedListData.sono).ToList();
        //                                //if (pickgendata.Count > 0)
        //                                //{
        //                                //    foreach (var aa in pickgendata)
        //                                //    {
        //                                //        var pickstoredata = _context.pickstorage.Where(a => a.genid == aa.gen_id && a.flag == 1).ToList();
        //                                //        foreach (var bb in pickstoredata)
        //                                //        {

        //                                //        }
        //                                //    }
        //                                //}
        //                                if (matchingRecordsCount == savepolist.Count())// && matchingRecordPick == savepolist.Count)
        //                                {
        //                                    ////update flag in pickstore
        //                                    //_context.SaveChanges();
        //                                    //// Update status flag in Storage_Operation
        //                                    //_context.SaveChanges();

        //                                    // Add records to Picking_Operation model
        //                                    _context.Picking_Operation.AddRange(savepolist);
        //                                    // Update status flag in picking opration
        //                                    _context.SaveChanges();
        //                                    // Clear the savepolist after saving to the database
        //                                    savepolist.Clear();
        //                                }
        //                            }
        //                            return Json(new
        //                            {
        //                                success = true,
        //                                message = "Data successfully processed.",
        //                                location1 = pickedListData.Location,
        //                                boxno1 = pickedListData.BoxNo,
        //                                batch1 = item.batchcode,
        //                                qtypicked = pickedListData.qtypicked,
        //                                balanceqty = pickedListData.balanceqty,
        //                                productcode1 = pickedListData.productcode,
        //                            });
        //                        }
        //                        else
        //                        {
        //                            return Json(new { success = false, message = "Any of BoxNo or Location Not Found in storage!" });
        //                        }
        //                    }
        //                    else
        //                    {
        //                        return Json(new { success = false, message = "Invalid Box No format!" });
        //                    }
        //                }
        //                else
        //                {
        //                    return Json(new { success = false, message = "Something Wrong ! " });
        //                }
        //            }
        //        }

        //        return Json(new { success = false, message = "Any of BoxNo, Location, Batchcode Not match !" });//done
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "An error occurred: " + ex.Message });
        //    }

        //}

        public IActionResult Getlist(string optionValue)
        {
            var exist = _context.Picking_Operation.Where(a => a.sono.Trim() == optionValue.Trim()).ToList();

            List<Picking_Operation> picklistDataList = new List<Picking_Operation>();

            var pglist = _context.Picklist_Generation
                .Where(a => a.sono.Trim() == optionValue.Trim())
                .ToList();

            foreach (var pg in pglist)
            {
                var pdlist = _context.pickstorage.Where(a => a.genid == pg.gen_id)
                    .OrderBy(a => a.location.Trim()) // Order by location
                    .ToList();

                var distinctLocationsAndBoxNos = pdlist
                   .GroupBy(a => new { a.location, BoxNoPart = a.boxno.Split('-')[1], a.batchcode, a.productcode })
                   .Select(group => new
                   {
                       productcode = group.Key.productcode.ToUpper(),
                       //productname = item.productname,
                       batchcode = group.Key.batchcode,
                       Location = group.Key.location,
                       BoxNo = group.Key.BoxNoPart,
                       Count = group.Count()
                   });
                foreach (var item in distinctLocationsAndBoxNos)
                {
                    var data = new Picking_Operation
                    {
                        productcode = item.productcode.ToUpper(), // You need to decide how to set this value since it's not present in your current 
                        location = item.Location,
                        batchcode = item.batchcode,
                        boxno = item.BoxNo,
                        instockqty = item.Count.ToString()
                    };
                    picklistDataList.Add(data);
                }
            }
            int data3 = exist.Count() > 0 ? picklistDataList.Sum(a => Convert.ToInt32(a.instockqty)) - exist.Count() : picklistDataList.Sum(a => Convert.ToInt32(a.instockqty));
            int data4 = exist.Count();
            var result = new
            {
                data1 = picklistDataList,
                data2 = exist,
                data3 = data3,
                data4 = data4,
            };
            return Ok(result);
        }

        public IActionResult TMPDataDetails(string sono)
        {
            if(sono  == null)
            {
                return Json(new { success = false, message = "Please select the sale order number first !" });
            }
            List<Picking_Operation> picklistDataList = new List<Picking_Operation>();

            var pglist = _context.Picklist_Generation
                .Where(a => a.sono.Trim() == sono.Trim())
                .ToList();

            if (pglist.Count > 0)
            {
                foreach (var pg in pglist)
                {
                    var pdlist = _context.pickstorage.Where(a => a.genid == pg.gen_id)
                        .Where(a => a.flag == 0)
                        .OrderBy(a => a.location.Trim()) // Order by location
                        .ToList();

                    var distinctLocationsAndBoxNos = pdlist
                       .GroupBy(a => new { a.location, BoxNoPart = a.boxno.Split('-')[1], a.batchcode, a.productcode })
                       .Select(group => new
                       {
                           productcode = group.Key.productcode.ToUpper(),
                           batchcode = group.Key.batchcode,
                           Location = group.Key.location,
                           BoxNo = group.Key.BoxNoPart,
                           Count = group.Count()
                       });
                    foreach (var item in distinctLocationsAndBoxNos)
                    {
                        var data = new Picking_Operation
                        {
                            productcode = item.productcode.ToUpper(), // You need to decide how to set this value since it's not present in your current 
                            location = item.Location,
                            batchcode = item.batchcode,
                            boxno = item.BoxNo,
                            instockqty = item.Count.ToString()
                        };
                        picklistDataList.Add(data);
                    }
                }

            }

            // Return result
            if (picklistDataList.Count > 0)
            {
                return Json(new { success = true, data = picklistDataList });
            }
            else
            {
                return Json(new { success = false, message = "No Data Found to pick against order no " + sono + "!" });
            }
        }


    }
}
