using AuthSystem.Data;
using eros.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AspNetCoreHero.ToastNotification.Abstractions;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace eros.Controllers
{
    public class Transport_MasterController : Controller
    {
        private readonly ErosDbContext _context;
        public INotyfService _notyfService { get; }

        public Transport_MasterController(ErosDbContext context, INotyfService notyfService)
        {
            _context = context; _notyfService = notyfService;
        }


        // GET: Transport_Master
        public async Task<IActionResult> Index()
        {
            return _context.Transport_Master != null ?
                        View(await _context.Transport_Master.ToListAsync()) :
                        Problem("Entity set 'ErosDbContext.Transport_Master'  is null.");
        }

        // GET: Transport_Master/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transport_Master = await _context.Transport_Master.FirstOrDefaultAsync(m => m.id == id);
            if (transport_Master == null)
            {
                return NotFound();
            }

            // Maintain logs
            var user = HttpContext.Session.GetString("User");
            if (user == null)
            {
                // Redirect to login page or handle unauthorized access
                return RedirectToAction("Login", "Account");
            }

            var logs = new Logs
            {
                pagename = "Transport Master",
                task = id+" Transport master View",
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

            return View(transport_Master);
        }
        // GET: Transport_Master/Create
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Transport_Master Transport_Master)
        {
            int maxId = _context.Transport_Master.Any() ? _context.Transport_Master.Max(e => e.id) + 1 : 1;
            Transport_Master.id = maxId;

            //Transport_Master.field1 = "-";
            //Transport_Master.field2 = "-";
            //Transport_Master.field3 = "-";
            //Transport_Master.field4 = "-";
            //Transport_Master.field5 = "-";
            _context.Add(Transport_Master);

            //maintain logs
            var user = HttpContext.Session.GetString("User");
            var logs = new Logs();
            logs.pagename = "Transport Master";
            logs.task = maxId+ " Transport master Create";
            logs.action = "Create";
            logs.taskid = Convert.ToInt32(maxId);
            logs.date = DateTime.Now.ToString("dd/MM/yyyy");
            logs.time = DateTime.Now.ToString("HH:mm:ss");
            logs.username = user;
            _context.Add(logs);

            await _context.SaveChangesAsync();
            _notyfService.Success("Created Successfully !");
            return RedirectToAction(nameof(Index));

            //return View(Transport_Master);
        }

        // GET: Transport_Master/Create
        public IActionResult CreateViewSupplier()
        {
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateViewtransport(Transport_Master Transport_Master)
        {
            int maxId = _context.Transport_Master.Any() ? _context.Transport_Master.Max(e => e.id) + 1 : 1;
            Transport_Master.id = maxId;
            _context.Add(Transport_Master);
            await _context.SaveChangesAsync();
            return RedirectToAction("Create", "purchases");
            return View(Transport_Master);
        }

        public ActionResult tidgenerate(string selectedValue)
        {
            try
            {

                //var month_mm = DateTime.Now.ToString("MM");
                //var year_yy = DateTime.Now.ToString("yy");
                var suppliernm = _context.Transport_Master.Where(e => e.transportname == selectedValue).FirstOrDefault();

                if (suppliernm != null)
                {

                    return null;

                }
                else
                {
                    int srNo = GetMaxId(selectedValue);
                    // int srNo = _context.Security.Any() ? _context.Security.Where(a => a.month_mm == month_mm && a.year_yy == year_yy && a.sitecode == selectedValue1 && a.typeofaction == selectedValue).Max(e => e.srno)  : 1;
                    //string query = "Select srno from \"Security\"";
                    //    var maxId = _context.Security.FromSqlRaw(query).FirstOrDefault();
                    var num = "";

                    if (srNo.ToString().Length == 1)
                    {
                        num = "TRA" + "00" + srNo.ToString();

                    }
                    else if (srNo.ToString().Length == 2)
                    {
                        num = "TRA" + "0" + srNo.ToString();
                    }
                    else if (srNo.ToString().Length == 3)
                    {
                        num = "TRA" + srNo.ToString();
                    }
                    return Json(num);
                }

            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }


        }


        public int GetMaxId(string selectedValue)
        {

            int maxId = _context.Transport_Master.Any() ? _context.Transport_Master.Max(e => e.id) : 0;
            //experience.id = maxId;
            //int maxId = _context.Supplier_Masters.Where(s => s.supplier_name == month_mm && s.year_yy == year_yy && s.typeofaction == selectedValue && s.sitecode == selectedValue1).Select(s => s.srno).AsEnumerable().DefaultIfEmpty(0).Max();
            return maxId + 1;

        }


        // GET: Transport_Master/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Transport_Master == null)
            {
                return NotFound();
            }

            var Transport_Master = await _context.Transport_Master.FindAsync(id);
            if (Transport_Master == null)
            {
                return NotFound();
            }
            return View(Transport_Master);
        }

        // POST: Transport_Master/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Transport_Master Transport_Master)
        {
            if (id != Transport_Master.id)
            {
                return NotFound();
            }


            try
            {

                _context.Update(Transport_Master);

                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "Transport Master";
                logs.task = Transport_Master.id + "$" + Transport_Master.transportcode + "$" + Transport_Master.transportname + "$" + Transport_Master.contactperson + "$" + Transport_Master.contactno + "$" + Transport_Master.address + "&" + Transport_Master.city + "$" + Transport_Master.state + "$" + Transport_Master.pincode;
                logs.action = "Update";
                logs.taskid = Convert.ToInt32(id);
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.username = user;
                _context.Add(logs);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Supplier_MasterExists(Transport_Master.id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            _notyfService.Success("Update Successfully !");
            return RedirectToAction(nameof(Index));

            // return View(Transport_Master);
        }

        // GET: Transport_Master/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Transport_Master == null)
            {
                return NotFound();
            }

            var Transport_Master = await _context.Transport_Master
                .FirstOrDefaultAsync(m => m.id == id);
            if (Transport_Master == null)
            {
                return NotFound();
            }

            return View(Transport_Master);
        }

        // POST: Transport_Master/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Transport_Master == null)
            {
                return Problem("Entity set 'ErosDbContext.Transport_Master'  is null.");
            }
            var Transport_Master = await _context.Transport_Master.FindAsync(id);
            if (Transport_Master != null)
            {

                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "Transport Master";
                logs.task = Transport_Master.id + "&" + Transport_Master.transportcode + "&" + Transport_Master.transportname + "&" + Transport_Master.contactperson + "&" + Transport_Master.contactno + "&" + Transport_Master.address + "&" + Transport_Master.city + "&" + Transport_Master.state + "&" + Transport_Master.pincode;
                //logs.task = "Delete Transport master";
                logs.action = "Delete";
                logs.taskid = Convert.ToInt32(id);
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.username = user;
                _context.Add(logs);

                _context.Transport_Master.Remove(Transport_Master);
            }

            await _context.SaveChangesAsync();
            _notyfService.Error("Deleted Successfully !");
            return RedirectToAction(nameof(Index));
        }

        private bool Supplier_MasterExists(int id)
        {
            return (_context.Transport_Master?.Any(e => e.id == id)).GetValueOrDefault();
        }
    }
}
