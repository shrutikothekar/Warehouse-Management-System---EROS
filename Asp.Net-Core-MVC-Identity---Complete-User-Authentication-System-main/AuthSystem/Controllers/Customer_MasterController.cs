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
using AspNetCoreHero.ToastNotification.Notyf;
using System.Globalization;
using System.Text;


namespace eros.Controllers
{
    public class Customer_MasterController : Controller
    {
        private readonly ErosDbContext _context;
        public INotyfService _notyfService { get; }

        public Customer_MasterController(ErosDbContext context, INotyfService notyfService)
        {
            _notyfService = notyfService;
            _context = context;
        }

        // GET: Customer_Master
        public async Task<IActionResult> Index()
        {
            //return _context.Customer_Master != null ?
            //            View(await _context.Customer_Master.ToListAsync()) :
            //            Problem("Entity set 'ErosDbContext.Customer_Master'  is null.");
            var pendingOrders = _context.Customer_Master
       .OrderByDescending(a => a.customer_id)
       .ToList();

            return View(pendingOrders);
        }

        // GET: Customer_Master/Details/5


        [HttpGet]
        public IActionResult Details(int id)
        {
            try
            {
                Customer_Master customer_Master = _context.Customer_Master
                    .Include(e => e.Consignee_masters)
                    .Where(a => a.customer_id == id)
                    .FirstOrDefault();

                // Get the username from session
                var user = HttpContext.Session.GetString("User");

                // Create a new log entry
                var logs = new Logs();
                logs.pagename = "Customer Master";
                //logs.task = "Custome Master View";
                logs.action = "View";
                logs.taskid = id;
                logs.task = id.ToString() + "id , Customer_Master view";
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.username = user;
                _context.Add(logs);

                // Save the log entry to the database
                _context.SaveChanges();

                return View(customer_Master);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }


        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null || _context.Customer_Master == null)
        //    {
        //        return NotFound();
        //    }

        //    var customer_Master = await _context.Customer_Master
        //        .FirstOrDefaultAsync(m => m.customer_id == id);
        //    if (customer_Master == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(customer_Master);

        //}
        [HttpGet]
        public IActionResult GetStatesByCountry(string countryId)
        {
            if (string.IsNullOrEmpty(countryId))
                return BadRequest();

            var states = _context.states
                .Where(s => s.country.Name == countryId)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(), // You may need to adjust this based on your state model
                    Text = s.Name
                })
                .ToList();

            return Json(states);
        }

        [HttpGet]
        public IActionResult GetCitiesByState(string stateId)
        {
            if (string.IsNullOrEmpty(stateId))
                return BadRequest();

            var cities = _context.cities
                .Where(c => c.state.Id == int.Parse(stateId)) // You may need to adjust this based on your city model
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(), // You may need to adjust this based on your city model
                    Text = c.Name
                })
                .ToList();

            return Json(cities);
        }
        private List<SelectListItem> Getcountry()
        {
            var lstProducts = new List<SelectListItem>();

            lstProducts = _context.Countries.AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.Name,
                Text = n.Name
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select Country----"
            };

            lstProducts.Insert(0, defItem);

            return lstProducts;
        }
        public IActionResult Getcountry11()
        {
            var products = _context.Countries.Select(a => a.Name).Distinct().ToList(); // Fetch products from the database
            return Json(products); // Return products as JSON
        }
        private List<SelectListItem> Getstate()
        {
            var lstProducts = new List<SelectListItem>();

            lstProducts = _context.states.AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.Name,
                Text = n.Name
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select State----"
            };

            lstProducts.Insert(0, defItem);

            return lstProducts;
        }
        private List<SelectListItem> Getcity()
        {
            var lstProducts = new List<SelectListItem>();

            lstProducts = _context.cities.AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.Name,
                Text = n.Name
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select City----"
            };

            lstProducts.Insert(0, defItem);

            return lstProducts;
        }
        // GET: Customer_Master/Create
        public IActionResult Create()
        {
            ViewBag.city = Getcity();
            ViewBag.state = Getstate();
            ViewBag.country = Getcountry();
            Customer_Master customer_Master = new Customer_Master();
            customer_Master.Consignee_masters.Add(new consignee { id = 1 });
            return View(customer_Master);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer_Master customer_Master)
        {

            int maxId = _context.Customer_Master.Any() ? _context.Customer_Master.Max(e => e.customer_id) + 1 : 1;
            customer_Master.customer_id = maxId;
            customer_Master.customername = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(customer_Master.customername.ToLower());
            customer_Master.contactperson = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(customer_Master.contactperson.ToLower());
            customer_Master.address = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(customer_Master.address.ToLower());
            customer_Master.city = customer_Master.city.ToUpper();
            customer_Master.state = customer_Master.state.ToUpper();
            customer_Master.Country = customer_Master.Country.ToUpper();
            customer_Master.emailid = string.IsNullOrEmpty(customer_Master.emailid) ? "-" : customer_Master.emailid.ToLower();
            customer_Master.gstno = customer_Master.gstno.ToUpper();
            //if (customer_Master.emailid == null)
            //{
            //    customer_Master.emailid = "-";
            //}
            //else
            //{
            //    customer_Master.emailid = customer_Master.emailid.ToLower();
            foreach (var a in customer_Master.Consignee_masters)
            {
                //a.customerid = customer_Master.customer_id;
                //a.consigneeemail = a.consigneeemail.ToLower();
                //a.consigneecity = a.consigneeemail.ToUpper();
                //a.consigneestate = a.consigneeemail.ToUpper();
                a.customerid = customer_Master.customer_id;
                a.consigneeemail = string.IsNullOrEmpty(a.consigneeemail) ? "-" : a.consigneeemail.ToLower();
                a.consigneecity = string.IsNullOrEmpty(a.consigneecity) ? "-" : a.consigneecity.ToUpper();
                a.consigneestate = string.IsNullOrEmpty(a.consigneestate) ? "-" : a.consigneestate.ToUpper();
                a.consigneename = string.IsNullOrEmpty(a.consigneename) ? "-" : a.consigneename;
                a.consigneecontact = string.IsNullOrEmpty(a.consigneecontact) ? "-" : a.consigneecontact;
                a.consigneeperson = string.IsNullOrEmpty(a.consigneeperson) ? "-" : a.consigneeperson;
                a.consigneeaddress = string.IsNullOrEmpty(a.consigneeaddress) ? "-" : a.consigneeaddress;
                a.consigneecity = string.IsNullOrEmpty(a.consigneecity) ? "-" : a.consigneecity;
                a.consigneestate = string.IsNullOrEmpty(a.consigneestate) ? "-" : a.consigneestate;
                a.consigneepincode = string.IsNullOrEmpty(a.consigneepincode) ? "-" : a.consigneepincode;

            }
            _context.Add(customer_Master);
            //maintain logs
            var user = HttpContext.Session.GetString("User");
            var logs = new Logs();
            logs.pagename = "Customer Master";
            //logs.task = "Custome Master Create";
            logs.action = "Create";
            logs.taskid = maxId;
            logs.task = maxId.ToString() + "id , Customer_Master Create";
            logs.date = DateTime.Now.ToString("dd/MM/yyyy");
            logs.time = DateTime.Now.ToString("HH:mm:ss");
            logs.username = user;
            _context.Add(logs);
            await _context.SaveChangesAsync();
            _notyfService.Success("Customer details created successfully");

            return RedirectToAction(nameof(Index));
        }

        //call to add customer view
        //public IActionResult CreateViewCustomer()
        //{
        //    return PartialView("CreateViewCustomer");
        //}

        //public IActionResult CreateViewCustomer()
        //{
        //    Customer_Master customer_Master = new Customer_Master();
        //    customer_Master.Consignee_masters.Add(new consignee { id = 1 });
        //    return PartialView("CreateViewCustomer", customer_Master);
        //}

        //// POST: Create a new customer
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> CreateViewCustomer(Customer_Master customer_Master)
        //    { 

        //    _context.Add(customer_Master);
        //    _context.SaveChanges();
        //    _notyfService.Success("Customer Added Successfully");
        //    //TempData["SuccessMessage"] = "Record has been created successfully.";
        //    return RedirectToAction("Create", "so_inward");
        //    return View(customer_Master);

        //}
        public IActionResult CreateViewCustomer()
        {
            Customer_Master customer_Master = new Customer_Master();
            customer_Master.Consignee_masters.Add(new consignee { id = 1 });
            return PartialView("CreateViewCustomer", customer_Master);
        }

        // POST: Create a new customer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateViewCustomer(Customer_Master customer_Master)
        {
            _context.Add(customer_Master);
            await _context.SaveChangesAsync();
            //TempData["SuccessMessage"] = "Record has been created successfully.";
            return RedirectToAction("Create", "so_inward");
            return View(customer_Master);

        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult CreateViewCustomer(Customer_Master customer)
        //{
        //    //if (ModelState.IsValid)
        //    //{
        //        _context.Customer_Master.Add(customer);
        //        _context.SaveChanges();
        //        return RedirectToAction("Create", "so_inward");
        //    //}
        //        return View(customer);
        //}


        //public IActionResult CreateViewCustomer()
        //{
        //    Customer_Master customer_Master = new Customer_Master();
        //    customer_Master.Consignee_masters.Add(new consignee { id = 1 });
        //    return View(customer_Master);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> CreateViewCustomer(Customer_Master customer_Master)
        //{
        //    _context.Add(customer_Master);
        //    await _context.SaveChangesAsync();
        //    //return RedirectToAction(nameof(CreateViewCustomer));
        //    return View(customer_Master);
        //    return RedirectToAction("Create", "so_inward");
        //}
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            ViewBag.city = Getcity();
            ViewBag.state = Getstate();
            ViewBag.country = Getcountry();
            if (id == null || _context.Customer_Master == null)
            {
                return NotFound();
            }

            var customer_Master = await _context.Customer_Master
                .Include(a => a.Consignee_masters) // Include the Consignee_masters navigation property
                .FirstOrDefaultAsync(a => a.customer_id == id);

            if (customer_Master == null)
            {
                return NotFound();
            }

            return View(customer_Master);
        }

        // POST: purchases/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer_Master customer_Master)
        {
            if (id != customer_Master.customer_id)
            {
                return NotFound();
            }
            try
            {
                customer_Master.customername = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(customer_Master.customername.ToLower());
                customer_Master.contactperson = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(customer_Master.contactperson.ToLower());
                customer_Master.address = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(customer_Master.address.ToLower());
                customer_Master.city = customer_Master.city.ToUpper();
                customer_Master.state = customer_Master.state.ToUpper();
                customer_Master.Country = customer_Master.Country.ToUpper();
                customer_Master.emailid = string.IsNullOrEmpty(customer_Master.emailid) ? "-" : customer_Master.emailid.ToLower();
                customer_Master.gstno = customer_Master.gstno.ToUpper();

                foreach (var a in customer_Master.Consignee_masters)
                {
                    //a.customerid = customer_Master.customer_id;
                    //a.consigneeemail = a.consigneeemail.ToLower();
                    //a.consigneecity = a.consigneeemail.ToUpper();
                    //a.consigneestate = a.consigneeemail.ToUpper();
                    a.customerid = customer_Master.customer_id;
                    a.consigneeemail = string.IsNullOrEmpty(a.consigneeemail) ? "-" : a.consigneeemail.ToLower();
                    a.consigneecity = string.IsNullOrEmpty(a.consigneecity) ? "-" : a.consigneecity.ToUpper();
                    a.consigneestate = string.IsNullOrEmpty(a.consigneestate) ? "-" : a.consigneestate.ToUpper();
                    a.consigneename = string.IsNullOrEmpty(a.consigneename) ? "-" : a.consigneename;
                    a.consigneecontact = string.IsNullOrEmpty(a.consigneecontact) ? "-" : a.consigneecontact;
                    a.consigneeperson = string.IsNullOrEmpty(a.consigneeperson) ? "-" : a.consigneeperson;
                    a.consigneeaddress = string.IsNullOrEmpty(a.consigneeaddress) ? "-" : a.consigneeaddress;
                    a.consigneecity = string.IsNullOrEmpty(a.consigneecity) ? "-" : a.consigneecity;
                    a.consigneestate = string.IsNullOrEmpty(a.consigneestate) ? "-" : a.consigneestate;
                    a.consigneepincode = string.IsNullOrEmpty(a.consigneepincode) ? "-" : a.consigneepincode;

                }

                //Customer_Master.emailid = Customer_Master.emailid.ToUpper();
                List<consignee> Consignee_masters = _context.consignee.Where(e => e.customerid == id).ToList();
                _context.consignee.RemoveRange(Consignee_masters);
                _context.Update(customer_Master);
                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "Customer Master";
                //logs.task = "Update Customer master";
                logs.action = "Update";
                logs.taskid = id;
                logs.task= id.ToString() + "id , Customer_Master Update";
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.username = user;
                _context.Add(logs);
                _context.SaveChanges();
                _notyfService.Success("Customer details Updated Succesfully");

                return RedirectToAction("Index");

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Customer_MasterExists(customer_Master.customer_id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
            return View(customer_Master);
        }

        //public async Task<IActionResult> Edit(int id, Customer_Master customer_Master)
        //{
        //    if (id != customer_Master.customer_id)
        //    {
        //        return NotFound();
        //    }
        //    try
        //    {
        //        customer_Master.customername = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(customer_Master.customername.ToLower());
        //        customer_Master.contactperson = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(customer_Master.contactperson.ToLower());
        //        customer_Master.address = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(customer_Master.address.ToLower());
        //        customer_Master.city = customer_Master.city.ToUpper();
        //        customer_Master.state = customer_Master.state.ToUpper();
        //        customer_Master.Country = customer_Master.Country.ToUpper();
        //        customer_Master.emailid = string.IsNullOrEmpty(customer_Master.emailid) ? "-" : customer_Master.emailid.ToLower();
        //        customer_Master.gstno = customer_Master.gstno.ToUpper();

        //        foreach (var a in customer_Master.Consignee_masters)
        //        {        
        //            a.customerid = customer_Master.customer_id;
        //            a.consigneeemail = string.IsNullOrEmpty(a.consigneeemail) ? "-" : a.consigneeemail.ToLower();
        //            a.consigneecity = string.IsNullOrEmpty(a.consigneecity) ? "-" : a.consigneecity.ToUpper();
        //            a.consigneestate = string.IsNullOrEmpty(a.consigneestate) ? "-" : a.consigneestate.ToUpper();
        //            a.consigneename = string.IsNullOrEmpty(a.consigneename) ? "-" : a.consigneename;
        //            a.consigneecontact = string.IsNullOrEmpty(a.consigneecontact) ? "-" : a.consigneecontact;
        //            a.consigneeperson = string.IsNullOrEmpty(a.consigneeperson) ? "-" : a.consigneeperson;
        //            a.consigneeaddress = string.IsNullOrEmpty(a.consigneeaddress) ? "-" : a.consigneeaddress;
        //            a.consigneecity = string.IsNullOrEmpty(a.consigneecity) ? "-" : a.consigneecity;
        //            a.consigneestate = string.IsNullOrEmpty(a.consigneestate) ? "-" : a.consigneestate;
        //            a.consigneepincode = string.IsNullOrEmpty(a.consigneepincode) ? "-" : a.consigneepincode;
        //        }

        //        // Retrieve the existing entity from the context
        //        var existingCustomer = await _context.Customer_Master.FindAsync(id);
        //        if (existingCustomer == null)
        //        {
        //            return NotFound();
        //        }

        //        // Generate change log
        //        var changeLog = GenerateChangeLog(existingCustomer, customer_Master);

        //        // Remove any related consignees
        //        var consignees = _context.consignee.Where(e => e.customerid == id).ToList();
        //        _context.consignee.RemoveRange(consignees);

        //        // Detach existing entity from the context
        //        _context.Entry(existingCustomer).State = EntityState.Detached;

        //        // Update the customer_master entity
        //        _context.Update(customer_Master);

        //        // Maintain logs
        //        var user = HttpContext.Session.GetString("User");
        //        var logs = new Logs
        //        {
        //            pagename = "Customer Master",
        //            task = changeLog,
        //            action = "Update",
        //            taskid = id,
        //            date = DateTime.Now.ToString("dd/MM/yyyy"),
        //            time = DateTime.Now.ToString("HH:mm:ss"),
        //            username = user
        //        };
        //        _context.Add(logs);

        //        // Save changes
        //        await _context.SaveChangesAsync();

        //        _notyfService.Success("Customer details Updated Successfully");

        //        return RedirectToAction("Index");
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        // Handle concurrency exception
        //        ModelState.AddModelError(string.Empty, "Concurrency error occurred.");
        //        // Log the concurrency error or handle it as needed
        //        // For example, you can return a view with an error message
        //        return View(customer_Master); // Assuming you're returning to the edit view
        //    }
        //}
        private bool Customer_MasterExists(int id)
        {
            return _context.Courier_Master.Any(e => e.id == id);
        }
        //private string GenerateChangeLog(Customer_Master existingCourier, Customer_Master updatedCourier)
        //{
        //    StringBuilder changeLogBuilder = new StringBuilder();

        //    if (existingCourier.customername != updatedCourier.customername)
        //    {
        //        changeLogBuilder.Append($"{existingCourier.customername} -> {updatedCourier.customername} | ");
        //    }

        //    if (existingCourier.contactperson != updatedCourier.contactperson)
        //    {
        //        changeLogBuilder.Append($"{existingCourier.contactperson} -> {updatedCourier.contactperson} | ");
        //    }

        //    if (existingCourier.contactperson != updatedCourier.contactperson)
        //    {
        //        changeLogBuilder.Append($"{existingCourier.contactperson} -> {updatedCourier.contactperson} | ");
        //    }

        //    if (existingCourier.gstno != updatedCourier.gstno)
        //    {
        //        changeLogBuilder.Append($"{existingCourier.gstno} -> {updatedCourier.gstno} | ");
        //    }

        //    if (existingCourier.contactno != updatedCourier.contactno)
        //    {
        //        changeLogBuilder.Append($"{existingCourier.contactno} -> {updatedCourier.contactno} | ");
        //    }

        //    if (existingCourier.emailid != updatedCourier.emailid)
        //    {
        //        changeLogBuilder.Append($"{existingCourier.emailid} -> {updatedCourier.emailid} | ");
        //    }

        //    if (existingCourier.address != updatedCourier.address)
        //    {
        //        changeLogBuilder.Append($"{existingCourier.address} -> {updatedCourier.address} | ");
        //    }

        //    if (existingCourier.city != updatedCourier.city)
        //    {
        //        changeLogBuilder.Append($"{existingCourier.city} -> {updatedCourier.city}");
        //    }
        //    if (existingCourier.state != updatedCourier.state)
        //    {
        //        changeLogBuilder.Append($"{existingCourier.state} -> {updatedCourier.state}");
        //    }
        //    if (existingCourier.Country != updatedCourier.Country)
        //    {
        //        changeLogBuilder.Append($"{existingCourier.Country} -> {updatedCourier.Country}");
        //    }
        //    if (existingCourier.pincode != updatedCourier.pincode)
        //    {
        //        changeLogBuilder.Append($"{existingCourier.pincode} -> {updatedCourier.pincode}");
        //    }

        //    return changeLogBuilder.ToString();
        //}


        // GET: Customer_Master/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Customer_Master == null)
            {
                return NotFound();
            }

            var Customer_Master = await _context.Customer_Master
                .FirstOrDefaultAsync(m => m.customer_id == id);
            if (Customer_Master == null)
            {
                return NotFound();
            }

            return View(Customer_Master);
        }

        // POST: Customer_Master/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Customer_Master == null)
            {
                return Problem("Entity set 'ErosDbContext.Customer_Master'  is null.");
            }
            var customer_Master = await _context.Customer_Master.FindAsync(id);
            if (customer_Master != null)
            {
                _context.Customer_Master.Remove(customer_Master);
            }

            //maintain logs
            var user = HttpContext.Session.GetString("User");
            var logs = new Logs();
            logs.pagename = "customer Master";
            logs.task = customer_Master.customer_id + "$" + customer_Master.customername + "$" + customer_Master.contactperson + "$" + customer_Master.gstno + "$" + customer_Master.contactno + "$" + customer_Master.emailid + "$" + customer_Master.address + "$" + customer_Master.city+ "$" + customer_Master.state + "$" + customer_Master.Country+ "$" + customer_Master.pincode;
            logs.action = "Delete";
            //logs.task = id.ToString();
            logs.taskid = id;
            logs.date = DateTime.Now.ToString("dd/MM/yyyy");
            logs.time = DateTime.Now.ToString("HH:mm:ss");
            logs.username = user;
            _context.Add(logs);

            await _context.SaveChangesAsync();
            _notyfService.Error("Customer details deleted successfully");

            return RedirectToAction(nameof(Index));
        }

        //private bool Customer_MasterExists(int id)
        //{
        //    return (_context.Customer_Master?.Any(e => e.customer_id == id)).GetValueOrDefault();
        //}
    }
}
