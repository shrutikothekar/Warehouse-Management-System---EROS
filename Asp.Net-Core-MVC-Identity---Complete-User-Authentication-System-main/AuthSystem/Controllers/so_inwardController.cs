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
using NuGet.Packaging;
using System.Collections;
using AspNetCoreHero.ToastNotification.Notyf;
using Elasticsearch.Net;
using System.Globalization;
using Newtonsoft.Json;
using System.Text.Json;

namespace eros.Controllers
{
    public class so_inwardController : Controller
    {
        private readonly ErosDbContext _context;
        public INotyfService _notyfService { get; }

        public so_inwardController(ErosDbContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;

        }
        public ActionResult FilterDataByDate(string status, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                List<so_inward> pendingOrders = new List<so_inward>();

                var ddisDate = "";
                if (status == null)
                {
                    status = "Pending";
                    // Build the query based on the provided dates
                    var purchaseQuery = _context.so_inward
                        .Where(po => po.status.Trim() == status.Trim()).OrderByDescending(a => a.sodate.Trim()).ToList();
                    if (fromDate.HasValue && toDate.HasValue)
                    {
                        purchaseQuery = purchaseQuery.Where(po => DateTime.TryParse(po.sodate, out var purchaseDate) &&
                                                                    purchaseDate >= fromDate &&
                                                                    purchaseDate <= toDate)
                                                     .ToList();
                    }
                    else if (fromDate.HasValue)
                    {
                        purchaseQuery = purchaseQuery.Where(po => DateTime.TryParse(po.sodate, out var purchaseDate) &&
                                                                    purchaseDate >= fromDate &&
                                                                    purchaseDate <= DateTime.Today)
                                                     .ToList();
                    }

                    var purchaseList = purchaseQuery
                        .OrderByDescending(po => po.id)
                        .ToList();

                    foreach (var data in purchaseList)
                    {
                        var disDate = _context.Loading_Dispatch_Operation.Where(a => a.sono == data.sono).FirstOrDefault();
                        if (disDate != null)
                        {
                            ddisDate = disDate.currentdate;
                        }
                        else
                        {
                            ddisDate = "NA";
                        }

                        var purchaseSum = _context.so_product
                            .Where(a => a.orderid == data.id)
                            .Sum(a => a.quantity);

                        var inwardDetails = _context.inwardPacket
                            .Where(a => a.sono == data.sono)
                            .GroupBy(p => p.sono)
                            .Select(group => new
                            {
                                ProductName = group.Key,
                                TotalQuantity = group.Sum(p => p.quantity),
                                TotalSubAssembly = group.Sum(p => p.totalsubassmbly)
                            })
                            .FirstOrDefault();

                        if (inwardDetails == null)
                        {
                            data.qty = purchaseSum;
                            data.pqty = purchaseSum;
                            data.dDate = ddisDate;
                        }
                        else
                        {
                            data.qty = purchaseSum;
                            data.pqty = purchaseSum - inwardDetails.TotalQuantity;

                            if (data.pqty < 0)
                            {
                                data.pqty = 0;
                            }
                            data.dDate = ddisDate;
                        }

                        pendingOrders.Add(data);
                    }

                }
                else if (status == "All")
                {
                    var purchaseQuery = _context.so_inward
                        .Where(po => po.status.Trim() == "Pending" || po.status == "Completed").OrderByDescending(a => a.sodate.Trim()).ToList();
                    if (fromDate.HasValue && toDate.HasValue)
                    {
                        purchaseQuery = purchaseQuery.Where(po => DateTime.TryParse(po.sodate, out var purchaseDate) &&
                                                                    purchaseDate >= fromDate &&
                                                                    purchaseDate <= toDate)
                                                     .ToList();
                    }
                    else if (fromDate.HasValue)
                    {
                        purchaseQuery = purchaseQuery.Where(po => DateTime.TryParse(po.sodate, out var purchaseDate) &&
                                                                    purchaseDate >= fromDate &&
                                                                    purchaseDate <= DateTime.Today)
                                                     .ToList();
                    }

                    var purchaseList = purchaseQuery
                        .OrderByDescending(po => po.id)
                        .ToList();

                    foreach (var data in purchaseList)
                    {
                        var disDate = _context.Loading_Dispatch_Operation.Where(a => a.sono == data.sono).FirstOrDefault();
                        if (disDate != null)
                        {
                            ddisDate = disDate.currentdate;
                        }
                        else
                        {
                            ddisDate = "NA";
                        }

                        var purchaseSum = _context.so_product
                            .Where(a => a.orderid == data.id)
                            .Sum(a => a.quantity);

                        var inwardDetails = _context.inwardPacket
                            .Where(a => a.sono == data.sono)
                            .GroupBy(p => p.sono)
                            .Select(group => new
                            {
                                ProductName = group.Key,
                                TotalQuantity = group.Sum(p => p.quantity),
                                TotalSubAssembly = group.Sum(p => p.totalsubassmbly)
                            })
                            .FirstOrDefault();

                        if (inwardDetails == null)
                        {
                            data.qty = purchaseSum;
                            data.pqty = purchaseSum;
                            data.dDate = ddisDate;
                        }
                        else
                        {
                            data.qty = purchaseSum;
                            data.pqty = purchaseSum - inwardDetails.TotalQuantity;

                            if (data.pqty < 0)
                            {
                                data.pqty = 0;
                            }
                            data.dDate = ddisDate;
                        }

                        pendingOrders.Add(data);
                    }

                }
                else
                {
                    var purchaseQuery = _context.so_inward
                        .Where(po => po.status.Trim() == status.Trim()).OrderByDescending(a => a.sodate.Trim()).ToList();
                    if (fromDate.HasValue && toDate.HasValue)
                    {
                        purchaseQuery = purchaseQuery.Where(po => DateTime.TryParse(po.sodate, out var purchaseDate) &&
                                                                    purchaseDate >= fromDate &&
                                                                    purchaseDate <= toDate)
                                                     .ToList();
                    }
                    else if (fromDate.HasValue)
                    {
                        purchaseQuery = purchaseQuery.Where(po => DateTime.TryParse(po.sodate, out var purchaseDate) &&
                                                                    purchaseDate >= fromDate &&
                                                                    purchaseDate <= DateTime.Today)
                                                     .ToList();
                    }

                    var purchaseList = purchaseQuery
                        .OrderByDescending(po => po.id)
                        .ToList();

                    foreach (var data in purchaseList)
                    {
                        var disDate = _context.Loading_Dispatch_Operation.Where(a => a.sono == data.sono).FirstOrDefault();
                        if (disDate != null)
                        {
                            ddisDate = disDate.currentdate;
                        }
                        else
                        {
                            ddisDate = "NA";
                        }

                        var purchaseSum = _context.so_product
                            .Where(a => a.orderid == data.id)
                            .Sum(a => a.quantity);

                        var inwardDetails = _context.inwardPacket
                            .Where(a => a.sono == data.sono)
                            .GroupBy(p => p.sono)
                            .Select(group => new
                            {
                                ProductName = group.Key,
                                TotalQuantity = group.Sum(p => p.quantity),
                                TotalSubAssembly = group.Sum(p => p.totalsubassmbly)
                            })
                            .FirstOrDefault();

                        if (inwardDetails == null)
                        {
                            data.qty = purchaseSum;
                            data.pqty = purchaseSum;
                            data.dDate = ddisDate;
                        }
                        else
                        {
                            data.qty = purchaseSum;
                            data.pqty = purchaseSum - inwardDetails.TotalQuantity;

                            if (data.pqty < 0)
                            {
                                data.pqty = 0;
                            }
                            data.dDate = ddisDate;
                        }

                        pendingOrders.Add(data);
                    }

                }


                return PartialView("_PendingOrdersPartial", pendingOrders); // Ensure this partial view exists


            }
            catch (Exception ex)
            {
                // Log the exception (use your logging mechanism)
                // Return an error view or a JSON response indicating failure
                return View("Internal server error: " + ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetSonoData(string orderType)
        {
            int nextNumber = 1;
            string seriesPrefix = "";
            string sonoData = ""; // Initialize sonoData as null

            if (orderType == "sono")
            {
                seriesPrefix = "SONO/SO/";
                var lastSono = _context.so_inward
                    .Where(e => e.sono.StartsWith("SONO/SO/")) // Filter by prefix
                    .OrderByDescending(e => e.sono)
                    .Select(e => e.sono)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(lastSono))
                {
                    var parts = lastSono.Split('/');
                    if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }
                sonoData = $"{seriesPrefix}{nextNumber}";
            }
            else if (orderType == "dono")
            {
                seriesPrefix = "DONO/SO/";
                var lastDono = _context.so_inward
                    .Where(e => e.sono.StartsWith("DONO/SO/")) // Filter by prefix
                    .OrderByDescending(e => e.sono)
                    .Select(e => e.sono)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(lastDono))
                {
                    var parts = lastDono.Split('/');
                    if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }
                sonoData = $"{seriesPrefix}{nextNumber}";
            }

            return Content(sonoData);
        }

        [HttpPost]
        public ActionResult SoCheck(string sono)
        {
            var sonoo = sono.Trim();
            // Logic to handle POST request
            if (sono != null)
            {
                var find = _context.so_inward.Where(a => a.sono.Trim() == sono).FirstOrDefault();
                if(find != null)
                {
                    return Json(new { success = false, message = "sale order number "+ sonoo  + " is alread in used !" });
                }
                else
                {
                    // Sale order number is not in use
                    return Json(new { success = true, message = "Sale order number is available." });
                }
            }
            return Json(new { success = true, message = "Sale order number is available." });
        }
        private List<SelectListItem> Getcategoryname()
        {
            var lstProducts = new List<SelectListItem>();

            lstProducts = _context.category_master.AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.categoryname,
                Text = n.categoryname
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select Category----"
            };

            lstProducts.Insert(0, defItem);

            return lstProducts;
        }
        private List<SelectListItem> Getsubcategory()
        {
            var lstProducts = new List<SelectListItem>();
            lstProducts = _context.subcategory_Master.AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.subcategory_name,
                Text = n.subcategory_name
            }).ToList();
            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select subcategory-Name----"
            };
            lstProducts.Insert(0, defItem);
            return lstProducts;
        }

        [HttpGet]
        public IActionResult CreateViewProduct()
        {
            ViewBag.categoryname = Getcategoryname();
            ViewBag.subcategory = Getsubcategory();
            return PartialView();
        }
        
        [HttpGet]
        public async Task<IActionResult> Index()
        {

            //var pendingOrders =  _context.so_inward.Where(a => a.status == "Pending").OrderByDescending
            //    (a=>a.id).ToList();
            //return View(pendingOrders);
            var pendingOrders = _context.so_inward.OrderByDescending(a => a.id).ToList();

            return View(pendingOrders);

        }
        public IActionResult GetProducts()
        {
            var products = _context.Product_Master.Select(a=>a.categoryname).Distinct().ToList(); // Fetch products from the database
            return Json(products); // Return products as JSON 
        }
        public IActionResult Getdess(Product_Master formData)
        {
            int maxId = _context.Product_Master.Any() ? _context.Product_Master.Max(e => e.id) : 0 + 1;
            if (formData.subcategory == null && formData.hsncode == null)
            {
                formData.subcategory = "-";
                formData.hsncode = "-";
            }
            var productMaster = new Product_Master
            {
                id = maxId+1,
                categorycode = formData.categorycode,
                categoryname = formData.categoryname,
                subcategory = formData.subcategory,
                productcode = formData.productcode.ToUpper(),
                productdescription = formData.productdescription.ToUpper(),
                brand = formData.brand,
                hsncode = formData.hsncode,
                uom = formData.uom,
            };
            
            _context.Product_Master.Add(productMaster);
            _context.SaveChanges();

            var descriptions = _context.Product_Master.OrderBy(p => p.productdescription)
                .Select(p => p.productdescription.ToUpper())
                
                .Distinct()
                .ToList();

            return Json(new { success = true, descriptions = descriptions });
            //return Json(new { success = true, descriptions = formData.productdescription.ToUpper() });
        }
        public IActionResult Getcuss(Customer_Master formData)
        {
            int maxId = _context.Customer_Master.Any() ? _context.Customer_Master.Max(e => e.customer_id) + 1 : 1;
            formData.customername = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(formData.customername.ToLower());
            formData.contactperson = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(formData.contactperson.ToLower());
            formData.address = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(formData.address.ToLower());
            formData.city = formData.city.ToUpper();
            formData.state = formData.state.ToUpper();
            formData.Country = formData.Country.ToUpper();
            //if (formData.emailid == null)
            //{
            //    formData.emailid = "-";
            //}
            //else
            //{
            //    formData.emailid = formData.emailid.ToLower();
            //}
            formData.emailid = string.IsNullOrEmpty(formData.emailid) ? "-" : formData.emailid.ToLower();

            List<consignee> consignees = new List<consignee>();
            var Customer_Master = new Customer_Master
            {
                customer_id = maxId,
                customername = formData.customername,
                contactperson = formData.contactperson,
                address = formData.address,
                city = formData.city,
                state = formData.state,
                pincode = formData.pincode,
                contactno = formData.contactno,
                emailid = formData.emailid,
                gstno = formData.gstno,
                Country = formData.Country,
                
            };

            _context.Customer_Master.Add(Customer_Master);
            _context.SaveChanges();

            // Retrieve the newly generated customer_id
            int newCustomerId = Customer_Master.customer_id;

            foreach (var rowData in formData.Consignee_masters)
            {
                consignee consignee = new consignee
                {
                    customerid = maxId,
                    //consigneename = rowData.consigneename,
                    //consigneeperson = rowData.consigneeperson,
                    //consigneecontact = rowData.consigneecontact,
                    //consigneeaddress = rowData.consigneeaddress,
                    //consigneecity = rowData.consigneecity,
                    //consigneeemail = rowData.consigneeemail,
                    //consigneepincode = rowData.consigneepincode,
                    //consigneestate = rowData.consigneestate,

                    consigneename = string.IsNullOrEmpty(rowData.consigneename) ? "-" : rowData.consigneename,
                    consigneeperson = string.IsNullOrEmpty(rowData.consigneeperson) ? "-" : rowData.consigneeperson,
                    consigneecontact = string.IsNullOrEmpty(rowData.consigneecontact) ? "-" : rowData.consigneecontact,
                    consigneeaddress = string.IsNullOrEmpty(rowData.consigneeaddress) ? "-" : rowData.consigneeaddress,
                    consigneecity = string.IsNullOrEmpty(rowData.consigneecity) ? "-" : rowData.consigneecity,
                    consigneeemail = string.IsNullOrEmpty(rowData.consigneeemail) ? "-" : rowData.consigneeemail,
                    consigneepincode = string.IsNullOrEmpty(rowData.consigneepincode) ? "-" : rowData.consigneepincode,
                    consigneestate = string.IsNullOrEmpty(rowData.consigneestate) ? "-" : rowData.consigneestate,
                };
                consignees.Add(consignee);
            }

            _context.consignee.AddRange(consignees);
            _context.SaveChanges();

            var descriptions = _context.Customer_Master
                .Select(p => p.customername)
                .Distinct()
                .ToList();

            return Json(new { success = true, descriptions = descriptions });
        }

        [HttpPost]
        public IActionResult checkquantity(string productcode, string quantity)
        {
            var productcodes = _context.Storage_Operation.Where(a => a.productcode.Trim().ToUpper() == productcode.Trim().ToUpper()).Select(a => a.productcode.Trim().ToUpper()).Distinct().ToList();

            List<InStockQty> inStockQuantities = new List<InStockQty>();

            foreach (var b in productcodes)
            {
                var storedata = _context.Storage_Operation
                    .Where(a => a.productcode.Trim().ToUpper() == b.ToUpper() && a.statusflag == "ST")// && a.statusflag == "ST" && a.statusflag == "PI"
                    .ToList();

                var allotstock = _context.pickstorage.Where(a => a.productcode.Trim().ToUpper() == productcode.Trim().ToUpper() && a.flag == 0).ToList();

                foreach (var rr in allotstock)
                {
                    var newitem = storedata.Where(r => r.productcode.Trim().ToUpper() == rr.productcode.Trim().ToUpper() && r.batchcode.Trim() == rr.batchcode.Trim() && r.boxno.Trim() == rr.boxno.Trim() && r.locationcode.Trim() == rr.location.Trim()).FirstOrDefault();
                    if (newitem != null)
                    {
                        storedata.Remove(newitem);
                    }
                }

                var groupedData = storedata
                    .GroupBy(q => GetSecondDigit(q.boxno.Trim()))
                    .ToDictionary(group => group.Key, group => group.Count());
                int minCount = groupedData.Values.Min();

                int secondDigitCount = 0;
                int secondDigitCount1 = 0;
                foreach (var kvp in groupedData)
                {
                    var secondDigit = kvp.Key.Trim();
                    secondDigitCount = kvp.Value;
                }
                var instcok = new InStockQty
                {
                    productcode = b.ToUpper(),
                    stcokallocate = secondDigitCount1,
                    currentqty = secondDigitCount,
                };
                inStockQuantities.Add(instcok);

                // Check if the available quantity is less than the requested quantity
                foreach (var item in inStockQuantities)
                {
                    if (item.currentqty == 0)
                    {
                        return Json(new { isQuantityAvailable = true });
                    }
                    else if (item.currentqty < int.Parse(quantity))
                    {
                        return Json(new { isQuantityAvailable = false, currentqty = minCount });
                        //return Json(new { isQuantityAvailable = false, currentqty = item.currentqty });
                    }
                    else if (item.currentqty == int.Parse(quantity))
                    {
                        return Ok();
                    }
                    else if (item.currentqty > int.Parse(quantity))
                    {
                        return Ok();
                    }
                }
            }
            return Json(new { isQuantityAvailable = true });
        }
        private string GetSecondDigit(string boxno)
        {
            string[] parts = boxno.Split('-');
            return parts.Length == 2 ? parts[1].Trim() : string.Empty;
        }
        
        //public ActionResult SalesPendingList()
        //{
        //    var purchase = _context.so_inward.Where(a => a.status == "Pending").OrderByDescending(a => a.sodate).ToList(); // Get a list of pending purchase orders

        //    List<so_inward> pendingOrders = new List<so_inward>(); // Create an empty list
        //    foreach (var data in purchase)
        //    {
        //        var purchasesum = _context.so_product.Where(a => a.orderid == data.id).Sum(a => a.quantity);
        //        var inwarddetails = _context.inwardPacket.Where(a => a.sono == data.sono)
        //                  .GroupBy(p => p.sono)
        //                           .Select(group => new
        //                           {
        //                               ProductName = group.Key,
        //                               TotalQuantity = group.Sum(p => p.quantity),
        //                               TotalSubAssembly = group.Sum(p => p.totalsubassmbly)
        //                           }).FirstOrDefault();
        //        //var inwarddetails = _context.inward.Where(a => a.pono == data.pono && a.flag == 1).AsNoTracking().ToList();
        //        if (inwarddetails == null)
        //        {
        //            data.qty = purchasesum;
        //            data.pqty = purchasesum;
        //        }
        //        else
        //        {
        //            data.qty = purchasesum; // Setting qty to purchasesum
        //            data.pqty = purchasesum - inwarddetails.TotalQuantity;
        //        }
        //        pendingOrders.Add(data);
        //    }

        //    return View("SalesPendingList", pendingOrders);
        //}

        public ActionResult SalesPendingList()
        {
            List<so_inward> pendingOrders = new List<so_inward>(); // Create an empty list

            //var purchase = _context.so_inward
            //    .Where(a => a.status == "Pending").OrderByDescending(a => a.id).ToList(); // Get a list of pending purchase orders
            var purchase = _context.so_inward
                    .Where(po => po.status == "Pending").OrderByDescending(po => po.id).AsNoTracking()
                    .ToList();
            var ddisDate="";
            foreach (var data in purchase)
            {
                var peoductdetails = _context.so_product.Where(a => a.orderid == data.id).FirstOrDefault();
                var purchasesum = _context.so_product.Where(a => a.orderid == data.id).Sum(a => a.quantity);
                var disDate = _context.Loading_Dispatch_Operation.Where(a => a.sono == data.sono).FirstOrDefault();
                if (disDate != null)
                {
                     ddisDate = disDate.currentdate;
                }
                else
                {
                     ddisDate = "NA";
                }
                var inwarddetails = _context.Picklist_Generation.Where(a => a.sono == data.sono)
                          .GroupBy(p => p.sono)
                                   .Select(group => new
                                   {
                                       ProductName = group.Key,
                                       Soqty = group.Sum(p => Convert.ToInt32(p.soqty)),
                                       Pickqty = group.Sum(p => Convert.ToInt32(p.pickingqty)),
                                   }).FirstOrDefault();
                //var inwarddetails = _context.inward.Where(a => a.pono == data.pono && a.flag == 1).AsNoTracking().ToList();
                if (inwarddetails == null)
                {
                    data.qty = purchasesum;
                    data.pqty = purchasesum;
                    data.dDate = ddisDate;
                }
                else
                {
                    data.qty = inwarddetails.Soqty; // Setting qty to purchasesum
                    data.pqty = Convert.ToInt32(inwarddetails.Soqty) - Convert.ToInt32(inwarddetails.Pickqty);// Ensure pqty is not negative
                    if (data.pqty < 0)
                    {
                        data.pqty = 0;
                    }
                    data.dDate = ddisDate;
                }
                pendingOrders.Add(data);
            }

            return View("SalesPendingList", pendingOrders);
        }
        public ActionResult _partialSubComponent(string productcode, string description, int quantity)
        {
            try
            {
                var so_inward = new so_inward();
                var so_inward1 = _context.Product_Master.FirstOrDefault(p => p.productdescription.ToUpper() == description.ToUpper() && p.productcode.ToUpper() == productcode.ToUpper());
                if (so_inward1 != null)
                {
                    var poproductDetailsList = _context.productmaster_Packet
                        .Where(pd => pd.productmasterId == so_inward1.id)
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
                            if (item.subcomponents != "-" && item.subcomponentcode != "-" && item.qty != 0 && item.uom != "-")
                            {
                                so_inward.so_subcomponent.Add(new so_subcomponent
                                {
                                    subcomponents = item.subcomponents,
                                    sccode = item.subcomponentcode,
                                    scqty = item.qty,
                                    scuom = item.uom,
                                    tqty = (quantity * item.qty)// Assign 'tqty'
                                });
                            }
                        }
                        return View(so_inward); // Return 'poProduct_details' instead of 'poproductDetailsList'
                    }
                    else
                    {
                        return View(so_inward);
                    }
                }
                else
                {
                    return Json(new { message = $"No product details found for pono: {productcode} " });
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        //public IActionResult _partialSubComponent(string description, int qty)
        //{
        //    so_inward soinward = new so_inward();

        //    var product = _context.Product_Master
        //        .Include(e => e.Productmaster_Packets)
        //        .FirstOrDefault(p => p.productdescription == description);

        //    so_product so_Product = new so_product();

        //    if (product != null)
        //    {
        //        bool productType = product.TypeOfProduct;

        //        if (productType == false)
        //        {
        //            var so_inward = new so_inward();

        //            var packets = product.Productmaster_Packets;

        //            if (packets != null && packets.Any())
        //            {
        //                int i = 0;
        //                foreach (var packet in packets)
        //                {
        //                    i++;
        //                    so_inward.soProduct_details.Add(new so_product()
        //                    {
        //                        id = i,
        //                        //subcomponents = packet.subcomponents,
        //                        //scuom = packet.uom,
        //                        //scqty = packet.qty,
        //                        //sccode = packet.subcomponentcode,
        //                        //tqty = (packet.qty * qty),
        //                    });
        //                }
        //                return PartialView(so_inward);
        //            }
        //        }
        //        else if (productType == true)
        //        {
        //            return Content("No details found!");
        //            //other wise  subcomponents = packet.subcomponents,scuom ,scqty ,sccode  all should be null 
        //        }
        //        else
        //        {
        //            return NotFound();
        //        }
        //    }

        //    // If the category is not "Chairs" or no packets are available, you can return an empty partial view or handle it as per your requirements.
        //    // Replace "YourEmptyView" with the actual name of your empty view.
        //    return Ok();
        //}
        public IActionResult ActionName_description(string description)
        {
            so_inward so_inward = new so_inward();

            var product = _context.Product_Master
                .Include(e => e.Productmaster_Packets)
                .FirstOrDefault(p => p.productdescription.ToUpper() == description.ToUpper());

            if (product != null)
            {
                bool productType = product.TypeOfProduct;

                var result = new
                {
                    id = product.id,
                    modelno = product.productcode,
                    brand = product.brand,
                    uom = product.uom,
                    hsncode = product.hsncode,
                    producttype = productType,
                };
                return Json(result);
            }
            return Ok();
        }

        private List<SelectListItem> Getdescription()
        {
            var lstProducts = new List<SelectListItem>();
                            lstProducts = _context.Product_Master
                            .AsNoTracking()
                            .OrderBy(n => n.productdescription.ToUpper()) // Order by product description in ascending order
                            .Select(n =>
                            new SelectListItem
                            {
                                Value = n.productdescription.ToUpper(),
                                Text = n.productdescription.ToUpper()
                            }).ToList();
                            var defItem = new SelectListItem()
                            {
                                Value = "",
                                Text = "Select ProductName "
                            };
                            var defItem1 = new SelectListItem()
                            {
                                Value = "addNewPro",
                                Text = " -- Add New ProductName -- "
                            };
                            lstProducts.Insert(0, defItem);
                            lstProducts.Insert(lstProducts.Count(), defItem1);
                            return lstProducts;
        }
        public IActionResult ActionName_customername(string selectedValue)
        {
            var wbridge = _context.Customer_Master.Where(n => n.customername == selectedValue).FirstOrDefault();
            return Json(wbridge);
        }
        private List<SelectListItem> Getcustomername()
        {
            var lstProducts = new List<SelectListItem>();

            lstProducts = _context.Customer_Master.AsNoTracking().OrderBy(n => n.customername).Select(n =>
            new SelectListItem
            {
                Value = n.customername,
                Text = n.customername
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "--- Select Customer Name ---"
            };
            var defItem1 = new SelectListItem()
            {
                Value = "addNewCus",
                Text = "--- Add New CustomerName ---"
            };

            lstProducts.Insert(0, defItem);
            //lstProducts.Count();
            lstProducts.Insert(lstProducts.Count(), defItem1);

            return lstProducts;
        }
        //public class checkStatus
        //{
        //    public string statusFound { get; set; }
        //}
        public async Task<IActionResult> PendingDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var so_inward = await _context.so_inward.FirstOrDefaultAsync(m => m.id == id);
            if (so_inward == null)
            {
                return NotFound();
            }
            so_inward so_inward1 = _context.so_inward.Include(e => e.soProduct_details).Where(a => a.id == id).FirstOrDefault();

            return View("PendingDetails", so_inward1);
        }
        public ActionResult OutwardPendingDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var so_inward = _context.so_inward.FirstOrDefaultAsync(m => m.id == id);
            if (so_inward == null)
            {
                return NotFound();
            }
            so_inward so_inwards = _context.so_inward.Include(e => e.soProduct_details).Where(a => a.id == id).FirstOrDefault();

            return View(so_inwards);
        }
        public ActionResult GetPendingOrders(string status)// && a.flag==1
        {
            List<so_inward> pendingOrders = new List<so_inward>();
            var ddisDate = "";
            //show pending , complete , all 
            if (status == "Pending")
            {
                var purchase = _context.so_inward
                    .Where(po => po.status == status).OrderByDescending(a => a.id).AsNoTracking()
                    .ToList();
                
                foreach (var data in purchase.OrderByDescending(po => po.id))
                {
                    var peoductdetails = _context.so_product.Where(a => a.orderid == data.id).FirstOrDefault();
                    var purchasesum = _context.so_product.Where(a => a.orderid == data.id).Sum(a => a.quantity);
                    var disDate = _context.Loading_Dispatch_Operation.Where(a => a.sono == data.sono).FirstOrDefault();
                    if (disDate != null)
                    {
                        ddisDate = disDate.currentdate;
                    }
                    else
                    {
                        ddisDate = "NA";
                    }
                    var inwarddetails = _context.Picklist_Generation.Where(a => a.sono == data.sono)
                              .GroupBy(p => p.sono)
                                       .Select(group => new
                                       {
                                           ProductName = group.Key,
                                           Soqty = group.Sum(p => Convert.ToInt32(p.soqty)),
                                           Pickqty = group.Sum(p => Convert.ToInt32(p.pickingqty)),
                                       }).FirstOrDefault();

                    if (inwarddetails == null)
                    {
                        data.qty = purchasesum;
                        data.pqty = purchasesum; data.dDate = ddisDate;
                    }
                    else
                    {
                        data.qty = inwarddetails.Soqty; // Setting qty to purchasesum
                        data.pqty = Convert.ToInt32(inwarddetails.Soqty) - Convert.ToInt32(inwarddetails.Pickqty);
                        // Ensure pqty is not negative
                        if (data.pqty < 0)
                        {
                            data.pqty = 0;
                        }
                        data.dDate = ddisDate;
                    }
                    pendingOrders.Add(data);
                }
            }
            else if (status == "Return")
            {
                var purchase = _context.so_inward
                    .Where(po => po.status == status).OrderByDescending(a => a.id).AsNoTracking()
                    .ToList();
                foreach (var data in purchase.OrderByDescending(po => po.id))
                {
                    var peoductdetails = _context.so_product.Where(a => a.orderid == data.id).FirstOrDefault();
                    var purchasesum = _context.so_product.Where(a => a.orderid == data.id).Sum(a => a.quantity); 
                    var disDate = _context.Loading_Dispatch_Operation.Where(a => a.sono == data.sono).FirstOrDefault();
                    if (disDate != null)
                    {
                        ddisDate = disDate.currentdate;
                    }
                    else
                    {
                        ddisDate = "NA";
                    }
                    var inwarddetails = _context.Picklist_Generation.Where(a => a.sono == data.sono)
                              .GroupBy(p => p.sono)
                                       .Select(group => new
                                       {
                                           ProductName = group.Key,
                                           Soqty = group.Sum(p => Convert.ToInt32(p.soqty)),
                                           Pickqty = group.Sum(p => Convert.ToInt32(p.pickingqty)),
                                       }).FirstOrDefault();

                    if (inwarddetails == null)
                    {
                        data.qty = purchasesum;
                        data.pqty = purchasesum; 
                        data.dDate = ddisDate;
                    }
                    else
                    {
                        data.qty = inwarddetails.Soqty; // Setting qty to purchasesum
                        data.pqty = Convert.ToInt32(inwarddetails.Soqty) - Convert.ToInt32(inwarddetails.Pickqty);
                        // Ensure pqty is not negative
                        if (data.pqty < 0)
                        {
                            data.pqty = 0;
                        }
                        data.dDate = ddisDate;
                    }
                    pendingOrders.Add(data);
                }
            }
            else if (status != "All")
            {
                
                var purchase = _context.so_inward
                  .Where(po => po.status == status).OrderByDescending(a => a.id).AsNoTracking()
                  .ToList();
                foreach (var data in purchase.OrderByDescending(po => po.id))
                {
                    var peoductdetails = _context.so_product.Where(a => a.orderid == data.id).FirstOrDefault();
                    var purchasesum = _context.so_product.Where(a => a.orderid == data.id).Sum(a => a.quantity); 
                    var disDate = _context.Loading_Dispatch_Operation.Where(a => a.sono == data.sono).FirstOrDefault();
                    if (disDate != null)
                    {
                        ddisDate = disDate.currentdate;
                    }
                    else
                    {
                        ddisDate = "NA";
                    }
                    var inwarddetails = _context.Picklist_Generation.Where(a => a.sono == data.sono)
                              .GroupBy(p => p.sono)
                                       .Select(group => new
                                       {
                                           ProductName = group.Key,
                                           Soqty = group.Sum(p => Convert.ToInt32(p.soqty)),
                                           Pickqty = group.Sum(p => Convert.ToInt32(p.pickingqty)),
                                       }).FirstOrDefault();
                    //var inwarddetails = _context.inward.Where(a => a.pono == data.pono && a.flag == 1).AsNoTracking().ToList();
                    if (inwarddetails == null)
                    {
                        data.qty = purchasesum;
                        data.pqty = purchasesum; 
                        data.dDate = ddisDate;
                    }
                    else
                    {
                        data.qty = inwarddetails.Soqty; // Setting qty to purchasesum
                        data.pqty = Convert.ToInt32(inwarddetails.Soqty) - Convert.ToInt32(inwarddetails.Pickqty);
                        // Ensure pqty is not negative
                        if (data.pqty < 0)
                        {
                            data.pqty = 0;
                        }
                        data.dDate = ddisDate;
                    }
                    pendingOrders.Add(data);
                }
            }
            else
            {
                var purchase = _context.so_inward.OrderByDescending(a => a.id).ToList();
                foreach (var data in purchase.OrderByDescending(po => po.id))
                {
                    var peoductdetails = _context.so_product.Where(a => a.orderid == data.id).FirstOrDefault();
                    var purchasesum = _context.so_product.Where(a => a.orderid == data.id).Sum(a => a.quantity);
                    var disDate = _context.Loading_Dispatch_Operation.Where(a => a.sono == data.sono).FirstOrDefault();
                    if (disDate != null)
                    {
                        ddisDate = disDate.currentdate;
                    }
                    else
                    {
                        ddisDate = "NA";
                    }
                    var inwarddetails = _context.Picklist_Generation.Where(a => a.sono == data.sono)
                              .GroupBy(p => p.sono)
                                       .Select(group => new
                                       {
                                           ProductName = group.Key,
                                           Soqty = group.Sum(p => Convert.ToInt32(p.soqty)),
                                           Pickqty = group.Sum(p => Convert.ToInt32(p.pickingqty)),
                                       }).FirstOrDefault();
                    //var inwarddetails = _context.inward.Where(a => a.pono == data.pono && a.flag == 1).AsNoTracking().ToList();
                    if (inwarddetails == null)
                    {
                        data.qty = purchasesum;
                        data.pqty = purchasesum; data.dDate = ddisDate;
                    }
                    else
                    {
                        data.qty = inwarddetails.Soqty; // Setting qty to purchasesum
                        data.pqty = Convert.ToInt32(inwarddetails.Soqty) - Convert.ToInt32(inwarddetails.Pickqty);
                        // Ensure pqty is not negative
                        if (data.pqty < 0)
                        {
                            data.pqty = 0;
                        }
                        data.dDate = ddisDate;
                    }
                    pendingOrders.Add(data);
                }
                pendingOrders = _context.so_inward.ToList();
            }

            return PartialView("_PendingOrdersPartial", pendingOrders);
        }

        // GET: so_inward/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var so_inward = await _context.so_inward
                .Include(s => s.soProduct_details) // Include related so_product details if needed
                .FirstOrDefaultAsync(m => m.id == id);

            if (so_inward == null)
            {
                return NotFound();
            }

            var foundcountry = _context.states.Where(a => a.Name.Trim().ToUpper() == so_inward.state.Trim().ToUpper()).Select(a => a.country.Name.ToUpper().Trim()).FirstOrDefault();
            if (foundcountry != null)
            {
                so_inward.Country = foundcountry;
            }
            else
            {
                so_inward.Country = "-";
            }
            return View(so_inward);
            //try
            //{
            //    if (id == null || _context.so_inward == null)
            //    {
            //        return NotFound();
            //    }

            //    so_inward soinward = _context.so_inward.Include(e => e.soProduct_details).Where(a => a.id == id).FirstOrDefault();

            //    if (soinward == null)
            //    {
            //        return NotFound();
            //    }

            //    return View(soinward);
            //}
            //catch (Exception ex)
            //{
            //    return Problem(ex.Message);
            //}
        }

        // GET: so_inward/Create
        private List<SelectListItem> Getcountry()
        {
            var lstProducts = new List<SelectListItem>();

            lstProducts = _context.Countries.AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.Name,
                Text = n.Name
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select Country----"
            };

            lstProducts.Insert(0, defItem);

            return lstProducts;
        }
        public IActionResult Getcountry11()
        {
            var products = _context.Countries.Select(a => a.Name).Distinct().ToList(); // Fetch products from the database
            return Json(products); // Return products as JSON
        }
        private List<SelectListItem> Getstate()
        {
            var lstProducts = new List<SelectListItem>();

            lstProducts = _context.states.AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.Name,
                Text = n.Name
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select State----"
            };

            lstProducts.Insert(0, defItem);

            return lstProducts;
        }
        private List<SelectListItem> Getcity()
        {
            var lstProducts = new List<SelectListItem>();

            lstProducts = _context.cities.AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.Name,
                Text = n.Name
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select City----"
            };

            lstProducts.Insert(0, defItem);

            return lstProducts;
        }

        public IActionResult Create()
        {
            ViewBag.city = Getcity();
            ViewBag.state = Getstate();
            ViewBag.country = Getcountry();

            ViewBag.description = Getdescription();
            ViewBag.customername = Getcustomername();

            //var so_inward = new so_inward
            //{
            //    soProduct_details = new List<so_product> { new so_product() } // Initialize with at least one empty so_product
            //};

            so_inward so_inward = new so_inward();
            so_inward.soProduct_details.Add(new so_product() { id = 1 });
            // Set the default status to 'Pending'
            so_inward.status = "Pending";
            return View(so_inward);
        }

        [HttpPost]
        public IActionResult MultipleSave([FromBody] JsonElement postData)
        {
            var soProduct_details = new List<so_product>();
            var so_inward = new so_inward();
            so_inward = JsonConvert.DeserializeObject<so_inward>(postData.GetProperty("so_inward").ToString());
            so_inward.soProduct_details = JsonConvert.DeserializeObject<List<so_product>>(postData.GetProperty("TableData").ToString());

            try
            {
                so_inward.state = so_inward.state.ToUpper();
                so_inward.city = so_inward.city.ToUpper();
                so_inward.Country = so_inward.Country.ToUpper();

                // Check if dcustomername or customername has value, set customername accordingly
                if (!string.IsNullOrEmpty(so_inward.dcustomername))
                {
                    so_inward.customername = so_inward.dcustomername;
                }
                else if (!string.IsNullOrEmpty(so_inward.customername))
                {
                    so_inward.customername = so_inward.customername;
                }
                if (!string.IsNullOrEmpty(so_inward.dono))
                {
                    so_inward.sono = so_inward.dono;
                }
                else if (!string.IsNullOrEmpty(so_inward.sono))
                {
                    so_inward.sono = so_inward.sono;
                }
                
                so_inward.dispatchdate = DateTime.Now.ToString("yyyy-MM-dd");
                so_inward.sono = so_inward.sono.Trim().ToUpper();
                so_inward.dono = so_inward.dono.Trim().ToUpper();


                so_inward.sono.Trim();
                int maxId = _context.so_inward.Any() ? _context.so_inward.Max(e => e.id) + 1 : 1;
                so_inward.id = maxId;
                so_inward.so_subcomponent.Clear();
                var newProducts = new List<so_subcomponent>();
                int maxIdp = _context.so_product.Any() ? _context.so_product.Max(e => e.id) + 0 : 0;

                foreach (var product in so_inward.soProduct_details)
                {
                    maxIdp++;
                    product.id = maxIdp;
                    //getting selected product complete details with sub component 
                    var productmaster = _context.Product_Master
                        .Include(e => e.Productmaster_Packets)
                        .FirstOrDefault(p => p.productdescription.ToUpper() == product.description.ToUpper());

                    //set both orderid with id
                    product.orderid = so_inward.id;

                    ////get details from productmaster_packet
                    //foreach (var a in productmaster.Productmaster_Packets)
                    //{
                    //    //if product is individual
                    //    if (productmaster.TypeOfProduct == false)
                    //    {
                    //        // Create a new so_product entity with the same details
                    //        var newProduct = new so_subcomponent
                    //        {
                    //            sccode = a.subcomponentcode,
                    //            subcomponents = a.subcomponents,
                    //            scqty = a.qty,
                    //            scuom = a.uom,
                    //            tqty = (a.qty * product.quantity),
                    //            pono = so_inward.sono
                    //        };
                    //        newProducts.Add(newProduct);
                    //    }
                    //    else
                    //    {
                    //        // Create a new so_product entity with the same details
                    //        var newProduct = new so_subcomponent
                    //        {

                    //            sccode = a.subcomponentcode,
                    //            subcomponents = a.subcomponents,
                    //            scqty = a.qty,
                    //            scuom = a.uom,
                    //            tqty = (a.qty * product.quantity),
                    //            pono = so_inward.sono.Trim(),
                    //        };
                    //        newProducts.Add(newProduct);
                    //    }
                    //}
                }
                so_inward.so_subcomponent.AddRange(newProducts);

                _context.Add(so_inward);
                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "Sale Order Operation";
                logs.task = "Genrate  Order";
                logs.taskid = maxId;
                logs.action = "Create";
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.username = user;
                _context.Add(logs);

                _context.SaveChanges();
                _notyfService.Success("Sale Order Create Succesfully");

                // Log successful creation
                Console.WriteLine($"Purchase Order {so_inward.id} created successfully with {newProducts.Count} subcomponents.");

                return Json(new { success = true, message = " Sale order process Completed Successfully!" });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error creating purchase order: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet]
        public ActionResult EditStatus(int? id)
        {
            ViewBag.description = Getdescription();
            ViewBag.customername = Getcustomername();

            if (id == null)
            {
                return NotFound();
            }
            //var purchase = _context.purchase.Where(a => a.id == id).FirstOrDefault();
            var inward = _context.so_inward.Where(a => a.id == id).Include(a => a.soProduct_details).FirstOrDefault();

            if (inward == null)
            {
                return NotFound();
            }
            // Determine the status and bind the dropdown values accordingly
            List<string> statusOptions = new List<string>();
            var Loading = _context.Loading_Dispatch_Operation.Where(a => a.sono.Trim() == inward.sono.Trim()).FirstOrDefault();
            var picklist = _context.Picklist_Generation.Where(a => a.sono.Trim() == inward.sono.Trim()).FirstOrDefault();
            if (inward != null && Loading != null && picklist != null)
            {
                //COMPLETE
                statusOptions.Add("Completed");
                statusOptions.Add("Return");
            }
            else if (inward != null && Loading == null && picklist == null)
            {
                //PENDING
                statusOptions.Add("Pending");
                statusOptions.Add("Cancel");
            }
            else if (inward != null && Loading == null && picklist != null)
            {
                //PENDING
                statusOptions.Add("In Process");
                //statusOptions.Add("Cancelled");
            }
            ViewBag.GetStatus = new SelectList(statusOptions);
            //end

            return View("EditStatus", inward);
        }
        // GET: so_inward/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            ViewBag.city = Getcity();
            ViewBag.state = Getstate();
            ViewBag.country = Getcountry();

            ViewBag.description = Getdescription();
            ViewBag.customername = Getcustomername();
            if (id == null || _context.so_inward == null)
            {
                return NotFound();
            }
            var so_inward = _context.so_inward.Where(a => a.id == id).Include(a => a.soProduct_details).FirstOrDefault();
            if (so_inward != null)
            {
                if(so_inward.status == "Cancel")
                {

                }
                if (so_inward.status == "Delete")
                {
                    so_inward.status = "Delete";
                }
            }
            else
            {
                return NotFound();
            }
            var foundcountry = _context.states.Where(a => a.Name.Trim().ToUpper() == so_inward.state.Trim().ToUpper()).Select(a => a.country.Name.ToUpper().Trim()).FirstOrDefault();
            if (foundcountry != null)
            {
                so_inward.Country = foundcountry;
            }
            else
            {
                so_inward.Country = "-";
            }

            return View(so_inward);
            return RedirectToAction(nameof(Index));
        }
        // POST: so_inward/Edit/5
        [HttpPost]
        public IActionResult sonoLoaded(string sono)
        {
            var loaded = _context.Loading_Dispatch_Operation.Where(a => a.sono == sono).ToList();
            var pickstorage = _context.pickstorage.Where(a => a.sono == sono && a.flag == 1).ToList();

                if (loaded != null && pickstorage != null)
                {
                    if (loaded.Count > 0 && pickstorage.Count > 0 && loaded.Count == pickstorage.Count)
                    {
                        return Json(new { success = true, message = "Status changed to Return!" });
                    }
                  
                    else
                    {
                        return Json(new { success = false, message = "Status can't be updated as return; No dispatch found against "+sono+ " order no." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Error occurred while processing!" });
                }
        }
        [HttpPost]
        public IActionResult sonoLoadedOnHold(string sono)
        {
            var inward = _context.so_inward
                .Include(a => a.soProduct_details)
                .FirstOrDefault(a => a.sono == sono);

            var picklist = _context.Picklist_Generation
                .Include(a => a.pickstorages)
                .FirstOrDefault(a => a.sono == sono);

            var lodadata = _context.Loading_Dispatch_Operation.Where(a => a.sono == sono).ToList();

            if (picklist == null && inward != null && lodadata == null)
            {
                return Json(new { success = true });
            }
            else if (inward != null && picklist != null && lodadata == null)
            {
                return Json(new { success = false, message = "Picklist already in process. Can't hold the " + sono + " order No. !" });
            }
            else if (inward != null && picklist != null && lodadata != null)
            {
                return Json(new { success = false, message = "Dispatch already done. Can't hold the " + sono + " order No. !" });
            }
            return Json(new { success = true });
        }
        [HttpPost]
        public IActionResult OnDeleteChangesono(string sono)
        {
            var inward = _context.so_inward
                .Include(a => a.soProduct_details)
                .FirstOrDefault(a => a.sono == sono);

            var picklist = _context.Picklist_Generation
                .Include(a => a.pickstorages)
                .FirstOrDefault(a => a.sono == sono);

            var lodadata = _context.Loading_Dispatch_Operation.Where(a => a.sono == sono).ToList();

            if (picklist == null && inward != null && lodadata == null)
            {
                return Json(new { success = true });
            }
            else if (inward != null && picklist != null && lodadata == null)
            {
                return Json(new { success = false, message = "Picklist already in process. Can't cancel the " + sono + " order No. !" });
            }
            else if (inward != null && picklist != null && lodadata != null)
            {
                return Json(new { success = false, message = "Dispatch already done. Can't cancel the " + sono + " order No. !" });
            }
            return Json(new { success = true });
        }
        [HttpPost]
        public IActionResult OnPendingChangesono(string sono)
        {
            var inward = _context.so_inward
                .Include(a => a.soProduct_details)
                .FirstOrDefault(a => a.sono == sono);

            var picklist = _context.Picklist_Generation
                .Include(a => a.pickstorages)
                .FirstOrDefault(a => a.sono == sono);

            var lodadata = _context.Loading_Dispatch_Operation.Where(a => a.sono == sono).ToList();

            if (picklist == null && inward != null && lodadata == null)
            {
                return Json(new { success = true });
            }
            else if (inward != null && picklist != null && lodadata == null)
            {
                return Json(new { success = false, message = "Picklist already in process. Can't pending the " + sono + " order No. !" });
            }
            else if (inward != null && picklist != null && lodadata != null)
            {
                return Json(new { success = false, message = "Dispatch already done. Can't pending the " + sono + " order No. !" });
            }
            return Json(new { success = true });
        }
        [HttpPost]
        //public async Task<IActionResult> Edit(int id, so_inward so_inward)
        //{
        //    if (id != so_inward.id)
        //    {
        //        return NotFound();
        //    }
        //    try
        //    {
        //        var exist = _context.Picklist_Generation.FirstOrDefault(a => a.sono.Trim() == so_inward.sono.Trim());
        //        if (exist != null)
        //        {
        //            return Json(new { success = false, Message = "You can't update that sale order " + so_inward.sono + " picklist already done!" });
        //        }
        //        else
        //        {
        //            List<so_product> productDetails = _context.so_product.Where(d => d.orderid == so_inward.id).AsNoTracking().ToList();
        //            if (productDetails.Count > 0)
        //            {
        //                try
        //                {
        //                    _context.so_product.RemoveRange(productDetails);
        //                    _context.SaveChanges();
        //                }
        //                catch (Exception ex)
        //                {
        //                    return Json(new { success = true, message = ex.Message });
        //                }
        //            }

        //            var getdata = _context.so_inward.Where(a => a.id == id).Include(a => a.soProduct_details).FirstOrDefault();
        //            if (getdata != null)
        //            {
        //                try
        //                {
        //                    _context.so_inward.Remove(getdata);
        //                    _context.SaveChanges();
        //                }
        //                catch (Exception ex)
        //                {
        //                    return Json(new { success = true, message = ex.Message });
        //                }
        //            }

        //            int maxId = _context.so_inward.Any() ? _context.so_inward.Max(e => e.id) + 1 : 1;
        //            so_inward.id = maxId;
        //            try
        //            {
        //                so_inward.state = so_inward.state.ToUpper();
        //                so_inward.city = so_inward.city.ToUpper();

        //                if (!string.IsNullOrEmpty(so_inward.dono))
        //                {
        //                    so_inward.sono = so_inward.dono;
        //                }
        //                else if (!string.IsNullOrEmpty(so_inward.sono))
        //                {
        //                    so_inward.sono = so_inward.sono;
        //                }

        //                so_inward.dispatchdate = DateTime.Now.ToString("yyyy-MM-dd");
        //                so_inward.sono = so_inward.sono.Trim().ToUpper();
        //                so_inward.dono = so_inward.sono.Trim().ToUpper();
        //                try
        //                {
        //                    so_inward.state = so_inward.state.ToUpper();
        //                    so_inward.city = so_inward.city.ToUpper();
        //                    so_inward.Country = so_inward.Country.ToUpper();

        //                    _context.so_inward.Add(so_inward);
        //                    _context.SaveChanges();
        //                    return Json(new { success = true, message = "Order Update Successfully !" });
        //                }
        //                catch (Exception ex)
        //                {
        //                    return Json(new { success = true, message = ex.Message });
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                return Json(new { success = true, message = ex.Message });
        //            }
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        return View(ex);
        //    }
        //}
        public async Task<IActionResult> Edit(int id, so_inward so_inward)
        {
            if (id != so_inward.id)
            {
                return NotFound();
            }
            try
            {
                var exist = _context.Picklist_Generation.FirstOrDefault(a => a.sono.Trim() == so_inward.sono.Trim());
                if (exist != null)
                {
                    return Json(new { success = false, Message = "You can't update that sale order " + so_inward.sono + " picklist already done!" });
                }
                else
                {
                    //List<so_product> productDetails = _context.so_product.Where(d => d.orderid == so_inward.id).AsNoTracking().ToList();
                    //if (productDetails.Count > 0)
                    //{
                    //    try
                    //    {
                    //        _context.so_product.RemoveRange(productDetails);
                    //        _context.SaveChanges();
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        return Json(new { success = true, message = ex.Message });
                    //    }
                    //}

                    var getdata = _context.so_inward.Where(a => a.id == id).Include(a => a.soProduct_details).FirstOrDefault();
                    if (getdata != null)
                    {
                        try
                        {
                            _context.so_inward.Remove(getdata);
                            _context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            return Json(new { success = true, message = ex.Message });
                        }
                    }

                    int maxId = _context.so_inward.Any() ? _context.so_inward.Max(e => e.id) + 1 : 1;
                    so_inward.id = maxId;
                    try
                    {
                        so_inward.state = so_inward.state.ToUpper();
                        so_inward.city = so_inward.city.ToUpper();

                        if (!string.IsNullOrEmpty(so_inward.dono))
                        {
                            so_inward.sono = so_inward.dono;
                        }
                        else if (!string.IsNullOrEmpty(so_inward.sono))
                        {
                            so_inward.sono = so_inward.sono;
                        }

                        so_inward.dispatchdate = DateTime.Now.ToString("yyyy-MM-dd");
                        so_inward.sono = so_inward.sono.Trim().ToUpper();
                        so_inward.dono = so_inward.sono.Trim().ToUpper();
                        try
                        {
                            so_inward.state = so_inward.state.ToUpper();
                            so_inward.city = so_inward.city.ToUpper();
                            so_inward.Country = so_inward.Country.ToUpper();

                            _context.so_inward.Add(so_inward);
                            _context.SaveChanges();
                            return Json(new { success = true, message = "Order Update Successfully !" });
                        }
                        catch (Exception ex)
                        {
                            return Json(new { success = true, message = ex.Message });
                        }
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = true, message = ex.Message });
                    }
                }


            }
            catch (Exception ex)
            {
                return View(ex);
            }
        }


        //public async Task<IActionResult> Edit(int id, so_inward so_Inward)
        //{
        //    if (so_Inward.dono == null)
        //    {
        //        so_Inward.dono = "-";
        //    }
        //    if (id != so_Inward.id)
        //    {
        //        return NotFound();
        //    }
        //    try
        //    {
        //        int maxId1 = _context.so_inward.Any() ? _context.so_inward.Max(e => e.id) : 1 + 1;
        //        int maxId2 = _context.so_product.Any() ? _context.so_product.Max(e => e.id) : 1 + 1;

        //        var getdata = _context.so_inward.Where(a => a.id == id).Include(a => a.soProduct_details).AsNoTracking().FirstOrDefault();
        //        var getload = _context.Picklist_Generation.Where(a => a.sono == getdata.sono).AsNoTracking().FirstOrDefault();
        //        if (getload != null)
        //        {
        //            return Json(new { success = false, Message = "You can't update that sale order " + getdata.sono + " picklist already done!" });
        //        }
        //        else
        //        {
        //            try
        //            {
        //                List<so_product> productDetails = _context.so_product.Where(d => d.orderid == so_Inward.id).AsNoTracking().ToList();
        //                _context.so_product.RemoveRange(productDetails);
        //                foreach(var item in so_Inward.soProduct_details)
        //                {
        //                    int maxId22 = _context.so_product.Any() ? _context.so_product.Max(e => e.id) : 1 + 1;
        //                    item.orderid = so_Inward.id;
        //                    item.id = maxId22;
        //                    _context.so_product.Update(item);
        //                    _context.SaveChanges();
        //                }
        //                _context.so_inward.Update(so_Inward);
        //                _context.SaveChanges();

        //                return Json(new { success = true, message = "Order Update Successfully !" });
        //            }
        //            catch (Exception ex)
        //            {
        //                return Json(new { success = true, message = ex.Message });
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = true, message = ex.Message });
        //    }
        //}
        [HttpPost]
        public async Task<IActionResult> EditStatus(int id, so_inward so_Inward)
        {
            try
            {
                if (id != so_Inward.id)
                {
                    return NotFound();
                }

                var existingOrder = _context.so_inward.FirstOrDefault(a => a.id == id);
                if (existingOrder == null)
                {
                    return NotFound();
                }

                if (string.IsNullOrEmpty(so_Inward.status))
                {
                    var response = new { success = false, message = "Please select a status to update." };
                    return Json(response);
                }

                if (existingOrder.status == so_Inward.status)
                {
                    var response = new { success = false, message = "Order is already in pending. Cannot update status." };
                    return Json(response);
                }

                existingOrder.status = so_Inward.status;
                _context.Update(existingOrder);

                var user = HttpContext.Session.GetString("User");
                var logs = new Logs
                {
                    pagename = "Sale Order Operation",
                    task = "Update Sale Order",
                    taskid = id,
                    action = "Update",
                    date = DateTime.Now.ToString("dd/MM/yyyy"),
                    time = DateTime.Now.ToString("HH:mm:ss"),
                    username = user
                };
                _context.Add(logs);

                await _context.SaveChangesAsync();

                var successResponse = new { success = true, message = "Order status updated successfully." };
                return Json(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new { success = false, message = "An error occurred while updating the sales order status." };
                return Json(errorResponse);
            }
        }

        //public async Task<IActionResult> Edit( int id, so_inward so_Inward)
        //{

        //    if(so_Inward.dono == null)
        //    {
        //        so_Inward.dono = "-";
        //    }
        //    if (id != so_Inward.id)
        //    {
        //        return NotFound();
        //    }
        //    try
        //    {
        //        if(so_Inward != null)
        //        {
        //            var getiddata = _context.so_inward.Where(a => a.id == id).FirstOrDefault();
        //            if (getiddata != null)
        //            {
        //                if (getiddata.status == so_Inward.status)
        //                {
        //                    var response = new { success = false, message = "Order is already in pending, Can't Update status !", redirect = true };
        //                    return Json(response);
        //                }
        //            }
        //            else
        //            {
        //                List<so_product> productDetails = _context.so_product.Where(d => d.orderid == so_Inward.id).ToList();
        //                _context.so_product.RemoveRange(productDetails);
        //                _context.Update(so_Inward);
        //                var user = HttpContext.Session.GetString("User");
        //                var logs = new Logs();
        //                logs.pagename = "Sale Order Operation";
        //                logs.task = "Update Sale Order";
        //                logs.taskid = id;
        //                logs.action = "Update";
        //                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
        //                logs.time = DateTime.Now.ToString("HH:mm:ss");
        //                logs.username = user;
        //                _context.Add(logs);
        //                _context.SaveChanges();
        //                var response = new { success = true, message = "Order Status updated successfully." };
        //                return Json(response);
        //                //_notyfService.Success("Sales Order Status update Succesfully");
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        var response = new { success = false, message = "An error occurred while updating the sales order status." };
        //        return Json(response);

        //    }
        //    return RedirectToAction("SalesPendingList", "so_inward");
        //}
        private bool salesExists(int id)
        {
            return (_context.so_inward?.Any(e => e.id == id)).GetValueOrDefault();
        }
        public async Task<IActionResult> PendingEdit(int? id)
        {
            if (id == null || _context.so_inward == null)
            {
                return NotFound();
            }
            var so_inward = _context.so_inward.Where(a => a.id == id).Include(a => a.soProduct_details).FirstOrDefault();
            if (so_inward == null)
            {
                return NotFound();
            }
            return View(so_inward);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PendingEdit(int id, so_inward so_inward)
        {
            if (id != so_inward.id)
            {
                return NotFound();
            }
            try
            {
                List<so_product> productDetails = _context.so_product.Where(d => d.orderid == so_inward.id).ToList();

                _context.so_product.RemoveRange(productDetails);
                _context.SaveChanges();

                _context.Update(so_inward);
                await _context.SaveChangesAsync();
                _notyfService.Success("Sales Order Updated Succesfully");
                //return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!salesExists(so_inward.id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
            return View("PendingEdit", so_inward);
        }
        // GET: so_inward/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.so_inward == null)
            {
                return NotFound();
            }
            //var so_inward = await _context.so_inward.FirstOrDefaultAsync(m => m.id == id);
            var so_inward = _context.so_inward.Include(e => e.soProduct_details).Where(e => e.id == id).FirstOrDefault();

            if (so_inward == null)
            {
                return NotFound();
            }
            return View(so_inward);
        }

        // POST: so_inward/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.so_inward == null)
            {
                return Problem("Entity set 'ErosDbContext.so_inward'  is null.");
            }
            var so_inward = await _context.so_inward.FindAsync(id);
            if (so_inward != null)
            {
                _context.so_inward.Remove(so_inward);
            }
            //maintain logs
            var user = HttpContext.Session.GetString("User");
            var logs = new Logs();
            logs.pagename = "Sale Order Operation";
            //logs.task = "Delete Sale Order";
            logs.task = so_inward.id+"$"+so_inward.sono + "$" + so_inward.sodate + "$" + so_inward.customername + "$" + so_inward.contactno + "$" + so_inward.emailid + "$" + so_inward.address + "$" + so_inward.city + "$" + so_inward.state + "$" + so_inward.dispatchdate + "$" + so_inward.status + "$" + so_inward.dono;
            logs.taskid = id;
            logs.action = "'Delete";
            logs.date = DateTime.Now.ToString("dd/MM/yyyy");
            logs.time = DateTime.Now.ToString("HH:mm:ss");
            logs.username = user;
            _context.Add(logs);
            _context.SaveChanges();

            _notyfService.Error("Sales Order Deleted Succesfully");
            return RedirectToAction(nameof(Index));
        }
        public ActionResult ActionName(string optionValue, string optionValue1)
        {
            if (optionValue == "pending" && optionValue1 == "Demo")
            {
                var q1 = _context.inward.Where(o => o.typeofreturn == "Returnable" && o.ordertype == "Demo").ToList();
                //var q2 = _context.Outward.Where(o => o.typeofreturn == "Returnable" && o.ordertype == "Demo").ToList();
                return PartialView("PendingList", q1);
            }
            else if (optionValue == "pending" && optionValue1 == "Repaire")
            {
                var q1 = _context.inward.Where(o => o.typeofreturn == "Returnable" && (o.ordertype == "Repaire")).ToList();
                //var q2 = _context.Outward.Where(o => o.typeofreturn == "Returnable" && (o.ordertype == "Repaire")).ToList();
                return PartialView("PendingList", q1);
            }
            else if (optionValue == "pending" && optionValue1 == "Replacement")
            {
                var q1 = _context.inward.Where(o => o.typeofreturn == "Returnable" && (o.ordertype == "Replacement")).ToList();
                //var q2 = _context.Outward.Where(o => o.typeofreturn == "Returnable" && (o.ordertype == "Replacement")).ToList();
                return PartialView("PendingList", q1);
            }
            else if (optionValue == "Completed" && optionValue1 == "Demo")
            {
                var q1 = _context.inward.Where(o => o.typeofreturn == "Returned" && (o.ordertype == "Demo")).ToList();
                //var q2 =_context.Outward.Where(o => o.typeofreturn == "Returned" && (o.ordertype == "Demo")).ToList();
                return PartialView("PendingList", q1);
            }
            else if (optionValue == "Completed" && optionValue1 == "Repaire")
            {
                var q1 = _context.inward.Where(o => o.typeofreturn == "Returned" && (o.ordertype == "Repaire")).ToList();
                //var q2 = _context.Outward.Where(o => o.typeofreturn == "Returned" && (o.ordertype == "Repaire")).ToList();
                return PartialView("PendingList", q1);
            }
            else if (optionValue == "Completed" && optionValue1 == "Replacement")
            {
                var q1 = _context.inward.Where(o => o.typeofreturn == "Returned" && (o.ordertype == "Replacement")).ToList();
                //var q2 = _context.Outward.Where(o => o.typeofreturn == "Returned" && (o.ordertype == "Replacement")).ToList();
                return PartialView("PendingList", q1);
            }
            else
            {
                return View("PendingList");
            }
        }


        //public async Task<IActionResult> EditStatus(int id, so_inward so_Inward)
        //{
        //    try
        //    {
        //        // Check if the id from the URL matches the id in the object
        //        if (id != so_Inward.id)
        //        {
        //            return NotFound();
        //        }

        //        // Find the existing order in the database
        //        var existingOrder = await _context.so_inward.FirstOrDefaultAsync(a => a.id == id);
        //        if (existingOrder == null)
        //        {
        //            return NotFound();
        //        }

        //        // Check if the status is empty or null
        //        if (string.IsNullOrEmpty(so_Inward.status))
        //        {
        //            var response = new { success = false, message = "Please select a status to update." };
        //            return Json(response);
        //        }

        //        // If the status is the same, send a message without updating
        //        if (existingOrder.status == so_Inward.status)
        //        {
        //            var response = new { success = false, message = "Order is already in pending. Cannot update status." };
        //            return Json(response);
        //        }

        //        // Update the status in the existing order
        //        existingOrder.status = so_Inward.status;
        //        _context.so_inward.Update(existingOrder);

        //        // Log the action
        //        var user = HttpContext.Session.GetString("User");
        //        var logs = new Logs
        //        {
        //            pagename = "Sale Order Operation",
        //            task = "Update Sale Order",
        //            taskid = id,
        //            action = "Update",
        //            date = DateTime.Now.ToString("dd/MM/yyyy"),
        //            time = DateTime.Now.ToString("HH:mm:ss"),
        //            username = user
        //        };
        //        await _context.AddAsync(logs);

        //        // Save changes to the database
        //        await _context.SaveChangesAsync();

        //        var successResponse = new { success = true, message = "Order status updated successfully." };
        //        return Json(successResponse);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception to help with debugging
        //        Console.WriteLine(ex.Message); // Optional: you could log this to a file or a logging system

        //        var errorResponse = new { success = false, message = "An error occurred while updating the sales order status.", details = ex.Message };
        //        return Json(errorResponse);
        //    }
        //}

    }
}
