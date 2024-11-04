using AuthSystem.Data;
using eros.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace eros.Controllers
{
    public class Picking_ProcessController : Controller
    {
        private readonly ErosDbContext _context;

        public Picking_ProcessController(ErosDbContext context)
        {
            _context = context;
        }

        // GET: Picking_Process
        public async Task<IActionResult> Index()
        {
            List<Picking_Process> sono = new List<Picking_Process>();
            sono = _context.Picking_Process.ToList();
            return View(sono);
        }

        // GET: Picking_Process/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Picking_Process == null)
            {
                return NotFound();
            }

            var Picking_Process = await _context.Picking_Process
                .FirstOrDefaultAsync(m => m.pick_id == id);
            if (Picking_Process == null)
            {
                return NotFound();
            }

            return View(Picking_Process);
        }

        // GET: Picking_Process/Create
        public IActionResult Create()
        {
            Picking_Process applicant = new Picking_Process();
            //applicant.Picking_Packet.Add(new Picking_Packet() { pick_id = 1 });
            List<SelectListItem> wbridge = _context.so_inward.AsNoTracking().OrderBy(n => n.sono).Select(n => new SelectListItem
            {

                Value = n.sono,
                Text = n.sono.ToString()
            }).ToList();
            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select Segement----"
            };

            wbridge.Insert(0, defItem);
            ViewBag.data1 = wbridge;
            return View(applicant);

        }

        // POST: Picking_Process/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Picking_Process Picking_Process)
        {

            int maxId = _context.Picking_Process.Any() ? _context.Picking_Process.Max(e => e.pick_id) + 1 : 1;
            Picking_Process.pick_id = maxId;
            List<Picking_Packet> packetsToRemove = new List<Picking_Packet>();
            _context.Add(Picking_Process);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

            return View(Picking_Process);
        }

        // GET: Picking_Process/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Picking_Process == null)
            {
                return NotFound();
            }

            var Picking_Process = await _context.Picking_Process.FindAsync(id);
            if (Picking_Process == null)
            {
                return NotFound();
            }
            return View(Picking_Process);
        }

        // POST: Picking_Process/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,rackname,shelves,bin")] Picking_Process Picking_Process)
        {
            if (id != Picking_Process.pick_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(Picking_Process);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!rack_masterExists(Picking_Process.pick_id))
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
            return View(Picking_Process);
        }

        // GET: Picking_Process/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Picking_Process == null)
            {
                return NotFound();
            }

            var Picking_Process = await _context.Picking_Process
                .FirstOrDefaultAsync(m => m.pick_id == id);
            if (Picking_Process == null)
            {
                return NotFound();
            }

            return View(Picking_Process);
        }

        // POST: Picking_Process/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Picking_Process == null)
            {
                return Problem("Entity set 'ErosDbContext.Picking_Process'  is null.");
            }
            var Picking_Process = await _context.Picking_Process.FindAsync(id);
            if (Picking_Process != null)
            {
                _context.Picking_Process.Remove(Picking_Process);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool rack_masterExists(int id)
        {
            return (_context.Picking_Process?.Any(e => e.pick_id == id)).GetValueOrDefault();
        }

        [HttpPost]
        public IActionResult GetSono(string optionValue)
        {
            Picking_Process picklist = new Picking_Process();

            var saleorder = _context.so_inward
                .Where(a => a.sono == optionValue)
                .Include(a => a.soProduct_details)
                .FirstOrDefault();

            if (saleorder == null)
            {
                return NotFound(); // Handle the case where no sale order is found
            }

            var product = _context.so_product.Where(a => a.orderid == saleorder.id).ToList();

            int i = 0;
            foreach (var mat in product)
            {
                i++;
                var storageOperationData = _context.Storage_Operation
                    .Where(a => a.productcode == mat.productcode)
                    .FirstOrDefault();

                //var picklistdata = _context.picklistgnerateprds
                // .Where(a => a.prdcode == mat.productcode)
                 
                // .SingleOrDefault();

                //picklist.Picking_Packet.Add(new Picking_Packet()
                //{
                //    pick_id = i,
                //    productcode = mat.productcode,
                //    productname = mat.description,
                //    soqty = mat.quantity.ToString(),
                //    // Add the Storage_Operation data to Picking_Packet if needed
                //    boxno = storageOperationData.boxno,
                //    batchcode = storageOperationData.batchcode,
                //    location = storageOperationData.locationcode,
                //    pickingqty = picklistdata.pickingqty,
                //});
            }

            return Json(picklist); // Assuming you want to return the entire Picking_Process object
        }


    }
}
