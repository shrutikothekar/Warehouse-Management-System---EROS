using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AuthSystem.Data;
using eros.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Identity.Client.Extensions.Msal;
using Newtonsoft.Json;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Hosting;
using System.Text.RegularExpressions;
using System.Text;
using System.Globalization;

namespace eros.Controllers
{
    public class Product_MasterController : Controller
    {
        private readonly ErosDbContext _context;
        public INotyfService _notyfService { get; }
        public Product_MasterController(ErosDbContext context, INotyfService notyfService)
        {
            _notyfService = notyfService;
            _context = context;
        }


        public IActionResult LoadPartialSubComponents(string selectedvalue, string productCodeValue)
        {
            //ViewBag.ProductCode = productCodeValue;

            var productMaster = new Product_Master();
            productMaster.Productmaster_Packets.Add(new productmaster_packet() { Id = 1,subcomponentcode= productCodeValue+"-1" });
            //productMaster.Productmaster_Packets.Add(new productmaster_packet() { Id = 1, subcomponentcode = selectedvalue + "-B" });
            return PartialView("_partialSubComponent", productMaster);
        }
        private List<SelectListItem> uomnames()
        {
            var lstProducts = new List<SelectListItem>();

            lstProducts = _context.uom.AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.shortcut,
                Text = n.shortcut
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select UOM----"
            };

            lstProducts.Insert(0, defItem);

            return lstProducts;
        }
        [HttpGet]

        public ActionResult ChangeCategory(string selectedValue)
        {
            List<SelectListItem> category = _context.subcategory_Master.Where(x => x.categoryname == selectedValue).AsNoTracking()
             .OrderBy(n => n.categoryname)
                 .Select(n =>
                 new SelectListItem
                 {
                     Selected = true,
                     Value = n.subcategory_name,
                     Text = n.subcategory_name
                 }).ToList();
            return Json(category);
        }
        [HttpPost]
        public IActionResult ActionName(string optionValue, string optionValue1)
        {
            var subcategory = _context.subcategory_Master.Where(a => a.subcategory_name.Equals(optionValue)).FirstOrDefault();
            return Json(new { data = subcategory }); // Return the data to bind to the textbox
        }
        [HttpPost]
        public IActionResult ActionNameRE(string optionValue, string optionValue1)
        {
            var subcategory = _context.subcategory_Master.Where(a => a.subcategory_name.Equals(optionValue)).FirstOrDefault();
            return Json(new { data = subcategory }); // Return the data to bind to the textbox
        }
        [HttpPost]
        public IActionResult ActionNameRL(string optionValue, string optionValue1)
        {
            var subcategory = _context.subcategory_Master.Where(a => a.subcategory_name.Equals(optionValue)).FirstOrDefault();
            return Json(new { data = subcategory }); // Return the data to bind to the textbox
        }

        //GET beg data from GRADE

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
            //lstProducts = _context.subcategory_Master.AsNoTracking().Select(n =>
            //new SelectListItem
            //{
            //    Value = n.subcategory_name,
            //    Text = n.subcategory_name
            //}).ToList();
            lstProducts = _context.subcategory_Master
    .AsNoTracking()
    .Select(n => n.subcategory_name) // Select just the subcategory names
    .Distinct() // Get distinct subcategory names
    .Select(n => new SelectListItem
    {
        Value = n,
        Text = n
    })
    .ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "--Select SubCategory--"
            };
            lstProducts.Insert(0, defItem);
            return lstProducts;
        }
        // GET: Product_Master
        public async Task<IActionResult> Index()
        {
            //return _context.Product_Master != null ?
            //            View(await _context.Product_Master.ToListAsync()) :
            //            Problem("Entity set 'ErosDbContext.Product_Master'  is null.");
            var pendingOrders = _context.Product_Master
       .OrderByDescending(a => a.id)
       .ToList();

            return View(pendingOrders);

        }
        
        //public IActionResult Index()
        //{
        //    List<Product_Master> suppliers = new List<Product_Master>();
        //    suppliers = _context.Product_Master.ToList();
        //    return View(suppliers);
        //}

        // GET: Product_Master/Details/5

        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                Product_Master product_Masterss = _context.Product_Master.Include(e => e.Productmaster_Packets).Where(a => a.id == id).FirstOrDefault();
                    
                    //maintain logs
                    var user = HttpContext.Session.GetString("User");
                    var logs = new Logs();
                    logs.pagename = "Product Master";
                    logs.task = "View product master ";
                    logs.action = "View";
                    logs.taskid = Convert.ToInt32(id);
                    logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                    logs.time = DateTime.Now.ToString("HH:mm:ss");
                    logs.username = user;
                    _context.Add(logs);

                _context.SaveChanges();
                return View(product_Masterss);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
            if (id == null || _context.Product_Master == null)
            {
                return NotFound();
            }
            var product_Master = await _context.Product_Master
                .FirstOrDefaultAsync(m => m.id == id);
            if (product_Master == null)
            {
                return NotFound();
            }
            return View(product_Master);
        }

        // GET: Product_Master/Create
        public IActionResult Create()
        {
            ViewBag.categoryname = Getcategoryname();
            ViewBag.subcategory = Getsubcategory();
            ViewBag.uomnames = uomnames();

            // Retrieve the generated product code from TempData
            string generatedCode = TempData["GeneratedProductCode"] as string;
            ViewBag.GeneratedProductCode = generatedCode;

            Product_Master pm = new Product_Master();
            pm.Productmaster_Packets.Add(new productmaster_packet() { Id = 1 });
            return View();
        }
        public IActionResult _PartialProductView()
        {
            return View();
        }
        public int GetMaxId(string selectedValue)
        {
            int maxId = _context.Product_Master.Any() ? _context.Product_Master.Max(e => e.id) : 0;
            return maxId + 1;
        }
        //[HttpPost]
        //public ActionResult ProductMasterCheck(string productcode)
        //{
        //    if (productcode.Trim() != null)
        //    {
        //        string pattern = @"[^a-zA-Z0-9\-\/]";

        //        productcode = Regex.Replace(productcode.Trim(), pattern, "");

        //        var find = _context.Product_Master.ToList();

        //        bool isDuplicate = false; 

        //        foreach (var item in find)
        //        {
        //            item.productcode = Regex.Replace(item.productcode.Trim(), pattern, "");
        //            if (item.productcode.ToUpper() == productcode.Trim().ToUpper())
        //            {
        //                isDuplicate = true;
        //                break; 
        //            }
        //        }

        //        if (isDuplicate)
        //        {
        //            return Json(new { success = false, message = "Product code already exists in the database." });
        //        }
        //        else
        //        {
        //            return Json(new { success = true, message = "Available." });
        //        }
        //    }
        //    return Json(new { success = true, message = "Product code is required." });
        //}

        [HttpPost]
        public ActionResult ProductMasterCheck(string productcode)
        {
            if (!string.IsNullOrWhiteSpace(productcode))
            {
                string normalizedProductCode = Regex.Replace(productcode.Trim().ToUpper(), @"\s+", "");

                var find = _context.Product_Master.ToList();

                foreach (var item in find)
                {
                    string dbProductCode = Regex.Replace(item.productcode.Trim().ToUpper(), @"\s+", "");
                    if (dbProductCode == normalizedProductCode)
                    {
                        return Json(new { success = false, message = "Product code already exists in the database." });
                    }
                }

                return Json(new { success = true, message = "Available." });
            }
            return Json(new { success = true, message = "Product code is required." });
        }


        //public ActionResult ProductMasterCheckdes(string description)
        //{

        //    if (description.Trim() != null)
        //    {
        //        // Define a regular expression pattern to keep only alphanumeric characters, hyphens, slashes
        //        //string pattern = @"[^\-\/]";
        //        string pattern = @"[^a-zA-Z0-9\\/]";

        //        // Remove characters that do not match the pattern from the input description
        //        description = Regex.Replace(description.Trim(), pattern, "");

        //        var find = _context.Product_Master.ToList();

        //        bool isDuplicate = false; // Flag to track if any duplicate is found

        //        foreach (var item in find)
        //        {
        //            item.productdescription = Regex.Replace(item.productdescription.Trim(), pattern, "");
        //            if (item.productdescription == description.Trim())
        //            {
        //                isDuplicate = true;
        //                break; // Exit loop once a duplicate is found
        //            }
        //        }

        //        if (isDuplicate)
        //        {
        //            return Json(new { success = false, message = "Product description already exists in the database." });
        //        }
        //        else
        //        {
        //            return Json(new { success = true, message = "Available." });
        //        }
        //    }
        //    return Json(new { success = true, message = "Product description is required." });
        //}


        public ActionResult ProductMasterCheckdes(string description)
        {

            if (!string.IsNullOrWhiteSpace(description))
            {
                string normalizedProductCode = Regex.Replace(description.Trim().ToUpper(), @"\s+", "");

                var find = _context.Product_Master.ToList();

                foreach (var item in find)
                {
                    string dbProductCode = Regex.Replace(item.productdescription.Trim().ToUpper(), @"\s+", "");
                    if (dbProductCode == normalizedProductCode)
                    {
                        return Json(new { success = false, message = "Product description already exists in the database." });
                    }
                }

                return Json(new { success = true, message = "Available." });
            }
            return Json(new { success = true, message = "Product description is required." });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product_Master product_Master)
        {
            //if(product_Master.subcategory == "--Select SubCategory--" || product_Master.categoryname == )
            //var exist = _context.Product_Master.Where(a => a.productcode == product_Master.productcode && a.productdescription == product_Master.productdescription).FirstOrDefault();
            //if (exist == null)
            //{
            product_Master.productcode = product_Master.productcode.ToUpper();
           // product_Master.productdescription = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(product_Master.productdescription.ToLower());
            product_Master.productdescription = product_Master.productdescription.ToUpper();
            int maxId = _context.Product_Master.Any() ? _context.Product_Master.Max(e => e.id) + 1 : 1;
                product_Master.id = maxId;
                productmaster_packet pmp = new productmaster_packet();
                _context.Add(product_Master);
            
                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "Product Master";
                logs.task = "Create product master ";
                logs.action = "Create";
                logs.taskid = Convert.ToInt32(maxId);
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.username = user;
                _context.Add(logs);
                _context.SaveChanges();
                _notyfService.Success("Product Details Created Successfully");
                //return Json(new { success = true, message = "Product Master create successfully !" });
                return RedirectToAction(nameof(Index));
                //return View(product_Master);
           
            
        }
        [HttpGet]

        // GET: Product_Master/Create ----- for so_inward
        public IActionResult CreateViewProduct()
        {
            ViewBag.uomnames = uomnames();
            ViewBag.categoryname = Getcategoryname();
            ViewBag.subcategory = Getsubcategory();
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateViewProduct(Product_Master product_Master)
        {
            int maxId = _context.Product_Master.Any() ? _context.Product_Master.Max(e => e.id) + 1 : 1;
            product_Master.id = maxId;
            productmaster_packet pmp = new productmaster_packet();
            //product_Master.Warranty = $"{product_Master.WarrantyDur} {product_Master.WarrantyUnit}";
            _context.Add(product_Master);
            _context.SaveChangesAsync();
            _notyfService.Success("Product Added Successfully");
            return RedirectToAction("Create", "so_inward");
            return View(product_Master);
        }
        // GET: Product_Master/Create ---- for purchase
        [HttpGet]
        public IActionResult CreateViewProduct_p()
        {
            ViewBag.categoryname = Getcategoryname();
            ViewBag.subcategory = Getsubcategory();
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateViewProduct_p(Product_Master product_Master)
        {
            int maxId = _context.Product_Master.Any() ? _context.Product_Master.Max(e => e.id) + 1 : 1;
            product_Master.id = maxId;
            productmaster_packet pmp = new productmaster_packet();
            //product_Master.Warranty = $"{product_Master.WarrantyDur} {product_Master.WarrantyUnit}";
            _context.Add(product_Master);
            _context.SaveChangesAsync();
            _notyfService.Success("Product Added Successfully");
            return RedirectToAction("Create", "purchases");
            return View(product_Master);
        }
        // GET: Product_Master/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Product_Master == null)
            {
                return NotFound();
            }
            var product_Master = _context.Product_Master
                .Include(a => a.Productmaster_Packets)
                .Where(a => a.id == id)
                .FirstOrDefault();
            Product_Master pm = new Product_Master();
            pm.Productmaster_Packets.Add(new productmaster_packet() { Id = 1 });
            var productcode = product_Master.productcode.ToUpper();
            if (product_Master == null)
            {
                return NotFound();
            }
            //// Splitting the Warranty string using space as delimiter
            //string[] warrantyParts = product_Master.Warranty?.Split(' ') ?? new string[0];
            //if (warrantyParts.Length == 2)
            //{
            //    if (int.TryParse(warrantyParts[0], out int warrantyDuration))
            //    {
            //        product_Master.WarrantyDur = warrantyDuration;
            //    }
            //    product_Master.WarrantyUnit = warrantyParts[1];
            //}
            ViewBag.categoryname = Getcategoryname();
            ViewBag.subcategory = Getsubcategory();
            //ViewBag.Templatename = ShowTemplateNames1(string productcode);
            return View(product_Master);
        }
        // POST: Product_Master/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, Product_Master product_Master)
        //{
        //    if (id != product_Master.id)
        //    {
        //        return NotFound();
        //    }
        //    try
        //    {
        //        //product_Master.Warranty = $"{product_Master.WarrantyDur} {product_Master.WarrantyUnit}";
        //        List<productmaster_packet> productDetails = _context.productmaster_Packet.Where(d => d.productmasterId == product_Master.id).ToList();
        //        _context.productmaster_Packet.RemoveRange(productDetails);
        //        _context.SaveChanges();
        //        _context.Update(product_Master);

        //        //maintain logs
        //        var user = HttpContext.Session.GetString("User");
        //        var logs = new Logs();
        //        logs.pagename = "Product Master";
        //        logs.task = "Update product master ";
        //        logs.action = "Update";
        //        logs.taskid = Convert.ToInt32(id);
        //        logs.date = DateTime.Now.ToString("dd/MM/yyyy");
        //        logs.time = DateTime.Now.ToString("HH:mm:ss");
        //        logs.username = user;
        //        _context.Add(logs);
        //        _context.SaveChanges();
        //        _notyfService.Success("Sales Order Updated Succesfully");

        //        return RedirectToAction(nameof(Index));
        //    }

        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!Product_MasterExists(product_Master.id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception or handle it appropriately
        //        ModelState.AddModelError(string.Empty, "Error occurred while updating the record.");
        //        // If the ModelState is invalid, return to the edit view with the existing model
        //        ViewBag.categoryname = Getcategoryname();
        //        ViewBag.subcategory = Getsubcategory();
        //        return View(product_Master);
        //    }


        //}

        public async Task<IActionResult> Edit(int id, Product_Master product_Master)
        {
            if (id != product_Master.id)
            {
                return NotFound();
            }
            try
            {
                // Retrieve the existing entity from the context
                var existingProduct = await _context.Product_Master.FindAsync(id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                // Detach the existing entity from the context
                _context.Entry(existingProduct).State = EntityState.Detached;

                // Generate change log
                var changeLog = GenerateChangeLog(existingProduct, product_Master);

                // Remove existing related entities
                List<productmaster_packet> productDetails = _context.productmaster_Packet.Where(d => d.productmasterId == product_Master.id).ToList();
                _context.productmaster_Packet.RemoveRange(productDetails);

                // Update the product master entity
                _context.Update(product_Master);

                // Maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs
                {
                    pagename = "Product Master",
                    task = product_Master.id + "$" + product_Master.categorycode + "$" + product_Master.categoryname + "$" + product_Master.subcategory + "$" + product_Master.productcode + "$" + product_Master.brand + "$" + product_Master.productdescription + "$" + product_Master.TypeOfProduct + "$" + product_Master.hsncode + "$" + product_Master.uom ,
                    //task = changeLog,
                    action = "Update",
                    taskid = id,
                    date = DateTime.Now.ToString("dd/MM/yyyy"),
                    time = DateTime.Now.ToString("HH:mm:ss"),
                    username = user,
                };
                _context.Add(logs);

                // Save changes
                await _context.SaveChangesAsync();

                _notyfService.Success("Product details Updated Successfully");

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Product_MasterExists(product_Master.id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool Product_MasterExists(int id)
        {
            return _context.Product_Master.Any(e => e.id == id);
        }

        private string GenerateChangeLog(Product_Master existingProduct, Product_Master updatedProduct)
        {
            StringBuilder changeLogBuilder = new StringBuilder();

            if (existingProduct.categoryname != updatedProduct.categoryname)
            {
                changeLogBuilder.Append($"{existingProduct.categoryname} -> {updatedProduct.categoryname} | ");
            }

            if (existingProduct.subcategory != updatedProduct.subcategory)
            {
                changeLogBuilder.Append($"{existingProduct.subcategory} -> {updatedProduct.subcategory} | ");
            }

            if (existingProduct.productcode != updatedProduct.productcode)
            {
                changeLogBuilder.Append($"{existingProduct.productcode} -> {updatedProduct.productcode} | ");
            }

            if (existingProduct.brand != updatedProduct.brand)
            {
                changeLogBuilder.Append($"{existingProduct.brand} -> {updatedProduct.brand} | ");
            }

            if (existingProduct.productdescription != updatedProduct.productdescription)
            {
                changeLogBuilder.Append($"{existingProduct.productdescription} -> {updatedProduct.productdescription} | ");
            }

            if (existingProduct.TypeOfProduct != updatedProduct.TypeOfProduct)
            {
                changeLogBuilder.Append($"{existingProduct.TypeOfProduct} -> {updatedProduct.TypeOfProduct} | ");
            }

            if (existingProduct.hsncode != updatedProduct.hsncode)
            {
                changeLogBuilder.Append($"{existingProduct.hsncode} -> {updatedProduct.hsncode} | ");
            }

            if (existingProduct.uom != updatedProduct.uom)
            {
                changeLogBuilder.Append($"{existingProduct.uom} -> {updatedProduct.uom} | ");
            }

            // Add similar comparisons for other properties

            return changeLogBuilder.ToString();
        }


        // GET: Product_Master/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Product_Master == null)
            {
                return NotFound();
            }

            var product_Master = await _context.Product_Master
                .FirstOrDefaultAsync(m => m.id == id);
            if (product_Master == null)
            {
                return NotFound();
            }

            return View(product_Master);
        }

        // POST: Product_Master/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Product_Master == null || _context.productmaster_Packet == null )
            {
                return Problem("Entity sets are null.");
            }

            var productMaster = await _context.Product_Master.FindAsync(id);
            if (productMaster == null)
            {
                return NotFound();
            }

            //// Retrieve related ProductMaster_Packet entities by ProductMasterId and delete them
            //var packetsToDelete = await _context.productmaster_Packet
            //    .Where(pp => pp.productmasterId == id)
            //    .ToListAsync();

            //if (packetsToDelete.Any())
            //{
            //    _context.productmaster_Packet.RemoveRange(packetsToDelete);
            //}


            //maintain logs
            var user = HttpContext.Session.GetString("User");
            var logs = new Logs();
            logs.pagename = "product Master";
            logs.task =productMaster.id + "$" + productMaster.categorycode + "$" + productMaster.categoryname + "$" + productMaster.subcategory + "$" + productMaster.productcode + "$" + productMaster.brand + "$" + productMaster.productdescription + "$" + productMaster.TypeOfProduct + "$" + productMaster.hsncode + "$" + productMaster.uom ;
            logs.action = "Delete";
            logs.taskid = Convert.ToInt32(id);
            logs.date = DateTime.Now.ToString("dd/MM/yyyy");
            logs.time = DateTime.Now.ToString("HH:mm:ss");
            logs.username = user;
            _context.Add(logs);
            _context.SaveChanges();

            _context.Product_Master.Remove(productMaster);

            await _context.SaveChangesAsync();
            _notyfService.Error(" Deleted Succesfully");
            return RedirectToAction(nameof(Index));
        }

        //private bool Product_MasterExists(int id)
        //{
        //    return (_context.Product_Master?.Any(e => e.id == id)).GetValueOrDefault();
        //}

        public ActionResult GETCategory_CODE(string selectedValue)
        {
            //int maxId = _context.Product_Master.Any() ? _context.Product_Master.Max(e => e.id) + 1 : 1;
            //var srno = maxId;
            //var code = "";
            //if (srno.ToString().Length == 1)
            //{
            //    code = "C000000" + maxId;
            //}
            //else if (srno.ToString().Length == 2)
            //{
            //    code = "C00000" + maxId;
            //}
            //else if (srno.ToString().Length == 3)
            //{
            //    code = "C0000" + maxId;
            //}
            //else if (srno.ToString().Length == 4)
            //{
            //    code = "C000" + maxId;
            //}
            //else if (srno.ToString().Length == 5)
            //{
            //    code = "C00" + maxId;
            //}
            //else if (srno.ToString().Length == 6)
            //{
            //    code = "C0" + maxId;
            //}
            //else if (srno.ToString().Length == 7)
            //{
            //    code = "C" + maxId;
            //}
            //return Json(code);

            var getid = _context.Category_Master.Where(a => a.categoryname.Trim() == selectedValue.Trim()).Select(a => a.categoryid).FirstOrDefault();
            return Json(getid);
        }

        //public ActionResult GETPROD_CODE(string selectedvalue)
        //{

        //    int maxId = _context.Product_Master.Any() ? _context.Product_Master.Max(e => e.id) + 1 : 1;
        //    var srno = maxId;
        //    var code = "";
        //    if (srno.ToString().Length == 1)
        //    {
        //        code = "P000000" + maxId;
        //    }
        //    else if (srno.ToString().Length == 2)
        //    {
        //        code = "P00000" + maxId;
        //    }
        //    else if (srno.ToString().Length == 3)
        //    {
        //        code = "P0000" + maxId;
        //    }
        //    else if (srno.ToString().Length == 4)
        //    {
        //        code = "P000" + maxId;
        //    }
        //    else if (srno.ToString().Length == 5)
        //    {
        //        code = "P00" + maxId;
        //    }
        //    else if (srno.ToString().Length == 6)
        //    {
        //        code = "P0" + maxId;
        //    }
        //    else if (srno.ToString().Length == 7)
        //    {
        //        code = "P" + maxId;
        //    }
        //    // Pass the generated code to the Create action using TempData
        //    TempData["GeneratedProductCode"] = code;
        //    return Json(code);
        //}
    }
}
