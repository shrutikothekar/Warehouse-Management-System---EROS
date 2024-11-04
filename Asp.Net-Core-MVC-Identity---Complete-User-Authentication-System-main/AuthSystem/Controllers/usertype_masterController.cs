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
    public class usertype_masterController : Controller
    {
        private readonly ErosDbContext _context;
        public INotyfService _notifyService { get; }
        public usertype_masterController(ErosDbContext context, INotyfService notifyService)
        {
            _context = context;
            _notifyService = notifyService;
        }

        // GET: usertype_master
        public async Task<IActionResult> Index()
        {
            return _context.usertype_Master != null ?
                        View(await _context.usertype_Master.ToListAsync()) :
                        Problem("Entity set 'ErosDbContext.usertype_Master'  is null.");
        }

        // GET: usertype_master/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.usertype_Master == null)
            {
                return NotFound();
            }

            var usertype_master = await _context.usertype_Master
                .FirstOrDefaultAsync(m => m.user_id == id);
            if (usertype_master == null)
            {
                return NotFound();
            }
            var user = HttpContext.Session.GetString("User");
            var logs = new Logs
            {
                pagename = "Usertype Master",
                task = "View Usertype Master",
                taskid = Convert.ToInt32(id),
                date = DateTime.Now.ToString("dd/MM/yyyy"),
                time = DateTime.Now.ToString("HH:mm:ss"),
                action = "View",
                username = user,
        };
           
            _context.Add(logs);
            _context.SaveChanges();
            return View(usertype_master);
        }

        // GET: usertype_master/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: usertype_master/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(usertype_master usertype_master)
        {
            int maxId = _context.usertype_Master.Any() ? _context.usertype_Master.Max(e => e.user_id) + 1 : 1;
            usertype_master.user_id = maxId;
            
            _context.Add(usertype_master);
            await _context.SaveChangesAsync();
            //maintain logs
            var user = HttpContext.Session.GetString("User");
            var logs = new Logs();
            logs.pagename = "Usertype Master";
            logs.task = "Create Usertype Master";
            logs.taskid = maxId;
            logs.date = DateTime.Now.ToString("dd/MM/yyyy");
            logs.time = DateTime.Now.ToString("HH:mm:ss");
            logs.action = "Create";
            logs.username = user;
            _context.Add(logs);
            _context.SaveChanges();
            _notifyService.Success("User Type created successfully !");
            //return View(usertype_master);
            return RedirectToAction(nameof(Index));
        }

        // GET: usertype_master/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.usertype_Master == null)
            {
                return NotFound();
            }

            var usertype_master = await _context.usertype_Master.FindAsync(id);
            if (usertype_master == null)
            {
                return NotFound();
            }
            return View(usertype_master);
        }

        // POST: usertype_master/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, usertype_master usertype_master)
        {
            if (id != usertype_master.user_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //usertype_master.field1 = "-";
                    //usertype_master.field2 = "-";
                    //usertype_master.field3 = "-";
                    //usertype_master.field4 = "-";
                    //usertype_master.field5 = "-";
                    _context.Update(usertype_master);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!usertype_masterExists(usertype_master.user_id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "Usertype Master";
                logs.task = usertype_master.user_id + "$" + usertype_master.usertype_name + "$" + usertype_master.designation; 
                logs.taskid = id;
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.action = "Update";
                logs.username = user;
                _context.Add(logs);
                _context.SaveChanges();
                _notifyService.Success("UserType update Successfully !");
                return RedirectToAction(nameof(Index));
            }
            return View(usertype_master);
        }

        // GET: usertype_master/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.usertype_Master == null)
            {
                return NotFound();
            }

            var usertype_master = await _context.usertype_Master
                .FirstOrDefaultAsync(m => m.user_id == id);
            if (usertype_master == null)
            {
                return NotFound();
            }

            return View(usertype_master);
        }

        // POST: usertype_master/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.usertype_Master == null)
            {
                return Problem("Entity set 'ErosDbContext.usertype_Master'  is null.");
            }
            var usertype_master = await _context.usertype_Master.FindAsync(id);
            if (usertype_master != null)
            {
                _context.usertype_Master.Remove(usertype_master);
            }

            await _context.SaveChangesAsync();
            var user = HttpContext.Session.GetString("User");
            var logs = new Logs();
            logs.pagename = "Usertype Master";
            //logs.task = "Delete Usertype Master";
            logs.task = usertype_master.user_id+"$"+ usertype_master.usertype_name+"$"+ usertype_master.designation;
            logs.taskid = id;
            logs.date = DateTime.Now.ToString("dd/MM/yyyy");
            logs.time = DateTime.Now.ToString("HH:mm:ss");
            logs.action = "Delete";
            logs.username = user;
            _context.Add(logs);
            _context.SaveChanges();
            _notifyService.Error("User Type deleted successfully !");
            return RedirectToAction(nameof(Index));
        }

        private bool usertype_masterExists(int id)
        {
            return (_context.usertype_Master?.Any(e => e.user_id == id)).GetValueOrDefault();
        }
    }
}
