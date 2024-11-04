using AspNetCoreHero.ToastNotification.Abstractions;
using AuthSystem.Data;
using eros.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Xml;

namespace eros.Controllers
{
    public class uomController : Controller
    {
        private readonly ErosDbContext _context;
        public INotyfService _notyfService { get; }

        public uomController(ErosDbContext context, INotyfService notyfService)
        {
            _context = context; _notyfService = notyfService;
        }

        //GET: uom - REAL
        public async Task<IActionResult> Index()
        {
            var listdata = _context.uom.ToList();
            return View(listdata);
        }

        //// GET: uom - WITH TALLY
        //public async Task<IActionResult> Index()
        //{
        //    string tallyUrl = "http://localhost:9000/";
        //    string xmlRequest = @"<ENVELOPE>
        //                    <HEADER>
        //                    <VERSION>1</VERSION>
        //                    <TALLYREQUEST>Export</TALLYREQUEST>
        //                    <TYPE>Collection</TYPE>
        //                    <ID>List of Ledgers</ID>
        //                    </HEADER>
        //                    <BODY>
        //                    <DESC>
        //                    <STATICVARIABLES /><TDL><TDLMESSAGE>
        //                    <COLLECTION ISMODIFY=""No"" ISFIXED=""No"" ISINITIALIZE=""Yes"" ISOPTION=""No"" ISINTERNAL=""No"" NAME=""List of Ledgers"">
        //                    <TYPE>Unit</TYPE>
        //                    <NATIVEMETHOD>Name</NATIVEMETHOD><NATIVEMETHOD>Parent</NATIVEMETHOD><NATIVEMETHOD>TAXTYPE</NATIVEMETHOD><NATIVEMETHOD>GSTDUTYHEAD</NATIVEMETHOD>
        //                    </COLLECTION></TDLMESSAGE></TDL>
        //                    </DESC>
        //                    </BODY>
        //                    </ENVELOPE>";

        //    using (HttpClient client = new HttpClient())
        //    {
        //        var content = new StringContent(xmlRequest, Encoding.UTF8, "application/xml");
        //        var response = await client.PostAsync(tallyUrl, content);
        //        string xmlResponse = await response.Content.ReadAsStringAsync(); // Fixed xmlResponse variable

        //        // Parse the XML response from Tally and convert it into a List<UOM>
        //        XmlDocument doc = new XmlDocument();
        //        doc.LoadXml(xmlResponse);


        //        ////DATA GETTTING START :
        //        //   <UNIT NAME="NOS" RESERVEDNAME="">
        //        //   <NAME TYPE="String"> NOS </NAME>
        //        //   </UNIT>
        //        ////DATA GETTING END ;

        //        var strArray = xmlResponse.Split(new string[] { "<UNIT NAME=" }, StringSplitOptions.RemoveEmptyEntries);
        //        var unitNames = new List<string>();

        //        foreach (var data in strArray)
        //        {
        //            if (data.Contains("RESERVEDNAME"))
        //            {
        //                var unitName = data.Substring(1, data.IndexOf("RESERVEDNAME") - 3).Replace("\"", "");
        //                unitNames.Add(unitName);
        //            }
        //        }

        //        // Get the highest existing primary key value
        //        var maxId = await _context.uom.MaxAsync(u => (int?)u.Id) ?? 0;

        //        // Check if each unit name exists in the database, if not, add it
        //        foreach (var unitName in unitNames)
        //        {
        //            var existingUnit = await _context.uom.FirstOrDefaultAsync(u => u.shortcut == unitName);
        //            if (existingUnit == null)
        //            {
        //                // Create a new unit and set the primary key value
        //                var newUnit = new uom
        //                {
        //                    Id = ++maxId,
        //                    shortcut = unitName,
        //                    Name = unitName, // Adjust this if needed
        //                };
        //                _context.uom.Add(newUnit);
        //            }
        //        }

        //        await _context.SaveChangesAsync();
        //        var units = await _context.uom.ToListAsync();
        //        return View(units);
        //    }
        //}


        // GET: uom/Details/5
        public async Task<IActionResult> Details(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }

            var uom = await _context.uom.FirstOrDefaultAsync(m => m.Id == Id);
            if (uom == null)
            {
                return NotFound();
            }

            // Maintain logs
            var user = HttpContext.Session.GetString("User");
            //if (user == null)
            //{
            //    // Redirect to login page or handle unauthorized access
            //    return RedirectToAction("Login", "Account");
            //}

            var logs = new Logs
            {
                pagename = "UOM Master",
                task = "View UOM master ",
                action = "View",
                taskid = Id.Value, // Convert.ToInt32(Id) is not necessary here since Id is already nullable int
                date = DateTime.Now.ToString("dd/MM/yyyy"),
                time = DateTime.Now.ToString("HH:mm:ss"),
                username = user
            };

            // Add logs to the context
            _context.Add(logs);

            // Save changes to the context
            await _context.SaveChangesAsync(); // Assuming _context is an EF DbContext

            return View(uom);
        }

        // GET: uom/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        public IActionResult uomnameCheck(string selectedValue)
        {
            var check = _context.uom.Where(a => a.Name.Trim().ToUpper() == selectedValue.ToUpper().Trim()).FirstOrDefault();
            if (check != null)
            {
                return Json(new { success = false, message = "Unit name is already exist !" });
            }
            else
            {

                return Json(new { success = true, selectedValue });
            }

        }
        public IActionResult uomcodeCheck(string selectedValue)
        {
            var check = _context.uom.Where(a => a.shortcut.Trim().ToUpper() == selectedValue.ToUpper().Trim()).FirstOrDefault();
            if (check != null)
            {
                return Json(new { success = false, message = "Unit code is already exist !" });
            }
            else
            {

                return Json(new { success = true, selectedValue });
            }

        }
        // POST: uom/Create  - REAL
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(uom uom)
        {
            if(uom.Name.Trim() == null ||  uom.shortcut.Trim() == null)
            {
                _notyfService.Warning("Please fill both unit name and unit code ! ");

            }
            else
            {
                int maxId = _context.uom.Any() ? _context.uom.Max(e => e.Id) + 1 : 1;
                uom.Id = maxId;

                _context.Add(uom);

                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "UOM Master";
                logs.task = "Create UOM master ";
                logs.action = "Create";
                logs.taskid = Convert.ToInt32(uom.Id);
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.username = user;
                _context.Add(logs);

                _context.SaveChanges();
                _notyfService.Success("Created Successfully ");
            }

            
            return RedirectToAction(nameof(Index));
        }

        //// POST: uom/Create  - WITH TALLY
        ////[HttpPost]
        ////[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(uom uom)
        //{
        //    int maxId = _context.uom.Any() ? _context.uom.Max(e => e.Id) + 1 : 1;
        //    uom.Id = maxId;

        //    _context.Add(uom);

        //    // Maintain logs
        //    var user = HttpContext.Session.GetString("User");
        //    var logs = new Logs
        //    {
        //        pagename = "UOM Master",
        //        task = "Create UOM master",
        //        action = "Create",
        //        taskid = Convert.ToInt32(uom.Id),
        //        date = DateTime.Now.ToString("dd/MM/yyyy"),
        //        time = DateTime.Now.ToString("HH:mm:ss"),
        //        username = user
        //    };
        //    _context.Add(logs);

        //    await _context.SaveChangesAsync();

        //    // Construct the XML request to add UOM into Tally
        //    string tallyUrl = "http://localhost:9000/";
        //    string xmlRequest = @"<ENVELOPE>" + "\n" +
        //    @"    <HEADER>" + "\n" +
        //    @"        <TALLYREQUEST>Import Data</TALLYREQUEST>" + "\n" +
        //    @"    </HEADER>" + "\n" +
        //    @"    <BODY>" + "\n" +
        //    @"        <IMPORTDATA>" + "\n" +
        //    @"            <REQUESTDESC>" + "\n" +
        //    @"                <REPORTNAME>All Masters</REPORTNAME>" + "\n" +
        //    @"            </REQUESTDESC>" + "\n" +
        //    @"            <REQUESTDATA>" + "\n" +
        //    @"                <TALLYMESSAGE" + "\n" +
        //    @"					xmlns:UDF=""TallyUDF"">" + "\n" +
        //    @"                    <UNIT NAME="""" ACTION=""CREATE"">" + "\n" +
        //    @"                        <NAME>" + uom.shortcut + "</NAME>" + "\n" +
        //    @"                       <ISSIMPLEUNIT>Yes</ISSIMPLEUNIT>" + "\n" +
        //    @"                       <FORPAYROLL>No</FORPAYROLL>" + "\n" +
        //    @"                     </UNIT>" + "\n" +
        //    @"                </TALLYMESSAGE>" + "\n" +
        //    @"            </REQUESTDATA>" + "\n" +
        //    @"        </IMPORTDATA>" + "\n" +
        //    @"    </BODY>" + "\n" +
        //    @"</ENVELOPE>";

        //    // Send the XML request to Tally
        //    using (HttpClient client = new HttpClient())
        //    {
        //        var content = new StringContent(xmlRequest, Encoding.UTF8, "application/xml");
        //        var response = await client.PostAsync(tallyUrl, content);
        //        string result = await response.Content.ReadAsStringAsync();

        //        // Optionally, you can check the response from Tally and act accordingly
        //        if (response.IsSuccessStatusCode)
        //        {
        //            _notyfService.Success("UOM created successfully in both the local database and Tally!");
        //        }
        //        else
        //        {
        //            _notyfService.Warning("UOM created locally but failed to add into Tally. Please try again.");
        //        }
        //    }

        //    return RedirectToAction(nameof(Index));
        //}


        // GET: uom/Edit/5
        public async Task<IActionResult> Edit(int? Id)
        {
            if (Id == null || _context.uom == null)
            {
                return NotFound();
            }

            var uom = await _context.uom.FindAsync(Id);
            if (uom == null)
            {
                return NotFound();
            }
            return View(uom);
        }

        // POST: uom/Edit/5- REAL

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, uom uom)
        {
            if (Id != uom.Id)
            {
                return NotFound();
            }
            try
            {
                _context.Update(uom);

                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "UOM Master";
                logs.task = uom.Id + "$" + uom.Name + "$" + uom.shortcut;
                logs.action = "Update";
                logs.taskid = Convert.ToInt32(uom.Id);
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.username = user;
                _context.Add(logs);

                await _context.SaveChangesAsync();
                _notyfService.Success("Updated Successfully");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!uomExists(uom.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            _notyfService.Success("Update Successfully ");
            return RedirectToAction(nameof(Index));

            //return View(uom);
        }

        // POST: uom/Edit/5- WITH TALLY
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int Id, uom uom)
        //{
        //    if (Id != uom.Id)
        //    {
        //        return NotFound();
        //    }
        //    try
        //    {
        //        string tallyUrl = "http://localhost:9000/";

        //        // Fetch the list of UOMs from Tally
        //        string xmlFetchRequest = @"<ENVELOPE>
        //                            <HEADER>
        //                                <VERSION>1</VERSION>
        //                                <TALLYREQUEST>Export</TALLYREQUEST>
        //                                <TYPE>Collection</TYPE>
        //                                <ID>List of UOMs</ID>
        //                            </HEADER>
        //                            <BODY>
        //                                <DESC>
        //                                    <STATICVARIABLES/>
        //                                    <TDL>
        //                                        <TDLMESSAGE>
        //                                            <COLLECTION ISMODIFY=""No"" ISFIXED=""No"" ISINITIALIZE=""Yes"" ISOPTION=""No"" ISINTERNAL=""No"" NAME=""List of UOMs"">
        //                                                <TYPE>Unit</TYPE>
        //                                                <NATIVEMETHOD>Name</NATIVEMETHOD>
        //                                                <NATIVEMETHOD>ISSIMPLEUNIT</NATIVEMETHOD>
        //                                                <NATIVEMETHOD>FORPAYROLL</NATIVEMETHOD>
        //                                            </COLLECTION>
        //                                        </TDLMESSAGE>
        //                                    </TDL>
        //                                </DESC>
        //                            </BODY>
        //                          </ENVELOPE>";

        //        using (HttpClient client = new HttpClient())
        //        {
        //            var content = new StringContent(xmlFetchRequest, Encoding.UTF8, "application/xml");
        //            var response = await client.PostAsync(tallyUrl, content);
        //            string xmlResponse = await response.Content.ReadAsStringAsync();

        //            // Parse the XML response from Tally
        //            XmlDocument doc = new XmlDocument();
        //            doc.LoadXml(xmlResponse);

        //            // Find the UOM to update
        //            var units = doc.GetElementsByTagName("UNIT");
        //            bool uomFound = false;

        //            foreach (XmlNode unit in units)
        //            {
        //                string name = unit.SelectSingleNode("NAME")?.InnerText;
        //                if (name.Trim() == uom.shortcut.Trim())
        //                {
        //                    uomFound = true;
        //                    break; // UOM found, exit loop
        //                }
        //            }

        //            if (!uomFound)
        //            {
        //                return NotFound("UOM not found in Tally.");
        //            }

        //            // If found, update the UOM in Tally
        //            string xmlUpdateRequest = $@"<ENVELOPE>
        //                                    <HEADER>
        //                                        <TALLYREQUEST>Import</TALLYREQUEST>
        //                                    </HEADER>
        //                                    <BODY>
        //                                        <IMPORTDATA>
        //                                            <REQUESTDESC>
        //                                                <REPORTNAME>All Masters</REPORTNAME>
        //                                            </REQUESTDESC>
        //                                            <REQUESTDATA>
        //                                                <TALLYMESSAGE xmlns:UDF=""TallyUDF"">
        //                                                    <UNIT NAME=""{uom.shortcut}"" ACTION=""Alter"">
        //                                                        <NAME>{uom.Name}</NAME>
        //                                                        <ISSIMPLEUNIT>Yes</ISSIMPLEUNIT>
        //                                                        <FORPAYROLL>No</FORPAYROLL>
        //                                                    </UNIT>
        //                                                </TALLYMESSAGE>
        //                                            </REQUESTDATA>
        //                                        </IMPORTDATA>
        //                                    </BODY>
        //                                </ENVELOPE>";

        //            try
        //            {
        //                var updateContent = new StringContent(xmlUpdateRequest, Encoding.UTF8, "application/xml");
        //                var updateResponse = await client.PostAsync(tallyUrl, updateContent);
        //                //string updateResult = await updateResponse.Content.ReadAsStringAsync();
        //                if (updateResponse.IsSuccessStatusCode)
        //                {
        //                    try
        //                    {
        //                        _context.Update(uom);
        //                        await _context.SaveChangesAsync();
        //                        _notyfService.Success("UOM updated successfully in both the local database and Tally.");
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        _notyfService.Warning(ex.Message);
        //                    }
        //                }
        //            }
        //            catch(Exception ex)
        //            {
        //                _notyfService.Warning(ex.Message);
        //            }



        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest("Error: " + ex.Message);
        //    }

        //    return RedirectToAction(nameof(Index));
        //}

        // GET: uom/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.uom == null)
            {
                return NotFound();
            }

            var uom = await _context.uom
                .FirstOrDefaultAsync(m => m.Id == id);
            if (uom == null)
            {
                return NotFound();
            }

            return View(uom);
        }

        // POST: uom/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int Id)
        {
            if (_context.uom == null)
            {
                return Problem("Entity set 'ErosDbContext.uom'  is null.");
            }
            var uom = await _context.uom.FindAsync(Id);
            if (uom != null)
            {
                //maintain logs
                var user = HttpContext.Session.GetString("User");
                var logs = new Logs();
                logs.pagename = "UOM Master";
                logs.task = uom.Id+"$"+ uom.Name+"$"+ uom.shortcut;
                //logs.task = "UOM master Delete";
                logs.action = "Delete";
                logs.taskid = Convert.ToInt32(uom.Id);
                logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                logs.time = DateTime.Now.ToString("HH:mm:ss");
                logs.username = user;
                _context.Add(logs);

                _context.uom.Remove(uom);
            }

            await _context.SaveChangesAsync();
            _notyfService.Error("Deleted Successfully");
            return RedirectToAction(nameof(Index));
        }

        private bool uomExists(int Id)
        {
            return (_context.uom?.Any(e => e.Id == Id)).GetValueOrDefault();
        }
    }
}
