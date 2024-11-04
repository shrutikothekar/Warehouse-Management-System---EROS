using AuthSystem.Data;
using Microsoft.AspNetCore.Mvc;

namespace eros.Controllers
{
    public class CascadeController : Controller
    {
        private readonly ErosDbContext _context;

        public CascadeController(ErosDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
