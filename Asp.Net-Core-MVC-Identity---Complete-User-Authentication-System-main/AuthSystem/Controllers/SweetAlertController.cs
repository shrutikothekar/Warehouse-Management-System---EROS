using Microsoft.AspNetCore.Mvc;

namespace eros.Controllers
{
    public class SweetAlertController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
