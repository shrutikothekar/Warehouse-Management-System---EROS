using AspNetCoreHero.ToastNotification.Abstractions;
using AuthSystem.Data;
using Microsoft.AspNetCore.Mvc;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System;
using eros.Models;
using OfficeOpenXml;
using AspNetCore;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using AspNetCoreHero.ToastNotification.Helpers;
using Humanizer;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Linq;
using Nest;

namespace eros.Controllers
{
    public class ReportController : Controller
    {
        private readonly ErosDbContext _context;
        public INotyfService _notyfService { get; }

        public ReportController(ErosDbContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public DateTime newDateTime ;

        private static List<PurchaseOrder> PurchaseOrder = new List<PurchaseOrder>();
        private static List<SalesOrder> SalesOrder = new List<SalesOrder>();
        private static List<CombinedViewModel> CombinedViewModel = new List<CombinedViewModel>();

        [HttpGet]
        public IActionResult GetStoreOperations(string productCode)
        {
            var storeOperations = _context.Storage_Operation
                .Where(a => a.productcode.Trim().ToUpper() == productCode.Trim().ToUpper() && a.statusflag == "ST" )
                .ToList();

            return Json(storeOperations);
        }

        [HttpGet]

        public IActionResult InOutStock(string date)
        {
            List<PurchaseOrder> PurchaseOrders = new List<PurchaseOrder>();
            List<SalesOrder> SalesOrders = new List<SalesOrder>();

            PurchaseOrders.Clear();
            SalesOrders.Clear();

            //PONO DATA
            var inwardData = _context.inwardPacket
                .Where(a => a.date.Trim() == date.Trim() && a.flag == 1)
                .ToList();
            foreach (var item in inwardData)
            {
                var category = _context.Product_Master
                    .FirstOrDefault(a => a.productcode.Trim().ToUpper() == item.productcode.Trim().ToUpper());

                if (category != null)
                {
                    item.Category = category.categoryname.Trim();
                }

                var check = _context.inward
                    .FirstOrDefault(a => a.pono.Trim() == item.pono.Trim());

                if (check != null)
                {
                    item.Customer = check.partyname.Trim();

                    var purchaseOrder = new PurchaseOrder
                    {
                        pono = item.pono,
                        poqty = item.quantity,
                        supplier = item.Customer,
                        product = item.productcode,
                        category = item.Category
                    };

                    PurchaseOrders.Add(purchaseOrder);
                }
            }

            //SONO DTAA 
            var loadingData = _context.Loading_Dispatch_Operation
             .Where(a => a.currentdate.Trim() == date.Trim())
                 .GroupBy(a => new { a.productcode, a.sono })
             .ToList();

            var picklistdata = _context.Picklist_Generation.ToList();

            picklistdata.Clear();
            foreach (var item in loadingData)
            {
                var productcode1 = item.Key.productcode;
                var sono1 = item.Key.sono;
                var category1 = item.First().Category;
                var customer1 = item.First().customer;

                var picklist = _context.Picklist_Generation.Where(a => a.sono.Trim() == sono1.Trim() && a.prdcode.Trim().ToUpper() == productcode1.Trim().ToUpper()).FirstOrDefault();

                var category = _context.Product_Master
                    .FirstOrDefault(a => a.productcode.Trim().ToUpper() == productcode1.Trim().ToUpper());

                if (category != null)
                {
                    category1 = category.categoryname.Trim();
                }

                var check = _context.so_inward
                    .FirstOrDefault(a => a.sono.Trim() == sono1.Trim());

                if (check != null)
                {
                    customer1 = check.customername.Trim();
                }

                if (picklist != null)
                {
                    picklist.category = category.categoryname.Trim();
                    picklist.date = date;
                    picklist.customer = customer1;
                    picklist.category = category1;
                    picklist.sono = sono1;
                    picklist.prdcode = productcode1;

                    var salesOrder = new SalesOrder
                    {
                        sono = sono1,
                        soqty = Convert.ToInt32(picklist.pickingqty),
                        customer = customer1,
                        product = productcode1,
                        category = category1 // Assuming 'category1' was a typo
                    };
                    SalesOrders.Add(salesOrder);
                }
                //picklistdata.Add(picklist);
            }

            // Set ViewBag variables
            ViewBag.PurchaseOrder = PurchaseOrders;
            ViewBag.SalesOrder = SalesOrders;
            ViewBag.NitCountSale = SalesOrders.Count;
            ViewBag.NitCountPurchase = PurchaseOrders.Count;

            // Parse and format date
            DateTime parsedDate;
            if (DateTime.TryParseExact(date.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                ViewBag.Date = parsedDate.ToString("dd-MM-yyyy");
            }
            else
            {
                ViewBag.Date = "Invalid Date";
            }

            return View();
        }

        public IActionResult InOutStockIndex()
        {
            return View();
        }

        //public IActionResult ConvertBatchCodeToDate(string batchCode)
        //{
        //    if (batchCode.Length < 5)
        //    {
        //        return BadRequest("Invalid batch code format."); // Return a proper HTTP 400 response
        //    }

        //    string yearString = batchCode.Substring(0, 2); // Extracts the first two characters for year
        //    char monthCode = batchCode[2]; // Extracts the third character for month code
        //    string dayString = batchCode.Substring(3, 2); // Extracts two characters starting from index 3 for day

        //    if (!int.TryParse(yearString, out int year) || year < 0 || year > 99)
        //    {
        //        return BadRequest("Invalid year component in batch code."); // Handle invalid year format
        //    }
        //    if (!int.TryParse(dayString, out int day) || day < 1 || day > 31)
        //    {
        //        return BadRequest("Invalid day component in batch code."); // Handle invalid day format
        //    }

        //    int month = monthCode switch
        //    {
        //        'A' => 1,
        //        'B' => 2,
        //        'C' => 3,
        //        'D' => 4,
        //        'E' => 5,
        //        'F' => 6,
        //        'G' => 7,
        //        'H' => 8,
        //        'I' => 9,
        //        'J' => 10,
        //        'K' => 11,
        //        'L' => 12,
        //        _ => throw new FormatException("Invalid month code.")
        //    };

        //    int fullYear = 2000 + year; // Calculate full year from the two-digit year

        //    DateTime date = new DateTime(fullYear, month, day);
        //    string formattedDate = date.ToString("yyyy-MM-dd");
        //    newDateTime = date; // Assign the calculated DateTime to newDateTime

        //    // Return the formatted date string or another appropriate response
        //    return Ok(formattedDate);
        //}

        public IActionResult ConvertBatchCodeToDate(string batchCode)
        {
            if (batchCode.Length < 5)
            {
                return BadRequest("Invalid batch code format."); // Return a proper HTTP 400 response
            }

            string yearString = batchCode.Substring(0, 2); // Extracts the first two characters for year
            char monthCode = batchCode[2]; // Extracts the third character for month code
            string dayString = batchCode.Substring(3, 2); // Extracts two characters starting from index 3 for day

            if (!int.TryParse(yearString, out int year) || year < 0 || year > 99)
            {
                return BadRequest("Invalid year component in batch code."); // Handle invalid year format
            }
            if (!int.TryParse(dayString, out int day) || day < 1 || day > 31)
            {
                return BadRequest("Invalid day component in batch code."); // Handle invalid day format
            }

            int month;
            try
            {
                month = monthCode switch
                {
                    'A' => 1,
                    'B' => 2,
                    'C' => 3,
                    'D' => 4,
                    'E' => 5,
                    'F' => 6,
                    'G' => 7,
                    'H' => 8,
                    'I' => 9,
                    'J' => 10,
                    'K' => 11,
                    'L' => 12,
                    _ => throw new FormatException("Invalid month code.")
                };
            }
            catch (FormatException ex)
            {
                return BadRequest(ex.Message); // Return detailed message for invalid month code
            }

            int fullYear = 2000 + year; // Calculate full year from the two-digit year

            DateTime date;
            try
            {
                date = new DateTime(fullYear, month, day);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message); // Handle invalid date components
            }

            string formattedDate = date.ToString("yyyy-MM-dd");
            newDateTime = date; // Assign the calculated DateTime to newDateTime

            // Return the formatted date string or another appropriate response
            return Ok(formattedDate);
        }

        //public IActionResult StockDetails(string productcode)
        //{
        //    // PO DETAILS
        //    PurchaseOrder.Clear();
        //    SalesOrder.Clear();
        //    var getlst = _context.inwardPacket.DistinctBy(a=>a.sono.Trim()).Where(a=>a.productcode.Trim().ToUpper() == productcode.Trim().ToUpper()).ToList();
        //    foreach(var item in getlst)
        //    {
        //        var supplier = _context.purchase.Where(a => a.pono.Trim() == item.pono.Trim()).FirstOrDefault();
        //        item.supplier = supplier.suppliername.Trim();
        //        var listcount = _context.inwardPacket.Where(a => a.sono == item.sono && a.productcode.Trim().ToUpper() == productcode.Titleize().ToUpper()).ToList();
        //        item.pickquantity = listcount.Sum(a => a.quantity);
        //    }
        //    // Grouping by date, supplier name, and sum of quantity from inwardData
        //    var groupedData = getlst
        //        .GroupBy(i => new { i.date, i.supplier, i.productcode, i.pickquantity })
        //        .Select(g => new
        //        {
        //            Date = g.Key.date,
        //            SupplierName = g.Key.supplier,
        //            TotalQuantity = g.Key.pickquantity,
        //            product = g.Key.productcode,
        //        });
        //    foreach (var group in groupedData)
        //    {
        //        PurchaseOrder.Add(new PurchaseOrder
        //        {
        //            date = group.Date,
        //            supplier = group.SupplierName,
        //            poqty = group.TotalQuantity,
        //            product = group.product,
        //        });
        //    }

        //    // SO DETAILS
        //    var getdata = _context.Loading_Dispatch_Operation.Where(a=>a.productcode.ToUpper().Trim() == productcode.Trim().ToUpper()).Distinct().ToList();
        //    foreach(var item in getdata)
        //    {
        //        //get customer
        //        var customer = _context.so_inward.Select(a=>a.customername.Trim()).FirstOrDefault();
        //        item.customer = customer;
        //        //get pickqty
        //        var getpicklist = _context.Picklist_Generation.Where(a => a.sono.Trim() == item.sono.Trim() && a.prdcode.Trim().ToUpper() == productcode.Trim().ToUpper()).ToList();
        //        var qtyy = getpicklist.Sum(a => Convert.ToInt32(a.pickingqty));
        //        item.quantity = qtyy;
        //    }
        //    // Grouping by date, supplier name, and sum of quantity from inwardData
        //    var groupedDatagetdata = getdata
        //        .GroupBy(i => new { i.currentdate, i.customer, i.productcode, i.quantity })
        //        .Select(g => new
        //        {
        //            Date = g.Key.currentdate,
        //            customer = g.Key.customer,
        //            TotalQuantity = g.Key.quantity,
        //            product = g.Key.productcode,
        //        });
        //    foreach (var group in groupedDatagetdata)
        //    {
        //        SalesOrder.Add(new SalesOrder
        //        {
        //            date = group.Date,
        //            customer = group.customer,
        //            soqty = group.TotalQuantity,
        //            product = group.product,
        //        });
        //    }

        //    ViewBag.PurchaseOrder = PurchaseOrder;
        //    ViewBag.SalesOrder = SalesOrder;
        //        return View();
        //}

        public IActionResult StockDetails(string productcode, int balqty)
        {
            // PO DETAILS
            PurchaseOrder.Clear();
            SalesOrder.Clear();

            // Load data from the database
            var inwardPackets = _context.inwardPacket
                                        .Where(a => a.productcode.Trim().ToUpper() == productcode.Trim().ToUpper())
                                        .ToList()
                                        .DistinctBy(a => a.sono.Trim())
                                        .ToList();

            foreach (var item in inwardPackets)
            {
                var supplier = _context.purchase
                                       .FirstOrDefault(a => a.pono.Trim() == item.pono.Trim());
                item.supplier = supplier?.suppliername.Trim();

                var listcount = _context.inwardPacket
                                        .Where(a => a.sono == item.sono && a.productcode.Trim().ToUpper() == productcode.Trim().ToUpper())
                                        .ToList();

                item.pickquantity = listcount.Sum(a => a.quantity);
            }

            // Grouping by date, supplier name, and sum of quantity from inwardData
            var groupedData = inwardPackets
                .GroupBy(i => new { i.date, i.supplier, i.productcode, i.pickquantity })
                .Select(g => new
                {
                    Date = g.Key.date,
                    SupplierName = g.Key.supplier,
                    TotalQuantity = g.Key.pickquantity,
                    product = g.Key.productcode,
                });

            foreach (var group in groupedData)
            {
                PurchaseOrder.Add(new PurchaseOrder
                {
                    date = group.Date,
                    supplier = group.SupplierName,
                    poqty = group.TotalQuantity,
                    product = group.product,
                    currentqty = balqty,
                });
            }

            // SO DETAILS
            var getdata = _context.Loading_Dispatch_Operation
                                  .Where(a => a.productcode.Trim().ToUpper() == productcode.Trim().ToUpper())
                                  .ToList()
                                  .Distinct()
                                  .ToList();

            foreach (var item in getdata)
            {
                // Get customer
                var customer = _context.so_inward
                                       .Where(a => a.sono.Trim() == item.sono.Trim())
                                       .Select(a => a.customername.Trim())
                                       .FirstOrDefault();
                item.customer = customer;

                // Get pick quantity
                var getpicklist = _context.Picklist_Generation
                                          .Where(a => a.sono.Trim() == item.sono.Trim() && a.prdcode.Trim().ToUpper() == productcode.Trim().ToUpper())
                                          .ToList();
                var qtyy = getpicklist.Sum(a => Convert.ToInt32(a.pickingqty));
                item.quantity = qtyy;
            }

            // Grouping by date, customer name, and sum of quantity from getdata
            var groupedDatagetdata = getdata
                .GroupBy(i => new { i.currentdate, i.customer, i.productcode, i.quantity })
                .Select(g => new
                {
                    Date = g.Key.currentdate,
                    customer = g.Key.customer,
                    TotalQuantity = g.Key.quantity,
                    product = g.Key.productcode,
                });

            foreach (var group in groupedDatagetdata)
            {
                SalesOrder.Add(new SalesOrder
                {
                    date = group.Date,
                    customer = group.customer,
                    soqty = group.TotalQuantity,
                    product = group.product,
                    currentqty = balqty,
                });
            }

            ViewBag.PurchaseOrder = PurchaseOrder;
            ViewBag.SalesOrder = SalesOrder;
            ViewBag.ProductCode = productcode;
            int nitCountsale = SalesOrder.Count; // Assuming nitCount is the count of sales orders
            ViewBag.NitCountSale = nitCountsale;
            int nitCountpurchase = PurchaseOrder.Count; // Assuming nitCount is the count of sales orders
            ViewBag.NitCountPurchase = nitCountpurchase;

            return View();
        }

        //public IActionResult StokeLedger()
        //{
        //    List<InStockQty> inStockQuantities = new List<InStockQty>();

        //    List<Picklist_Generation> outqty = new List<Picklist_Generation>();

        //    var listdata =  _context.Storage_Operation.ToList();

        //    var pc = listdata.Select(a=>a.productcode.Trim().ToUpper()).Distinct().ToList();

        //    foreach (var b in pc)
        //    {
        //        //IN DATA
        //        var storedata_in = listdata
        //        .Where(a => (a.statusflag == "PI" || a.statusflag == "ST" || a.statusflag == "LD") && a.productcode.Trim().ToUpper() == b.Trim().ToUpper())
        //        .ToList();

        //        var groupedData_in = storedata_in
        //            .GroupBy(q => GetSecondDigit(q.boxno))
        //            .ToDictionary(group => group.Key, group => group.Count());

        //        int secondDigitCount_in = 0;
        //        foreach (var kvp in groupedData_in)
        //        {
        //            var secondDigit = kvp.Key.Trim();
        //            secondDigitCount_in = kvp.Value;

        //        }


        //        //OUT DATA
        //        var storedata_out = listdata
        //            .Where(a => a.statusflag == "LD" && a.productcode.Trim().ToUpper() == b.Trim().ToUpper())
        //            .ToList();

        //        var groupedData_out = storedata_out
        //            .GroupBy(q => GetSecondDigit(q.boxno))
        //            .ToDictionary(group => group.Key, group => group.Count());

        //        int secondDigitCountOut = 0;
        //        foreach (var kvp in groupedData_out)
        //        {
        //            var secondDigit = kvp.Key.Trim();
        //            secondDigitCountOut = kvp.Value;
        //        }

        //        //CURRENT CLOSING BAL
        //        var ClosingBal = listdata
        //            .Where(a => (a.statusflag == "ST" || a.statusflag == "PI") && a.productcode.Trim().ToUpper() == b.Trim().ToUpper())
        //            .ToList();

        //        var groupedClosingBal = ClosingBal
        //            .GroupBy(q => GetSecondDigit(q.boxno))
        //            .ToDictionary(group => group.Key, group => group.Count());

        //        int ClosingBalCount = 0;
        //        foreach (var kvp in groupedClosingBal)
        //        {
        //            var secondDigit = kvp.Key.Trim();
        //            ClosingBalCount = kvp.Value;
        //        }

        //        //PRODUCT NAME
        //        var productname = _context.Product_Master
        //            .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper())
        //            .Select(a => a.productdescription.Trim().ToUpper())
        //            .FirstOrDefault();

        //        //SEND DATA
        //        var instcok = new InStockQty
        //        {
        //            productcode = b,
        //            productname = productname,
        //            inqty = secondDigitCount_in,
        //            outqty = secondDigitCountOut,
        //            currentqty = ClosingBalCount,
        //        };

        //        inStockQuantities = inStockQuantities.OrderBy(item => item.id).ToList();
        //        inStockQuantities.Add(instcok);
        //    }

        //    //inStockQuantities = inStockQuantities.OrderBy(item => item.id).ToList();

        //    return View(inStockQuantities);
        //}

        public IActionResult StokeLedger()
        {
            List<InStockQty> inStockQuantities = new List<InStockQty>();

            var listdata = _context.Storage_Operation.ToList();

            var pc = listdata.Select(a => a.productcode.Trim().ToUpper()).Distinct().ToList();

            foreach (var b in pc)
            {
                // IN DATA
                var storedata_in = listdata
                    .Where(a => (a.statusflag == "PI" || a.statusflag == "ST" || a.statusflag == "LD") && a.productcode.Trim().ToUpper() == b.Trim().ToUpper())
                    .ToList();

                var groupedData_in = storedata_in
                    .GroupBy(q => GetSecondDigit(q.boxno))
                    .ToDictionary(group => group.Key, group => group.Count());

                int minInQtyCount = groupedData_in.Count > 0 ? groupedData_in.Min(kvp => kvp.Value) : 0;
                int InQty = _context.inwardPacket
                    .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper())
                    .Sum(a => a.quantity);

                // OUT DATA
                var storedata_out = listdata
                    .Where(a => a.statusflag == "LD" && a.productcode.Trim().ToUpper() == b.Trim().ToUpper())
                    .ToList();

                var groupedData_out = storedata_out
                    .GroupBy(q => GetSecondDigit(q.boxno))
                    .ToDictionary(group => group.Key, group => group.Count());

                int minOutQtyCount = groupedData_out.Count > 0 ? groupedData_out.Min(kvp => kvp.Value) : 0;

                // CURRENT CLOSING BALANCE
                var closingBalData = listdata
                    .Where(a => (a.statusflag == "ST" || a.statusflag == "PI") && a.productcode.Trim().ToUpper() == b.Trim().ToUpper())
                    .ToList();
                    
                var result = new List<KeyValuePair<string, int>>();
                int minClosingBalCount = int.MaxValue;

                var box = listdata.Where(a=>a.productcode.Trim().ToUpper()== b.Trim().ToUpper()).Select(a => a.boxno.Trim()).FirstOrDefault();
                var splitbox = GetSpliBox(box);

                var groupedClosingBal = closingBalData
                    .GroupBy(q => GetSecondDigit(q.boxno))
                    .ToDictionary(group => group.Key, group => group.Count());

                //int minClosingBalCount = groupedClosingBal.Count > 0 ? groupedClosingBal.Min(kvp => kvp.Value) : 0;
                var possibleBoxes = new List<string>();
                if (groupedClosingBal.Count == 0)
                {
                    minClosingBalCount = 0;
                }
                else
                {
                    possibleBoxes = GetPossibleBoxes(splitbox);
                    foreach (var item in possibleBoxes)
                    {
                        int count = 0;
                        foreach (var kvp in groupedClosingBal)
                        {
                            if (kvp.Key.Contains(item))
                            {
                                count = kvp.Value;
                                break;
                            }
                        }
                        result.Add(new KeyValuePair<string, int>(item, count));
                    }
                }
                if (result.Count > 0)
                {
                    minClosingBalCount = result.Min(kvp => kvp.Value);
                }
                else
                {
                    minClosingBalCount = 0;
                }

                // PRODUCT NAME
                var productname = _context.Product_Master
                    .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper())
                    .Select(a => a.productdescription.Trim().ToUpper())
                    .FirstOrDefault();

                // SEND DATA
                var instcok = new InStockQty
                {
                    productcode = b,
                    productname = productname,
                    inqty = InQty,
                    //inqty = minInQtyCount,
                    outqty = minOutQtyCount,
                    currentqty = minClosingBalCount
                };

                inStockQuantities.Add(instcok);
            }

            inStockQuantities = inStockQuantities.OrderBy(item => item.id).ToList();

            return View(inStockQuantities);
        }

        //public ActionResult StokeLedger_FilterDataByDate(DateTime? fromDate, DateTime? toDate)
        //{
        //    DateTime currentDate = DateTime.Now;
        //    DateTime effectiveToDate = toDate ?? currentDate;
        //    DateTime effectiveFromDate = fromDate ?? DateTime.MinValue;

        //    List<InStockQty> inStockQuantities = new List<InStockQty>();

        //    var listdata1 = _context.Storage_Operation.ToList();
        //    foreach (var item in listdata1)
        //    {
        //        ConvertBatchCodeToDate(item.batchcode);
        //        item.date = newDateTime; // Ensure newDateTime is defined appropriately
        //    }

        //    var listdata = listdata1
        //        .Where(a => a.date >= effectiveFromDate && a.date < effectiveToDate)
        //        .ToList();

        //    var productCodes = listdata
        //        .Select(a => a.productcode.Trim().ToUpper())
        //        .Distinct()
        //        .ToList();

        //    foreach (var productCode in productCodes)
        //    {
        //        // IN DATA
        //        var storedata_in = listdata
        //            .Where(a => (a.statusflag == "PI" || a.statusflag == "ST" || a.statusflag == "LD") && a.productcode.Trim().ToUpper() == productCode)
        //            .GroupBy(q => GetSecondDigit(q.boxno))
        //            .ToDictionary(group => group.Key, group => group.Count());

        //        int inQtyCount = storedata_in.Values.Sum();

        //        // OUT DATA
        //        var storedata_out = listdata
        //            .Where(a => a.statusflag == "LD" && a.productcode.Trim().ToUpper() == productCode)
        //            .GroupBy(q => GetSecondDigit(q.boxno))
        //            .ToDictionary(group => group.Key, group => group.Count());

        //        int outQtyCount = storedata_out.Values.Sum();

        //        // CURRENT CLOSING BALANCE
        //        var closingBalData = listdata
        //            .Where(a => (a.statusflag == "ST" || a.statusflag == "PI") && a.productcode.Trim().ToUpper() == productCode)
        //            .GroupBy(q => GetSecondDigit(q.boxno))
        //            .ToDictionary(group => group.Key, group => group.Count());

        //        int closingBalCount = closingBalData.Values.Sum();

        //        // PRODUCT NAME
        //        var productName = _context.Product_Master
        //            .Where(a => a.productcode.Trim().ToUpper() == productCode)
        //            .Select(a => a.productdescription.Trim().ToUpper())
        //            .FirstOrDefault();

        //        // SEND DATA
        //        var inStock = new InStockQty
        //        {
        //            productcode = productCode,
        //            productname = productName,
        //            inqty = inQtyCount,
        //            outqty = outQtyCount,
        //            currentqty = closingBalCount // Ensure this property is present in InStockQty
        //        };

        //        inStockQuantities.Add(inStock);
        //    }

        //    // Order the final list by some meaningful property if needed
        //    inStockQuantities = inStockQuantities.OrderBy(item => item.productcode).ToList();
        //  //return PartialView("StokeLedgerView", inStockQuantities);
        //    return Json(inStockQuantities);
        //}

        public ActionResult StokeLedger_FilterDataByDate(DateTime? fromDate, DateTime? toDate)
        {
            DateTime currentDate = DateTime.Now;
            DateTime effectiveToDate = toDate ?? currentDate;
            DateTime effectiveFromDate = fromDate ?? DateTime.MinValue;

            List<InStockQty> inStockQuantities = new List<InStockQty>();

            var listdata1 = _context.Storage_Operation.ToList();
            foreach (var item in listdata1)
            {
                ConvertBatchCodeToDate(item.batchcode);
                item.date = newDateTime; // Ensure newDateTime is defined appropriately
            }

            var listdata = listdata1
                .Where(a => a.date >= effectiveFromDate && a.date <= effectiveToDate)
                .ToList();

            var productCodes = listdata
                .Select(a => a.productcode.Trim().ToUpper())
                .Distinct()
                .ToList();

            foreach (var productCode in productCodes)
            {
                // IN DATA
                var storedata_in = listdata
                    .Where(a => (a.statusflag == "PI" || a.statusflag == "ST" || a.statusflag == "LD") && a.productcode.Trim().ToUpper() == productCode)
                    .GroupBy(q => GetSecondDigit(q.boxno))
                    .ToDictionary(group => group.Key, group => group.Count());

                int inQtyCount = storedata_in.Count > 0 ? storedata_in.Min(kvp => kvp.Value) : 0;

                // OUT DATA
                var storedata_out = listdata
                    .Where(a => a.statusflag == "LD" && a.productcode.Trim().ToUpper() == productCode)
                    .GroupBy(q => GetSecondDigit(q.boxno))
                    .ToDictionary(group => group.Key, group => group.Count());

                int outQtyCount = storedata_out.Count > 0 ? storedata_out.Min(kvp => kvp.Value) : 0;

                // CURRENT CLOSING BALANCE
                var closingBalData = listdata
                    .Where(a => (a.statusflag == "ST" || a.statusflag == "PI") && a.productcode.Trim().ToUpper() == productCode)
                    .GroupBy(q => GetSecondDigit(q.boxno))
                    .ToDictionary(group => group.Key, group => group.Count());

                //int closingBalCount = closingBalData.Count > 0 ? closingBalData.Min(kvp => kvp.Value) : 0;
                var result = new List<KeyValuePair<string, int>>();
                int closingBalCount = int.MaxValue;
                var box = listdata.Select(a => a.boxno.Trim()).FirstOrDefault();
                var splitbox = GetSpliBox(box);

                var possibleBoxes = new List<string>();
                if (closingBalData.Count == 0)
                {
                    closingBalCount = 0;
                }
                else
                {
                    possibleBoxes = GetPossibleBoxes(splitbox);
                }
                foreach (var item in possibleBoxes)
                {
                    int count = 0;
                    foreach (var kvp in closingBalData)
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
                    closingBalCount = result.Min(kvp => kvp.Value);
                }
                else
                {
                    closingBalCount = 0;
                }


                // PRODUCT NAME
                var productName = _context.Product_Master
                    .Where(a => a.productcode.Trim().ToUpper() == productCode)
                    .Select(a => a.productdescription.Trim().ToUpper())
                    .FirstOrDefault();

                // SEND DATA
                var inStock = new InStockQty
                {
                    productcode = productCode,
                    productname = productName,
                    inqty = inQtyCount,
                    outqty = outQtyCount,
                    currentqty = closingBalCount // Ensure this property is present in InStockQty
                };

                inStockQuantities.Add(inStock);
            }

            // Order the final list by some meaningful property if needed
            inStockQuantities = inStockQuantities.OrderBy(item => item.productcode).ToList();
            //return PartialView("StokeLedgerView", inStockQuantities);
            return Json(inStockQuantities);
        }

        //public IActionResult AgeingReport()
        //{
        //    List<InStockQty> inStockQuantities = new List<InStockQty>();

        //    var listdata = _context.Storage_Operation
        //        .Where(a => a.statusflag == "PI" || a.statusflag == "ST")
        //        .ToList();
        //    // Convert batch code to date
        //    foreach (var packet in listdata)
        //    {
        //        ConvertBatchCodeToDate(packet.batchcode.Trim());
        //        packet.date = newDateTime;
        //    }

        //    var productcodes = listdata
        //        .Select(a => a.productcode.Trim().ToUpper())
        //        .Distinct()
        //        .ToList();

        //    foreach (var b in productcodes)
        //    {
        //        DateTime currDate = DateTime.Now;

        //        // Define date thresholds for different aging categories
        //        DateTime less30daysThreshold = currDate.AddDays(-30);
        //        DateTime days30to60Threshold = currDate.AddDays(-60);
        //        DateTime days60to90Threshold = currDate.AddDays(-90);
        //        DateTime days90to180Threshold = currDate.AddDays(-180);
        //        DateTime days180to270Threshold = currDate.AddDays(-270);
        //        DateTime days270to365Threshold = currDate.AddDays(-365);
        //        DateTime days365to1460Threshold = currDate.AddDays(-1460);

        //        var productname = _context.Product_Master
        //            .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper())
        //            .Select(a => a.productdescription.Trim().ToUpper())
        //            .FirstOrDefault();

        //        //Current Stock
        //        var storedata = listdata
        //            .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper())
        //            .ToList();

        //        var groupedData = storedata
        //            .GroupBy(q => GetSecondDigit(q.boxno))
        //            .ToDictionary(group => group.Key, group => group.Count());

        //        int secondDigitCount = 0;
        //        foreach (var kvp in groupedData)
        //        {
        //            var secondDigit = kvp.Key.Trim();
        //            secondDigitCount = kvp.Value;
        //        }

        //        // Last 30 days stock
        //        var last30daysSTData = listdata
        //            .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper() && a.date >= less30daysThreshold)
        //            .ToList();

        //        var last30DaysGroupedData = last30daysSTData
        //            .GroupBy(q => GetSecondDigit(q.boxno))
        //            .ToDictionary(group => group.Key, group => group.Count());

        //        int last30DaysCount = 0;
        //        foreach (var kvp in last30DaysGroupedData)
        //        {
        //            var secondDigit = kvp.Key.Trim();
        //            last30DaysCount = kvp.Value;
        //        }

        //        // 30-60 days stock
        //        //var days30to60STData = listdata
        //        //    .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper() && a.date >= days30to60Threshold)
        //        //    .ToList();
        //        var days30to60STData = listdata
        //        .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper() && a.date >= days30to60Threshold && a.date < less30daysThreshold)
        //        .ToList();

        //        var days30to60GroupedData = days30to60STData
        //            .GroupBy(q => GetSecondDigit(q.boxno))
        //            .ToDictionary(group => group.Key, group => group.Count());

        //        int days30to60Count = 0;
        //        foreach (var kvp in days30to60GroupedData)
        //        {
        //            var secondDigit = kvp.Key.Trim();
        //            days30to60Count = kvp.Value;
        //        }

        //        // 60-90 days stock
        //        //var days60to90STData = listdata
        //        //    .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper() && a.date >= days30to90Threshold)
        //        //    .ToList();
        //        var days60to90STData = listdata
        //       .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper() && a.date >= days60to90Threshold && a.date < days30to60Threshold)
        //       .ToList();

        //        var days60to90GroupedData = days60to90STData
        //            .GroupBy(q => GetSecondDigit(q.boxno))
        //            .ToDictionary(group => group.Key, group => group.Count());

        //        int days60to90Count = 0;
        //        foreach (var kvp in days60to90GroupedData)
        //        {
        //            var secondDigit = kvp.Key.Trim();
        //            days60to90Count = kvp.Value;
        //        }
        //        // 90-180 days stock
        //        //var days90to180STData = listdata
        //        //    .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper() && a.date >= days90to180Threshold)
        //        //    .ToList();
        //        var days90to180STData = listdata
        //        .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper() && a.date >= days90to180Threshold && a.date < days60to90Threshold)
        //        .ToList();

        //        var days90to180GroupedData = days90to180STData
        //            .GroupBy(q => GetSecondDigit(q.boxno))
        //            .ToDictionary(group => group.Key, group => group.Count());

        //        int days90to180Count = 0;
        //        foreach (var kvp in days90to180GroupedData)
        //        {
        //            var secondDigit = kvp.Key.Trim();
        //            days90to180Count = kvp.Value;
        //        }

        //        // 180-270 days stock
        //        //var days180to270STData = listdata
        //        //    .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper() && a.date >= days180to270Threshold)
        //        //    .ToList();
        //        var days180to270STData = listdata
        //        .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper() && a.date >= days180to270Threshold && a.date < days90to180Threshold)
        //        .ToList();


        //        var days180to270GroupedData = days180to270STData
        //            .GroupBy(q => GetSecondDigit(q.boxno))
        //            .ToDictionary(group => group.Key, group => group.Count());

        //        int days180to270Count = 0;
        //        foreach (var kvp in days180to270GroupedData)
        //        {
        //            var secondDigit = kvp.Key.Trim();
        //            days180to270Count = kvp.Value;
        //        }

        //        // 270-365 days stock
        //        //var days270to365STData = listdata
        //        //    .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper() && a.date >= days270to365Threshold)
        //        //    .ToList();
        //        var days270to365STData = listdata
        //        .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper() && a.date >= days270to365Threshold && a.date < days180to270Threshold)
        //        .ToList();


        //        var days270to365GroupedData = days270to365STData
        //            .GroupBy(q => GetSecondDigit(q.boxno))
        //            .ToDictionary(group => group.Key, group => group.Count());

        //        int days270to365Count = 0;
        //        foreach (var kvp in days270to365GroupedData)
        //        {
        //            var secondDigit = kvp.Key.Trim();
        //            days270to365Count = kvp.Value;
        //        }
        //        // 365t-1460 days stock
        //        //var days365to1460STData = listdata
        //        //    .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper() && a.date >= days365to1460Threshold)
        //        //    .ToList();
        //        var days365to1460STData = listdata
        //        .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper() && a.date >= days365to1460Threshold && a.date < days270to365Threshold)
        //        .ToList();

        //        var days365to1460GroupedData = days365to1460STData
        //            .GroupBy(q => GetSecondDigit(q.boxno))
        //            .ToDictionary(group => group.Key, group => group.Count());

        //        int days365to1460Count = 0;
        //        foreach (var kvp in days365to1460GroupedData)
        //        {
        //            var secondDigit = kvp.Key.Trim();
        //            days365to1460Count = kvp.Value;
        //        }


        //        //SEND DATA
        //        var instcok = new InStockQty
        //        {
        //            productcode = b,
        //            currentqty = secondDigitCount,
        //            productname = productname,
        //            less30days = last30DaysCount,
        //            days30to60 = days30to60Count,
        //            days60to90 = days60to90Count,
        //            days90to180 = days90to180Count,
        //            days180to270 = days180to270Count,
        //            days270to365 = days270to365Count,
        //            days365to1460 = days365to1460Count,

        //        };

        //        inStockQuantities.Add(instcok);
        //    }

        //    //inStockQuantities = inStockQuantities.OrderBy(item => item.id).ToList();

        //    inStockQuantities = inStockQuantities.OrderBy(item =>
        //     item.less30days > 0 ? 0 :
        //     item.days30to60 > 0 ? 1 :
        //     item.days60to90 > 0 ? 2 :
        //     item.days90to180 > 0 ? 3 :
        //     item.days180to270 > 0 ? 4 :
        //     item.days270to365 > 0 ? 5 :
        //     item.days365to1460 > 0 ? 6 :
        //     item.above1460days > 0 ? 7 : 8)
        //     .ThenBy(item => item.productcode)
        //     .ToList();

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

        public IActionResult AgeingReport()
        {
            try
            {
                List<InStockQty> inStockQuantities = new List<InStockQty>();

                var listdata = _context.Storage_Operation
                    .Where(a => a.statusflag == "PI" || a.statusflag == "ST")
                    .ToList();

                foreach (var packet in listdata)
                {
                    packet.productcode = packet.productcode?.Trim();
                    packet.batchcode = packet.batchcode?.Trim();
                    packet.boxno = packet.boxno?.Trim();
                    packet.locationcode = packet.locationcode?.Trim();
                    packet.statusflag = packet.statusflag?.Trim();
                    packet.grnno = packet.grnno?.Trim();

                    ConvertBatchCodeToDate(packet.batchcode.Trim());
                    packet.date = newDateTime; // Ensure newDateTime is set properly in ConvertBatchCodeToDate method
                }

                var productcodes = listdata
                    .Select(a => a.productcode.Trim().ToUpper())
                    .Distinct()
                    .ToList();

                foreach (var b in productcodes)
                {
                    DateTime currDate = DateTime.Now;

                    // Define date thresholds for different aging categories
                    DateTime less30daysThreshold = currDate.AddDays(-30);
                    DateTime days30to60Threshold = currDate.AddDays(-60);
                    DateTime days60to90Threshold = currDate.AddDays(-90);
                    DateTime days90to180Threshold = currDate.AddDays(-180);
                    DateTime days180to270Threshold = currDate.AddDays(-270);
                    DateTime days270to365Threshold = currDate.AddDays(-365);
                    DateTime days365to1460Threshold = currDate.AddDays(-1460);

                    var productMaster = _context.Product_Master
                        .FirstOrDefault(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper());

                    if (productMaster != null)
                    {
                        var productName = productMaster.productdescription.Trim().ToUpper();
                        var category = productMaster.categoryname.Trim().ToUpper();

                        // Current Stock
                        var storedata = listdata
                            .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper())
                            .ToList();

                        //ADD CONDITION TO CHECK MIN COUNT
                        var groupedDatach = storedata
    .GroupBy(q => GetSecondDigit(q.boxno))
    .ToDictionary(group => group.Key, group => group.Count());

                        int minCount = int.MaxValue; // Initialize with a large number for finding minimum
                        var result = new List<KeyValuePair<string, int>>();

                        int secondDigitCount = 0;
                        int secondDigitCountPI = 0;

                        var box = storedata.Select(a => a.boxno.Trim()).FirstOrDefault();
                        var splitbox = GetSpliBox(box);

                        var possibleBoxes = new List<string>();
                        if (groupedDatach.Count == 0)
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
                            foreach (var kvp in groupedDatach)
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
                        //END


                        //var box = storedata.Select(a => a.boxno.Trim()).FirstOrDefault();
                        //var result = new List<KeyValuePair<string, int>>();
                        //var possibleBoxes = new List<string>();
                        //var splitbox = GetSpliBox(box);
                        //possibleBoxes = GetPossibleBoxes(splitbox);

                        // Calculate stock for each aging category
                        var instock = new InStockQty
                        {
                            productcode = b,
                            currentqty = minCount,
                            //currentqty = GetMinCount(storedata, DateTime.MinValue),
                            productname = productName,
                            less30days = GetMinCount(storedata.Where(a => a.date >= less30daysThreshold && a.date <= currDate).ToList(), less30daysThreshold),
                            days30to60 = GetMinCount(storedata.Where(a => a.date >= days30to60Threshold && a.date < less30daysThreshold).ToList(), days30to60Threshold),
                            days60to90 = GetMinCount(storedata.Where(a => a.date >= days60to90Threshold && a.date < days30to60Threshold).ToList(), days60to90Threshold),
                            days90to180 = GetMinCount(storedata.Where(a => a.date >= days90to180Threshold && a.date < days60to90Threshold).ToList(), days90to180Threshold),
                            days180to270 = GetMinCount(storedata.Where(a => a.date >= days180to270Threshold && a.date < days90to180Threshold).ToList(), days180to270Threshold),
                            days270to365 = GetMinCount(storedata.Where(a => a.date >= days270to365Threshold && a.date < days180to270Threshold).ToList(), days270to365Threshold),
                            days365to1460 = GetMinCount(storedata.Where(a => a.date >= days365to1460Threshold && a.date < days270to365Threshold).ToList(), days365to1460Threshold),
                        };

                        inStockQuantities.Add(instock);
                    }
                }

                inStockQuantities = inStockQuantities
                    .OrderBy(item =>
                        item.less30days > 0 ? 0 :
                        item.days30to60 > 0 ? 1 :
                        item.days60to90 > 0 ? 2 :
                        item.days90to180 > 0 ? 3 :
                        item.days180to270 > 0 ? 4 :
                        item.days270to365 > 0 ? 5 :
                        item.days365to1460 > 0 ? 6 :
                        item.above1460days > 0 ? 7 : 8)
                    .ThenBy(item => item.productcode)
                    .ToList();

                return View(inStockQuantities);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return View(error);
            }
        }

        //private int GetMinCount(List<Storage_Operation> stockData, DateTime thresholdDate)
        //{
        //    if (stockData == null || stockData.Count == 0)
        //        return 0;

        //    return stockData
        //        .Where(a => a.date >= thresholdDate)
        //        .GroupBy(q => GetSecondDigit(q.boxno)) // Assuming GetSecondDigit is defined elsewhere
        //        .Select(group => group.Count())
        //        .DefaultIfEmpty(0)
        //        .Min();

        //}
        private int GetMinCount(List<Storage_Operation> stockData, DateTime thresholdDate)
        {
            if (stockData == null || stockData.Count == 0)
                return 0;

            // Group by the second digit of boxno and count occurrences
            var groupedDatach = stockData
                .Where(a => a.date >= thresholdDate)
                .GroupBy(q => GetSecondDigit(q.boxno)) // Assuming GetSecondDigit is defined
                .ToDictionary(group => group.Key, group => group.Count());

            // Initialize with a large number for finding the minimum
            int minCount = int.MaxValue;

            // Get the first box number and possible split
            var box = stockData.Select(a => a.boxno.Trim()).FirstOrDefault();
            var splitbox = GetSpliBox(box); // Assuming this method splits box number

            // Get possible boxes based on the splitbox logic
            var possibleBoxes = new List<string>();
            if (groupedDatach.Count == 0)
            {
                minCount = 0; // No data found, set minCount to 0
            }
            else
            {
                possibleBoxes = GetPossibleBoxes(splitbox); // Assuming GetPossibleBoxes logic is defined
            }

            var result = new List<KeyValuePair<string, int>>();

            // Check each possible box and find matching group counts
            foreach (var item in possibleBoxes)
            {
                int count = 0;
                foreach (var kvp in groupedDatach)
                {
                    if (kvp.Key.Contains(item))
                    {
                        count = kvp.Value;
                        break;
                    }
                }
                result.Add(new KeyValuePair<string, int>(item, count));
            }

            // Find the minimum count from the result
            if (result.Count > 0)
            {
                minCount = result.Min(kvp => kvp.Value);
            }
            else
            {
                minCount = 0; // Default to 0 if no valid count found
            }

            return minCount;
        }

        //public ActionResult FilterDataByDate(DateTime? fromDate, DateTime? toDate)
        //{
        //    DateTime currentDate = DateTime.Now;
        //    DateTime effectiveToDate = toDate ?? currentDate;
        //    DateTime effectiveFromDate = fromDate ?? DateTime.MinValue;

        //    // Convert dates to strings in the required format for display purposes
        //    var fromDateString = effectiveFromDate.ToString("dd/MM/yyyy HH:mm:ss tt");
        //    var toDateString = effectiveToDate.ToString("dd/MM/yyyy HH:mm:ss tt");

        //    List<InStockQty> inStockQuantities = new List<InStockQty>();

        //    var listdata = _context.Storage_Operation
        //        .Where(a => a.statusflag == "PI" || a.statusflag == "ST")
        //        .ToList();

        //    // Convert batch code to date
        //    foreach (var packet in listdata)
        //    {
        //        ConvertBatchCodeToDate(packet.batchcode.Trim());
        //        packet.date = newDateTime;
        //    }

        //    var productcodes = listdata
        //        .Select(a => a.productcode.Trim().ToUpper())
        //        .Distinct()
        //        .ToList();

        //    foreach (var b in productcodes)
        //    {
        //        var productname = _context.Product_Master
        //            .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper())
        //            .Select(a => a.productdescription.Trim().ToUpper())
        //            .FirstOrDefault();

        //        var storedata = listdata
        //            .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper() && a.date >= effectiveFromDate && a.date <= effectiveToDate)
        //            .ToList();

        //        // Calculate counts for different aging categories
        //        var less30daysCount = storedata.Count(a => a.date >= effectiveToDate.AddDays(-30));
        //        var days30to60Count = storedata.Count(a => a.date < effectiveToDate.AddDays(-30) && a.date >= effectiveToDate.AddDays(-60));
        //        var days60to90Count = storedata.Count(a => a.date < effectiveToDate.AddDays(-60) && a.date >= effectiveToDate.AddDays(-90));
        //        var days90to180Count = storedata.Count(a => a.date < effectiveToDate.AddDays(-90) && a.date >= effectiveToDate.AddDays(-180));
        //        var days180to270Count = storedata.Count(a => a.date < effectiveToDate.AddDays(-180) && a.date >= effectiveToDate.AddDays(-270));
        //        var days270to365Count = storedata.Count(a => a.date < effectiveToDate.AddDays(-270) && a.date >= effectiveToDate.AddDays(-365));
        //        var days365to1460Count = storedata.Count(a => a.date < effectiveToDate.AddDays(-365) && a.date >= effectiveToDate.AddDays(-1460));
        //        var above1460daysCount = storedata.Count(a => a.date < effectiveToDate.AddDays(-1460));

        //        //SEND DATA
        //        var instcok = new InStockQty
        //        {
        //            productcode = b,
        //            currentqty = storedata.Count,
        //            productname = productname,
        //            less30days = less30daysCount,
        //            days30to60 = days30to60Count,
        //            days60to90 = days60to90Count,
        //            days90to180 = days90to180Count,
        //            days180to270 = days180to270Count,
        //            days270to365 = days270to365Count,
        //            days365to1460 = days365to1460Count,
        //            above1460days = above1460daysCount
        //        };

        //        inStockQuantities.Add(instcok);
        //    }

        //    inStockQuantities = inStockQuantities.OrderBy(item =>
        //        item.less30days > 0 ? 0 :
        //        item.days30to60 > 0 ? 1 :
        //        item.days60to90 > 0 ? 2 :
        //        item.days90to180 > 0 ? 3 :
        //        item.days180to270 > 0 ? 4 :
        //        item.days270to365 > 0 ? 5 :
        //        item.days365to1460 > 0 ? 6 :
        //        item.above1460days > 0 ? 7 : 8)
        //        .ThenBy(item => item.productcode)
        //        .ToList();

        //   // return PartialView("AgeingView", inStockQuantities);
        //    return Json(inStockQuantities);
        //}

        public ActionResult FilterDataByDate(DateTime? fromDate, DateTime? toDate)
        {
            DateTime currentDate = DateTime.Now;
            DateTime effectiveToDate = toDate ?? currentDate;
            DateTime effectiveFromDate = fromDate ?? DateTime.MinValue;

            // Convert dates to strings in the required format for display purposes
            var fromDateString = effectiveFromDate.ToString("dd/MM/yyyy HH:mm:ss tt");
            var toDateString = effectiveToDate.ToString("dd/MM/yyyy HH:mm:ss tt");

            List<InStockQty> inStockQuantities = new List<InStockQty>();

            var listdata = _context.Storage_Operation
                .Where(a => a.statusflag == "PI" || a.statusflag == "ST")
                .ToList();

            // Convert batch code to date
            foreach (var packet in listdata)
            {
                ConvertBatchCodeToDate(packet.batchcode.Trim());
                packet.date = newDateTime;
            }

            var productcodes = listdata
                .Select(a => a.productcode.Trim().ToUpper())
                .Distinct()
                .ToList();

            foreach (var b in productcodes)
            {
                var productname = _context.Product_Master
                    .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper())
                    .Select(a => a.productdescription.Trim().ToUpper())
                    .FirstOrDefault();

                var storedata = listdata
                    .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper() && a.date >= effectiveFromDate && a.date <= effectiveToDate)
                    .ToList();
                int minCount = int.MaxValue;
                var groupedDataPI = storedata
                      .GroupBy(q => GetSecondDigit(q.boxno))
                      .ToDictionary(group => group.Key, group => group.Count());
                if(groupedDataPI.Count == 0)
                {
                    minCount = 0;
                }
                else
                {
                    // Find minimum count from groupedData
                    foreach (var kvp in groupedDataPI)
                    {
                        var count = kvp.Value;
                        if (count < minCount)
                        {
                            minCount = count;
                        }
                    }
                }
               

                // Calculate counts for different aging categories
                var less30daysCount = storedata.Where(a => a.date >= effectiveToDate.AddDays(-30)).GroupBy(a => GetSecondDigit(a.boxno)).Select(g => g.Count()).DefaultIfEmpty(0).Min();
                var days30to60Count = storedata.Where(a => a.date < effectiveToDate.AddDays(-30) && a.date >= effectiveToDate.AddDays(-60)).GroupBy(a => GetSecondDigit(a.boxno)).Select(g => g.Count()).DefaultIfEmpty(0).Min();
                var days60to90Count = storedata.Where(a => a.date < effectiveToDate.AddDays(-60) && a.date >= effectiveToDate.AddDays(-90)).GroupBy(a => GetSecondDigit(a.boxno)).Select(g => g.Count()).DefaultIfEmpty(0).Min();
                var days90to180Count = storedata.Where(a => a.date < effectiveToDate.AddDays(-90) && a.date >= effectiveToDate.AddDays(-180)).GroupBy(a => GetSecondDigit(a.boxno)).Select(g => g.Count()).DefaultIfEmpty(0).Min();
                var days180to270Count = storedata.Where(a => a.date < effectiveToDate.AddDays(-180) && a.date >= effectiveToDate.AddDays(-270)).GroupBy(a => GetSecondDigit(a.boxno)).Select(g => g.Count()).DefaultIfEmpty(0).Min();
                var days270to365Count = storedata.Where(a => a.date < effectiveToDate.AddDays(-270) && a.date >= effectiveToDate.AddDays(-365)).GroupBy(a => GetSecondDigit(a.boxno)).Select(g => g.Count()).DefaultIfEmpty(0).Min();
                var days365to1460Count = storedata.Where(a => a.date < effectiveToDate.AddDays(-365) && a.date >= effectiveToDate.AddDays(-1460)).GroupBy(a => GetSecondDigit(a.boxno)).Select(g => g.Count()).DefaultIfEmpty(0).Min();
                var above1460daysCount = storedata.Where(a => a.date < effectiveToDate.AddDays(-1460)).GroupBy(a => GetSecondDigit(a.boxno)).Select(g => g.Count()).DefaultIfEmpty(0).Min();

                //SEND DATA
                var instcok = new InStockQty
                {
                    productcode = b,
                    currentqty = minCount,
                    productname = productname,
                    less30days = less30daysCount,
                    days30to60 = days30to60Count,
                    days60to90 = days60to90Count,
                    days90to180 = days90to180Count,
                    days180to270 = days180to270Count,
                    days270to365 = days270to365Count,
                    days365to1460 = days365to1460Count,
                    above1460days = above1460daysCount
                };

                inStockQuantities.Add(instcok);
            }

            inStockQuantities = inStockQuantities.OrderBy(item =>
                item.less30days > 0 ? 0 :
                item.days30to60 > 0 ? 1 :
                item.days60to90 > 0 ? 2 :
                item.days90to180 > 0 ? 3 :
                item.days180to270 > 0 ? 4 :
                item.days270to365 > 0 ? 5 :
                item.days365to1460 > 0 ? 6 :
                item.above1460days > 0 ? 7 : 8)
                .ThenBy(item => item.productcode)
                .ToList();

            return Json(inStockQuantities);
        }
        public IActionResult Index()
        {
            // Redirect to the StockMovementList action in the Inwards controller
            return RedirectToAction("StockMovementList", "Inwards");
        }

        public IActionResult LogReport()
        {
            ViewBag.actionlist = GetAction(); // Use PascalCase for ViewBag key
            return View();
        }

        private List<SelectListItem> GetAction()
        {

            var lstProducts = new List<SelectListItem>();
            var role = HttpContext.Session.GetString("Role");

            if (role == "ADMIN")
            {
                var segment = _context.logs
                    .Where(a => a.username == HttpContext.Session.GetString("User"))
                    .Select(n => new SelectListItem
                    {
                        Value = n.action,
                        Text = n.action
                    })
                    .Distinct()
                    .ToList();

                var defItem = new SelectListItem()
                {
                    Value = "",
                    Text = "--Select Action--"
                };

                lstProducts.AddRange(segment);
                lstProducts.Insert(0, defItem);
                // Add "All" option to the list
                lstProducts.Add(new SelectListItem { Text = "All", Value = "all" });
                return lstProducts;
            }
            else
            {
                // Return an empty list if the role is not "ADMIN"
                return lstProducts;
            }
        }

        [HttpGet]
        public IActionResult LogReportsearch(string date, string actionname)
        {
            try
            {
                if(actionname == "all")
                {
                    DateTime parsedDate = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    string formattedDate = parsedDate.ToString("dd/MM/yyyy");

                    var logs = _context.logs
                        .Where(a => a.date.Trim() == formattedDate.Trim())
                        .ToList();

                    return Json(logs);
                }
                else
                {
                    DateTime parsedDate = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    string formattedDate = parsedDate.ToString("dd/MM/yyyy");

                    var logs = _context.logs
                        .Where(a => a.date.Trim() == formattedDate.Trim() && a.action.Trim() == actionname.Trim())
                        .ToList();

                    return Json(logs);
                }
                
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        public IActionResult productlocation()
        {
            if (_context.Storage_Operation == null)
            {
                return Problem("Entity set 'AuthDbContext.do_allotment' is null.");
            }
            var storages = _context.Storage_Operation.Where(a => a.statusflag == "ST").OrderByDescending(a => a.id).ToList();
            return View(storages);
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
        public async Task<IActionResult> Damage()
        {

            if (_context.Storage_Operation == null)
            {
                return Problem("Entity set 'AuthDbContext.do_allotment' is null.");
            }
            var storages = _context.Storage_Operation.Where(a => a.statusflag == "DMG").ToList();

            return View(storages);
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

        public IActionResult productreport()
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

                    var groupedData = storeData
                        .GroupBy(q => GetSecondDigit(q.boxno))
                        .ToDictionary(group => group.Key, group => group.Count());

                    var storeDataPI = _context.Storage_Operation
                        .Where(a => a.productcode.Trim().ToUpper() == b && a.statusflag == "PI")
                        .ToList();

                    var groupedDataPI = storeDataPI
                        .GroupBy(q => GetSecondDigit(q.boxno))
                        .ToDictionary(group => group.Key, group => group.Count());

                    int minCount = int.MaxValue; // Initialize with a large number for finding minimum
                    var result = new List<KeyValuePair<string, int>>();

                    int secondDigitCount = 0;
                    int secondDigitCountPI = 0;
                   
                    var box = storeData.Select(a => a.boxno.Trim()).FirstOrDefault();
                    var splitbox = GetSpliBox(box);

                    var possibleBoxes = new List<string>();
                    if(groupedData.Count == 0)
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
                        stcokallocate = secondDigitCountPI,
                        currentqty = minCount, // Assign minimum count as currentqty
                        productname = productName,
                        category = category,
                    };

                    inStockQuantities.Add(instcok);
                }
            }

            inStockQuantities = inStockQuantities.OrderBy(item => item.id).ToList();

            return View(inStockQuantities);


            inStockQuantities = inStockQuantities.OrderBy(item => item.id).ToList();

            return View(inStockQuantities);
            
        }

        //// Find minimum count from groupedData
        //foreach (var kvp in groupedData)
        //{
        //    var count = kvp.Value;
        //    if (count < minCount)
        //    {
        //        minCount = count;
        //    }
        //}
        //List<InStockQty> inStockQuantities = new List<InStockQty>();

        //var indata = _context.inward
        //    .Where(a => a.flag == 1)
        //    .Select(a => a.inward_id)
        //    .ToList();

        //var inPacketData = _context.inwardPacket
        //    .Where(a => indata.Contains(a.inwardId))
        //    .ToList();

        //var productcodes = _context.Storage_Operation
        //    .Where(a => a.statusflag == "PI" || a.statusflag == "ST")
        //    .Select(a => a.productcode.Trim().ToUpper())
        //    .Distinct()
        //    .ToList();

        //var productmasterdata = _context.Product_Master
        //    .Where(a => !productcodes.Contains(a.productcode.Trim().ToUpper()))
        //    .ToList();

        //foreach (var item in productmasterdata)
        //{
        //    var instcok1 = new InStockQty
        //    {
        //        productcode = item.productcode.Trim().ToUpper(),
        //        stcokallocate = 0,
        //        currentqty = 0,
        //        productname = item.productdescription.Trim().ToUpper(),
        //        category = item.categoryname.Trim(),
        //    };
        //    inStockQuantities.Add(instcok1);
        //}

        //foreach (var b in productcodes)
        //{
        //    var productname = _context.Product_Master
        //        .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper())
        //        .Select(a => a.productdescription.Trim().ToUpper())
        //        .FirstOrDefault();
        //    var category = _context.Product_Master
        //        .Where(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper())
        //        .Select(a => a.categoryname.Trim().ToUpper())
        //        .FirstOrDefault();

        //    var storedata = _context.Storage_Operation
        //        .Where(a => a.productcode.Trim().ToUpper() == b && (a.statusflag == "PI" || a.statusflag == "ST"))
        //        .ToList();
        //    var groupedData = storedata
        //        .GroupBy(q => GetSecondDigit(q.boxno))
        //        .ToDictionary(group => group.Key, group => group.Count());
        //    int secondDigitCount = 0;
        //    foreach (var kvp in groupedData)
        //    {
        //        var secondDigit = kvp.Key.Trim();
        //        secondDigitCount = kvp.Value;
        //    }

        //    var storedata1 = _context.Storage_Operation
        //        .Where(a => a.productcode.Trim().ToUpper() == b && a.statusflag == "PI")
        //        .ToList();
        //    var groupedData1 = storedata1
        //        .GroupBy(q => GetSecondDigit(q.boxno))
        //        .ToDictionary(group => group.Key, group => group.Count());

        //    int secondDigitCount1 = 0;
        //    foreach (var kvp in groupedData1)
        //    {
        //        var secondDigit = kvp.Key.Trim();
        //        secondDigitCount1 = kvp.Value;
        //    }

        //    var instcok = new InStockQty
        //    {
        //        productcode = b,
        //        stcokallocate = secondDigitCount1,
        //        currentqty = secondDigitCount,
        //        productname = productname,
        //        category = category,

        //    };

        //    inStockQuantities.Add(instcok);
        //}

        //inStockQuantities = inStockQuantities.OrderBy(item => item.id).ToList();

        //return View(inStockQuantities);
        public ActionResult VarianceReport()
        {
            var variancereport = _context.SaveVariance.Distinct().ToList();
            // Pass the data to the view
            ViewBag.variancereport = variancereport;
            return View(variancereport);
        }

        public IActionResult searchvariance(string selectedValue, string selectedProduct)
        {
            if(selectedValue == null)
            {
                return Ok();
            }

            if (selectedProduct.ToUpper() == "BOX")
            {
                var save = _context.SaveVariance.Where(a => a.physicalid.Trim() == selectedValue.Trim()).ToList();
                return Json(save);
            }
            else
            {
                var save = _context.productwisesave.Where(a => a.physicalid.Trim() == selectedValue.Trim()).ToList();
                return Json(save);
            }

        }


        //public IActionResult downloadexcel(string selectedValue, string selectedProduct)
        //{
        //    if (selectedProduct.ToUpper() == "BOX")
        //    {
        //        var save = _context.SaveVariance.Where(a => a.physicalid == selectedValue).ToList();

        //        if (save == null)
        //        {
        //            return NotFound();
        //        }


        //        using (var excelPackage = new ExcelPackage())
        //        {
        //            // Create a worksheet
        //            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("FG Stock");

        //            worksheet.Cells[1, 1].Value = "EROS GENERAL AGENCIES PVT LTD.";
        //            worksheet.Cells[1, 1, 1, 9].Merge = true; // Merge the heading across the first 8 columns
        //            worksheet.Cells[1, 1].Style.Font.Size = 14; // Increase font size for the heading
        //            worksheet.Cells[1, 1].Style.Font.Bold = true; // Make the heading bold
        //            worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center; // Center align the heading

        //            DateTime currentDate = DateTime.Now;
        //            string heading1 = "Variance Report";
        //            string formattedDate = currentDate.ToString("dd/MM/yyyy");

        //            // Format the time as HH:mm:ss (equivalent to slice(0, 8) in JavaScript)
        //            string formattedTime = currentDate.ToString("HH:mm:ss");


        //            worksheet.Cells[2, 1].Value = $"{heading1} - Generated on: {formattedDate} at {formattedTime}";
        //            worksheet.Cells[2, 1, 2, 9].Merge = true; // Merge the subheading across the first 8 columns
        //            worksheet.Cells[2, 1].Style.Font.Size = 12; // Set font size for the subheading
        //            worksheet.Cells[2, 1].Style.Font.Bold = true; // Make the subheading bold
        //            worksheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center; // Center align the subheading
        //                                                                                                                   // Define headers

        //            worksheet.Cells[3, 1].Value = "PST Id";
        //            worksheet.Cells[3, 2].Value = "BoxNo";
        //            worksheet.Cells[3, 3].Value = "Location";
        //            worksheet.Cells[3, 4].Value = "Product Code";
        //            worksheet.Cells[3, 5].Value = "Batch Code";
        //            worksheet.Cells[3, 6].Value = "PST BoxCount";
        //            worksheet.Cells[3, 7].Value = "Sys BoxCount";
        //            worksheet.Cells[3, 8].Value = "Stock Variance";
        //            worksheet.Cells[3, 9].Value = "Remarks";


        //            // Add more headers as needed
        //            // Apply bold formatting to the header row
        //            //using (var range = worksheet.Cells["A1:K1"])
        //            using (var range = worksheet.Cells["A3:I3"])
        //            {
        //                range.Style.Font.Bold = true;
        //            }

        //            // Populate data
        //            int row = 4;
        //            foreach (var postProcess in save)
        //            {
        //                worksheet.Cells[row, 1].Value = postProcess.physicalid;
        //                worksheet.Cells[row, 2].Value = postProcess.boxno;
        //                worksheet.Cells[row, 3].Value = postProcess.locationcode;
        //                worksheet.Cells[row, 4].Value = postProcess.productcode;
        //                worksheet.Cells[row, 5].Value = postProcess.batchcode;
        //                worksheet.Cells[row, 6].Value = postProcess.storagecount;
        //                worksheet.Cells[row, 7].Value = postProcess.stockvariance;
        //                worksheet.Cells[row, 8].Value = postProcess.pstcount;
        //                worksheet.Cells[row, 9].Value = postProcess.Remark;

        //                // Add more data fields as needed
        //                row++;
        //            }

        //            // Set column widths (adjust as necessary)
        //            worksheet.Column(1).Width = 15; // PST Id
        //            worksheet.Column(2).Width = 20; // Location
        //            worksheet.Column(3).Width = 20; // Location
        //            worksheet.Column(4).Width = 15; // Product Code
        //            worksheet.Column(5).Width = 15; // Batch Code
        //            worksheet.Column(6).Width = 15; // Stock Count
        //            worksheet.Column(7).Width = 15; // Stock Variance
        //            worksheet.Column(8).Width = 15; // PST Count
        //            worksheet.Column(9).Width = 25; // Remarks

        //            var borderStyle = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

        //            // Set borders for header row
        //            using (var range = worksheet.Cells[1, 1, row - 1, 9])
        //            {
        //                range.Style.Border.Top.Style = borderStyle;
        //                range.Style.Border.Bottom.Style = borderStyle;
        //                range.Style.Border.Left.Style = borderStyle;
        //                range.Style.Border.Right.Style = borderStyle;

        //                // Optional: If you want to add a border inside the range (between cells), you can do this:
        //                for (int r = 1; r <= (row - 1); r++)
        //                {
        //                    for (int c = 1; c <= 9; c++)
        //                    {
        //                        // Add inner borders
        //                        if (c < 9) // Horizontal borders
        //                        {
        //                            worksheet.Cells[r, c].Style.Border.Right.Style = borderStyle;
        //                        }
        //                        if (r < (row - 1)) // Vertical borders
        //                        {
        //                            worksheet.Cells[r, c].Style.Border.Bottom.Style = borderStyle;
        //                        }
        //                    }
        //                }
        //            }

        //            // Auto-fit columns for better readability
        //            worksheet.Cells.AutoFitColumns();

        //            // Generate the file content
        //            byte[] fileContents = excelPackage.GetAsByteArray();

        //            // Return the Excel file
        //            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PST Report.xlsx");

        //        }

        //    }
        //    else
        //    {

        //        var save = _context.productwisesave.Where(a => a.physicalid == selectedValue).ToList();

        //        if (save == null)
        //        {
        //            return NotFound();
        //        }


        //        using (var excelPackage = new ExcelPackage())
        //        {

        //            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("FG Stock");
        //            worksheet.Cells[1, 1].Value = "EROS GENERAL AGENCIES PVT LTD.";
        //            worksheet.Cells[1, 1, 1, 6].Merge = true; // Merge the heading across the first 8 columns
        //            worksheet.Cells[1, 1].Style.Font.Size = 14; // Increase font size for the heading
        //            worksheet.Cells[1, 1].Style.Font.Bold = true; // Make the heading bold
        //            worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center; // Center align the heading

        //            DateTime currentDate = DateTime.Now;
        //            string heading1 = "Variance Report";
        //            string formattedDate = currentDate.ToString("dd/MM/yyyy");

        //            // Format the time as HH:mm:ss (equivalent to slice(0, 8) in JavaScript)
        //            string formattedTime = currentDate.ToString("HH:mm:ss");


        //            worksheet.Cells[2, 1].Value = $"{heading1} - Generated on: {formattedDate} at {formattedTime}";
        //            worksheet.Cells[2, 1, 2, 6].Merge = true; // Merge the subheading across the first 8 columns
        //            worksheet.Cells[2, 1].Style.Font.Size = 12; // Set font size for the subheading
        //            worksheet.Cells[2, 1].Style.Font.Bold = true; // Make the subheading bold
        //            worksheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center; // Center align the subheading

        //            // Create a worksheet

        //            // Define headers
        //            //worksheet.Cells[1, 1].Value = "PST Id";
        //            //worksheet.Cells[1, 2].Value = "Product Code";
        //            //worksheet.Cells[1, 3].Value = "Stock Count";
        //            //worksheet.Cells[1, 4].Value = "Stock Variance";
        //            //worksheet.Cells[1, 5].Value = "PST Count";
        //            //worksheet.Cells[1, 6].Value = "Remarks";
        //            worksheet.Cells[3, 1].Value = "PST Id";
        //            worksheet.Cells[3, 2].Value = "Product Code";
        //            worksheet.Cells[3, 3].Value = "Stock Count";
        //            worksheet.Cells[3, 4].Value = "Stock Variance";
        //            worksheet.Cells[3, 5].Value = "PST Count";
        //            worksheet.Cells[3, 6].Value = "Remarks";


        //            // Add more headers as needed
        //            // Apply bold formatting to the header row
        //            using (var range = worksheet.Cells["A1:H3"])
        //            {
        //                range.Style.Font.Bold = true;
        //            }

        //            // Populate data
        //            int row = 4;
        //            foreach (var postProcess in save)
        //            {
        //                worksheet.Cells[row, 1].Value = postProcess.physicalid;
        //                worksheet.Cells[row, 2].Value = postProcess.productcode;
        //                worksheet.Cells[row, 3].Value = postProcess.storagecount;
        //                worksheet.Cells[row, 4].Value = postProcess.stockvariance;
        //                worksheet.Cells[row, 5].Value = postProcess.pstcount;
        //                worksheet.Cells[row, 6].Value = postProcess.remark;


        //                // worksheet.Cells[row, 6].Value = postProcess.shade_descript;

        //                // Add more data fields as needed
        //                row++;
        //            }
        //            // Set column widths (adjust as necessary)
        //            worksheet.Column(1).Width = 15; // PST Id
        //            worksheet.Column(2).Width = 20; // Location
        //            worksheet.Column(3).Width = 15; // Product Code
        //            worksheet.Column(4).Width = 15; // Batch Code
        //            worksheet.Column(5).Width = 15; // Stock Count
        //            worksheet.Column(6).Width = 15; // Stock Variance

        //            var borderStyle = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

        //            // Set borders for header row
        //            using (var range = worksheet.Cells[1, 1, row - 1, 6])
        //            {
        //                range.Style.Border.Top.Style = borderStyle;
        //                range.Style.Border.Bottom.Style = borderStyle;
        //                range.Style.Border.Left.Style = borderStyle;
        //                range.Style.Border.Right.Style = borderStyle;

        //                // Optional: If you want to add a border inside the range (between cells), you can do this:
        //                for (int r = 1; r <= (row - 1); r++)
        //                {
        //                    for (int c = 1; c <= 6; c++)
        //                    {
        //                        // Add inner borders
        //                        if (c < 6) // Horizontal borders
        //                        {
        //                            worksheet.Cells[r, c].Style.Border.Right.Style = borderStyle;
        //                        }
        //                        if (r < (row - 1)) // Vertical borders
        //                        {
        //                            worksheet.Cells[r, c].Style.Border.Bottom.Style = borderStyle;
        //                        }
        //                    }
        //                }
        //            }


        //            // Auto-fit columns for better readability
        //            worksheet.Cells.AutoFitColumns();

        //            // Generate the file content
        //            byte[] fileContents = excelPackage.GetAsByteArray();

        //            // Return the Excel file
        //            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PST Report.xlsx");

        //        }
        //    }

        //}
        public IActionResult downloadexcel(string selectedValue, string selectedProduct)
        {
            if (selectedProduct.ToUpper() == "BOX")
            {
                var save = _context.SaveVariance.Where(a => a.physicalid == selectedValue).ToList();
                if (save == null)
                {
                    return NotFound();
                }

                using (var excelPackage = new ExcelPackage())
                {
                    // Create a worksheet
                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("FG Stock");

                    // Heading and Subheading
                    worksheet.Cells[1, 1].Value = "EROS GENERAL AGENCIES PVT LTD.";
                    worksheet.Cells[1, 1, 1, 9].Merge = true;
                    worksheet.Cells[1, 1].Style.Font.Size = 14;
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    DateTime currentDate = DateTime.Now;
                    string heading1 = "Variance Report";
                    string formattedDate = currentDate.ToString("dd/MM/yyyy");
                    string formattedTime = currentDate.ToString("HH:mm:ss");

                    worksheet.Cells[2, 1].Value = $"{heading1} - Generated on: {formattedDate} at {formattedTime}";
                    worksheet.Cells[2, 1, 2, 9].Merge = true;
                    worksheet.Cells[2, 1].Style.Font.Size = 12;
                    worksheet.Cells[2, 1].Style.Font.Bold = true;
                    worksheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    // Define headers
                    worksheet.Cells[3, 1].Value = "PST Id";
                    worksheet.Cells[3, 2].Value = "BoxNo";
                    worksheet.Cells[3, 3].Value = "Location";
                    worksheet.Cells[3, 4].Value = "Product Code";
                    worksheet.Cells[3, 5].Value = "Batch Code";
                    worksheet.Cells[3, 6].Value = "PST BoxCount";
                    worksheet.Cells[3, 7].Value = "Sys BoxCount";
                    worksheet.Cells[3, 8].Value = "Stock Variance";
                    worksheet.Cells[3, 9].Value = "Remarks";

                    using (var range = worksheet.Cells["A3:I3"])
                    {
                        range.Style.Font.Bold = true;
                    }

                    // Populate data
                    int row = 4;
                    foreach (var postProcess in save)
                    {
                        worksheet.Cells[row, 1].Value = postProcess.physicalid;
                        worksheet.Cells[row, 2].Value = postProcess.boxno;
                        worksheet.Cells[row, 3].Value = postProcess.locationcode;
                        worksheet.Cells[row, 4].Value = postProcess.productcode;
                        worksheet.Cells[row, 5].Value = postProcess.batchcode;
                        worksheet.Cells[row, 6].Value = postProcess.storagecount;
                        worksheet.Cells[row, 7].Value = postProcess.stockvariance;
                        worksheet.Cells[row, 8].Value = postProcess.pstcount;
                        worksheet.Cells[row, 9].Value = postProcess.Remark;
                        row++;
                    }

                    // Column widths
                    worksheet.Column(1).Width = 15;
                    worksheet.Column(2).Width = 20;
                    worksheet.Column(3).Width = 20;
                    worksheet.Column(4).Width = 15;
                    worksheet.Column(5).Width = 15;
                    worksheet.Column(6).Width = 15;
                    worksheet.Column(7).Width = 15;
                    worksheet.Column(8).Width = 15;
                    worksheet.Column(9).Width = 25;

                    // Center last 5 columns for "BOX"
                    for (int i = 6; i <= 9; i++)
                    {
                        worksheet.Column(i).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    }

                    // Apply borders to all cells
                    using (var range = worksheet.Cells[1, 1, row - 1, 9])
                    {
                        range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    }

                    worksheet.Cells.AutoFitColumns();

                    byte[] fileContents = excelPackage.GetAsByteArray();
                    return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PST Report.xlsx");
                }
            }
            else
            {
                // PRODUCT section
                var save = _context.productwisesave.Where(a => a.physicalid == selectedValue).ToList();
                if (save == null)
                {
                    return NotFound();
                }

                using (var excelPackage = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("FG Stock");

                    // Heading and Subheading
                    worksheet.Cells[1, 1].Value = "EROS GENERAL AGENCIES PVT LTD.";
                    worksheet.Cells[1, 1, 1, 6].Merge = true;
                    worksheet.Cells[1, 1].Style.Font.Size = 14;
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    DateTime currentDate = DateTime.Now;
                    string heading1 = "Variance Report";
                    string formattedDate = currentDate.ToString("dd/MM/yyyy");
                    string formattedTime = currentDate.ToString("HH:mm:ss");

                    worksheet.Cells[2, 1].Value = $"{heading1} - Generated on: {formattedDate} at {formattedTime}";
                    worksheet.Cells[2, 1, 2, 6].Merge = true;
                    worksheet.Cells[2, 1].Style.Font.Size = 12;
                    worksheet.Cells[2, 1].Style.Font.Bold = true;
                    worksheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    // Define headers
                    worksheet.Cells[3, 1].Value = "PST Id";
                    worksheet.Cells[3, 2].Value = "Product Code";
                    worksheet.Cells[3, 3].Value = "Stock Count";
                    worksheet.Cells[3, 4].Value = "Stock Variance";
                    worksheet.Cells[3, 5].Value = "PST Count";
                    worksheet.Cells[3, 6].Value = "Remarks";

                    using (var range = worksheet.Cells["A3:F3"])
                    {
                        range.Style.Font.Bold = true;
                    }

                    // Populate data
                    int row = 4;
                    foreach (var postProcess in save)
                    {
                        worksheet.Cells[row, 1].Value = postProcess.physicalid;
                        worksheet.Cells[row, 2].Value = postProcess.productcode;
                        worksheet.Cells[row, 3].Value = postProcess.storagecount;
                        worksheet.Cells[row, 4].Value = postProcess.stockvariance;
                        worksheet.Cells[row, 5].Value = postProcess.pstcount;
                        worksheet.Cells[row, 6].Value = postProcess.remark;
                        row++;
                    }

                    // Column widths
                    worksheet.Column(1).Width = 15;
                    worksheet.Column(2).Width = 20;
                    worksheet.Column(3).Width = 15;
                    worksheet.Column(4).Width = 15;
                    worksheet.Column(5).Width = 15;
                    worksheet.Column(6).Width = 15;

                    // Center last 4 columns for "PRODUCT"
                    for (int i = 3; i <= 6; i++)
                    {
                        worksheet.Column(i).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    }

                    // Apply borders to all cells
                    using (var range = worksheet.Cells[1, 1, row - 1, 6])
                    {
                        range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    }

                    worksheet.Cells.AutoFitColumns();

                    byte[] fileContents = excelPackage.GetAsByteArray();
                    return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PST Report.xlsx");
                }
            }
        }

        public IActionResult Box(string selectedValue, string pstid)
        {
            if (selectedValue.ToUpper() == "BOX")
            {
                var variancereport = _context.SaveVariance.Distinct().Where(a => a.physicalid.Trim() == pstid.Trim()).ToList();
                // Pass the data to the view
                //ViewBag.variancereport = variancereport;
                return Json(variancereport);
            }
            else
            {
                var variancereport = _context.productwisesave.Distinct().Where(a=>a.physicalid.Trim() == pstid.Trim()).ToList();
                // Pass the data to the view
                //ViewBag.variancereport = variancereport;
                return Json(variancereport);

            }

        }

    }
}
