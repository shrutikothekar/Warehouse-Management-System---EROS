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
using System.Globalization;
using System.Text.RegularExpressions;

namespace eros.Controllers
{
    public class subcategory_masterController : Controller
    {
        private readonly ErosDbContext _context;
        public INotyfService _notifyService { get; }

        public subcategory_masterController(ErosDbContext context, INotyfService notyfService)
        {
            _context = context;
            _notifyService = notyfService;

        }
        public ActionResult subcategoryCheck(string subcategory_name, string exampleFormControlSelect1)
        {
           
                if (subcategory_name.Trim() != null)
                {
             
                string pattern = @"[^a-zA-Z0-9\-\/]";
                subcategory_name = Regex.Replace(subcategory_name.Trim(), pattern, "");

                    var find = _context.subcategory_Master.ToList();

                    bool isDuplicate = false; // Flag to track if any duplicate is found

                    foreach (var item in find.Where(c => c.categoryname == exampleFormControlSelect1.Trim()))
                    {
                        item.subcategory_name = Regex.Replace(item.subcategory_name.Trim(), pattern, "");
                        //item.categoryname = Regex.Replace(item.categoryname.Trim(), pattern, "");
                        if (item.subcategory_name.Trim().ToUpper() == subcategory_name.Trim().ToUpper())
                        {
                            isDuplicate = true;
                            break; // Exit loop once a duplicate is found
                        }
                    }

                    if (isDuplicate)
                    {
                        return Json(new { success = false, message = "subcategory name already exists in the database." });
                    }
                    else
                    {
                        return Json(new { success = true, message = "Available." });
                    }
                }

            //}

            return Json(new { success = true, message = "Product description is required." });
        }


        // GET: subcategory_master
        public async Task<IActionResult> Index()
        {
            //return _context.subcategory_Master != null ?
            //            View(await _context.subcategory_Master.ToListAsync()) :
            //            Problem("Entity set 'ErosDbContext.subcategory_Master'  is null.");

            var pendingOrders = _context.subcategory_Master
       .OrderByDescending(a => a.subcategory_id)
       .ToList();

            return View(pendingOrders);
        }

        // GET: subcategory_master/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.subcategory_Master == null)
            {
                return NotFound();
            }

            var subcategory_master = await _context.subcategory_Master
                .FirstOrDefaultAsync(m => m.subcategory_id == id);
            if (subcategory_master == null)
            {
                return NotFound();
            }

            //maintain logs
            var user = HttpContext.Session.GetString("User");
            var logs = new Logs
            {
                pagename = "subcategory Master",
                task = id +" subcategory master View",
                action = "View",
                taskid = id.Value, // Convert.ToInt32(id) is not necessary here since id is already nullable int
                date = DateTime.Now.ToString("dd/MM/yyyy"),
                time = DateTime.Now.ToString("HH:mm:ss"),
                username = user
            };
            _context.Add(logs);
            _context.SaveChanges();

            return View(subcategory_master);
        }


        // GET: subcategory_master/Create
        public IActionResult Create()
        {
            ViewBag.categoryname = Getcategoryname();
            
            return View();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(subcategory_master subcategory_master)
        {
            int maxId = _context.subcategory_Master.Any() ? _context.subcategory_Master.Max(e => e.subcategory_id) + 1 : 1;
            subcategory_master.subcategory_id = maxId;
            //subcategory_master.categoryname = subcategory_master.categoryname.ToUpper();
            subcategory_master.subcategory_name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(subcategory_master.subcategory_name.ToLower());
            //subcategory_master.subcategory_name = subcategory_master.subcategory_name.ToUpper();

            _context.Add(subcategory_master);
            //maintain logs
            var user = HttpContext.Session.GetString("User");
            var logs = new Logs();
            logs.pagename = "subcategory Master";
            logs.task = maxId+" subcategory master create";
            logs.action = "Create";
            logs.taskid = Convert.ToInt32(maxId);
            logs.date = DateTime.Now.ToString("dd/MM/yyyy");
            logs.time = DateTime.Now.ToString("HH:mm:ss");
            logs.username = user;
            _context.Add(logs);

            await _context.SaveChangesAsync();
            _notifyService.Success("SubCategory Added Successfully");

            return RedirectToAction(nameof(Index));
        }

        // GET: subcategory_master/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            ViewBag.categoryname = Getcategoryname();
            if (id == null || _context.subcategory_Master == null)
            {
                return NotFound();
            }

            var subcategory_master = await _context.subcategory_Master.FindAsync(id);
            if (subcategory_master == null)
            {
                return NotFound();
            }
            return View(subcategory_master);
            _notifyService.Success("SubCategory Create Succesfully");
        }

        // POST: subcategory_master/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, subcategory_master subcategory_master)
        {
            if (id != subcategory_master.subcategory_id)
            {
                return NotFound();
            }
            try
            {
                subcategory_master.subcategory_name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(subcategory_master.subcategory_name.ToLower());
                subcategory_master.categoryname = CultureInfo.CurrentCulture.TextInfo.ToTitleCase
                    (subcategory_master.categoryname.ToLower());                    
                //subcategory_master.subcategory_name = subcategory_master.subcategory_name.ToUpper();
                _context.subcategory_Master.Update(subcategory_master);
                
                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "subcategory Master";
                logs.task = subcategory_master.subcategory_id + "$" + subcategory_master.categoryname + "$" + subcategory_master.subcategory_name; ;
                logs.action = "Update";
                logs.taskid = Convert.ToInt32(id);
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.username = user;
                _context.Add(logs);

                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!subcategory_masterExists(subcategory_master.subcategory_id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            _notifyService.Success("SubCategory Updated Succesfully");

            return RedirectToAction(nameof(Index));
        }

        // GET: subcategory_master/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.subcategory_Master == null)
            {
                return NotFound();
            }

            var subcategory_master = await _context.subcategory_Master
                .FirstOrDefaultAsync(m => m.subcategory_id == id);
            if (subcategory_master == null)
            {
                return NotFound();
            }

            return View(subcategory_master);
        }

        // POST: subcategory_master/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.subcategory_Master == null)
            {
                return Problem("Entity set 'ErosDbContext.subcategory_Master'  is null.");
            }
            var subcategory_master = await _context.subcategory_Master.FindAsync(id);
            if (subcategory_master != null)
            {
                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "subcategory Master";
                logs.task = subcategory_master.subcategory_id + "$" + subcategory_master.categoryname + "$" + subcategory_master.subcategory_name;
                //logs.task = "Delete subcategory master ";
                logs.action = "Delete";
                logs.taskid = Convert.ToInt32(id);
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.username = user;
                _context.Add(logs);

                _context.subcategory_Master.Remove(subcategory_master);
            }

            await _context.SaveChangesAsync();
            _notifyService.Error("Sub Category deleted successfully");

            return RedirectToAction(nameof(Index));
        }

        private bool subcategory_masterExists(int id)
        {
            return (_context.subcategory_Master?.Any(e => e.subcategory_id == id)).GetValueOrDefault();
        }
    }
}
