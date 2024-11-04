using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AspNetCoreHero.ToastNotification.Abstractions;
using AuthSystem.Data;
using eros.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;
using eros.Models.Cascade;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Nest;

namespace eros.Controllers
{

    public class category_masterController : Controller
    {
        private readonly ErosDbContext _context;
        public INotyfService _notifyService { get; }

        public category_masterController(ErosDbContext context, INotyfService notyfService)
        {
            _context = context;
            _notifyService = notyfService;

        }
        public  IActionResult CheckStatename(string selectedValue)
        {
            var check = _context.states.Where(a => a.Name.Trim().ToUpper() == selectedValue.ToUpper().Trim()).FirstOrDefault();
            if (check != null)
            {
                return Json(new { success = false, message = "State name is already exist !" });
            }
            else
            {

                return Json(new { success = true, selectedValue });
            }
        }
        public IActionResult CheckCountryname(string selectedValue)
        {
            var check = _context.Countries.Where(a => a.Name.Trim().ToUpper() == selectedValue.ToUpper().Trim()).FirstOrDefault();
            if (check != null)
            {
                return Json(new { success = false, message = "Country name is already exist !" });
            }
            else
            {

                return Json(new { success = true, selectedValue });
            }
        }
        public IActionResult CheckCityname(string selectedValue)
        {
            var check = _context.cities.Where(a => a.Name.Trim().ToUpper() == selectedValue.ToUpper().Trim()).FirstOrDefault();
            if (check != null)
            {
                return Json(new { success = false, message = "City name is already exist !" });
            }
            else
            {

                return Json(new { success = true, selectedValue });
            }
        }
        public ActionResult categorynameCheck(string categoryname)
        {
            if (categoryname.Trim() != null)
            {
                // Define a regular expression pattern to keep only alphanumeric characters, hyphens, slashes
                //string pattern = @"[^\-\/]";
                string pattern = @"[^a-zA-Z0-9\-\/]";
                // Remove characters that do not match the pattern from the input description
                categoryname = Regex.Replace(categoryname.Trim(), pattern, "");

                var find = _context.category_master.ToList();

                bool isDuplicate = false; // Flag to track if any duplicate is found

                foreach (var item in find)
                {
                    item.categoryname = Regex.Replace(item.categoryname.Trim(), pattern, "");
                    if (item.categoryname.ToUpper() == categoryname.Trim().ToUpper())
                    {
                        isDuplicate = true;
                        break; // Exit loop once a duplicate is found
                    }
                }

                if (isDuplicate)
                {
                    return Json(new { success = false, message = "Csategoryname already exists in the database." });
                }
                else
                {
                    return Json(new { success = true, message = "Available." });
                }
            }
            return Json(new { success = true, message = "Product description is required." });
        }

        // GET: category_master
        public async Task<IActionResult> Index()
        {
            var categories = await _context.category_master.OrderByDescending(a=>a.id).ToListAsync();

            if (categories != null)
            {
                return View(categories);
            }
            else
            {
                return Problem("Entity set 'ErosDbContext.category_master' is null.");
            }
        }
        public async Task<IActionResult> IndexState()
        {
            var categories = await _context.states.OrderByDescending(a => a.Id).ToListAsync();
            
            if (categories != null)
            {
                return View(categories);
            }
            else
            {
                return Problem("Entity set 'ErosDbContext.category_master' is null.");
            }
        }
        public async Task<IActionResult> IndexCity()
        {
            var categories = await _context.cities.OrderByDescending(a=>a.Id).ToListAsync();
            
            if (categories != null)
            {
                return View(categories);
            }
            else
            {
                return Problem("Entity set 'ErosDbContext.category_master' is null.");
            }
        }
        public async Task<IActionResult> IndexCountry()
        {
            var categories = await _context.Countries.OrderByDescending(a => a.Id).ToListAsync();

            if (categories != null)
            {
                return View(categories);
            }
            else
            {
                return Problem("Entity set 'ErosDbContext.category_master' is null.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> StateData()
        {
            ViewBag.countrynames = countrynames();
            return View();
        }
        [HttpPost]
        public IActionResult StateData(state state)
        {
            try
            {
                // Check if the corresponding country exists
                var existingCountry = _context.Countries.Where(c => c.Name.Trim() == state.countryname.Trim()).FirstOrDefault();
                if (existingCountry != null)
                {
                    int cmaxId = _context.states.Any() ? _context.states.Max(e => e.Id) + 1 : 1;

                    var newCity = new state
                    {
                        Id = cmaxId,
                        Name = state.Name,
                        country = existingCountry,
                    };

                    _context.states.Add(newCity);
                    _context.SaveChanges();
                }
                return Json(new { Success = true, Message = "State Added Successfully !" });
               
            }
            catch (Exception ex)
            {
                //return StatusCode(500, "An error occurred: " + ex.Message);
                return Json(new { Success = false, Message = ex.Message });
            }
        }

        private List<SelectListItem> countrynames()
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
                Text = "---- Select Country Name ---",
            };

            lstProducts.Insert(0, defItem);

            return lstProducts;
        }

        private List<SelectListItem> statenames()
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
                Text = "---- Select state Name ---",
            };

            lstProducts.Insert(0, defItem);

            return lstProducts;
        }

        private List<SelectListItem> citynames()
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
                Text = "---- Select city Name ---",
            };

            lstProducts.Insert(0, defItem);

            return lstProducts;
        }
        [HttpGet]
        public async Task<IActionResult> CountryData()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CountryData(country country)
        {
            // Here you can handle the form submission, such as saving the data to the database
            try
            {
                int CmaxId = _context.Countries.Any() ? _context.Countries.Max(e => e.Id) + 1 : 1;
                country.Id = CmaxId;
                int smaxId = _context.states.Any() ? _context.states.Max(e => e.Id) + 1 : 1;
                int cmaxId = _context.cities.Any() ? _context.cities.Max(e => e.Id) + 1 : 1;

                _context.Countries.Add(country);
                _context.SaveChanges();

                return Json(new { Success = true, Message = "Country Added Successfully !" });
            }
            catch (Exception ex)
            {
                //return StatusCode(500, "An error occurred: " + ex.Message);
                return Json(new { Success = false, Message = ex.Message });
            }
        }
         [HttpGet]
        public async Task<IActionResult> CityData()
        {
            ViewBag.statenames = statenames();
            return View();
        }
        [HttpPost]
        public IActionResult CityData(city city)
        {
            try
            {
                var existingCountry = _context.states.Where(c => c.Name.Trim() == city.statename.Trim()).FirstOrDefault();
                if (existingCountry != null)
                {
                    int cmaxId = _context.cities.Any() ? _context.cities.Max(e => e.Id) + 1 : 1;

                    var newCity = new city
                    {
                        Id = cmaxId,
                        Name = city.Name,
                        state = existingCountry,
                    };

                    _context.cities.Add(newCity);
                    _context.SaveChanges();
                }
                return Json(new { Success = true, Message = "City Added Successfully !" });
            }
            catch (Exception ex)
            {
                //return StatusCode(500, "An error occurred: " + ex.Message);
                return Json(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditCountry(int? id)
        {
            if (id == null || _context.Countries == null)
            {
                return NotFound();
            }

            var category_master = await _context.Countries.FindAsync(id);
            if (category_master == null)
            {
                return NotFound();
            }
            return View(category_master);
        }
        [HttpPost]
        public async Task<IActionResult> EditCountry(int? id, country country)
        {
            try
            {
                _context.Countries.Update(country);
                _context.SaveChanges();
                return Json(new { Success = true, Message = "Country Updated Successfully !" });
            }
            catch (Exception ex)
            {
                return View( ex.Message);
            }
            //return View(category_master);
        }
        [HttpGet]
        public async Task<IActionResult> EditCity(int? id)
        {
            if (id == null || _context.cities == null)
            {
                return NotFound();
            }

            var category_master = await _context.cities.FindAsync(id);
            if (category_master == null)
            {
                return NotFound();
            }
            return View(category_master);
        }
        [HttpPost]
        public async Task<IActionResult> EditCity(int? id, city city)
        {
            try
            {
                _context.cities.Update(city);
                _context.SaveChanges();
                return Json(new { Success = true, Message = "City Updated Successfully !" });
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
            //return View(category_master);
        }
        [HttpGet]
        public async Task<IActionResult> EditState(int? id)
        {
            ViewBag.countrynames = countrynames();

            if (id == null || _context.states == null)
            {
                return NotFound();
            }
            var category_master = await _context.states.FindAsync(id);
            if (category_master == null)
            {
                return NotFound();
            }
            return View(category_master);
        }
        [HttpPost]
        public async Task<IActionResult> EditState(int? id, state state)
        {
            try
            {
                _context.states.Update(state);
                _context.SaveChanges();
                return Json(new { Success = true, Message = "State Updated Successfully !" });
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
            //return View(category_master);
        }
        [HttpGet]
        public async Task<IActionResult> DeleteCountry(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var country = await _context.Countries.FindAsync(id);

            if (country == null)
            {
                return NotFound();
            }

            try
            {
                _context.Countries.Remove(country);
                await _context.SaveChangesAsync();
                _notifyService.Error("Country removed successfully !");
                return RedirectToAction(nameof(IndexCountry));
            }
            catch (Exception ex)
            {
                _notifyService.Error("An error occurred while deleting the country: " + ex.Message);
                return RedirectToAction(nameof(IndexCountry));
            }
        }
        [HttpGet]
        public async Task<IActionResult> DeleteState(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var country = await _context.states.FindAsync(id);

            if (country == null)
            {
                return NotFound();
            }

            try
            {
                _context.states.Remove(country);
                await _context.SaveChangesAsync();
                _notifyService.Error("State removed successfully");
                return RedirectToAction(nameof(IndexState));
            }
            catch (Exception ex)
            {
                _notifyService.Error("An error occurred while deleting the country: " + ex.Message);
                return RedirectToAction(nameof(IndexState));
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteCity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var country = await _context.cities.FindAsync(id);

            if (country == null)
            {
                return NotFound();
            }

            try
            {
                _context.cities.Remove(country);
                await _context.SaveChangesAsync();
                _notifyService.Error("City removed successfully !");
                return RedirectToAction(nameof(IndexCity));
            }
            catch (Exception ex)
            {
                _notifyService.Error("An error occurred while deleting the country: " + ex.Message);
                return RedirectToAction(nameof(IndexCity));
            }
        }

        // GET: category_master/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.category_master == null)
            {
                return NotFound();
            }

            var category_master = await _context.category_master
                .FirstOrDefaultAsync(m => m.id == id);
            if (category_master == null)
            {
                return NotFound();
            }
            //maintain logs
            var user = HttpContext.Session.GetString("User");
            var logs = new Logs();
            logs.pagename = "category master Master";
            logs.task = id.ToString() + " category master id view";
            //logs.task = category_master.categoryid + "$" +category_master.categoryname;
            logs.action = "View";
            logs.taskid = Convert.ToInt32(id);
            logs.date = DateTime.Now.ToString("dd/MM/yyyy");
            logs.time = DateTime.Now.ToString("HH:mm:ss");
            logs.username = user;
            _context.Add(logs);
            _context.SaveChanges();
            //_notifyService.Success("Category Create Succesfully ");
            return View(category_master);
        }

        // GET: category_master/Create
        public IActionResult Create()
        {
            return View();
        }
  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(category_master Category_Master)
            {
                if(Category_Master.categoryname == null || Category_Master.categoryid == null)
                {
                    _notifyService.Warning("Some values found null !");
                }
                else
                {
                int maxId = _context.category_master.Any() ? _context.category_master.Max(e => e.id) + 1 : 1;
                Category_Master.id = maxId;
                Category_Master.categoryid = Category_Master.categoryid.ToUpper();
                Category_Master.categoryname = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Category_Master.categoryname.ToLower());

                _context.Add(Category_Master);
                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "Category Master";
                logs.task = maxId.ToString() + " category master id create";
                //logs.task = Category_Master.categoryid+"$"+ Category_Master.categoryname;
                logs.action = "Create";
                logs.taskid = maxId;
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.username = user;
                _context.Add(logs);

                _context.SaveChanges();
                _notifyService.Success("Category created successfully");

            }

            return RedirectToAction(nameof(Index));


            //return Json(new { success = true });

        }




        // GET: category_master/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.category_master == null)
            {
                return NotFound();
            }

            var category_master = await _context.category_master.FindAsync(id);
            if (category_master == null)
            {
                return NotFound();
            }
            return View(category_master);
            

        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, category_master category_master)
        {
            if (id != category_master.id)
            {
                return NotFound();
            }

            try
            {
                category_master.categoryid = category_master.categoryid.ToUpper();
                category_master.categoryname = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(category_master.categoryname.ToLower());

                _context.category_master.Update(category_master);

                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "Category Master";
                //logs.task = id.ToString();
                logs.task = category_master.categoryid + "$" + category_master.categoryname;
                logs.action = "Update";
                logs.taskid = id;
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.username = user;
                _context.Add(logs);

                _context.SaveChanges();
                _notifyService.Success("Category Updated Succesfully");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return RedirectToAction(nameof(Index));
        }

        //public async Task<IActionResult> Edit(int id, category_master categoryMaster)
        //{
        //    if (id != categoryMaster.id)
        //    {
        //        return NotFound();
        //    }

        //    try
        //    {
        //        // Retrieve the existing entity from the context
        //        var existingCategory = await _context.category_master.FindAsync(id);
        //        if (existingCategory == null)
        //        {
        //            return NotFound();
        //        }

        //        // Generate change log
        //        var changeLog = GenerateChangeLog(existingCategory, categoryMaster);

        //        // Maintain logs
        //        var user = HttpContext.Session.GetString("User");
        //        var logs = new Logs();
        //        logs.pagename = "Category Master";
        //        logs.action = "Update";
        //        logs.taskid = id;
        //        logs.date = DateTime.Now.ToString("dd/MM/yyyy");
        //        logs.time = DateTime.Now.ToString("HH:mm:ss");
        //        logs.username = user;

        //        // Log the changes
        //        logs.task = changeLog;
        //        _context.Add(logs);

        //        // Update the existing entity with the new values
        //        existingCategory.categoryid = categoryMaster.categoryid.ToUpper();
        //        existingCategory.categoryname = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(categoryMaster.categoryname.ToLower());

        //        await _context.SaveChangesAsync();
        //        _notifyService.Success("Category Updated Successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //    return RedirectToAction(nameof(Index));
        //}
        //private string GenerateChangeLog(category_master existingCategory, category_master updatedCategory)
        //{
        //    StringBuilder changeLogBuilder = new StringBuilder();

        //    if (existingCategory.categoryid != updatedCategory.categoryid)
        //    {
        //        changeLogBuilder.Append($"{existingCategory.categoryid} -> {updatedCategory.categoryid} | ");
        //    }

        //    if (existingCategory.categoryname != updatedCategory.categoryname)
        //    {
        //        changeLogBuilder.Append($"{existingCategory.categoryname} -> {updatedCategory.categoryname}");
        //    }

        //    return changeLogBuilder.ToString();
        //}
        private bool category_masterExists(int id)
        {
            return (_context.category_master?.Any(e => e.id == id)).GetValueOrDefault();
        }


        // GET: category_master/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.category_master == null)
            {
                return NotFound();
            }

            var category_master = await _context.category_master
                .FirstOrDefaultAsync(m => m.id == id);
            if (category_master == null)
            {
                return NotFound();
            }

            return View(category_master);
        }

        // POST: category_master/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category_master = await _context.category_master.FindAsync(id);
            if (category_master == null)
            {
                return NotFound();
            }

            try
            {
                _context.category_master.Remove(category_master);
                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "Category Master";
                logs.task = category_master.id + "$" + category_master.categoryid + "$" +category_master.categoryname;
                id.ToString();
                logs.action = "Delete";
                logs.taskid = id;
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.username = user;
                _context.Add(logs);
                await _context.SaveChangesAsync();

                _notifyService.Error("Category deleted successfully");
            }
            catch (Exception ex)
            {
                // Handle any exception that may occur during deletion
                _notifyService.Error("An error occurred while deleting the category.");
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
