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
    public class state_MasterController : Controller
    {
        private readonly ErosDbContext _context;

        public state_MasterController(ErosDbContext context)
        {
            _context = context;
        }

        // GET: state_Master
        public async Task<IActionResult> Index()
        {
            return _context.state_Master != null ?
                        View(await _context.state_Master.ToListAsync()) :
                        Problem("Entity set 'ErosDbContext.state_Master'  is null.");
        }

        // GET: state_Master/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.state_Master == null)
            {
                return NotFound();
            }

            var state_Master = await _context.state_Master
                .FirstOrDefaultAsync(m => m.state_id == id);
            if (state_Master == null)
            {
                return NotFound();
            }

            return View(state_Master);
        }

        // GET: state_Master/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: state_Master/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(state_master state_Master)
        {

            int maxId = _context.state_Master.Any() ? _context.state_Master.Max(e => e.state_id) + 1 : 1;
            state_Master.state_id = maxId;

            //state_Master.field1 = "-";
            //state_Master.field2 = "-";
            //state_Master.field3 = "-";
            //state_Master.field4 = "-";
            //state_Master.field5 = "-";
            _context.Add(state_Master);
            await _context.SaveChangesAsync();
       
            return RedirectToAction(nameof(Index));

            return View(state_Master);
        }

        // GET: state_Master/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.state_Master == null)
            {
                return NotFound();
            }

            var state_Master = await _context.state_Master.FindAsync(id);
            if (state_Master == null)
            {
                return NotFound();
            }
            return View(state_Master);
        }

        // POST: state_Master/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,state_master state_Master)
        {
            if (id != state_Master.state_id)
            {
                return NotFound();
            }

          
                try
                {
                    _context.Update(state_Master);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!state_MasterExists(state_Master.state_id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            
            return View(state_Master);
        }

        // GET: state_Master/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.state_Master == null)
            {
                return NotFound();
            }

            var state_Master = await _context.state_Master
                .FirstOrDefaultAsync(m => m.state_id == id);
            if (state_Master == null)
            {
                return NotFound();
            }

            return View(state_Master);
        }

        // POST: state_Master/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.state_Master == null)
            {
                return Problem("Entity set 'ErosDbContext.state_Master'  is null.");
            }
            var state_Master = await _context.state_Master.FindAsync(id);
            if (state_Master != null)
            {
                _context.state_Master.Remove(state_Master);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool state_MasterExists(int id)
        {
            return (_context.state_Master?.Any(e => e.state_id == id)).GetValueOrDefault();
        }
    }
}
