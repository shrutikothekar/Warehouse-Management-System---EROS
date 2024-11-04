using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AuthSystem.Data;
using eros.Models;
using AspNetCoreHero.ToastNotification.Abstractions;
using AspNetCore;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Text;
using AspNetCoreHero.ToastNotification.Notyf;
using Newtonsoft.Json;
using System.Text.Json;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.Kernel.Pdf;
using iText.Html2pdf;
using iText.Kernel.Geom;
using DocumentFormat.OpenXml.Drawing.Charts;

namespace eros.Controllers
{
    public class purchasesController : Controller
    {
        private readonly ErosDbContext _context;
        public INotyfService _notifyService { get; }

        public purchasesController(ErosDbContext context, INotyfService notyfService)
        {
            _context = context;
            _notifyService = notyfService;
        }
       
        [HttpPost]
        //public IActionResult ponoLoaded(string pono)
        //{
        //    var inward = _context.inward
        //        .Include(a => a.inwardPacket)
        //        .FirstOrDefault(a => a.pono == pono);

        //    var purchase = _context.purchase
        //        .Include(a => a.poProduct_details)
        //        .FirstOrDefault(a => a.pono == pono);

        //    var storage = _context.
        //    if (purchase != null && inward == null)
        //    {
        //        return Json(new { success = true });
        //    }
        //    else if (inward != null && purchase != null)
        //    {
        //        return Json(new { success = false, message = "Inward data exists, Cancel operation not allowed!" });
        //    }
        //    return Json(new { success = true });
        //}

        public IActionResult MultipleSave([FromBody] JsonElement postData)
        {
            var poProduct_details = new List<purchase_product>();
            var purchase = new purchase();
            purchase = JsonConvert.DeserializeObject<purchase>(postData.GetProperty("purchase").ToString());
            purchase.poProduct_details = JsonConvert.DeserializeObject<List<purchase_product>>(postData.GetProperty("TableData").ToString());

            try
            {
                purchase.pono = purchase.pono.Trim().ToUpper();
                int maxId = _context.purchase.Any() ? _context.purchase.Max(e => e.id) + 1 : 1;
                purchase.id = maxId;
                purchase.purchase_subcomponent.Clear();
                var newProducts = new List<purchase_subcomponent>();

                foreach (var product in purchase.poProduct_details)
                {
                    //getting selected product complete details with sub component 
                    var productmaster = _context.Product_Master
                        .Include(e => e.Productmaster_Packets)
                        .FirstOrDefault(p => p.productdescription.ToUpper() == product.description.ToUpper());

                    //set both orderid with id
                    product.porderid = purchase.id;

                    //get details from productmaster_packet
                    foreach (var a in productmaster.Productmaster_Packets)
                    {
                        //if product is individual
                        if (productmaster.TypeOfProduct == false)
                        {
                            // Create a new so_product entity with the same details
                            var newProduct = new purchase_subcomponent
                            {
                                sccode = a.subcomponentcode,
                                subcomponents = a.subcomponents,
                                scqty = a.qty,
                                scuom = a.uom,
                                tqty = (a.qty * product.quantity),
                            };
                            newProducts.Add(newProduct);
                        }
                    }
                }
                purchase.purchase_subcomponent.AddRange(newProducts);

                _context.Add(purchase);

                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "Purchase Order Operation";
                logs.task = "Generate Purchase Order";
                logs.taskid = maxId;
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.action = "Create";
                logs.username = user;
                _context.Add(logs);
                _notifyService.Success("Created Successfully!");

                _context.SaveChanges();

                // Log successful creation
                //Console.WriteLine($"Purchase Order {purchase.id} created successfully with {newProducts.Count} subcomponents.");

                return Json(new { success = true, message = "Purchase order created successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error creating purchase order: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        public IActionResult ponoLoaded(string pono)
        {
            var inward = _context.inward
                                    .Include(a => a.inwardPacket)
                                    .FirstOrDefault(a => a.pono == pono);
            var inwardso = _context.inward
                                    .Include(a => a.inwardPacket)
                                    .Where(a => a.pono == pono)
                                    .ToList();
            var purchase = _context.purchase
                                    .Include(a => a.poProduct_details)
                                    .FirstOrDefault(a => a.pono == pono);

            if (inward != null && purchase != null)
            {
                bool isInStorage = false; // Flag to track if inward batch code is in storage
                bool isQuantityValid = false; // Flag to track if inward quantity is within purchase quantity
                foreach (var item in inwardso)
                {
                    foreach (var item1 in item.inwardPacket)
                    {
                        var inbatchcode = item.batchcode;
                        var checkinstorage = _context.Storage_Operation.Any(a => a.batchcode == inbatchcode && a.statusflag == "ST");

                        if (checkinstorage)
                        {
                            isInStorage = true;
                            break; // No need to continue checking if already found in storage
                        }
                    }
                }
                foreach (var item in inward.inwardPacket.Where(a => a.pono == pono))
                {
                    foreach (var item1 in purchase.poProduct_details.Where(a => a.porderid == purchase.id))
                    {
                        var sumOfInQty = inward.inwardPacket
                                              .Where(ip => ip.productcode.ToUpper() == item1.productcode.ToUpper())
                                              .Sum(ip => ip.quantity);

                        if (sumOfInQty > 0 && sumOfInQty <= item1.quantity)
                        {
                            isQuantityValid = true;
                            break; // No need to continue checking if already found valid quantity
                        }
                    }
                }
                if (isInStorage && isQuantityValid)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "The product not in storage, May be already loaded !" });
                }
            }
            else
            {
                return Json(new { success = false, message = "Please inward the stock first!" });
            }
            //return NotFound(); // or BadRequest(), or any other appropriate status code
        }
        [HttpPost]
        public IActionResult ponoLoadedOnHold(string pono)
        {
            var inward = _context.inward
                .Include(a => a.inwardPacket)
                .FirstOrDefault(a => a.pono == pono);

            var purchase = _context.purchase
                .Include(a => a.poProduct_details)
                .FirstOrDefault(a => a.pono == pono);

            if (purchase != null && inward == null)
            {
                return Json(new { success = true });
            }
            else if (inward != null && purchase != null)
            {
                return Json(new { success = false, message = "Inward data exists, Status can't be updated to hold!" });
            }
            return Json(new { success = true });
        }
        [HttpPost]
        public IActionResult OnDeleteChange(string pono)
        {
            var inward = _context.inward
                .Include(a => a.inwardPacket)
                .FirstOrDefault(a => a.pono == pono);

            var purchase = _context.purchase
                .Include(a => a.poProduct_details)
                .FirstOrDefault(a => a.pono == pono);

            if (purchase != null && inward == null)
            {
                return Json(new { success = true });
            }
            else if (inward != null && purchase != null)
            {
                return Json(new { success = false, message = "Inward data exists. Can't change the status of " + pono + " purchase order No. to 'cancel'!" });

            }
            return Json(new { success = true });
        }
        [HttpPost]
        public IActionResult OnPendingChange(string pono)
        {
            var inward = _context.inward
                .Include(a => a.inwardPacket)
                .FirstOrDefault(a => a.pono == pono);

            var purchase = _context.purchase
                .Include(a => a.poProduct_details)
                .FirstOrDefault(a => a.pono == pono);

            if (purchase != null && inward == null)
            {
                return Json(new { success = true });
            }
            else if (inward != null && purchase != null)
            {
                return Json(new { success = false, message = "Inward data exists. Can't change the status of " + pono + " purchase order No. to 'pending'!" });

            }
            return Json(new { success = true });
        }


        //if (loaded != null && pickstorage != null)
        //{
        //    if (loaded.Count == pickstorage.Count)
        //    {
        //        return Json(new { success = true, message = "Status changed to Cancel!" });
        //    }
        //    else
        //    {
        //        return Json(new { success = false, message = "Status Can't be Updated!" });
        //    }
        //}
        //else
        //{
        //    return Json(new { success = false, message = "Error occurred while processing!" });
        //}

        public IActionResult Getsuppliers(Supplier_Master formData)
        {
            // Apply validations and transformations
            if (formData.emailid == null)
            {
                formData.emailid = "-";
            }
            else
            {
                formData.emailid = formData.emailid.ToLower();
            }
            if (formData.contactno == null)
            {
                formData.contactno = "0000000000";
            }
            if(formData.pincode == null)
            {
                formData.pincode = "000000";
            }
            formData.gstno = formData.gstno.ToUpper();
            formData.city = formData.city.ToUpper();
            formData.state = formData.state.ToUpper();
            formData.country = formData.country.ToUpper();
            formData.supplier_name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(formData.supplier_name.ToLower());
            formData.address = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(formData.address.ToLower());


            int maxId = _context.Supplier_Master.Any() ? _context.Supplier_Master.Max(e => e.supplierid) + 1 : 1;
            var Supplier_Master = new Supplier_Master
            {
                // No need to generate customer_id manually
                //supplierid = maxId,
                //suppliercode = formData.suppliercode,
                //supplier_name = formData.supplier_name,
                //address = formData.address,
                //city = formData.city,
                //country = formData.country,
                //state = formData.state,
                //pincode = formData.pincode,
                //contactno = formData.contactno,
                //emailid = formData.emailid,
                //gstno = formData.gstno,
                //brand = formData.brand,
                supplierid = maxId,
                suppliercode = formData.suppliercode,
                supplier_name = formData.supplier_name,
                address = formData.address,
                city = formData.city,
                country = formData.country,
                state = formData.state,
                pincode = formData.pincode,
                contactno = formData.contactno,
                emailid = formData.emailid,
                gstno = formData.gstno,
                brand = formData.brand,
            };

            _context.Supplier_Master.Add(Supplier_Master);
            _context.SaveChanges();

            var descriptions = _context.Supplier_Master
                .Select(p => p.supplier_name)
                .Distinct()
                .ToList();

            return Json(new { success = true, descriptions = descriptions });
        }

        //public async Task<IActionResult> Index()
        //{
        //    //return _context.purchase != null ?
        //    //            View(await _context.purchase.ToListAsync()) :
        //    //            Problem("Entity set 'ErosDbContext.purchase'  is null.");
        //    var pendingOrders = await _context.purchase
        //        //.Where(a => a.status == "Pending")
        //        .ToListAsync();
        //    return View(pendingOrders);
        //}
        public async Task<IActionResult> Index()
        {
            var pendingOrders =  _context.purchase.OrderByDescending(a => a.id).ToList();

            return View(pendingOrders);
        }
        //[HttpPost]
        //public ActionResult FilterDataByDate(string status, DateTime? fromDate, DateTime? toDate)
        //{
        //    try
        //    {
        //        List<purchase> pendingOrders = new List<purchase>();

        //        var purchase = _context.purchase
        //                .Where(po => po.status == "Pending").OrderByDescending(po => po.id).AsNoTracking()
        //                .ToList();

        //        foreach (var data in purchase)
        //        {
        //            var purchasesum = _context.poProduct_details.Where(a => a.porderid == data.id).Sum(a => a.quantity);
        //            var inwarddetails = _context.inwardPacket.Where(a => a.pono == data.pono)
        //                      .GroupBy(p => p.pono)
        //                        .Select(group => new
        //                        {
        //                            ProductName = group.Key,
        //                            TotalQuantity = group.Sum(p => p.quantity),
        //                            TotalSubAssembly = group.Sum(p => p.totalsubassmbly)
        //                        }).FirstOrDefault();
        //            //var inwarddetails = _context.inward.Where(a => a.pono == data.pono && a.flag == 1).AsNoTracking().ToList();
        //            if (inwarddetails == null)
        //            {
        //                data.qty = purchasesum;
        //                data.pqty = purchasesum;
        //            }
        //            else
        //            {
        //                data.qty = purchasesum; // Setting qty to purchasesum
        //                data.pqty = purchasesum - inwarddetails.TotalQuantity;
        //                // Ensure pqty is not negative
        //                if (data.pqty < 0)
        //                {
        //                    data.pqty = 0;
        //                }
        //            }
        //            pendingOrders.Add(data);
        //        }

        //        return View("PurchasePendingList", pendingOrders);
        //    }
        //    catch (Exception ex)
        //    {
        //        return View("Internal server error: " + ex.Message);

        //    }
        //}
        public ActionResult FilterDataByDate(string status, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                List<purchase> pendingOrders = new List<purchase>();

                var InDate = "";
                if (status == null)
                {
                    status = "Pending";

                    // Build the query based on the provided dates
                    var purchaseQuery = _context.purchase
                        .Where(po => po.status.Trim() == status.Trim()).ToList();

                    if (fromDate.HasValue && toDate.HasValue)
                    {
                        purchaseQuery = purchaseQuery.Where(po => DateTime.TryParse(po.podate, out var purchaseDate) &&
                                                                    purchaseDate >= fromDate &&
                                                                    purchaseDate <= toDate)
                                                     .ToList();
                    }
                    else if (fromDate.HasValue)
                    {
                        purchaseQuery = purchaseQuery.Where(po => DateTime.TryParse(po.podate, out var purchaseDate) &&
                                                                    purchaseDate >= fromDate &&
                                                                    purchaseDate <= DateTime.Today)
                                                     .ToList();
                    }

                    var purchaseList = purchaseQuery
                        .OrderByDescending(po => po.id)
                        .ToList();

                    foreach (var data in purchaseList)
                    {
                        var purchaseSum = _context.poProduct_details
                            .Where(a => a.porderid == data.id)
                            .Sum(a => a.quantity);
                        var InDatee = _context.inward.Where(a => a.pono == data.pono && a.flag == 1).OrderByDescending(a => a.inward_id).FirstOrDefault();
                        if (InDatee != null)
                        {
                            InDate = InDatee.date;
                        }
                        else
                        {
                            InDate = "NA";
                        }
                        var inwardDetails = _context.inwardPacket
                            .Where(a => a.pono == data.pono)
                            .GroupBy(p => p.pono)
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
                            data.InDate = InDate;
                        }
                        else
                        {
                            data.qty = purchaseSum;
                            data.pqty = purchaseSum - inwardDetails.TotalQuantity;

                            if (data.pqty < 0)
                            {
                                data.pqty = 0;
                            }
                            data.InDate = InDate;
                        }

                        pendingOrders.Add(data);
                    }

                }
                else if(status == "All")
                {
                    // Build the query based on the provided dates
                    var purchaseQuery = _context.purchase
                        .Where(po => po.status.Trim() == "Pending" || po.status == "Completed").ToList();

                    if (fromDate.HasValue && toDate.HasValue)
                    {
                        purchaseQuery = purchaseQuery.Where(po => DateTime.TryParse(po.podate, out var purchaseDate) &&
                                                                    purchaseDate >= fromDate &&
                                                                    purchaseDate <= toDate)
                                                     .ToList();
                    }
                    else if (fromDate.HasValue)
                    {
                        purchaseQuery = purchaseQuery.Where(po => DateTime.TryParse(po.podate, out var purchaseDate) &&
                                                                    purchaseDate >= fromDate &&
                                                                    purchaseDate <= DateTime.Today)
                                                     .ToList();
                    }

                    var purchaseList = purchaseQuery
                        .OrderByDescending(po => po.id)
                        .ToList();

                    foreach (var data in purchaseList)
                    {
                        var purchaseSum = _context.poProduct_details
                            .Where(a => a.porderid == data.id)
                            .Sum(a => a.quantity);
                        var InDatee = _context.inward.Where(a => a.pono == data.pono && a.flag == 1).OrderByDescending(a => a.inward_id).FirstOrDefault();
                        if (InDatee != null)
                        {
                            InDate = InDatee.date;
                        }
                        else
                        {
                            InDate = "NA";
                        }
                        var inwardDetails = _context.inwardPacket
                            .Where(a => a.pono == data.pono)
                            .GroupBy(p => p.pono)
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
                            data.InDate = InDate;
                        }
                        else
                        {
                            data.qty = purchaseSum;
                            data.pqty = purchaseSum - inwardDetails.TotalQuantity;

                            if (data.pqty < 0)
                            {
                                data.pqty = 0;
                            }
                            data.InDate = InDate;
                        }

                        pendingOrders.Add(data);
                    }
                }
                else
                {
                    // Build the query based on the provided dates
                    var purchaseQuery = _context.purchase
                        .Where(po => po.status.Trim() == status.Trim()).ToList();

                    if (fromDate.HasValue && toDate.HasValue)
                    {
                        purchaseQuery = purchaseQuery.Where(po => DateTime.TryParse(po.podate, out var purchaseDate) &&
                                                                    purchaseDate >= fromDate &&
                                                                    purchaseDate <= toDate)
                                                     .ToList();
                    }
                    else if (fromDate.HasValue)
                    {
                        purchaseQuery = purchaseQuery.Where(po => DateTime.TryParse(po.podate, out var purchaseDate) &&
                                                                    purchaseDate >= fromDate &&
                                                                    purchaseDate <= DateTime.Today)
                                                     .ToList();
                    }

                    var purchaseList = purchaseQuery
                        .OrderByDescending(po => po.id)
                        .ToList();

                    foreach (var data in purchaseList)
                    {
                        var purchaseSum = _context.poProduct_details
                            .Where(a => a.porderid == data.id)
                            .Sum(a => a.quantity);
                        var InDatee = _context.inward.Where(a => a.pono == data.pono && a.flag == 1).OrderByDescending(a => a.inward_id).FirstOrDefault();
                        if (InDatee != null)
                        {
                            InDate = InDatee.date;
                        }
                        else
                        {
                            InDate = "NA";
                        }
                        var inwardDetails = _context.inwardPacket
                            .Where(a => a.pono == data.pono)
                            .GroupBy(p => p.pono)
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
                            data.InDate = InDate;
                        }
                        else
                        {
                            data.qty = purchaseSum;
                            data.pqty = purchaseSum - inwardDetails.TotalQuantity;

                            if (data.pqty < 0)
                            {
                                data.pqty = 0;
                            }
                            data.InDate = InDate;
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
                return View( "Internal server error: " + ex.Message);
            }
        }


        [HttpPost]
        public ActionResult ExportToPdf([FromBody] RowDataModel model)
        {
            // Filter out lists with zero elements
            model.RowData = model.RowData.Where(row => row.Count > 0).ToList();

            try
            {

                StringBuilder sb = new StringBuilder();


                // HttpContext.Session.GetString("UserName")
                // Table start.
                sb.Append("<table border='1' cellpadding='5' cellspacing='0' style='border: 1px solid #ccc;font-family: Arial; font-size: 10pt;'>");

                // Building the Header row.
                sb.Append("<tr>");
                sb.Append("<th style='background-color: #B8DBFD;border: 1px solid #ccc'>SrNo</th>");
                sb.Append("<th style='background-color: #B8DBFD;border: 1px solid #ccc'>AssetNumber</th>");
                sb.Append("<th style='background-color: #B8DBFD;border: 1px solid #ccc'>SubId</th>");
                sb.Append("<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Descp</th>");
                sb.Append("<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Category</th>");
                sb.Append("<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Plant</th>");
                sb.Append("<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Location</th>"); // Add a new column for the image.
              
                sb.Append("</tr>");
                int i = 1;
                // Building the Data rows.
                foreach (var data in model.RowData)
                {
                    sb.Append("<tr>");
                    sb.Append("<td style='border: 1px solid #ccc'>" + i + "</td>");
                    sb.Append("<td style='border: 1px solid #ccc'>" + data[1] + "</td>");
                    sb.Append("<td style='border: 1px solid #ccc'>" + data[2] + "</td>");
                    sb.Append("<td style='border: 1px solid #ccc'>" + data[3] + "</td>");
                    sb.Append("<td style='border: 1px solid #ccc'>" + data[4] + "</td>");
                    sb.Append("<td style='border: 1px solid #ccc'>" + data[5] + "</td>");
                    sb.Append("<td style='border: 1px solid #ccc'>" + data[6] + "</td>");
                       
                    sb.Append("</tr>");
                    i++;
                }

                // Table end.
                sb.Append("</table>");

                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString())))
                {
                    using (MemoryStream byteArrayOutputStream = new MemoryStream())
                    {
                        PdfWriter writer = new PdfWriter(byteArrayOutputStream);
                        PdfDocument pdfDocument = new PdfDocument(writer);
                        pdfDocument.SetDefaultPageSize(iText.Kernel.Geom.PageSize.A4);
                        HtmlConverter.ConvertToPdf(stream, pdfDocument, new ConverterProperties().SetBaseUri(""));
                        pdfDocument.Close();
                        return File(byteArrayOutputStream.ToArray(), "application/pdf", "AssetVerificationData.pdf");
                    }
                }



                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return Json(new { success = false, error = ex.Message });
            }
        }
        //public ActionResult PurchasePendingList()
        //{
        //    var q1 = _context.purchase.Where(a => a.status == "Pending").OrderByDescending(a => a.podate).ToList(); //pending orderd list
        //    List<purchase> intList = new List<purchase>(); //empyty list
        //    foreach (var data in q1)
        //    {
        //        var purchasesum = _context.poProduct_details.Where(a => a.porderid == data.id).Sum(a => a.quantity); //Purchase ordered qty == 40
        //        var inwarddetails = _context.inward.Where(a => a.pono == data.pono).FirstOrDefault(); //inwarded entered qty of that purchase order
        //        int inwardpacketqty = _context.inwardPacket.Where(a => a.inwardId == inwarddetails.inward_id).Sum(a => a.quantity); //inward product details qty

        //        if (inwarddetails == null)
        //        {
        //            data.qty = purchasesum;  //purcsae actual qty
        //            data.pqty = purchasesum; //purchase inwarded qty of that puchase ordered
        //        }
        //        else
        //        {
        //            data.qty = purchasesum;
        //            data.pqty = purchasesum - inwardpacketqty;
        //        }
        //        intList.Add(data);
        //    }
        //    return View("PurchasePendingList", intList);
        //}

        //PARTIALVIEW FOR DETAILS
        public ActionResult PurchasePendingList()
        {
            List<purchase> pendingOrders = new List<purchase>();

            var purchase = _context.purchase
                    .Where(po => po.status == "Pending").OrderByDescending(po => po.id).AsNoTracking()
                    .ToList();
            var InDate = "";
            foreach (var data in purchase)
            {
                var purchasesum = _context.poProduct_details.Where(a => a.porderid == data.id).Sum(a => a.quantity);
                var InDatee = _context.inward.Where(a => a.pono == data.pono && a.flag == 1).OrderByDescending(a=>a.inward_id).FirstOrDefault();
                if (InDatee != null)
                {
                    InDate = InDatee.date;
                }
                else
                {
                    InDate = "NA";
                }
                var inwarddetails = _context.inwardPacket.Where(a => a.pono == data.pono)
                          .GroupBy(p => p.pono)
                            .Select(group => new
                            {
                                ProductName = group.Key,
                                TotalQuantity = group.Sum(p => p.quantity),
                                TotalSubAssembly = group.Sum(p => p.totalsubassmbly)
                            }).FirstOrDefault();
                //var inwarddetails = _context.inward.Where(a => a.pono == data.pono && a.flag == 1).AsNoTracking().ToList();
                if (inwarddetails == null)
                {
                    data.qty = purchasesum;
                    data.pqty = purchasesum;
                    data.InDate = InDate;
                }
                else
                {
                    data.qty = purchasesum; // Setting qty to purchasesum
                    data.pqty = purchasesum - inwarddetails.TotalQuantity;
                    // Ensure pqty is not negative
                    if (data.pqty < 0)
                    {
                        data.pqty = 0;
                    }
                    data.InDate = InDate;
                }
                pendingOrders.Add(data);
            }
            
            return View("PurchasePendingList", pendingOrders);
        }

        //public IActionResult partialDetail(string porderid, string productcode, int qty)
        //{
        //    try
        //    {
        //        var purchase = new purchase();
        //        var purchase1 = _context.Product_Master.FirstOrDefault(p => p.productcode == productcode);
        //        if (purchase1 != null)
        //        {
        //            var poproductDetailsList = _context.productmaster_Packet
        //                .Where(pd => pd.productmasterId == purchase1.id)
        //                .Select(a => new
        //                {
        //                    a.subcomponents,
        //                    a.subcomponentcode,
        //                    a.qty,
        //                    a.uom,
        //                    //a.tqty // Include 'tqty' in the select query
        //                })
        //                .ToList();

        //            if (poproductDetailsList.Any())
        //            {
        //                foreach (var item in poproductDetailsList)
        //                {
        //                    // Additional checks before adding to the list
        //                    if (item.subcomponents != "-" && item.subcomponentcode != "-" && item.qty != 0 && item.uom != "-")
        //                    {
        //                        purchase.purchase_subcomponent.Add(new purchase_subcomponent
        //                        {
        //                            subcomponents = item.subcomponents,
        //                            sccode = item.subcomponentcode,
        //                            scqty = item.qty,
        //                            scuom = item.uom,
        //                            //tqty = (quantity * item.qty)// Assign 'tqty'
        //                        });
        //                    }

        //                }
        //                //return Json(poproductDetailsList);

        //                return View(purchase); // Return 'poProduct_details' instead of 'poproductDetailsList'
        //            }
        //            else
        //            {
        //                //return NotFound($"No product details found for pono: {productcode} and productCode: {productcode}");
        //                //return Json(new { message = $"No product details found for productCode: {productcode}" });
        //                return View(purchase);

        //            }
        //        }
        //        else
        //        {
        //            //return NotFound($"No purchase record found for pono: {productcode}");
        //            return Json(new { message = $"No product details found for pono: {productcode} " });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"Error: {ex.Message}");
        //    }







        //    //if (productcode == null)
        //    //{
        //    //    return NotFound();
        //    //}

        //    //// Fetch the so_inward entity based on orderid
        //    ////var so_inward = _context.so_inward.FirstOrDefault(i => i.id == orderid);
        //    ////var purchase = _context.purchase
        //    ////                .Include(i => i.poProduct_details)
        //    ////                .FirstOrDefault(i => i.id == porderid && i.poProduct_details.Any(p => p.productcode == productcode));
        //    //purchase p = new purchase();
        //    //var purchase = _context.poProduct_details.Where(a => a.porderid == p.id).FirstOrDefault();


        //    //if (purchase == null)
        //    //{
        //    //    return NotFound();
        //    //}
        //    //var purchasesc = _context.purchase_subcomponent.Where(a=>a.purchaseproduct_id == p.id).FirstOrDefault();

        //    //// Retrieve the subcomponent details associated with the so_inward
        //    //var subcomponentDetails = _context.poProduct_details
        //    //    .Where(p => p.porderid == purchase.id && p.productcode == productcode)
        //    //    .Select(p => new purchase_product
        //    //    {
        //    //        subcomponents = p.subcomponents,
        //    //        sccode = p.sccode,
        //    //        scuom = p.scuom,
        //    //        scqty = p.scqty,
        //    //        //tqty = (p.scqty * qty),
        //    //    })
        //    //    .ToList();

        //    //so.poProduct_details = subcomponentDetails;

        //    ////return PartialView("partialDetail", so);
        //    //return PartialView(so);
        //}

        public ActionResult _partialSubComponent(string productcode, string description, int quantity)
        {
            try
            {

                var purchase = new purchase();


                var purchase1 = _context.Product_Master.FirstOrDefault(p => p.productdescription.ToUpper() == description.ToUpper() && p.productcode.ToUpper() == productcode.ToUpper());

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
                            // Additional checks before adding to the list
                            if (item.subcomponents != "-" && item.subcomponentcode != "-" && item.qty != 0 && item.uom != "-")
                            {
                                purchase.purchase_subcomponent.Add(new purchase_subcomponent
                                {
                                    subcomponents = item.subcomponents,
                                    sccode = item.subcomponentcode,
                                    scqty = item.qty,
                                    scuom = item.uom,
                                    tqty = (quantity * item.qty)// Assign 'tqty'
                                });
                            }

                        }
                        //return Json(poproductDetailsList);

                        return View(purchase); // Return 'poProduct_details' instead of 'poproductDetailsList'
                    }
                    else
                    {
                        //return NotFound($"No product details found for pono: {productcode} and productCode: {productcode}");
                        //return Json(new { message = $"No product details found for productCode: {productcode}" });
                        return View(purchase);

                    }
                }
                else
                {
                    //return NotFound($"No purchase record found for pono: {productcode}");
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
        //    purchase purchase = new purchase();

        //    var product = _context.Product_Master
        //        .Include(e => e.Productmaster_Packets)
        //        .FirstOrDefault(p => p.productdescription == description);

        //    purchase_product purchase_product = new purchase_product();

        //    if (product != null)
        //    {
        //        bool productType = product.TypeOfProduct;

        //        if (productType == false)
        //        {
        //            var purchase1 = new purchase();

        //            var packets = product.Productmaster_Packets;

        //            if (packets != null && packets.Any())
        //            {
        //                //for()
        //                int i = 0;
        //                foreach (var packet in packets)
        //                {
        //                    i++;
        //                    purchase1.poProduct_details.Add(new purchase_product()
        //                    {
        //                        id = i,
        //                        subcomponents = packet.subcomponents,
        //                        scuom = packet.uom,
        //                        scqty = packet.qty,
        //                        sccode = packet.subcomponentcode,
        //                        tqty = (packet.qty * qty),
        //                    }) ;
        //                }
        //                return PartialView(purchase1);
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
            if (description != null)
            {

                ////var wbridge = _context.Product_Master.Where(n => n.productdescription == selectedValue).FirstOrDefault();
                ////return Json(wbridge);
                var purchase = new purchase();
                var product = _context.Product_Master
                    .Include(e => e.Productmaster_Packets)
                    .FirstOrDefault(p => p.productdescription.ToUpper() == description.ToUpper());

                if (product != null)
                {
                    bool productType = product.TypeOfProduct;

                    //var templateNames = _context.BOQproducts
                    //                    .Where(p => p.productcode == product.productcode)
                    //                    .Select(p => p.templatename)
                    //                    .Distinct()
                    //                    .ToList();

                    //// Add "Select Template" to the list of template names
                    //templateNames.Insert(0, "Select Template");

                    var result = new
                    {
                        id = product.id,
                        modelno = product.productcode.ToUpper(),
                        brand = product.brand,
                        uom = product.uom,
                        hsncode = product.hsncode,
                        producttype = productType,
                    };

                    //string productCode = product.productcode;
                    //TempData["ProductCode"] = productCode; // Store in TempData
                    //ViewBag.TemplateNames = ShowTemplateNames(productCode); // Pass productCode to generate the select list

                    return Json(result);
                }
            }
            return Ok();
        }
        private List<SelectListItem> Getdescription()
        {
            var lstProducts = new List<SelectListItem>();
            lstProducts = _context.Product_Master.AsNoTracking().OrderBy(n => n.productdescription.ToUpper()).Select(n =>
            new SelectListItem
            {
                Value = n.productdescription.ToUpper(),
                Text = n.productdescription.ToUpper()
            }).ToList();
            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "  Select ProductName  "
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
        public IActionResult ActionName_suppliername(string selectedValue)
        {
            var wbridge = _context.Supplier_Master.Where(n => n.supplier_name == selectedValue).FirstOrDefault();
            return Json(wbridge);
        }
        private List<SelectListItem> Getsuppliername()
        {
            var lstProducts = new List<SelectListItem>();

            lstProducts = _context.Supplier_Master.AsNoTracking().OrderBy(n => n.supplier_name).Select(n =>
            new SelectListItem
            {
                Value = n.supplier_name,
                Text = n.supplier_name
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "--- Select Supplier Name ---"
            };
            var defItem1 = new SelectListItem()
            {
                Value = "addNewSup",
                Text = "--- Add New Supplier Name ---"
            };

            lstProducts.Insert(0, defItem);
            lstProducts.Insert(lstProducts.Count(), defItem1);

            return lstProducts;
        }
        public async Task<IActionResult> PendingDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchase = await _context.purchase
                .FirstOrDefaultAsync(m => m.id == id);
            if (purchase == null)
            {
                return NotFound();
            }
            purchase purchases1 = _context.purchase.Include(e => e.poProduct_details).Where(a => a.id == id).FirstOrDefault();

            return View("PendingDetails", purchases1);
        }

        public ActionResult InwardPendingDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchase = _context.purchase.FirstOrDefaultAsync(m => m.id == id);
            if (purchase == null)
            {
                return NotFound();
            }
            purchase purchases = _context.purchase.Include(e => e.poProduct_details).Where(a => a.id == id).FirstOrDefault();

            return View(purchases);
        }
        public ActionResult GetPendingOrders(string status) // && a.flag==1
        {
            List<purchase> pendingOrders = new List<purchase>();
            var InDate = "";
            if (status == "Pending")
            {
                var purchase = _context.purchase
                    .Where(po => po.status == status).OrderByDescending(po=>po.id).AsNoTracking()
                    .ToList();

                foreach (var data in purchase.OrderByDescending(po => po.id))
                {
                    var purchasesum = _context.poProduct_details.Where(a => a.porderid == data.id).Sum(a => a.quantity);
                    var InDatee = _context.inward.Where(a => a.pono == data.pono && a.flag == 1).OrderByDescending(a => a.inward_id).FirstOrDefault();
                    if (InDatee != null)
                    {
                        InDate = InDatee.date;
                    }
                    else
                    {
                        InDate = "NA";
                    }
                    var inwarddetails = _context.inwardPacket.Where(a => a.pono == data.pono)
                              .GroupBy(p => p.pono)
                                       .Select(group => new
                                       {
                                           ProductName = group.Key,
                                           TotalQuantity = group.Sum(p => p.quantity),
                                           TotalSubAssembly = group.Sum(p => p.totalsubassmbly)
                                       }).FirstOrDefault();
                    //var inwarddetails = _context.inward.Where(a => a.pono == data.pono && a.flag == 1).AsNoTracking().ToList();
                    if (inwarddetails == null)
                    {
                        data.qty = purchasesum;
                        data.pqty = purchasesum; data.InDate = InDate;
                    }
                    else
                    {
                        data.qty = purchasesum; // Setting qty to purchasesum
                        data.pqty = purchasesum - inwarddetails.TotalQuantity;
                        // Ensure pqty is not negative
                        if (data.pqty < 0)
                        {
                            data.pqty = 0;
                        }
                        data.InDate = InDate;
                    }
                    pendingOrders.Add(data);
                }
            }
            else if (status == "Return")
            {
                var purchase = _context.purchase
                    .Where(po => po.status == status).OrderByDescending(po => po.id).AsNoTracking()
                    .ToList();

                foreach (var data in purchase.OrderByDescending(po => po.id))
                {
                    var purchasesum = _context.poProduct_details.Where(a => a.porderid == data.id).Sum(a => a.quantity);
                    var InDatee = _context.inward.Where(a => a.pono == data.pono && a.flag == 1).OrderByDescending(a => a.inward_id).FirstOrDefault();
                    if (InDatee != null)
                    {
                        InDate = InDatee.date;
                    }
                    else
                    {
                        InDate = "NA";
                    }
                    var inwarddetails = _context.inwardPacket.Where(a => a.pono == data.pono)
                              .GroupBy(p => p.pono)
                                       .Select(group => new
                                       {
                                           ProductName = group.Key,
                                           TotalQuantity = group.Sum(p => p.quantity),
                                           TotalSubAssembly = group.Sum(p => p.totalsubassmbly)
                                       }).FirstOrDefault();
                    //var inwarddetails = _context.inward.Where(a => a.pono == data.pono && a.flag == 1).AsNoTracking().ToList();
                    if (inwarddetails == null)
                    {
                        data.qty = purchasesum;
                        data.pqty = purchasesum; data.InDate = InDate;
                    }
                    else
                    {
                        data.qty = purchasesum; // Setting qty to purchasesum
                        data.pqty = purchasesum - inwarddetails.TotalQuantity;
                        // Ensure pqty is not negative
                        if (data.pqty < 0)
                        {
                            data.pqty = 0;
                        }
                        data.InDate = InDate;
                    }
                    pendingOrders.Add(data);
                }
            }
            else if (status != "All")
            {

                var purchase = _context.purchase
                .Where(po => po.status == status).OrderByDescending(po => po.id).AsNoTracking()
                .ToList();

                foreach (var data in purchase.OrderByDescending(po => po.id))
                {
                    var purchasesum = _context.poProduct_details.Where(a => a.porderid == data.id).Sum(a => a.quantity);
                    var InDatee = _context.inward.Where(a => a.pono == data.pono && a.flag == 1).OrderByDescending(a => a.inward_id).FirstOrDefault();
                    if (InDatee != null)
                    {
                        InDate = InDatee.date;
                    }
                    else
                    {
                        InDate = "NA";
                    }
                    var inwarddetails = _context.inwardPacket.Where(a => a.pono == data.pono)
                              .GroupBy(p => p.pono)
                                       .Select(group => new
                                       {
                                           ProductName = group.Key,
                                           TotalQuantity = group.Sum(p => p.quantity),
                                           TotalSubAssembly = group.Sum(p => p.totalsubassmbly)
                                       }).FirstOrDefault();
                    //var inwarddetails = _context.inward.Where(a => a.pono == data.pono && a.flag == 1).AsNoTracking().ToList();
                    if (inwarddetails == null)
                    {
                        data.qty = purchasesum;
                        data.pqty = purchasesum; data.InDate = InDate;
                    }
                    else
                    {
                        data.qty = purchasesum; // Setting qty to purchasesum
                        data.pqty = purchasesum - inwarddetails.TotalQuantity;
                        // Ensure pqty is not negative
                        if (data.pqty < 0)
                        {
                            data.pqty = 0;
                        }
                        data.InDate = InDate;
                    }
                    pendingOrders.Add(data);
                }
            }
            else
            {
                var purchase = _context.purchase.ToList();

                foreach (var data in purchase.OrderByDescending(po => po.id))
                {
                    var purchasesum = _context.poProduct_details.Where(a => a.porderid == data.id).OrderByDescending(po => po.id).Sum(a => a.quantity);
                    var InDatee = _context.inward.Where(a => a.pono == data.pono && a.flag == 1).OrderByDescending(a => a.inward_id).FirstOrDefault();
                    if (InDatee != null)
                    {
                        InDate = InDatee.date;
                    }
                    else
                    {
                        InDate = "NA";
                    }
                    var inwarddetails = _context.inwardPacket.Where(a => a.pono == data.pono)
                              .GroupBy(p => p.pono)
                                       .Select(group => new
                                       {
                                           ProductName = group.Key,
                                           TotalQuantity = group.Sum(p => p.quantity),
                                           TotalSubAssembly = group.Sum(p => p.totalsubassmbly)
                                       }).FirstOrDefault();
                    //var inwarddetails = _context.inward.Where(a => a.pono == data.pono && a.flag == 1).AsNoTracking().ToList();
                    if (inwarddetails == null)
                    {
                        data.qty = purchasesum;
                        data.pqty = purchasesum; data.InDate = InDate;
                    }
                    else
                    {
                        data.qty = purchasesum; // Setting qty to purchasesum
                        data.pqty = purchasesum - inwarddetails.TotalQuantity;
                        // Ensure pqty is not negative
                        if (data.pqty < 0)
                        {
                            data.pqty = 0;
                        }
                        data.InDate = InDate;
                    }
                    pendingOrders.Add(data);
                }

                pendingOrders = _context.purchase.ToList();
            }

            return PartialView("_PendingOrdersPartial", pendingOrders);
        }

        // GET: purchases/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchase = await _context.purchase.Include(s => s.poProduct_details).FirstOrDefaultAsync(m => m.id == id);

            if (purchase == null)
            {
                return NotFound();
            }

            return View(purchase);

            //if (id == null || _context.purchase == null)
            //{
            //    return NotFound();
            //}

            //var purchase = await _context.purchase.FirstOrDefaultAsync(m => m.id == id);
            //if (purchase == null)
            //{
            //    return NotFound();
            //}
            //purchase purchases = _context.purchase.Include(e => e.poProduct_details).Where(a => a.id == id).FirstOrDefault();

            //return View(purchases);
        }

        // GET: purchases/Create
        public IActionResult Create()
        {
            //GET bag :Supplier name
            ViewBag.suppliername = Getsuppliername();
            ViewBag.description = Getdescription();
            // Retrieve TempData for productCode if set
            //string productCode = TempData["ProductCode"] as string;
            //if (!string.IsNullOrEmpty(productCode))
            //{
            //    ViewBag.TemplateNames = ShowTemplateNames(productCode); // Generate the select list
            //}
            purchase purchase = new purchase();
            purchase.poProduct_details.Add(new purchase_product { id = 1 });
            // Set the default status to 'Pending'
            purchase.status = "Pending";
            return View(purchase);
        }

        // POST: CheckSono/so_inward
        [HttpPost]
        public ActionResult PoCheck(string pono)
        {
            var ponoo = pono.Trim();

            if (pono != null)
            {
                var find = _context.purchase.FirstOrDefault(a => a.pono.Trim() == ponoo);
                if (find != null)
                {
                    return Json(new { success = false, message = "Purchase order number " + ponoo + " is already in use!" });
                }
                else
                {
                    return Json(new { success = true, message = "Purchase order number is available." });
                }
            }
            return Json(new { success = true, message = "Purchase order number is available." });
        }


            //public async Task<IActionResult> Create(purchase purchase, eros.Models.purchase _partialSubComponent)
        //{
        //    int maxId = _context.purchase.Any() ? _context.purchase.Max(e => e.id) + 1 : 1;

        //    // Set the order ID for the so_inward entity
        //    purchase.id = maxId;

        //    // Add the so_inward entity to the context
        //    _context.Add(purchase);

        //    await _context.SaveChangesAsync();

        //    // Create a list to hold new so_product entities
        //    var newProducts = new List<purchase_product>();

        //    //SoProduct details count in list - product list 
        //    foreach (var product in purchase.poProduct_details.Where(i => i.porderid == purchase.id).ToList())
        //    {
        //        //getting selected product complete details with sub component 
        //        var productmaster = _context.Product_Master
        //            .Include(e => e.Productmaster_Packets)
        //            .FirstOrDefault(p => p.productdescription == product.description);

        //        //set both orderid with id
        //        product.porderid = purchase.id;

        //        //get details from productmaster_packet
        //        foreach (var a in productmaster.Productmaster_Packets)
        //        {
        //            //if product is individual
        //            if (product.subcomponents == "-" && product.scqty == 0 && product.sccode == "-" && product.scuom == "-")
        //            {
        //                product.subcomponents = a.subcomponents;
        //                product.scuom = a.uom;
        //                product.scqty = a.qty;
        //                product.sccode = a.subcomponentcode;
        //                product.tqty = purchase.qty;
        //            }
        //            else
        //            {
        //                // Create a new so_product entity with the same details
        //                var newProduct = new purchase_product
        //                {
        //                    porderid = purchase.id,
        //                    productname = product.productname,
        //                    productcode = product.productcode,
        //                    description = product.description,
        //                    brandname = product.brandname,
        //                    quantity = product.quantity,
        //                    uom = product.uom,
        //                    sccode = a.subcomponentcode,
        //                    subcomponents = a.subcomponents,
        //                    scqty = a.qty,
        //                    scuom = a.uom,
        //                    //tqty = ?,
        //                };
        //                newProducts.Add(newProduct);
        //            }
        //        }
        //    }

        //    // Add the new so_product entities to the context
        //    _context.AddRange(newProducts);

        //    await _context.SaveChangesAsync();

        //    return RedirectToAction(nameof(Index));
        //}

        // POST: purchases/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(purchase purchase)
        {
            try
            {
                purchase.pono = purchase.pono.Trim().ToUpper();
                int maxId = _context.purchase.Any() ? _context.purchase.Max(e => e.id) + 1 : 1;
                purchase.id = maxId;
                purchase.purchase_subcomponent.Clear();
                var newProducts = new List<purchase_subcomponent>();

                foreach (var product in purchase.poProduct_details)
                {
                    //getting selected product complete details with sub component 
                    var productmaster = _context.Product_Master
                        .Include(e => e.Productmaster_Packets)
                        .FirstOrDefault(p => p.productdescription.ToUpper() == product.description.ToUpper());

                    //set both orderid with id
                    product.porderid = purchase.id;

                    //get details from productmaster_packet
                    foreach (var a in productmaster.Productmaster_Packets)
                    {
                        //if product is individual

                        if (productmaster.TypeOfProduct == false)
                        {
                            // Create a new so_product entity with the same details
                            var newProduct = new purchase_subcomponent
                            {
                                sccode = a.subcomponentcode,
                                subcomponents = a.subcomponents,
                                scqty = a.qty,
                                scuom = a.uom,
                                tqty = (a.qty * product.quantity),
                            };
                            newProducts.Add(newProduct);
                        }

                    }
                }
                purchase.purchase_subcomponent.AddRange(newProducts);

                _context.Add(purchase);

                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "Purchase Order Operation";
                logs.task = "Genrate Purchase Order";
                logs.taskid = maxId;
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.action = "Create";
                logs.username = user;
                _context.Add(logs);
                _context.SaveChanges();
                _notifyService.Success("Create Successfully !");
                // Log successful creation
                //Console.WriteLine($"Purchase Order {purchase.id} created successfully with {newProducts.Count} subcomponents.");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error creating purchase order: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        //public async Task<IActionResult> Create(purchase purchase)
        //{
        //    int maxId = _context.purchase.Any() ? _context.purchase.Max(e => e.id) + 1 : 1;
        //    // Set the order ID for the so_inward entity
        //    purchase.id = maxId;

        //    // Add the so_inward entity to the context
        //    _context.Add(purchase);

        //    await _context.SaveChangesAsync();

        //    // Create a list to hold new so_product entities
        //    var newProducts = new List<purchase_product>();

        //    //SoProduct details count in list - product list 
        //    foreach (var product in purchase.poProduct_details.Where(i => i.porderid == purchase.id).ToList())
        //    {
        //        //getting selected product complete details with sub component 
        //        var productmaster = _context.Product_Master
        //            .Include(e => e.Productmaster_Packets)
        //            .FirstOrDefault(p => p.productdescription == product.description);

        //        //set both orderid with id
        //        product.porderid = purchase.id;

        //        //get details from productmaster_packet
        //        foreach (var a in productmaster.Productmaster_Packets)
        //        {
        //            //if product is individual
        //            if (product.subcomponents == "-" && product.scqty == 0 && product.sccode == "-" && product.scuom == "-")
        //            {

        //                product.subcomponents = a.subcomponents;
        //                product.scuom = a.uom;
        //                product.scqty = a.qty;
        //                product.sccode = a.subcomponentcode;
        //                product.tqty = (purchase.qty * a.qty); // Calculate 'tqty' based on 'quantity' and 'a.qty'
        //            }
        //            else
        //            {
        //                // Create a new so_product entity with the same details
        //                var newProduct = new purchase_product
        //                {
        //                    porderid = purchase.id,
        //                    productname = product.productname,
        //                    productcode = product.productcode,
        //                    description = product.description,
        //                    brandname = product.brandname,
        //                    quantity = product.quantity,
        //                    uom = product.uom,
        //                    sccode = a.subcomponentcode,
        //                    subcomponents = a.subcomponents,
        //                    scqty = a.qty,
        //                    scuom = a.uom,
        //                    tqty = (purchase.qty * a.qty) // Calculate 'tqty' based on 'quantity' and 'a.qty'

        //                };
        //                newProducts.Add(newProduct);
        //            }
        //        }
        //    }

        //    // Add the new so_product entities to the context
        //    _context.AddRange(newProducts);

        //    await _context.SaveChangesAsync();

        //    return RedirectToAction(nameof(Index));
        //}

        private List<SelectListItem> GetStatus()
        {
            var lstProducts = new List<SelectListItem>();

            lstProducts = _context.Supplier_Master.AsNoTracking().OrderBy(n => n.supplier_name).Select(n =>
            new SelectListItem
            {
                Value = n.supplier_name,
                Text = n.supplier_name
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "--- Select Supplier Name ---"
            };
            var defItem1 = new SelectListItem()
            {
                Value = "addNewSup",
                Text = "--- Add New Supplier Name ---"
            };

            lstProducts.Insert(0, defItem);
            lstProducts.Insert(lstProducts.Count(), defItem1);

            return lstProducts;
        }


        [HttpGet]
        public ActionResult EditStatus(int? id)
        {
            ViewBag.suppliername = Getsuppliername();
            ViewBag.description = Getdescription();
            if (id == null)
            {
                return NotFound();
            }
            //var purchase = _context.purchase.Where(a => a.id == id).FirstOrDefault();
            var purchase = _context.purchase.Where(a => a.id == id).Include(a => a.poProduct_details).FirstOrDefault();

            if (purchase == null)
            {
                return NotFound();
            }

            //get status values
            // Determine the status and bind the dropdown values accordingly
            List<string> statusOptions = new List<string>();
            var noinward = _context.inward.Where(a => a.pono.Trim() == purchase.pono.Trim()).FirstOrDefault();
            if (purchase != null && noinward != null)
            {
                //COMPLETE
                statusOptions.Add("Completed");
                statusOptions.Add("Return");
            }
            else if (purchase != null && noinward == null)
            {
                //PENDING
                statusOptions.Add("Pending");
                statusOptions.Add("Cancel");
            }

            ViewBag.GetStatus = new SelectList(statusOptions);
            //end

            return View("EditStatus", purchase);
        }

        // POST: purchases/Edit/5

        // GET: purchases/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            ViewBag.suppliername = Getsuppliername();
            ViewBag.description = Getdescription();
            if (id == null || _context.purchase == null)
            {
                return NotFound();
            }
            var po_inward = _context.purchase.Where(a => a.id == id).Include(a => a.poProduct_details).FirstOrDefault();
            if (po_inward == null)
            {
                return NotFound();
            }
            return View(po_inward);
            //return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, purchase purchase)
        //{
        //    if (id != purchase.id)
        //    {
        //        return NotFound();
        //    }
        //    try
        //    {

        //        var existingOrder = _context.purchase.FirstOrDefault(a => a.id == id);
        //        if (existingOrder == null)
        //        {
        //            return NotFound();
        //        }

        //        if (string.IsNullOrEmpty(purchase.status))
        //        {
        //            var response = new { success = false, message = "Please select a status to update." };
        //            return Json(response);
        //        }

        //        if (existingOrder.status == purchase.status)
        //        {
        //            var response = new { success = false, message = "Order is already in pending. Cannot update status." };
        //            return Json(response);
        //        }

        //        List<purchase_product> productDetails = _context.poProduct_details.Where(d => d.porderid == purchase.id).ToList();
        //        _context.poProduct_details.RemoveRange(productDetails);
        //        // Set warranty to "No Warranty" for each product based on the status
        //        foreach (var product in purchase.poProduct_details)
        //        {
        //            if (purchase.status == "Cancel")
        //            {
        //                //product.Warranty = "No";
        //            }
        //            if (purchase.status == "Delete")
        //            {
        //                //product.Warranty = "No";
        //                purchase.status = "Delete";
        //            }
        //            if (purchase.status == "Return")
        //            {
        //                //product.Warranty = "No";
        //                purchase.status = "Return";
        //            }
        //        }
        //        _context.Update(purchase);
        //        //maintain logs
        //        var user = HttpContext.Session.GetString("User");
        //        var logs = new Logs();
        //        logs.pagename = "Purchase Order Operation";
        //        logs.task = purchase.id + "$" + purchase.pono + "$" + purchase.podate + "$" + purchase.contactno + "$" + purchase.suppliername + "$" + purchase.contactno + "$" + purchase.gstinno + "$" + purchase.address+ "$" + purchase.status+ "$" + purchase.pqty+ "$" + purchase.qty;
        //        logs.taskid = id;
        //        logs.date = DateTime.Now.ToString("dd/MM/yyyy");
        //        logs.time = DateTime.Now.ToString("HH:mm:ss");
        //        logs.username = user;
        //        logs.action = "Update";
        //        _context.Add(logs);
        //        _context.SaveChanges();
        //        _notifyService.Success("Purchase Order Updated Succesfully");
        //        //return RedirectToAction("Index");
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!purchaseExists(purchase.id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //    //return RedirectToAction(nameof(Index));
        //    return RedirectToAction("PurchasePendingList", "purchases");

        //    return View(purchase);
        //}

        public async Task<IActionResult> Edit(int id, purchase purchase)
        {
            if (id != purchase.id)
            {
                return NotFound();
            }
            try
            {
                List<purchase_product> productDetails = _context.poProduct_details.Where(d => d.porderid == purchase.id).AsNoTracking().ToList();

                var getdata = _context.purchase
                    .Where(a => a.id == id).AsNoTracking()
                    .Include(a => a.poProduct_details)
                    .FirstOrDefault();

                var exist = _context.inward.FirstOrDefault(a => a.pono.Trim() == getdata.pono.Trim());
                if (exist != null)
                {
                    return Json(new { success = false, Message = "You can't update that purchase order " + getdata.pono + " Inward already done!" });
                }
                else
                {
                    _context.poProduct_details.RemoveRange(productDetails);
                    _context.Update(purchase);
                }
                //_context.poProduct_details.RemoveRange(productDetails);
                //_context.Update(purchase);
                //_context.SaveChangesAsync();

                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "Purchase Order Operation";
                logs.task = purchase.id + "$" + purchase.pono + "$" + purchase.podate + "$" + purchase.contactno + "$" + purchase.suppliername + "$" + purchase.contactno + "$" + purchase.gstinno + "$" + purchase.address + "$" + purchase.status + "$" + purchase.pqty + "$" + purchase.qty;
                logs.taskid = id;
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.username = user;
                logs.action = "Update";
                _context.Add(logs);

                _context.SaveChanges();
                
                //_notifyService.Success("Purchase Order Updated Succesfully");
                return Json(new { success = true, message = "Order Update Successfully !" });
            }
            catch (Exception ex)
            {
                return View(ex);
            }
        }
        [HttpPost]
        public async Task<IActionResult> EditStatus(int id, purchase purchase)
        {
            try
            {
                if (id != purchase.id)
                {
                    return NotFound();
                }

                var existingOrder = _context.purchase.FirstOrDefault(a => a.id == id);
                if (existingOrder == null)
                {
                    return NotFound();
                }

                if (string.IsNullOrEmpty(purchase.status))
                {
                    var response = new { success = false, message = "Please select a status to update." };
                    return Json(response);
                }

                if (existingOrder.status == purchase.status)
                {
                    var response = new { success = false, message = "Order is already in pending. Cannot update status." };
                    return Json(response);
                }

                existingOrder.status = purchase.status;
                _context.Update(existingOrder);

                var user = HttpContext.Session.GetString("User");
                var logs = new Logs
                {
                    pagename = "Purchase Order Operation",
                    task = "Update Purchase Order",
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
                var errorResponse = new { success = false, message = "An error occurred while updating the purchase order status." };
                return Json(errorResponse);
            }
        }

        private bool purchaseExists(int id)
        {
            return (_context.purchase?.Any(e => e.id == id)).GetValueOrDefault();
        }

        //public async Task<IActionResult> Edit(int id, purchase updatedPurchase)
        //{
        //    if (id != updatedPurchase.id)
        //    {
        //        return NotFound();
        //    }
        //    try
        //    {
        //        // Retrieve the existing entity from the context
        //        var existingPurchase = await _context.purchase
        //            .Include(p => p.poProduct_details) // Include related purchase product details
        //            .FirstOrDefaultAsync(p => p.id == id);

        //        if (existingPurchase == null)
        //        {
        //            return NotFound();
        //        }

        //        // Generate change log
        //        var changeLog = GenerateChangeLog(existingPurchase, updatedPurchase);

        //        // Update purchase product details
        //        foreach (var updatedProductDetail in updatedPurchase.poProduct_details)
        //        {
        //            var existingProductDetail = existingPurchase.poProduct_details
        //                .FirstOrDefault(pd => pd.id == updatedProductDetail.id);

        //            if (existingProductDetail != null)
        //            {
        //                // Update existing product detail
        //                _context.Entry(existingProductDetail).CurrentValues.SetValues(updatedProductDetail);
        //            }
        //            else
        //            {
        //                // Add new product detail
        //                existingPurchase.poProduct_details.Add(updatedProductDetail);
        //            }
        //        }

        //        // Update the existing purchase entity
        //        _context.Entry(existingPurchase).CurrentValues.SetValues(updatedPurchase);

        //        // Maintain logs
        //        var user = HttpContext.Session.GetString("User");
        //        var logs = new Logs
        //        {
        //            pagename = "Purchase Order Operation",
        //            task = changeLog,
        //            action = "Update",
        //            taskid = id,
        //            date = DateTime.Now.ToString("dd/MM/yyyy"),
        //            time = DateTime.Now.ToString("HH:mm:ss"),
        //            username = user
        //        };
        //        _context.Add(logs);

        //        // Save changes
        //        await _context.SaveChangesAsync();

        //        _notifyService.Success("Purchase Order Updated Successfully");

        //        return RedirectToAction("PurchasePendingList", "purchases");
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!purchaseExists(updatedPurchase.id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //}

        //private bool purchaseExists(int id)
        //{
        //    return _context.purchase.Any(e => e.id == id);
        //}
        //private string GenerateChangeLog(purchase existingPurchase, purchase updatedPurchase)
        //{
        //    StringBuilder changeLogBuilder = new StringBuilder();

        //    if (existingPurchase.pono != updatedPurchase.pono)
        //    {
        //        changeLogBuilder.Append($"{existingPurchase.pono} -> {updatedPurchase.pono} | ");
        //    }

        //    if (existingPurchase.podate != updatedPurchase.podate)
        //    {
        //        changeLogBuilder.Append($"{existingPurchase.podate} -> {updatedPurchase.podate} | ");
        //    }

        //    if (existingPurchase.suppliername != updatedPurchase.suppliername)
        //    {
        //        changeLogBuilder.Append($"{existingPurchase.suppliername} -> {updatedPurchase.suppliername} | ");
        //    }

        //    if (existingPurchase.contactno != updatedPurchase.contactno)
        //    {
        //        changeLogBuilder.Append($"{existingPurchase.contactno} -> {updatedPurchase.contactno} | ");
        //    }

        //    if (existingPurchase.gstinno != updatedPurchase.gstinno)
        //    {
        //        changeLogBuilder.Append($"{existingPurchase.gstinno} -> {updatedPurchase.gstinno} | ");
        //    }

        //    if (existingPurchase.address != updatedPurchase.address)
        //    {
        //        changeLogBuilder.Append($"{existingPurchase.address} -> {updatedPurchase.address} | ");
        //    }

        //    if (existingPurchase.status != updatedPurchase.status)
        //    {
        //        changeLogBuilder.Append($"{existingPurchase.status} -> {updatedPurchase.status}");
        //    }

        //    // Add similar comparisons for other properties

        //    return changeLogBuilder.ToString();
        //}


        public async Task<IActionResult> PendingEdit(int? id)
        {
            if (id == null || _context.purchase == null)
            {
                return NotFound();
            }
            var po_inward = _context.purchase.Where(a => a.id == id).Include(a => a.poProduct_details).FirstOrDefault();
            if (po_inward == null)
            {
                return NotFound();
            }
            return View(po_inward);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PendingEdit(int id, purchase purchase)
        {
            if (id != purchase.id)
            {
                return NotFound();
            }
            try
            {
                List<purchase_product> productDetails = _context.poProduct_details.Where(d => d.porderid == purchase.id).ToList();
                _context.poProduct_details.RemoveRange(productDetails);
                _context.SaveChanges();

                _context.Update(purchase);
                await _context.SaveChangesAsync();
                _notifyService.Success("Purchase Order Updated Succesfully");
                //return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!purchaseExists(purchase.id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
            return View("PendingEdit", purchase);
        }

        // GET: purchases/Delete/5S
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.purchase == null)
            {
                return NotFound();
            }

            var purchase = await _context.purchase
                .FirstOrDefaultAsync(m => m.id == id);
            if (purchase == null)
            {
                return NotFound();
            }

            return View(purchase);
        }

        // POST: purchases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.purchase == null)
            {
                return Problem("Entity set 'ErosDbContext.purchase'  is null.");
            }
            var purchase = await _context.purchase.FindAsync(id);
            if (purchase != null)
            {
                _context.purchase.Remove(purchase);
            }
            //maintain logs
            var user = HttpContext.Session.GetString("User");
            var logs = new Logs();
            logs.pagename = "Purchase Order Operation";
            //logs.task = "Delete Purchase Order";
            logs.taskid = id;
            logs.task = purchase.id + "$" + purchase.pono + "$" + purchase.podate + "$" + purchase.suppliername + "$" + purchase.contactno + "$" + purchase.gstinno + "$" + purchase.address + "$" + purchase.status;
            logs.action = "Delete";
            logs.date = DateTime.Now.ToString("dd/MM/yyyy");
            logs.time = DateTime.Now.ToString("HH:mm:ss");
            logs.username = user;
            _context.Add(logs);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

    }
    public class RowDataModel
    {
        public List<List<string>> RowData { get; set; }
    }
}
