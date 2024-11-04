using AspNetCoreHero.ToastNotification.Abstractions;
using AuthSystem.Data;
using eros.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace eros.Controllers
{
    public class Courier_MasterController : Controller
    {
        private readonly ErosDbContext _context;
        public INotyfService _notyfService { get; }

        public Courier_MasterController(ErosDbContext context, INotyfService notyfService)
        {
            _context = context; _notyfService = notyfService;
        }


        // GET: Courier_Master
        public async Task<IActionResult> Index()
        {
            return _context.Courier_Master != null ?
                        View(await _context.Courier_Master.ToListAsync()) :
                        Problem("Entity set 'ErosDbContext.Courier_Master'  is null.");
        }

        // GET: Courier_Master/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courier_Master = await _context.Courier_Master.FirstOrDefaultAsync(m => m.id == id);
            if (courier_Master == null)
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
                pagename = "Courier Master",
                //task = courier_Master.couriercode + "$" + courier_Master.couriername + "$" + courier_Master.contactperson + "$" + courier_Master.contactno + "$" + courier_Master.address + "$" + courier_Master.city + "$" + courier_Master.state + "$" + courier_Master.pincode,
                task = id.ToString()+ " Courier master view",
                action = "View",
                taskid = id.Value, // Convert.ToInt32(id) is not necessary here since id is already nullable int
                date = DateTime.Now.ToString("dd/MM/yyyy"),
                time = DateTime.Now.ToString("HH:mm:ss"),
                username = user,
            };

            // Add logs to the context
            _context.Add(logs);

            // Save changes to the context
            await _context.SaveChangesAsync(); // Assuming _context is an EF DbContext

            return View(courier_Master);
        }

        // GET: Courier_Master/Create
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Courier_Master Courier_Master)
        {
            int maxId = _context.Courier_Master.Any() ? _context.Courier_Master.Max(e => e.id) + 1 : 1;
            Courier_Master.id = maxId;

            //Courier_Master.field1 = "-";
            //Courier_Master.field2 = "-";
            //Courier_Master.field3 = "-";
            //Courier_Master.field4 = "-";
            //Courier_Master.field5 = "-";
            _context.Add(Courier_Master);
            //maintain logs
            var user = HttpContext.Session.GetString("User");
            var logs = new Logs();
            logs.pagename = "Courier Master";
            //logs.task = Courier_Master.couriercode+"$"+Courier_Master.couriername + "$" +Courier_Master.contactperson + "$" +Courier_Master.contactno + "$" +Courier_Master.address + "$" +Courier_Master.city + "$" +Courier_Master.state + "$" +Courier_Master.pincode;
            logs.action = "Create";
            logs.taskid = maxId;
            logs.task = maxId.ToString() + " Courier master Create";
            logs.date = DateTime.Now.ToString("dd/MM/yyyy");
            logs.time = DateTime.Now.ToString("HH:mm:ss");
            logs.username = user;
            _context.Add(logs);
            await _context.SaveChangesAsync();
            _notyfService.Success("Created Successfully ");
            return RedirectToAction(nameof(Index));

            //return View(Courier_Master);
        }

        // GET: Courier_Master/Create
        public IActionResult CreateViewSupplier()
        {
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateViewtransport(Courier_Master Courier_Master)
        {
            int maxId = _context.Courier_Master.Any() ? _context.Courier_Master.Max(e => e.id) + 1 : 1;
            Courier_Master.id = maxId;
            _context.Add(Courier_Master);
            await _context.SaveChangesAsync();
            return RedirectToAction("Create", "purchases");
            return View(Courier_Master);
        }

        public ActionResult tidgenerate(string selectedValue)
        {
            try
            {

                //var month_mm = DateTime.Now.ToString("MM");
                //var year_yy = DateTime.Now.ToString("yy");
                var suppliernm = _context.Courier_Master.Where(e => e.couriername == selectedValue).FirstOrDefault();

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
                        num = "COR" + "00" + srNo.ToString();

                    }
                    else if (srNo.ToString().Length == 2)
                    {
                        num = "COR" + "0" + srNo.ToString();
                    }
                    else if (srNo.ToString().Length == 3)
                    {
                        num = "COR" + srNo.ToString();
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

            int maxId = _context.Courier_Master.Any() ? _context.Courier_Master.Max(e => e.id) : 0;
            //experience.id = maxId;
            //int maxId = _context.Supplier_Masters.Where(s => s.supplier_name == month_mm && s.year_yy == year_yy && s.typeofaction == selectedValue && s.sitecode == selectedValue1).Select(s => s.srno).AsEnumerable().DefaultIfEmpty(0).Max();
            return maxId + 1;

        }


        // GET: Courier_Master/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Courier_Master == null)
            {
                return NotFound();
            }

            var Courier_Master = await _context.Courier_Master.FindAsync(id);
            if (Courier_Master == null)
            {
                return NotFound();
            }
            return View(Courier_Master);
        }

        // POST: Courier_Master/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Courier_Master Courier_Master)
        {
            if (id != Courier_Master.id)
            {
                return NotFound();
            }


            try
            {

                _context.Update(Courier_Master);
                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "Curier Master";
                logs.task = Courier_Master.couriercode + "$" + Courier_Master.couriername + "$" + Courier_Master.contactperson + "$" + Courier_Master.contactno + "$" + Courier_Master.address + "$" + Courier_Master.city + "$" + Courier_Master.state + "$" + Courier_Master.pincode;
                logs.action = "Update";
                logs.taskid = id;
                //logs.task = id.ToString();
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.username = user;
                _context.Add(logs);
                await _context.SaveChangesAsync();
                _notyfService.Success("Update Sussesfully !");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Supplier_MasterExists(Courier_Master.id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));

            // return View(Courier_Master);
        }

        //public async Task<IActionResult> Edit(int id, Courier_Master courierMaster)
        //{
        //    if (id != courierMaster.id)
        //    {
        //        return NotFound();
        //    }

        //    try
        //    {
        //        // Retrieve the existing entity from the context
        //        var existingCourier = await _context.Courier_Master.FindAsync(id);
        //        if (existingCourier == null)
        //        {
        //            return NotFound();
        //        }

        //        // Generate change log
        //        var changeLog = GenerateChangeLog(existingCourier, courierMaster);

        //        // Maintain logs
        //        var user = HttpContext.Session.GetString("User");
        //        var logs = new Logs();
        //        logs.pagename = "Courier Master";
        //        logs.action = "Update";
        //        logs.taskid = id;
        //        logs.date = DateTime.Now.ToString("dd/MM/yyyy");
        //        logs.time = DateTime.Now.ToString("HH:mm:ss");
        //        logs.username = user;

        //        // Log the changes
        //        logs.task = changeLog;
        //        _context.Add(logs);

        //        // Update the existing entity with the new values
        //        _context.Entry(existingCourier).CurrentValues.SetValues(courierMaster);

        //        await _context.SaveChangesAsync();
        //        _notyfService.Success("Update Successfully !");
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!Courier_MasterExists(courierMaster.id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //    return RedirectToAction(nameof(Index));
        //}
        //private bool Courier_MasterExists(int id)
        //{
        //    return _context.Courier_Master.Any(e => e.id == id);
        //}

        //private string GenerateChangeLog(Courier_Master existingCourier, Courier_Master updatedCourier)
        //{
        //    StringBuilder changeLogBuilder = new StringBuilder();

        //    if (existingCourier.couriercode != updatedCourier.couriercode)
        //    {
        //        changeLogBuilder.Append($"{existingCourier.couriercode} -> {updatedCourier.couriercode} | ");
        //    }

        //    if (existingCourier.couriername != updatedCourier.couriername)
        //    {
        //        changeLogBuilder.Append($"{existingCourier.couriername} -> {updatedCourier.couriername} | ");
        //    }

        //    if (existingCourier.contactperson != updatedCourier.contactperson)
        //    {
        //        changeLogBuilder.Append($"{existingCourier.contactperson} -> {updatedCourier.contactperson} | ");
        //    }

        //    if (existingCourier.contactno != updatedCourier.contactno)
        //    {
        //        changeLogBuilder.Append($"{existingCourier.contactno} -> {updatedCourier.contactno} | ");
        //    }

        //    if (existingCourier.address != updatedCourier.address)
        //    {
        //        changeLogBuilder.Append($"{existingCourier.address} -> {updatedCourier.address} | ");
        //    }

        //    if (existingCourier.city != updatedCourier.city)
        //    {
        //        changeLogBuilder.Append($"{existingCourier.city} -> {updatedCourier.city} | ");
        //    }

        //    if (existingCourier.state != updatedCourier.state)
        //    {
        //        changeLogBuilder.Append($"{existingCourier.state} -> {updatedCourier.state} | ");
        //    }

        //    if (existingCourier.pincode != updatedCourier.pincode)
        //    {
        //        changeLogBuilder.Append($"{existingCourier.pincode} -> {updatedCourier.pincode}");
        //    }

        //    return changeLogBuilder.ToString();
        //}


        // GET: Courier_Master/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Courier_Master == null)
            {
                return NotFound();
            }

            var Courier_Master = await _context.Courier_Master
                .FirstOrDefaultAsync(m => m.id == id);
            if (Courier_Master == null)
            {
                return NotFound();
            }

            return View(Courier_Master);
        }

        // POST: Courier_Master/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Courier_Master == null)
            {
                return Problem("Entity set 'ErosDbContext.Courier_Master'  is null.");
            }
            var Courier_Master = await _context.Courier_Master.FindAsync(id);
            if (Courier_Master != null)
            {
                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "curiour Master";
                logs.task = Courier_Master.id + "$" + Courier_Master.couriercode + "$" + Courier_Master.couriername + "$" + Courier_Master.contactperson + "$" + Courier_Master.contactno + "$" + Courier_Master.address + "$" + Courier_Master.city + "$" + Courier_Master.state + "$" + Courier_Master.pincode;
                logs.action = "Delete";
                logs.taskid = id;
                //logs.task = id.ToString();
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.username = user;
                _context.Add(logs);
                _context.Courier_Master.Remove(Courier_Master);

            }

            await _context.SaveChangesAsync();
            _notyfService.Error("Deleted Successfully ");
            return RedirectToAction(nameof(Index));
        }

        private bool Supplier_MasterExists(int id)
        {
            return (_context.Courier_Master?.Any(e => e.id == id)).GetValueOrDefault();
        }
    }
}
