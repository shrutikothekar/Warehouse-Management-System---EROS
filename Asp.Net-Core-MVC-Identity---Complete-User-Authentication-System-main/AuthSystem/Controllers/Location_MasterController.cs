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
using System.Drawing;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO;

namespace eros.Controllers
{
    public class Location_MasterController : Controller
    {
        private readonly ErosDbContext _context;
        public INotyfService _notyfService { get; }
        private IWebHostEnvironment _webHostEnvironment;
        //string printer = "IMPACT by Honeywell IH-2 (ZPL)";
        string printer = "IMPACT by Honeywell IH-2 (ZPL) (Copy 1)";
        String InstalledPrinters;
        Font printFont;
        StreamReader streamToPrint;

        public Location_MasterController(ErosDbContext context, INotyfService notyfService, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _notyfService = notyfService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Location_Master
        public async Task<IActionResult> Index()
        {
            List<Location_Master> locations = new List<Location_Master>();
            locations = _context.Location_Master.OrderByDescending(a => a.id).ToList();
            return View(locations);
        }

        // GET: Location_Master/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.RackItems = GenerateRackItems();
            ViewBag.FromShelfItems = GenerateFromShelfItems();
            ViewBag.ToShelfItems = GenerateToShelfItems((List<SelectListItem>)ViewBag.FromShelfItems);
            return View();
        }
        public List<SelectListItem> GenerateRackItems()
        {
            List<SelectListItem> rackItems = new List<SelectListItem>();
            for (char c = 'A'; c <= 'Z'; c++)
            {
                rackItems.Add(new SelectListItem { Text = c.ToString(), Value = c.ToString() });
            }

            // Add the "Add New Location" option
            //rackItems.Add(new SelectListItem { Text = "Add New Location", Value = "-1" });

            return rackItems;
        }

        //public List<SelectListItem> GenerateRackItems()
        //{
        //    List<SelectListItem> rackItems = new List<SelectListItem>();
        //    for (char c = 'A'; c <= 'Z'; c++)
        //    {
        //        rackItems.Add(new SelectListItem { Text = c.ToString(), Value = c.ToString() });
        //    }

        //    // Add the "Add New Location" option
        //    rackItems.Add(new SelectListItem { Text = "Add New Location", Value = "Add New Location" });

        //    // Add the temporary location option
        //    rackItems.Add(new SelectListItem { Text = "Temporary Location", Value = "Temporary Location" });

        //    return rackItems;
        //}


        public List<SelectListItem> GenerateFromShelfItems()
        {
            List<SelectListItem> fromShelfItems = new List<SelectListItem>();
            for (int i = 1; i <= 20; i++)
            {
                fromShelfItems.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            return fromShelfItems;
        }

        public List<SelectListItem> GenerateToShelfItems(List<SelectListItem> fromShelfItems)
        {
            int maxFromShelf = fromShelfItems.Max(x => int.Parse(x.Value));
            List<SelectListItem> toShelfItems = new List<SelectListItem>();
            for (int i = maxFromShelf + 1; i <= maxFromShelf + 10; i++) // Generating 10 numbers greater than the maximum from Shelf
            {
                toShelfItems.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            return toShelfItems;
        }


        // POST: Location_Master/Create
        //[HttpPost]
        //public IActionResult Create(Location_Master applicant)
        //{
        //    int maxId = _context.Location_Master.Any() ? _context.Location_Master.Max(e => e.id) + 1 : 1;
        //    applicant.id = maxId;

        //    List<string> locationCodes = new List<string>();

        //    // Generate location code based on rack, FromShelf, and ToShelf
        //    if (applicant.ToShelf == "0")
        //    {
        //        string locationCode = $"{applicant.Rack}-{applicant.FromShelf}";
        //        locationCodes.Add(locationCode);
        //    }
        //    else
        //    {
        //        for (int i = int.Parse(applicant.FromShelf); i <= int.Parse(applicant.ToShelf); i++)
        //        {
        //            string locationCode = $"{applicant.Rack}-{i}";
        //            locationCodes.Add(locationCode);
        //        }
        //    }

        //    // Pass both applicant and locationCodes to GenerateSticker method
        //    GenerateSticker(applicant, locationCodes);
        //    _context.Add(applicant);

        //    foreach (var i in locationCodes)
        //    {
        //        //maintain logs
        //        var user = HttpContext.Session.GetString("User");
        //        var logs = new Logs();
        //        logs.pagename = "Location Master";
        //        logs.task = applicant.id.ToString();
        //        //logs.task = i + " shelf Create in Location master ";
        //        logs.action = "Create";
        //        logs.taskid = Convert.ToInt32(applicant.id);
        //        logs.date = DateTime.Now.ToString("dd/MM/yyyy");
        //        logs.time = DateTime.Now.ToString("HH:mm:ss");
        //        logs.username = user;
        //        _context.Add(logs);
        //    }

        //    _context.SaveChanges();
        //    _notyfService.Success("Location Create Succesfully !");
        //    return RedirectToAction(nameof(Index));
        //}

        //private void GenerateSticker(Location_Master applicant, List<string> locationCodes)
        //{

        //    List<string> fileContents = new List<string>();

        //    string printprn = "";
        //    var path = $"{_webHostEnvironment.WebRootPath}\\Sticker\\location.prn";
        //    var value = $"{_webHostEnvironment.WebRootPath}\\Sticker\\locnvalue.prn";
        //    if (System.IO.File.Exists(path) == true)
        //    {
        //        foreach (string locationCode in locationCodes)
        //        {
        //            System.IO.File.Delete(value);
        //            System.IO.File.Copy(path, value);
        //            string value1 = $"{_webHostEnvironment.WebRootPath}\\Sticker\\StorageBox.prn";
        //            string fileContent = System.IO.File.ReadAllText(value1);
        //            fileContent = fileContent.Replace("<D001>", locationCode);
        //            System.IO.File.WriteAllText(value, fileContent);
        //            fileContents.Add(fileContent);
        //        }
        //        string finalContent = string.Join(Environment.NewLine, fileContents);
        //        System.IO.File.WriteAllText(value, finalContent);
        //        byte[] byteArray = Encoding.ASCII.GetBytes(finalContent);
        //        MemoryStream stream = new MemoryStream(byteArray);
        //        return File(stream, "text/plain", "example.prn");
        //        //return Ok(new { success = true, fileContent = fileContents, downloadUrl = "STB_Container_Sticker.prn" });
        //    }
        //}

        [HttpPost]
        public IActionResult Create(Location_Master applicant)
        {
            int maxId = _context.Location_Master.Any() ? _context.Location_Master.Max(e => e.id) + 0 : 0;
            applicant.id = maxId;

            List<string> fileContents = new List<string>();
            List<string> locationCodes = new List<string>();

            // Generate location code based on Rack, FromShelf, and ToShelf
            if (applicant.ToShelf == "0")
            {
                string locationCode = $"{applicant.Rack}-{applicant.FromShelf}";
                locationCodes.Add(locationCode);
            }
            else
            {
                for (int i = int.Parse(applicant.FromShelf); i <= int.Parse(applicant.ToShelf); i++)
                {
                    string locationCode = $"{applicant.Rack}-{i}";
                    locationCodes.Add(locationCode);
                }
            }

            // Generate sticker file content
            string stickerTemplatePath = $"{_webHostEnvironment.WebRootPath}\\Sticker\\location.prn";
            string tempStickerPath = $"{_webHostEnvironment.WebRootPath}\\Sticker\\locnvalue.prn";
            if (System.IO.File.Exists(stickerTemplatePath))
            {
                foreach (string locationCode in locationCodes)
                {
                    System.IO.File.Delete(tempStickerPath);
                    System.IO.File.Copy(stickerTemplatePath, tempStickerPath);

                    string storageBoxTemplatePath = $"{_webHostEnvironment.WebRootPath}\\Sticker\\location.prn";
                    string fileContent = System.IO.File.ReadAllText(storageBoxTemplatePath);
                    fileContent = fileContent.Replace("<D001>", locationCode);
                    System.IO.File.WriteAllText(tempStickerPath, fileContent);
                    fileContents.Add(fileContent);
                }
            }
            string finalContent = string.Join(Environment.NewLine, fileContents);
            System.IO.File.WriteAllText(tempStickerPath, finalContent);
            byte[] byteArray = Encoding.ASCII.GetBytes(finalContent);
            MemoryStream stream = new MemoryStream(byteArray);

            foreach (var locationCode in locationCodes)
            {
                maxId++;
                Location_Master location = new Location_Master()
                {
                    id = maxId,
                    Rack = applicant.Rack,
                    locationcode = locationCode,
                    FromShelf = applicant.FromShelf,
                    ToShelf = applicant.ToShelf,
                };
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs
                {
                    pagename = "Location Master",
                    task = applicant.id.ToString(),
                    action = "Create",
                    taskid = applicant.id,
                    date = DateTime.Now.ToString("dd/MM/yyyy"),
                    time = DateTime.Now.ToString("HH:mm:ss"),
                    username = user
                };
                _context.Add(logs);
                _context.Location_Master.Add(location);
                _context.SaveChanges();
            }

            _context.SaveChanges();
            return File(stream, "text/plain", "location_codes.prn");
        }

        // GET: Location_Master/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Location_Master == null)
            {
                return NotFound();
            }

            var location_Master = await _context.Location_Master
                .FirstOrDefaultAsync(m => m.id == id);
            if (location_Master == null)
            {
                return NotFound();
            }
            //maintain logs
            var user = HttpContext.Session.GetString("User");
            var logs = new Logs();
            logs.pagename = "Location Master";
            //logs.task = "View in Location master ";
            logs.task = id.ToString();
            logs.action = "View";
            logs.taskid = Convert.ToInt32(id);
            logs.date = DateTime.Now.ToString("dd/MM/yyyy");
            logs.time = DateTime.Now.ToString("HH:mm:ss");
            logs.username = user;
            _context.Add(logs);
            _context.SaveChanges();
            return View(location_Master);
        }




        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        // GET: Location_Master/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Location_Master == null)
            {
                return NotFound();
            }

            var location_Master = await _context.Location_Master.FindAsync(id);
            if (location_Master == null)
            {
                return NotFound();
            }
            return View(location_Master);
        }
        

        // POST: Location_Master/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,locationname,locationcode")] Location_Master location_Master)
        {
            if (id != location_Master.id)
            {
                return NotFound();
            }

              try
            {
                //location_Master.field1 = "-";
                //location_Master.field2 = "-";
                //location_Master.field3 = "-";
                //location_Master.field4 = "-";
                _context.Update(location_Master);
                    await _context.SaveChangesAsync();
                    //return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Location_MasterExists(location_Master.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            
            return View(location_Master);
        }

        // GET: Location_Master/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Location_Master == null)
            {
                return NotFound();
            }

            var location_Master = await _context.Location_Master
                .FirstOrDefaultAsync(m => m.id == id);
            if (location_Master == null)
            {
                return NotFound();
            }

            return View(location_Master);
        }

        // POST: Location_Master/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Location_Master == null)
            {
                return Problem("Entity set 'ErosDbContext.Location_Master'  is null.");
            }
            var location_Master = await _context.Location_Master.FindAsync(id);
            if (location_Master != null)
            {
                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "Location Master";
                //logs.task = "Remove shelf "+location_Master.Rack+" from Location master ";
                logs.task = location_Master.id + "$" + location_Master.Rack + "$" + location_Master.locationcode + "$" + location_Master.FromShelf + "$" + location_Master.ToShelf;
                logs.action = "Delete";
                logs.taskid = Convert.ToInt32(id);
                logs.task = id.ToString();
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.username = user;
                _context.Add(logs);
                _context.Location_Master.Remove(location_Master);
            }

            await _context.SaveChangesAsync();
            _notyfService.Error("Location delete Succesfully !");
            return RedirectToAction(nameof(Index));
        }

        private bool Location_MasterExists(int id)
        {
            return (_context.Location_Master?.Any(e => e.id == id)).GetValueOrDefault();
        }
    }
}
