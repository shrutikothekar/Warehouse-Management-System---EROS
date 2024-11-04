using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AuthSystem.Data;
using eros.Models;

namespace eros.Controllers
{
    public class rack_masterController : Controller
    {
        private readonly ErosDbContext _context;

        public rack_masterController(ErosDbContext context)
        {
            _context = context;
        }

        // GET: rack_master
        public async Task<IActionResult> Index()
        {
            return _context.rack_master != null ?
                        View(await _context.rack_master.ToListAsync()) :
                        Problem("Entity set 'ErosDbContext.rack_master'  is null.");
        }

        // GET: rack_master/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.rack_master == null)
            {
                return NotFound();
            }

            var rack_master = await _context.rack_master
                .FirstOrDefaultAsync(m => m.id == id);
            if (rack_master == null)
            {
                return NotFound();
            }

            return View(rack_master);
        }

        // GET: rack_master/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: rack_master/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(rack_master rack_master)
        {

            int maxId = _context.rack_master.Any() ? _context.rack_master.Max(e => e.id) + 1 : 1;
            rack_master.id = maxId;

            //rack_master.field1 = "-";
            //rack_master.field2 = "-";
            //rack_master.field3 = "-";
            //rack_master.field4 = "-";
            //rack_master.field5 = "-";
            _context.Add(rack_master);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

            return View(rack_master);
        }

        // GET: rack_master/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.rack_master == null)
            {
                return NotFound();
            }

            var rack_master = await _context.rack_master.FindAsync(id);
            if (rack_master == null)
            {
                return NotFound();
            }
            return View(rack_master);
        }

        // POST: rack_master/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,rackname,shelves,bin")] rack_master rack_master)
        {
            if (id != rack_master.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rack_master);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!rack_masterExists(rack_master.id))
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
            return View(rack_master);
        }

        // GET: rack_master/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.rack_master == null)
            {
                return NotFound();
            }

            var rack_master = await _context.rack_master
                .FirstOrDefaultAsync(m => m.id == id);
            if (rack_master == null)
            {
                return NotFound();
            }

            return View(rack_master);
        }

        // POST: rack_master/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.rack_master == null)
            {
                return Problem("Entity set 'ErosDbContext.rack_master'  is null.");
            }
            var rack_master = await _context.rack_master.FindAsync(id);
            if (rack_master != null)
            {
                _context.rack_master.Remove(rack_master);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool rack_masterExists(int id)
        {
            return (_context.rack_master?.Any(e => e.id == id)).GetValueOrDefault();
        }
    }
}
