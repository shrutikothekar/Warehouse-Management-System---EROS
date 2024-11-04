using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AuthSystem.Data;
using eros.Models;
using Newtonsoft.Json.Linq;
using AspNetCoreHero.ToastNotification.Abstractions;
using AspNetCoreHero.ToastNotification.Notyf;
using System.Globalization;

namespace eros.Controllers
{
    public class Supplier_MasterController : Controller
    {
        private readonly ErosDbContext _context;
        public INotyfService _notyfService { get; }
        public Supplier_MasterController(ErosDbContext context, INotyfService notyfService)
        {
            _notyfService = notyfService;
            _context = context;
        }

        public IActionResult customernameCheck( string selectedValue)
        { 
            var check  = _context.Customer_Master.Where(a=>a.customername.Trim().ToUpper() == selectedValue.ToUpper().Trim()).FirstOrDefault();
            if (check != null)
            {
                return Json(new { success = false, message = "Customer name is already exist !" });
            }
            else
            {
                
                return Json(new { success = true, selectedValue });
            }

        }
        public IActionResult CheckShowroom( string selectedValue)
        { 
            var check  = _context.Showroom_Master.Where(a=>a.Showroom_name.Trim().ToUpper() == selectedValue.ToUpper().Trim()).FirstOrDefault();
            if (check != null)
            {
                return Json(new { success = false, message = "Showroom name is already exist !" });
            }
            else
            {
                
                return Json(new { success = true, selectedValue });
            }

        }public IActionResult categoryidCheck( string selectedValue)
        { 
            var check  = _context.category_master.Where(a=>a.categoryid.Trim().ToUpper() == selectedValue.ToUpper().Trim()).FirstOrDefault();
            if (check != null)
            {
                return Json(new { success = false, message = "Category Id is already exist !" });
            }
            else
            {
                
                return Json(new { success = true, selectedValue });
            }

        }
        public IActionResult categorynameCheck( string selectedValue)
        { 
            var check  = _context.category_master.Where(a=>a.categoryname.Trim().ToUpper() == selectedValue.ToUpper().Trim()).FirstOrDefault();
            if (check != null)
            {
                return Json(new { success = false, message = "Category name is already exist !" });
            }
            else
            {
                
                return Json(new { success = true, selectedValue });
            }

        }  public IActionResult usertype_nameCheck( string selectedValue)
        { 
            var check  = _context.usertype_Master.Where(a=>a.usertype_name.Trim().ToUpper() == selectedValue.ToUpper().Trim()).FirstOrDefault();
            if (check != null)
            {
                return Json(new { success = false, message = "Username is already exist !" });
            }
            else
            {
                
                return Json(new { success = true, selectedValue });
            }

        }

        // GET: Supplier_Master
        public async Task<IActionResult> Index()
        {
            //return _context.Supplier_Master != null ?
            //            View(await _context.Supplier_Master.ToListAsync()) :
            //            Problem("Entity set 'ErosDbContext.Supplier_Master'  is null.");

            var pendingOrders = _context.Supplier_Master
       .OrderByDescending(a => a.supplierid)
       .ToList();

            return View(pendingOrders);
        }

        // GET: Supplier_Master/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier_Master = await _context.Supplier_Master.FirstOrDefaultAsync(m => m.supplierid == id);
            if (supplier_Master == null)
            {
                return NotFound();
            }

            // Maintain logs
            var user = HttpContext.Session.GetString("User");
            //if (user == null)
            //{
            //    // Redirect to login page or handle unauthorized access
            //    return RedirectToAction("Login", "Account");
            //}

            var logs = new Logs
            {
                pagename = "Supplier Master",
                task = id+ " Supplier master View ",
                action = "View",
                taskid = id.Value, // Convert.ToInt32(id) is not necessary here since id is already nullable int
                date = DateTime.Now.ToString("dd/MM/yyyy"),
                time = DateTime.Now.ToString("HH:mm:ss"),
                username = user
            };

            // Add logs to the context
            _context.Add(logs);

            // Save changes to the context
            await _context.SaveChangesAsync(); // Assuming _context is an EF DbContext

            return View(supplier_Master);
        }
        [HttpGet]
        public IActionResult GetStatesByCountry(string countryId)
        {
            if (string.IsNullOrEmpty(countryId))
                return BadRequest();
            var selectid = _context.Countries.Where(a => a.Name == countryId.Trim()).Select(a => a.Id).FirstOrDefault();
            var states = _context.states
                .Where(s => s.country.Id == selectid)
                .Select(s => new SelectListItem
                {
                    Value = s.Name.ToString(), // You may need to adjust this based on your state model
                    Text = s.Name
                })
                .ToList();

            return Json(states);
        }

        [HttpGet]
        public IActionResult GetCitiesByState(string stateId)
        {
            if (string.IsNullOrEmpty(stateId))
                return BadRequest();
            var selectid = _context.states.Where(a => a.Name == stateId.Trim()).Select(a => a.Id).FirstOrDefault();
            var cities = _context.cities
                .Where(c => c.state.Id == selectid) // You may need to adjust this based on your city model
                .Select(c => new SelectListItem
                {
                    Value = c.Name, // You may need to adjust this based on your city model
                    Text = c.Name
                })
                .ToList();

            return Json(cities);
        }
        // GET: Supplier_Master/Create
        public IActionResult Create()
        {
            ViewBag.city = Getcity();
            ViewBag.state = Getstate();
            ViewBag.country = Getcountry();
            return View();
        }
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
            
            //var products = _context.Countries.Select(a => a.Name).Distinct().ToList(); // Fetch products from the database
            //products.Add("-- Select Country --");
            //return Json(products); // Return products as JSON


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

            return Json(lstProducts);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier_Master supplier_Master)
        {

            int maxId = _context.Supplier_Master.Any() ? _context.Supplier_Master.Max(e => e.supplierid) + 1 : 1;
            supplier_Master.supplierid = maxId;
            //if(supplier_Master.emailid == null)
            //{

            //}
            if (supplier_Master.emailid == null)
            {
                supplier_Master.emailid = "-";
            }
            else
            {
                supplier_Master.emailid = supplier_Master.emailid.ToLower();
            }
            //supplier_Master.emailid = supplier_Master.emailid.ToLower();
            supplier_Master.gstno = supplier_Master.gstno.ToUpper();
            supplier_Master.city = supplier_Master.city.ToUpper();
            supplier_Master.state = supplier_Master.state.ToUpper();
            supplier_Master.country = supplier_Master.country.ToUpper();
            supplier_Master.supplier_name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(supplier_Master.supplier_name.ToLower());
            supplier_Master.address = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(supplier_Master.address.ToLower());

            _context.Add(supplier_Master);
            //maintain logs
            var user = HttpContext.Session.GetString("User");
            var logs = new Logs();
            logs.pagename = "Supplier Master";
            logs.task = maxId + " Supplier master Create";
            logs.action = "Create";
            logs.taskid = Convert.ToInt32(maxId);
            logs.date = DateTime.Now.ToString("dd/MM/yyyy");
            logs.time = DateTime.Now.ToString("HH:mm:ss");
            logs.username = user;
            _context.Add(logs);

            _context.SaveChanges();
            _notyfService.Success("Supplier details created successfully");

            return RedirectToAction(nameof(Index));

            //return View(supplier_Master);
        }

        // GET: Supplier_Master/Create
        public IActionResult CreateViewSupplier()
        {
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateViewSupplier(Supplier_Master supplier_Master)
        {
            int maxId = _context.Supplier_Master.Any() ? _context.Supplier_Master.Max(e => e.supplierid) + 1 : 1;
            supplier_Master.supplierid = maxId;
            _context.Add(supplier_Master);
            await _context.SaveChangesAsync();
            return RedirectToAction("Create", "purchases");
            return View(supplier_Master);
        }

        //public ActionResult tidgenerate(string selectedValue)
        //{
        //    try
        //    {

        //        //var month_mm = DateTime.Now.ToString("MM");
        //        //var year_yy = DateTime.Now.ToString("yy");
        //        var suppliernm = _context.Supplier_Master.Where(e => e.supplier_name.ToUpper().Trim() == selectedValue.ToUpper().Trim()).FirstOrDefault();

        //        if (suppliernm != null)
        //        {
        //            return Json(new { success = true, message = "Duplicate supplier name !" });
        //        }
        //        else
        //        {
        //            int srNo = GetMaxId(selectedValue);
        //            // int srNo = _context.Security.Any() ? _context.Security.Where(a => a.month_mm == month_mm && a.year_yy == year_yy && a.sitecode == selectedValue1 && a.typeofaction == selectedValue).Max(e => e.srno)  : 1;
        //            //string query = "Select srno from \"Security\"";
        //            //    var maxId = _context.Security.FromSqlRaw(query).FirstOrDefault();
        //            var num = "";

        //            if (srNo.ToString().Length == 1)
        //            {
        //                num = "EROS_HO_S_" + "000" + srNo.ToString();
        //            }
        //            else if (srNo.ToString().Length == 2)
        //            {
        //                num = "EROS_HO_S_" + "00" + srNo.ToString();
        //            }
        //            else if (srNo.ToString().Length == 3)
        //            {
        //                num = "EROS_HO_S_" + "0" + srNo.ToString();
        //            }
        //            else if (srNo.ToString().Length == 4)
        //            {
        //                num = "EROS_HO_S_" + srNo.ToString();
        //            }
        //            return Json(new { success = true, num });
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        return Problem(ex.Message);
        //    }


        //}
        public ActionResult tidgenerate(string selectedValue)
        {
            try
            {
                var suppliernm = _context.Supplier_Master
                                          .Where(e => e.supplier_name.ToUpper().Trim() == selectedValue.ToUpper().Trim())
                                          .FirstOrDefault();

                if (suppliernm != null)
                {
                    return Json(new { success = false, message = "Duplicate supplier name!" });
                }
                else
                {
                    int srNo = GetMaxId(selectedValue);
                    string num = "";

                    // Generate the supplier code based on the srNo length
                    if (srNo.ToString().Length == 1)
                    {
                        num = "EROS_HO_S_" + "000" + srNo.ToString();
                    }
                    else if (srNo.ToString().Length == 2)
                    {
                        num = "EROS_HO_S_" + "00" + srNo.ToString();
                    }
                    else if (srNo.ToString().Length == 3)
                    {
                        num = "EROS_HO_S_" + "0" + srNo.ToString();
                    }
                    else if (srNo.ToString().Length == 4)
                    {
                        num = "EROS_HO_S_" + srNo.ToString();
                    }

                    // Return the generated supplier code
                    return Json(new { success = true, num });
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }


        public int GetMaxId(string selectedValue)
        {

            int maxId = _context.Supplier_Master.Any() ? _context.Supplier_Master.Max(e => e.supplierid) : 0;
            //experience.id = maxId;
            //int maxId = _context.Supplier_Masters.Where(s => s.supplier_name == month_mm && s.year_yy == year_yy && s.typeofaction == selectedValue && s.sitecode == selectedValue1).Select(s => s.srno).AsEnumerable().DefaultIfEmpty(0).Max();
            return maxId + 1;

        }

        [HttpGet]
        // GET: Supplier_Master/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            ViewBag.city = Getcity();
            ViewBag.state = Getstate();
            ViewBag.country = Getcountry();
            if (id == null || _context.Supplier_Master == null)
            {
                return NotFound();
            }

            var supplier_Master = await _context.Supplier_Master.FindAsync(id);
            if (supplier_Master == null)
            {
                return NotFound();
            }
            return View(supplier_Master);
        }

        // POST: Supplier_Master/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, Supplier_Master supplier_Master)
        //{
        //    if (id != supplier_Master.supplierid)
        //    {
        //        return NotFound();
        //    }

        //    try
        //    {

        //        supplier_Master.emailid = supplier_Master.emailid.ToLower();
        //        supplier_Master.gstno = supplier_Master.gstno.ToUpper();
        //        supplier_Master.city = supplier_Master.city.ToUpper();
        //        supplier_Master.state = supplier_Master.state.ToUpper();
        //        supplier_Master.country = supplier_Master.country.ToUpper();
        //        supplier_Master.supplier_name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(supplier_Master.supplier_name.ToLower());
        //        supplier_Master.address = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(supplier_Master.address.ToLower());

        //        _context.Update(supplier_Master);
        //        _context.SaveChanges();
        //        return RedirectToAction(nameof(Index));
        //        _notyfService.Success("Supplier details update Successfully");

        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!Supplier_MasterExists(supplier_Master.supplierid))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //    return RedirectToAction(nameof(Index));

        //    // return View(supplier_Master);
        //}

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Supplier_Master supplier_Master)
        {
            if (id != supplier_Master.supplierid)
            {
                return NotFound();
            }
            try
            {
                if (supplier_Master.emailid == null)
                {
                    supplier_Master.emailid = "-";
                }
                else
                {
                    supplier_Master.emailid = supplier_Master.emailid.ToLower();
                }
                // supplier_Master.emailid = supplier_Master.emailid.ToLower();
                supplier_Master.gstno = supplier_Master.gstno.ToUpper();
                supplier_Master.city = supplier_Master.city.ToUpper();
                supplier_Master.state = supplier_Master.state.ToUpper();
                supplier_Master.country = supplier_Master.country.ToUpper();
                supplier_Master.supplier_name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(supplier_Master.supplier_name.ToLower());
                supplier_Master.address = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(supplier_Master.address.ToLower());

                _context.Update(supplier_Master);
                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "Supplier Master";
                logs.task = supplier_Master.supplierid + "$" + supplier_Master.suppliercode + "$" + supplier_Master.supplier_name + "$" + supplier_Master.brand + "$" + supplier_Master.gstno + "$" + supplier_Master.contactno + "$" + supplier_Master.emailid + "$" + supplier_Master.address + "$" + supplier_Master.city + "$" + supplier_Master.state + "$" + supplier_Master.country + "$" + supplier_Master.pincode;
                logs.action = "Update";
                logs.taskid = Convert.ToInt32(id);
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.username = user;
                _context.Add(logs);

                _context.SaveChanges();
                _notyfService.Success("Supplier details Updated Succesfully");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Supplier_MasterExists(supplier_Master.supplierid))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }


            // return View(supplier_Master);
        }
        // GET: Supplier_Master/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Supplier_Master == null)
            {
                return NotFound();
            }

            var supplier_Master = await _context.Supplier_Master
                .FirstOrDefaultAsync(m => m.supplierid == id);
            if (supplier_Master == null)
            {
                return NotFound();
            }

            return View(supplier_Master);
        }

        // POST: Supplier_Master/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Supplier_Master == null)
            {
                return Problem("Entity set 'ErosDbContext.Supplier_Master'  is null.");
            }
            var supplier_Master = await _context.Supplier_Master.FindAsync(id);
            if (supplier_Master != null)
            {
                _context.Supplier_Master.Remove(supplier_Master);
            }
            //maintain logs
            var user = HttpContext.Session.GetString("User");
            var logs = new Logs();
            logs.pagename = "Supplier Master";
            logs.task = supplier_Master.supplierid + "$" + supplier_Master.suppliercode + "$" + supplier_Master.supplier_name + "$" + supplier_Master.brand + "$" + supplier_Master.gstno + "$" + supplier_Master.contactno + "$" + supplier_Master.emailid + "$" + supplier_Master.address + "$" + supplier_Master.city + "$" + supplier_Master.state + "$" + supplier_Master.country + "$" + supplier_Master.pincode;
            //logs.task = "Supplier master Delete";
            logs.action = "Delete";
            logs.taskid = Convert.ToInt32(id);
            logs.date = DateTime.Now.ToString("dd/MM/yyyy");
            logs.time = DateTime.Now.ToString("HH:mm:ss");
            logs.username = user;
            _context.Add(logs);

            await _context.SaveChangesAsync();
            _notyfService.Error("Supplier details deleted successfully");
            return RedirectToAction(nameof(Index));
        }

        private bool Supplier_MasterExists(int id)
        {
            return (_context.Supplier_Master?.Any(e => e.supplierid == id)).GetValueOrDefault();
        }
    }
}
