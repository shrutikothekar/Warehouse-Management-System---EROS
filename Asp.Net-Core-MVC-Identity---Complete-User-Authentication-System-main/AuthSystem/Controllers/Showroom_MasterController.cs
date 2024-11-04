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

namespace eros.Controllers
{
    public class Showroom_MasterController : Controller
    {
        private readonly ErosDbContext _context;
        public INotyfService _notyfService { get; }

        public Showroom_MasterController(ErosDbContext context, INotyfService notyfService)
        {
            _notyfService = notyfService;
            _context = context;
        }

        // GET: Showroom_Master
        public async Task<IActionResult> Index()
        {
            return _context.Showroom_Master != null ?
                        View(await _context.Showroom_Master.ToListAsync()) :
                        Problem("Entity set 'ErosDbContext.Showroom_Master'  is null.");
        }

        // GET: Showroom_Master/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Showroom_Master == null)
            {
                return NotFound();
            }

            var showroom_Master = await _context.Showroom_Master
                .FirstOrDefaultAsync(m => m.Showroomid == id);
            if (showroom_Master == null)
            {
                return NotFound();
            }
            //maintain logs
            var user = HttpContext.Session.GetString("User");
            var logs = new Logs();
            logs.pagename = "showroom Master";
            logs.task = "View showroom master ";
            logs.action = "View";
            logs.taskid = Convert.ToInt32(id);
            logs.date = DateTime.Now.ToString("dd/MM/yyyy");
            logs.time = DateTime.Now.ToString("HH:mm:ss");
            logs.username = user;
            _context.Add(logs);
            _context.SaveChanges();
            return View(showroom_Master);
        }

        // GET: Showroom_Master/Create
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Showroom_Master showroom_Master)
        {
            int maxId = _context.Showroom_Master.Any() ? _context.Showroom_Master.Max(e => e.Showroomid) + 1 : 1;
            showroom_Master.Showroomid = maxId;

            //showroom_Master.field1 = "-";
            //showroom_Master.field2 = "-";
            //showroom_Master.field3 = "-";
            //showroom_Master.field4 = "-";
            //showroom_Master.field5 = "-";
            _context.Add(showroom_Master);
            //maintain logs
            var user = HttpContext.Session.GetString("User");
            var logs = new Logs();
            logs.pagename = "showroom Master";
            logs.task = "Create showroom master ";
            logs.action = "Create";
            logs.taskid = Convert.ToInt32(maxId);
            logs.date = DateTime.Now.ToString("dd/MM/yyyy");
            logs.time = DateTime.Now.ToString("HH:mm:ss");
            logs.username = user;
            _context.Add(logs);
            _context.SaveChanges();

            await _context.SaveChangesAsync();
            _notyfService.Success("Showroom name Created Successfully ");
            return RedirectToAction(nameof(Index));

            return View(showroom_Master);
        }

        // GET: Showroom_Master/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Showroom_Master == null)
            {
                return NotFound();
            }

            var showroom_Master = await _context.Showroom_Master.FindAsync(id);
            if (showroom_Master == null)
            {
                return NotFound();
            }
            return View(showroom_Master);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Showroomid,Showroom_name,address,gstno,contactno")] Showroom_Master showroom_Master)
        {
            if (id != showroom_Master.Showroomid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(showroom_Master);
                    //maintain logs
                    var user = HttpContext.Session.GetString("User");
                    var logs = new Logs();
                    logs.pagename = "showroom Master";
                    logs.task = showroom_Master.Showroomid + "$" + showroom_Master.Showroom_name + "$" + showroom_Master.address + "$" + showroom_Master.gstno + "$" + showroom_Master.contactno;
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
                    if (!Showroom_MasterExists(showroom_Master.Showroomid))
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
            return View(showroom_Master);
        }

        // GET: Showroom_Master/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Showroom_Master == null)
            {
                return NotFound();
            }

            var showroom_Master = await _context.Showroom_Master
                .FirstOrDefaultAsync(m => m.Showroomid == id);
            if (showroom_Master == null)
            {
                return NotFound();
            }

            return View(showroom_Master);
            _notyfService.Success("Update Successfully !");
        }

        // POST: Showroom_Master/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Showroom_Master == null)
            {
                return Problem("Entity set 'ErosDbContext.Showroom_Master'  is null.");
            }
            var showroom_Master = await _context.Showroom_Master.FindAsync(id);
            if (showroom_Master != null)
            {
                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "showroom Master";
                //logs.task = "Delete showroom master ";
                logs.task = showroom_Master.Showroomid+"$"+showroom_Master.Showroom_name + "$" + showroom_Master.address + "$" + showroom_Master.gstno + "$" + showroom_Master.contactno;
                logs.action = "Delete";
                logs.taskid = Convert.ToInt32(id);
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.username = user;
                _context.Add(logs);
                _context.SaveChanges();

                _context.Showroom_Master.Remove(showroom_Master);
            }

            await _context.SaveChangesAsync();
            _notyfService.Error("Deleted Successfully");
            return RedirectToAction(nameof(Index));
        }

        private bool Showroom_MasterExists(int id)
        {
            return (_context.Showroom_Master?.Any(e => e.Showroomid == id)).GetValueOrDefault();
        }
    }
}
