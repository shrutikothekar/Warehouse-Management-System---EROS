using AuthSystem.Areas.Identity.Data;
using AuthSystem.Data;
using AuthSystem.Models;
using eros.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;

namespace AuthSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        public readonly ErosDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;


        public HomeController(SignInManager<ApplicationUser> signInManager, ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, ErosDbContext context)
        {
            _signInManager = signInManager;
            _logger = logger;
            this._userManager = userManager;
            _context = context;

        }

        [HttpGet]

        public IActionResult GetData()
        {
            // Fetch purchase data from the database
            var purchaseData = _context.purchase.ToList();

            // Fetch sale order data from the database
            var saleOrderData = _context.so_inward.ToList();

            var months = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;

            var purchaseByMonth = purchaseData
                .Where(so => so.status == "Completed")
                .GroupBy(p => new {
                    Year = DateTime.ParseExact(p.podate, "yyyy-MM-dd", CultureInfo.InvariantCulture).Year,
                    Month = DateTime.ParseExact(p.podate, "yyyy-MM-dd", CultureInfo.InvariantCulture).Month
                })
                .Select(group => new {
                    Month = months[group.Key.Month - 1], // Subtract 1 since C# months are 1-based
                    EntryCount = group.Count()
                })
                .ToList();

            var completedSaleOrders = saleOrderData
                .Where(so => so.status == "Completed")
                .GroupBy(so => new {
                    Year = DateTime.ParseExact(so.sodate, "yyyy-MM-dd", CultureInfo.InvariantCulture).Year,
                    Month = DateTime.ParseExact(so.sodate, "yyyy-MM-dd", CultureInfo.InvariantCulture).Month
                })
                .Select(group => new {
                    Month = months[group.Key.Month - 1], // Subtract 1 since C# months are 1-based
                    EntryCount = group.Count()
                })
                .ToList();



            var data = new List<object>();

            foreach (var month in months)
            {
                int y1 = purchaseByMonth.FirstOrDefault(e => e.Month == month)?.EntryCount ?? 0; // Sample value for y1
                int y2 = completedSaleOrders.FirstOrDefault(e => e.Month == month)?.EntryCount ?? 0; // Sample value for y2

                // Create an object for the current month and add it to the data list
                var monthData = new { x = month, y1 = y1, y2 = y2 };
                data.Add(monthData);
            }

            return Json(data);
        }

        private int DMRPRRPCount()
        {
            var counter = 0;

            // Fetch data from database first (still translated to SQL)
            var inward = _context.inward
                .Where(a => a.status.Trim() == "Pending" &&
                            (a.ordertype.Trim() == "Demo" ||
                             a.ordertype.Trim() == "Repair" ||
                             a.ordertype.Trim() == "Replacement"))
                .ToList();  // Executes query and brings data into memory

            // Apply DistinctBy in memory using Linq to Objects
            var distinctInward = inward
                .DistinctBy(a => a.pono.Trim()) // This works in-memory
                .ToList();

            if (distinctInward.Count > 0)
            {
                counter = distinctInward.Count;
            }

            return counter;
        }

        private int GetSalesOrderCount()
        {
            var purchase = _context.so_inward.Where(a => a.status == "Pending").OrderByDescending(a => a.sodate).ToList(); // Get a list of pending purchase orders

            List<so_inward> pendingOrders = new List<so_inward>(); // Create an empty list
            foreach (var data in purchase)
            {
                var purchasesum = _context.so_product.Where(a => a.orderid == data.id).Sum(a => a.quantity);
                var inwarddetails = _context.inwardPacket.Where(a => a.sono == data.sono)
                          .GroupBy(p => p.sono)
                                   .Select(group => new
                                   {
                                       ProductName = group.Key,
                                       TotalQuantity = group.Sum(p => p.quantity),
                                       TotalSubAssembly = group.Sum(p => p.totalsubassmbly)
                                   }).FirstOrDefault();
                //var inwarddetails = _context.inward.Where(a => a.pono == data.pono && a.flag == 1).AsNoTracking().ToList();
                if (inwarddetails == null)
                {
                    data.qty = purchasesum;
                    data.pqty = purchasesum;
                }
                else
                {
                    data.qty = purchasesum; // Setting qty to purchasesum
                    data.pqty = purchasesum - inwarddetails.TotalQuantity;
                }
                pendingOrders.Add(data);
            }

            //return View("SalesPendingList", pendingOrders);
            return pendingOrders.Count;
        }
        private int GetpurchaseOrderCount()
        {
            var purchase = _context.purchase.Where(a => a.status == "Pending").OrderByDescending(a => a.podate).ToList(); // Get a list of pending purchase orders

            List<purchase> pendingOrders = new List<purchase>(); // Create an empty list
            foreach (var data in purchase)
            {
                var purchasesum = _context.poProduct_details.Where(a => a.porderid == data.id).Sum(a => a.quantity);
                var inwarddetails = _context.inwardPacket.Where(a => a.pono == data.pono)
                          .GroupBy(p => p.pono)
                                   .Select(group => new
                                   {
                                       ProductName = group.Key,
                                       TotalQuantity = group.Sum(p => p.quantity),
                                       TotalSubAssembly = group.Sum(p => p.totalsubassmbly)
                                   }).FirstOrDefault();
                //var inwarddetails = _context.inward.Where(a => a.pono == data.pono && a.flag == 1).AsNoTracking().ToList();
                if (inwarddetails == null)
                {
                    data.qty = purchasesum;
                    data.pqty = purchasesum;
                }
                else
                {
                    data.qty = purchasesum; // Setting qty to purchasesum
                    data.pqty = purchasesum - inwarddetails.TotalQuantity;
                }
                pendingOrders.Add(data);
            }

            //return View("SalesPendingList", pendingOrders);
            return pendingOrders.Count;
        }
        //public IActionResult Logout()
        //{
        //    // Clear the session
        //    HttpContext.Session.Clear();

        //    // Sign out the user
        //    return RedirectToAction("Index", "Home");
        //}
        
        public IActionResult Index()
        {
            int salesOrderCount = GetSalesOrderCount();
            ViewBag.DMRPRRPCount = DMRPRRPCount();
            ViewBag.SalesOrderCount = salesOrderCount;
            int purchaseOrderCount = GetpurchaseOrderCount();
            ViewBag.purchaseOrderCount = purchaseOrderCount;
            ViewData["UserID"] = _userManager.GetUserId(this.User);
            string username = _userManager.GetUserName(this.User);

            var pageallot = _context.UserManagement.Where(a => a.UserName == username).Select(a => a.PageName).ToList();
            var role = _context.UserManagement.Where(a => a.UserName == username).Select(a => a.Role).FirstOrDefault();
          
            // Check if role is not null before setting the session
            if (role != null)
            {
                HttpContext.Session.Remove("Role");
                HttpContext.Session.SetString("Role", role);
                HttpContext.Session.SetString("User", username);
            }
            else
            {
                HttpContext.Session.Remove("Role");
            }
            // Log login event
            int maxId = _context.loginlog.Any() ? _context.loginlog.Max(e => e.Id) + 1 : 1;
            var Id = maxId;
            var loginLog = new loginlog
            {
                Id = Id,
                logoutdt = "",
                logouttime = "",
                username = username,
                logindt = DateTime.Now.ToString("yyyy-MM-dd"), // Log the login timestamp
                logintime = DateTime.Now.ToString("HH:mm:ss"), // Log the login timestamp
            };
            
            _context.loginlog.Add(loginLog);
            _context.SaveChanges();
            // Data show in list
            var data = _context.MenuModel.Where(menu => pageallot.Contains(menu.Title)).ToList();
            var jsonData = JsonConvert.SerializeObject(data);
            HttpContext.Session.SetString("MenuMaster", jsonData);

            return View();
        }
        
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Get the username of the currently logged-in user
            string username = _userManager.GetUserName(this.User);

            // Find the last login entry for the current user where logout date is null
            var lastLogin = _context.loginlog.Where(l => l.username == username)
                                              .OrderByDescending(l => l.date)
                                              .FirstOrDefault();

            // Update the logout date and time only if the last login entry corresponds to the current session
            if (lastLogin != null)
            {
                lastLogin.logoutdt = DateTime.Now.ToString("yyyy-MM-dd");
                lastLogin.logouttime = DateTime.Now.ToString("HH:mm:ss");
                _context.loginlog.Update(lastLogin);
                _context.SaveChanges();
            }
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home"); // Redirect to home page or any other page after logout
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout1()
        {
            // Get the username of the currently logged-in user
            string username = _userManager.GetUserName(this.User);

            // Find the last login entry for the current user where logout date is null
            var lastLogin = _context.loginlog.Where(l => l.username == username)
                                              .OrderByDescending(l => l.date)
                                              .FirstOrDefault();

            // Update the logout date and time only if the last login entry corresponds to the current session
            if (lastLogin != null)
            {
                lastLogin.logoutdt = DateTime.Now.ToString("yyyy-MM-dd");
                lastLogin.logouttime = DateTime.Now.ToString("HH:mm:ss");
                _context.loginlog.Update(lastLogin);
                await _context.SaveChangesAsync(); // Make sure to await this
            }

            await _signInManager.SignOutAsync();

            // Return a JSON response indicating success
            return Json(new { success = true, message = "Logged out successfully." });
        }



        //public IActionResult Index()
        //{
        //    ViewData["UserID"] = _userManager.GetUserId(this.User);
        //    string username = _userManager.GetUserName(this.User);


        //    var pageallot =_context.UserManagement.Where(a=>a.UserName == username).Select(a=>a.PageName).ToList();
        //    //data show in list
        //    var data = _context.MenuModel.Where(menu => pageallot.Contains(menu.Title)).ToList();
        //    var jsonData = JsonConvert.SerializeObject(data);
        //    // Store the JSON string in the session // Set a value in the session
        //    HttpContext.Session.SetString("MenuMaster", jsonData);

        //    return View();
        //}

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}