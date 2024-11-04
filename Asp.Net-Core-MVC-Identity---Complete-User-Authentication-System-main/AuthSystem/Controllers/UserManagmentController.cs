using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AuthSystem.Data;
using eros.Models;
using Microsoft.AspNetCore.Identity;
using AuthSystem.Areas.Identity.Data;
using AspNetCoreHero.ToastNotification.Abstractions;
using System.Runtime.InteropServices;
//using eros.Migrations;

namespace eros.Controllers
{
    public class UserManagmentController : Controller
    {
        private readonly ErosDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public INotyfService _notyfService { get; }
        public UserManagmentController(ErosDbContext context, UserManager<ApplicationUser> userManager, INotyfService notyfService)
        {
            _context = context;
            this._userManager = userManager;
            _notyfService = notyfService;
        }
        [HttpPost]
        public IActionResult CheckUserData(string tableData)
        {
            var results = new List<UserCheckResult>();

            //foreach (var user in tableData)
            //{
            //    var isFound = _context.UserManagement.Any(a => a.UserName.Trim() == user.Trim());
            //    results.Add(new UserCheckResult { UserName = user.UserName, IsFound = isFound });
            //}

            return Ok(results);
        }

       
        public class UserCheckResult
        {
            public string UserName { get; set; }
            public bool IsFound { get; set; }
        }


        // GET: UserManagements

        public async Task<IActionResult> Index()
        {
            ViewData["UserID"] = _userManager.GetUserId(this.User);

            var users = await _userManager.Users.Where(u => u.Email != "admin@gmail.com").ToListAsync();

            var results = users.Select(user => new UserCheckResult
            {
                UserName = user.UserName,
                IsFound = _context.UserManagement.Any(um => um.UserName.Trim() == user.UserName.Trim())
            }).ToList();

            ViewBag.MyList = results; // Pass the results instead of users

            return View();
        }


        //public async Task<IActionResult> Index()
        //{
        //    ViewData["UserID"] = _userManager.GetUserId(this.User);

        //    //var users = await _userManager.Users.ToListAsync(); // Using Entity Framework Core for async querying
        //    //return View(users);

        //    ////show registered user in list
        //    //var userNames = _context.Users.Select(x => x.UserName).ToList();
        //    //return View(userNames); 
        //    //  var userId = _userManager.Users.Select(x => x.Id).ToList();
        //    //var userNames = _userManager.Users.Select(u => u.UserName).ToList();

        //    ViewBag.MyList = _userManager.Users.Select(u => new
        //    //ViewBag.MyList = _userManager.Users.Select(u => new
        //    {
        //        UserName = u.UserName,
        //        Id = u.Id,
        //    }).ToList();
        //    return View();
        //}

        // GET: UserManagements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.UserManagement == null)
            {
                return NotFound();
            }
            var userManagement = await _context.UserManagement
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userManagement == null)
            {
                return NotFound();
            }
            return View(userManagement);
        }

        // GET: UserManagements/Create  -  get main menu
        private List<SelectListItem> GetMainMenu()
        {
            var lstProducts = new List<SelectListItem>();
            lstProducts = _context.MenuModel.Where(a=>a.ParentMenuId==0).AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.Title,
                Text = n.Title
            }).ToList();

            return lstProducts;
        }
        // GET: UserManagements/Create  -  get sub menu
        private List<SelectListItem> GetSubMenu()
        {
            var lstProducts = new List<SelectListItem>();
            lstProducts = _context.MenuModel.Where(a => a.ParentMenuId == 2).AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.Title,
                Text = n.Title
            }).ToList();

            return lstProducts;
        }
        // GET: UserManagements/Create  -  get opration menu
        private List<SelectListItem> GetOprationMenu()
        {
            var lstProducts = new List<SelectListItem>();
            lstProducts = _context.MenuModel.Where(a => a.ParentMenuId == 11).AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.Title,
                Text = n.Title
            }).ToList();

            return lstProducts;
        }
        private List<SelectListItem> GetReportMenu()
        {
            var lstProducts = new List<SelectListItem>();
            lstProducts = _context.MenuModel.Where(a => a.ParentMenuId == 36).AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.Title,
                Text = n.Title
            }).ToList();

            return lstProducts;
        }
        //create view show
        private List<SelectListItem> GetRole()
        {
            var lstProducts = new List<SelectListItem>();

            lstProducts = _context.usertype_Master.AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.designation,
                Text = n.designation
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select Role----"
            };

            lstProducts.Insert(0, defItem);

            return lstProducts;
        }
        public IActionResult Create(string? username)
        {
            //ViewBag.role= GetRole();
            //return View();

            ViewBag.role = GetRole();
            ViewBag.MainMenu = GetMainMenu();
            ViewBag.SubMenu = GetSubMenu();
            ViewBag.OprationMenu = GetOprationMenu();
            ViewBag.reportmenu = GetReportMenu();

            if (username == null || _context.UserManagement == null)
            {
                return NotFound();
            }
            
            var userManagement = _context.UserManagement.Where(a => a.UserName == username).FirstOrDefault();
            if (userManagement == null)
            {
                return View();
            }
            return View(userManagement);

        }

        // POST: UserManagements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string[] mainmenu, string[] submenu, string[] oprationmenu,
            string[] reportmenu, UserManagment userManagement)
        {
            
            var previewdata = _context.UserManagement.Where(a => a.UserName == userManagement.UserName).ToList();
          
            _context.RemoveRange(previewdata);
            
            _context.SaveChanges();

            // Add new records based on the fruitIds
            foreach (var fruit in mainmenu)
            {
                int maxId = _context.UserManagement.Any() ? _context.UserManagement.Max(e => e.Id) + 1 : 1;
                userManagement.Id = maxId;
                var newUserManagement = new UserManagment
                {
                    UserName = userManagement.UserName,
                    PageName = fruit,
                    Role = userManagement.Role,
                    Id = maxId,
                };

                _context.Add(newUserManagement);
                _context.SaveChanges();
            }

            foreach (var fruit in submenu)
            {
                int maxId = _context.UserManagement.Any() ? _context.UserManagement.Max(e => e.Id) + 1 : 1;
                userManagement.Id = maxId;
                var newUserManagement = new UserManagment
                {
                    UserName = userManagement.UserName,
                    PageName = fruit,
                    Role = userManagement.Role,
                    Id = maxId,
                };

                _context.Add(newUserManagement);
                _context.SaveChanges();
            }

            foreach (var fruit in oprationmenu)
            {
                int maxId = _context.UserManagement.Any() ? _context.UserManagement.Max(e => e.Id) + 1 : 1;
                userManagement.Id = maxId;
                var newUserManagement = new UserManagment
                {
                    UserName = userManagement.UserName,
                    PageName = fruit,
                    Role = userManagement.Role,
                    Id = maxId,
                };

                _context.Add(newUserManagement);
                _context.SaveChanges();
            }
            foreach (var fruit in reportmenu)
            {
                int maxId = _context.UserManagement.Any() ? _context.UserManagement.Max(e => e.Id) + 1 : 1;
                userManagement.Id = maxId;
                var newUserManagement = new UserManagment
                {
                    UserName = userManagement.UserName,
                    PageName = fruit,
                    Role = userManagement.Role,
                    Id = maxId,
                };

                _context.Add(newUserManagement);
                _context.SaveChanges();
            }

            //maintain logs
            //ViewData["UserID"] = _userManager.GetUserId(this.User);
            //string username = _userManager.GetUserName(this.User);
            //var logs = new Logs();
            //logs.pagename = "User Managment";
            //logs.action = "Create";
            //logs.task = "Allot Page Access";
            //logs.taskid = userManagement.Id;
            //logs.date = DateTime.Now.ToString("dd/MM/yyyy");
            //logs.time = DateTime.Now.ToString("HH:mm:ss");
            //logs.username = username;
            //_context.Add(logs);


            // Save changes to the database
            _notyfService.Success("Pages Alloted Successfully !");
            return RedirectToAction(nameof(Index));
        }

        // GET: UserManagements/Edit/5 : for view bag
        public async Task<IActionResult> Edit(string? username)
        {
            ViewBag.role = GetRole();
            ViewBag.MainMenu = GetMainMenu();
            ViewBag.SubMenu = GetSubMenu();
            ViewBag.OprationMenu = GetOprationMenu();
            ViewBag.reportmenu = GetReportMenu();

            if (username == null || _context.UserManagement == null)
            {
                return NotFound();
            }
            List<string>accesslist=_context.UserManagement.Where(a=>a.UserName == username).Select(a=>a.PageName).ToList();
            var role = _context.UserManagement.Where(a => a.UserName == username).Select(a => a.Role).FirstOrDefault();

            var usermanage = new UserManagment
            {
                UserName = username,
                PageName = "",
                selectedpages = accesslist,
                Role = role,

            };

            if (usermanage == null)
            {
                return View();
            }
            return View(usermanage);
        }

        // POST: UserManagements/Edit/5 : display edit page on login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string[] mainmenu, string[] submenu, string[] oprationmenu,
            string[] reportmenu, UserManagment userManagement)
        {
            
            var previewdata = _context.UserManagement.Where(a => a.UserName == userManagement.UserName).ToList();

            _context.RemoveRange(previewdata);

            _context.SaveChanges();

            // Add new records based on the fruitIds
            foreach (var fruit in mainmenu)
            {
                int maxId = _context.UserManagement.Any() ? _context.UserManagement.Max(e => e.Id) + 1 : 1;
                userManagement.Id = maxId;
                var newUserManagement = new UserManagment
                {
                    Id = maxId,
                    UserName = userManagement.UserName,
                    PageName = fruit,
                    Role = userManagement.Role,
                };

                _context.Add(newUserManagement);
                _context.SaveChanges();
            }

            foreach (var fruit in submenu)
            { 
                int maxId = _context.UserManagement.Any() ? _context.UserManagement.Max(e => e.Id) + 1 : 1;
                userManagement.Id = maxId;
                var newUserManagement = new UserManagment
                {
                    Id = maxId,
                    UserName = userManagement.UserName,
                    PageName = fruit,
                    Role = userManagement.Role,
                };

                _context.Add(newUserManagement);
            _context.SaveChanges();
            }

            foreach (var fruit in oprationmenu)
            {
                int maxId = _context.UserManagement.Any() ? _context.UserManagement.Max(e => e.Id) + 1 : 1;
                userManagement.Id = maxId;
                var newUserManagement = new UserManagment
                {
                    Id = maxId,
                    UserName = userManagement.UserName,
                    PageName = fruit,
                    Role = userManagement.Role,
                };

                _context.Add(newUserManagement);
                _context.SaveChanges();
            }
            foreach (var fruit in reportmenu)
            {
                int maxId = _context.UserManagement.Any() ? _context.UserManagement.Max(e => e.Id) + 1 : 1;
                userManagement.Id = maxId;
                var newUserManagement = new UserManagment
                {
                    Id = maxId,
                    UserName = userManagement.UserName,
                    PageName = fruit,
                    Role = userManagement.Role,
                };

                _context.Add(newUserManagement);
                _context.SaveChanges();
            }

            //maintain logs
            ViewData["UserID"] = _userManager.GetUserId(this.User);
            string username = _userManager.GetUserName(this.User);
            var logs = new Logs();
            logs.pagename = "User Managment";
            logs.action = "Create";
            logs.task = "Allot Page Access";
            logs.taskid = userManagement.Id;
            logs.date = DateTime.Now.ToString("dd/MM/yyyy");
            logs.time = DateTime.Now.ToString("HH:mm:ss");
            logs.username = username;
            _context.Add(logs);
            _context.SaveChanges();
            _notyfService.Success("Updated Pages Alloted Successfully !");
            // Save changes to the database

            return RedirectToAction(nameof(Index));
        }

        // GET: UserManagements/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(string username) // Changed parameter type from string? to string
        {
            if (string.IsNullOrEmpty(username)) // Check if username is null or empty
            {
                return NotFound();
            }

            var aspUser = await _userManager.FindByNameAsync(username); // Find the ASP.NET Core Identity user

            if (aspUser != null)
            {
                var userManagement = _context.UserManagement.Where(m => m.UserName == username).ToList();
                foreach (var item in userManagement)
                {
                    _context.UserManagement.Remove(item);
                }

                await _context.SaveChangesAsync(); // Save changes to the UserManagement table
                await _userManager.DeleteAsync(aspUser); // Delete the ASP.NET Core Identity user
            }

            return Json(new { success = true, message = "User Removed Successfully !" });
        }

        
        private bool UserManagementExists(int id)
        {
          return (_context.UserManagement?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
