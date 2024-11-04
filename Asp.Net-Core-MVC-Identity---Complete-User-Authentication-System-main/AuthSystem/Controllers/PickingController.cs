using AspNetCoreHero.ToastNotification.Abstractions;
using AuthSystem.Data;
using eros.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace eros.Controllers
{
    public class PickingController : Controller
    {
        public readonly ErosDbContext _context;
        public INotyfService _notyfService { get; }

        public PickingController(ErosDbContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }
        //public IActionResult Index()
        //{
        //    return View();
        //}

        [HttpGet]
        public ActionResult Changesono(string selectedValue)
        {
            if (selectedValue == "sono")
            {
                List<SelectListItem> wbridge = _context.so_inward.AsNoTracking().OrderBy(n => n.sono).Select(n => new SelectListItem
                {
                    Selected = true,
                    Value = n.sono,
                    Text = n.sono.ToString()
                }).ToList();
                return Json(wbridge);
            }
            else
            {
                //return Json(Ok());
                return Ok();
            }
        }
        [HttpPost]
        public IActionResult ActionName2(string optionValue)
        {

            Picking Picking = new Picking();

            var purchaseOrder = _context.so_inward.Where(a => a.sono == optionValue).Include(a => a.soProduct_details).FirstOrDefault();
            var product = _context.so_product.Where(a => a.orderid == purchaseOrder.id).ToList();
            if(product != null)
            {
                int i = 0;
                foreach (var mat in product)
                {
                    i++;

                    Picking.pickingmaster.Add(new picking_master()
                    {
                        id = i,
                        productcode = mat.productcode,
                        productname = mat.description,
                        quantity = mat.quantity,
                    });
                }

                return PartialView("_partialpicking", Picking);
            }
            else
            {
                return Ok();
            }
            

        }

        // GET: Picking
        // GET: Pickings
        public IActionResult Index()
        {
            List<Picking> sono = new List<Picking>();
            sono = _context.Picking.ToList();
            return View(sono);
        }

        [HttpPost]

        // GET: Pickings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Picking == null)
            {
                return NotFound();
            }

            var Picking = await _context.Picking
                .FirstOrDefaultAsync(m => m.id == id);
            if (Picking == null)
            {
                return NotFound();
            }
            Picking Pickings = _context.Picking.Include(e => e.pickingmaster).Where(a => a.id == id).FirstOrDefault();

            return View(Pickings);
        }

        // GET: Pickings/Create
        public IActionResult Create()
        {
            List<SelectListModels> sono = _context.so_inward.AsNoTracking().OrderBy(n => n.sono).Select(n => new SelectListModels
            {
                Value = n.sono,
                Text = n.sono.ToString()
            }).ToList();

            ViewBag.DropDownDatasono = new SelectList(sono, "Value", "Text");

            Picking applicant = new Picking();
            applicant.pickingmaster.Add(new picking_master() { id = 1 });
            return View(applicant);
        }


        private List<SelectListItem> GetSONO()
        {
            var lstProducts = new List<SelectListItem>();
            lstProducts = _context.so_inward.AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.sono,
                Text = n.sono
            }).ToList();
            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select SONO----"
            };
            lstProducts.Insert(0, defItem);
            return lstProducts;
        }

        // POST: Pickings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Picking Picking)
        {
            int maxId = _context.Picking.Any() ? _context.Picking.Max(e => e.id) + 1 : 1;
            Picking.id = maxId;

            _context.Add(Picking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
            return View(Picking);
        }

        [HttpGet]
        public IActionResult GetData(string selectedValue)
        {
            // Fetch data from your database or any data source based on the selectedValue
            var data = _context.so_inward.Where(a => a.sono == selectedValue).ToList(); // Replace with your data retrieval logic

            return Json(data);
        }


        // GET: Pickings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Picking == null)
            {
                return NotFound();
            }
            var so_inward = _context.Picking.Where(a => a.id == id).Include(a => a.pickingmaster).FirstOrDefault();
            if (so_inward == null)
            {
                return NotFound();
            }
            return View(so_inward);
        }

        // POST: Pickings/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id)
        {
            try
            {

                Picking Picking = _context.Picking.Include(e => e.pickingmaster).Where(a => a.id == id).FirstOrDefault();


                return View(Picking);

            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }


        // GET: Pickings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Picking == null)
            {
                return NotFound();
            }

            var Picking = await _context.Picking.FirstOrDefaultAsync(m => m.id == id);
            if (Picking == null)
            {
                return NotFound();
            }

            return View(Picking);
        }

        // POST: Pickings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Picking == null)
            {
                return Problem("Entity set 'ErosDbContext.Picking'  is null.");
            }
            var Picking = await _context.Picking.FindAsync(id);
            if (Picking != null)
            {
                _context.Picking.Remove(Picking);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PickingExists(int id)
        {
            return (_context.Picking?.Any(e => e.id == id)).GetValueOrDefault();
        }
    }
}
