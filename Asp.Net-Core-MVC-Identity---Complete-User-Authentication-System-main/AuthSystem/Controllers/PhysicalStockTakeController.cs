using AspNetCoreHero.ToastNotification.Abstractions;
using AuthSystem.Data;
using eros.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;
using System.Web;
using DocumentFormat.OpenXml.Office.CustomUI;
using ClosedXML.Excel;
using OfficeOpenXml;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Math;
using OfficeOpenXml.Style;
using DocumentFormat.OpenXml.Office2010.CustomUI;

namespace eros.Controllers
{
    public class PhysicalStockTakeController : Controller
    {

        private readonly ErosDbContext _context;
        public INotyfService _notyfService { get; }

        public PhysicalStockTakeController(ErosDbContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }
        private static List<PhysicalStockTake> Storage_OperationAdd = new List<PhysicalStockTake>();

        public async Task<IActionResult> Index()
        {
            if (_context.PhysicalStockTake == null)
            {
                return Problem("Entity set 'AuthDbContext.do_allotment' is null.");
            }
            string currentdate = DateTime.Now.Date.ToString();

            //var physicalstorage = await _context.PhysicalStockTake.Where(f => f.flag == 0).ToListAsync();

            //return _context.PhysicalStockTake != null ?
            //            View(await _context.PhysicalStockTake.Where(f => f.flag == 0).GroupBy(ps => ps.pstid)
            //            .Select(group => group.First()).ToListAsync()) :
            //            Problem("Entity set '_context.Physical_Stock'  is null.");

            var physicalstorage =
                       await _context.PhysicalStockTake.Where(f => f.flag == 0).GroupBy(ps => ps.physicalid)
                        .Select(group => new PhysicalStockTake
                        {
                            physicalid = group.Key,
                            boxcount = group.Count(),
                        }).ToListAsync();


            return View(physicalstorage);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public JsonResult CompareWithStorage(string locationCode, string productCode, string batchCode, string boxNo)
        {
            var existingProduct = _context.Storage_Operation.FirstOrDefault(p =>
                p.locationcode == locationCode &&
                p.productcode.ToUpper() == productCode.ToUpper() &&
                p.batchcode == batchCode &&
                p.boxno == boxNo);

            if (existingProduct != null)
            {
                // Data exists in the database and matches location
                return Json(new { flag = "Exist" });
            }
            else
            {
                // Data does not exist in the database or mismatched location
                return Json(new { flag = "Not Exist" });
            }
        }

        //public JsonResult CheckExistence(string productCode)
        //{
        //    var totalQuantity = _context.inwardPacket.Where(p =>        
        //    p.productcode == productCode).Sum(p=>p.quantity);

        //    if (totalQuantity > 0)
        //    {
        //        // Data exists in the database, and totalQuantity contains the calculated quantity
        //        return Json(new { flag = "Exist", quantity = totalQuantity });
        //    }
        //    else
        //    {
        //        // Data does not exist in the database
        //        return Json(new { flag = "Not Exist" });
        //    }
        //}


        [HttpPost]
        public IActionResult saveInSTList(string batch, string grn, string box, string loc, string pro)
        {
            var splitbox = box.Split("-");
            var firstpart = splitbox[0];
            var secondpart = splitbox[1];
            var splitsecand = splitbox[1].Split("/");
            var numerator = splitsecand[0];
            var denominator = splitsecand[1];

            if (Convert.ToInt64(numerator) > Convert.ToInt32(denominator))
            {
                var foundlist1 = Storage_OperationAdd
                .Where(a => a.productcode.Trim() == pro.Trim()
                            && a.batchcode.Trim() == batch.Trim()
                            && a.grnno.Trim() == grn.Trim()
                            && a.boxes.Trim() == box.Trim())
                .FirstOrDefault();
                if (foundlist1 != null)
                {
                    return Json(new { success = false, message = "Already Scanned !" });
                }
                else
                {
                    PhysicalStockTake st = new PhysicalStockTake()
                    {
                        productcode = pro,   // Assign product code
                        batchcode = batch,   // Assign batch code
                        grnno = grn,         // Assign GRN number
                        boxes = box,         // Assign box number
                    };
                    Storage_OperationAdd.Add(st);
                    return Json(new { success = true });
                }

            }
            else
            {
                var found = _context.PhysicalStockTake
                .Where(a => a.productcode.Trim() == pro.Trim()
                            && a.batchcode.Trim() == batch.Trim()
                            && a.grnno.Trim() == grn.Trim()
                            && a.boxes.Trim() == box.Trim()
                            && a.flag == 0)
                .FirstOrDefault();

                var foundlist = Storage_OperationAdd
                    .Where(a => a.productcode.Trim() == pro.Trim()
                                && a.batchcode.Trim() == batch.Trim()
                                && a.grnno.Trim() == grn.Trim()
                                && a.boxes.Trim() == box.Trim())
                    .FirstOrDefault();


                if (found != null || foundlist != null)
                {
                    return Json(new { success = false, message = "Already Scanned !" });
                }
                else
                {

                    PhysicalStockTake st = new PhysicalStockTake()
                    {
                        productcode = pro,   // Assign product code
                        batchcode = batch,   // Assign batch code
                        grnno = grn,         // Assign GRN number
                        boxes = box,         // Assign box number
                    };
                    Storage_OperationAdd.Add(st);
                    return Json(new { success = true });
                }
            }
            
        }


        [HttpPost]
        public ActionResult InsertCustomers(List<PhysicalStockTake> storage)
        {
            try
            {
                var checkout = _context.PhysicalStockTake.Where(a => a.flag == 0).ToList();
                var checkout1 = _context.checkphysicalstock.Where(a => a.flag == 0).ToList();
                if(checkout.Count > 0 || checkout1.Count> 0)
                {
                    return Json(new { success = false, message = "Please continue the existing PST data to proceed !" });
                }

                //int pstyear = DateTime.Now.Year;
                string pstdate = DateTime.Today.ToString("ddMMyyyy");
                int maxId = _context.PhysicalStockTake.Any() ? _context.PhysicalStockTake.Max(e => e.pstid) + 1 : 1;
                string result = $"{pstdate}{maxId}";
                string result1 = "PST" + pstdate + maxId.ToString();

                if (storage.Count > 0)
                {
                    foreach (PhysicalStockTake customer in storage)
                    {

                        customer.batchcode = customer.batchcode?.Replace("\u001e", "") ?? "";
                        customer.productcode = customer.productcode?.Replace("\u001e", "") ?? "";
                        customer.grnno = customer.grnno?.Replace("\u001e", "") ?? "";
                        customer.boxes = customer.boxes?.Replace("\u001e", "") ?? "";

                        //var splitbox1 = customer.boxes.Split("-");
                        //var firstpart = splitbox1[0];
                        //var secondpart = splitbox1[1];
                        //var splitsecand = splitbox1[1].Split("/");
                        //var numerator = splitsecand[0];
                        //var denominator = splitsecand[1];

                        //if (Convert.ToInt64(numerator) > Convert.ToInt64(denominator))
                        //{
                        //    List<string> list = new List<string>();
                        //    var qty = 1;
                        //    var box = 1;
                        //    for (int i = 1; i <= Convert.ToInt64(numerator); i++)
                        //    {
                        //        var data = i + "-" + qty + "/" + box;
                        //        list.Add(data);
                        //    }
                        //    int count = 0;
                        //    foreach (var boxes in list)
                        //    {
                        //        count++;
                        //        customer.storageflag = "ST";

                        //        int maxiddd = _context.PhysicalStockTake.Any() ? _context.PhysicalStockTake.Max(e => e.Id) + 1 : 1;

                        //        var splitBox = boxes.Split('-');
                        //        splitBox[0] = count.ToString();

                        //        var physicalstock = new PhysicalStockTake();
                        //        physicalstock.pstid = maxiddd;
                        //        physicalstock.productcode = customer.productcode.ToUpper();
                        //        physicalstock.batchcode = customer.batchcode;
                        //        physicalstock.locationcode = customer.locationcode;
                        //        physicalstock.boxes = string.Join("-", splitBox).Trim(); ;
                        //        physicalstock.storageflag = customer.storageflag;
                        //        physicalstock.grnno = customer.grnno;
                        //        physicalstock.physicalid = result1;

                        //        try
                        //        {
                        //            _context.Add(physicalstock);
                        //            _context.SaveChanges();
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            return Json(new { success = false, message = "Catch Exception : " + ex.Message });
                        //        }

                        //    }
                        //}
                        var splitbox1 = customer.boxes.Split("-");
                        var firstpart = splitbox1[0];
                        var secondpart = splitbox1[1];
                        var splitsecand = splitbox1[1].Split("/");
                        var numerator = splitsecand[0];
                        var denominator = splitsecand[1];

                        if (Convert.ToInt64(numerator) > Convert.ToInt64(denominator))
                        {
                            int counter = 1;
                            var chekkout = _context.PhysicalStockTake
                                .Where(a => a.productcode.Trim() == customer.productcode.Trim()
                                            && a.batchcode.Trim() == customer.batchcode.Trim()
                                            && a.grnno.Trim() == customer.grnno.Trim())
                                .ToList();

                            List<string> list = new List<string>();
                            var qty = 1;
                            var box = 1;

                            if (chekkout.Count > 0)
                            {
                                counter = chekkout.Count;
                                for (int i = 1; i <= Convert.ToInt64(numerator); i++)
                                {
                                    counter++;
                                    // Create the data string based on loop index, quantity, and box number
                                    var data = counter.ToString() + "-" + qty + "/" + box;
                                    list.Add(data);
                                }
                            }
                            else
                            {
                                for (int i = counter; i <= Convert.ToInt64(numerator); i++)
                                {
                                    // Create the data string based on loop index, quantity, and box number
                                    var data = i.ToString() + "-" + qty + "/" + box;
                                    list.Add(data);
                                }
                            }

                            int count = 0;
                            foreach (var boxes in list)
                            {
                                count++;
                                customer.storageflag = "ST";

                                int maxiddd = _context.PhysicalStockTake.Any() ? _context.PhysicalStockTake.Max(e => e.Id) + 1 : 1;

                                //var splitBox = boxes.Split('-');
                                //splitBox[0] = count.ToString();

                                var physicalstock = new PhysicalStockTake();
                                physicalstock.pstid = maxiddd;
                                physicalstock.productcode = customer.productcode.ToUpper();
                                physicalstock.batchcode = customer.batchcode;
                                physicalstock.locationcode = customer.locationcode;
                                physicalstock.boxes = boxes.Trim() ;
                                //physicalstock.boxes = string.Join("-", splitBox).Trim(); ;
                                physicalstock.storageflag = customer.storageflag;
                                physicalstock.grnno = customer.grnno;
                                physicalstock.physicalid = result1;

                                try
                                {
                                    _context.Add(physicalstock);
                                    _context.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    return Json(new { success = false, message = "Catch Exception : " + ex.Message });
                                }

                            }


                        }
                        else
                        {

                            var physicalstock = new PhysicalStockTake();
                            physicalstock.pstid = maxId;
                            physicalstock.productcode = customer.productcode.ToUpper();
                            physicalstock.batchcode = customer.batchcode;
                            physicalstock.locationcode = customer.locationcode;
                            physicalstock.boxes = customer.boxes;
                            physicalstock.storageflag = customer.storageflag;
                            physicalstock.grnno = customer.grnno;
                            physicalstock.physicalid = result1;

                            try
                            {
                                _context.Add(physicalstock);
                                _context.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                return Json(new { success = false, message = "While saving : " + ex.Message });
                            }

                        }
                    }


                    //return RedirectToAction(nameof(Index));
                    Storage_OperationAdd.Clear();
                    return Json(new { success = true, message = "PST Data save successfully !" });
                }
                else
                {
                    //// Display an alert when 'storage' is null
                    //string alertMessage = "No data provided.";
                    //// Replace this with your preferred JavaScript alert function
                    //// Here's an example using SweetAlert
                    //string script = $"Swal.fire({{ icon: 'error', title: 'Error', text: '{alertMessage}' }});";
                    //return Content(script, "application/javascript");
                    // Return a JSON response indicating failure
                    return Json(new { success = false, message = "Scan the boxes count is " + storage.Count + " only !" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }


        [HttpPost]
        public IActionResult DeleteFromList(string productCode, string batchCode, string box, string grnno)
        {
            var splitbox1 = box.Split("-");
            var firstpart = splitbox1[0];
            var secondpart = splitbox1[1];
            var splitsecand = splitbox1[1].Split("/");
            var numerator = splitsecand[0];
            var denominator = splitsecand[1];

            if (Convert.ToInt64(numerator) > Convert.ToInt64(denominator))
            {
                return Json(new { success = false, message = "Functionality Not defined !" });
            }
            else
            {
                var found = Storage_OperationAdd
                       .Where(a => a.productcode.Trim().ToUpper() == productCode.Trim().ToUpper()
                       && a.grnno.Trim() == grnno.Trim()
                       && a.batchcode.Trim() == batchCode.Trim()
                       && a.boxes.Trim() == box.Trim())
                       .FirstOrDefault();

                if (found != null)
                {
                    Storage_OperationAdd.Remove(found);
                    return Json(new { success = true, data = found });
                }
                else
                {
                    return Json(new { success = false, message = "Shipper Not Found !" });
                }
            }
               
        }

        [HttpPost]
       
        public ActionResult update(List<PhysicalStockTake> storage)
        {
            try
            {
                if (storage.Count > 0)
                {
                    foreach (PhysicalStockTake customer in storage)
                    {
                        //if (customer.productcode.Contains(''))
                        //{
                        //    customer.productcode = customer.productcode.Replace("\u001e", "").ToUpper();
                        //}
                        // Clean unwanted ASCII character (ASCII 30 - '\u001e') from fields
                        customer.batchcode = customer.batchcode?.Replace("\u001e", "") ?? "";
                        customer.productcode = customer.productcode?.Replace("\u001e", "") ?? "";
                        customer.grnno = customer.grnno?.Replace("\u001e", "") ?? "";
                        customer.boxes = customer.boxes?.Replace("\u001e", "") ?? "";


                        //var splitbox1 = customer.boxes.Split("-");
                        //var firstpart = splitbox1[0];
                        //var secondpart = splitbox1[1];
                        //var splitsecand = splitbox1[1].Split("/");
                        //var numerator = splitsecand[0];
                        //var denominator = splitsecand[1];
                        //if (Convert.ToInt64(numerator) > Convert.ToInt64(denominator))
                        //{
                        //    List<string> list = new List<string>();
                        //    var qty = 1;
                        //    var box = 1;
                        //    for (int i = 1; i <= Convert.ToInt64(numerator); i++)
                        //    {
                        //        var data = i + "-" + qty + "/" + box;
                        //        list.Add(data);
                        //    }
                        //    int count = 0;
                        //    foreach (var boxes in list)
                        //    {
                        //        count++;
                        //        customer.storageflag = "ST";

                        //        int maxiddd = _context.PhysicalStockTake.Any() ? _context.PhysicalStockTake.Max(e => e.Id) + 1 : 1;

                        //        var splitBox = boxes.Split('-');
                        //        splitBox[0] = count.ToString();

                        //        var physicalstock = new PhysicalStockTake();
                        //        physicalstock.pstid = maxiddd;
                        //        physicalstock.productcode = customer.productcode.ToUpper();
                        //        physicalstock.batchcode = customer.batchcode;
                        //        physicalstock.locationcode = customer.locationcode;
                        //        physicalstock.boxes = string.Join("-", splitBox).Trim(); ;
                        //        physicalstock.storageflag = customer.storageflag;
                        //        physicalstock.grnno = customer.grnno;
                        //        physicalstock.physicalid = customer.physicalid;

                        //        try
                        //        {
                        //            _context.Add(physicalstock);
                        //            _context.SaveChanges();
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            return Json(new { success = false, message = "Catch Exception : " + ex.Message });
                        //        }

                        //    }
                        //}
                        var splitbox1 = customer.boxes.Split("-");
                        var firstpart = splitbox1[0];
                        var secondpart = splitbox1[1];
                        var splitsecand = splitbox1[1].Split("/");
                        var numerator = splitsecand[0];
                        var denominator = splitsecand[1];

                        if (Convert.ToInt64(numerator) > Convert.ToInt64(denominator))
                        {
                            int counter = 1;
                            var chekkout = _context.PhysicalStockTake
                                .Where(a => a.productcode.Trim() == customer.productcode.Trim()
                                            && a.batchcode.Trim() == customer.batchcode.Trim() 
                                            && a.grnno.Trim() == customer.grnno.Trim())
                                .ToList();

                            List<string> list = new List<string>();
                            var qty = 1;
                            var box = 1;

                            if (chekkout.Count > 0)
                            {
                                counter = chekkout.Count;
                                for (int i = 1; i <= Convert.ToInt64(numerator); i++)
                                {
                                    counter++;
                                    // Create the data string based on loop index, quantity, and box number
                                    var data = counter.ToString() + "-" + qty + "/" + box;
                                    list.Add(data);
                                }
                            }
                            else
                            {
                                for (int i = counter; i <= Convert.ToInt64(numerator); i++)
                                {
                                    // Create the data string based on loop index, quantity, and box number
                                    var data = i.ToString() + "-" + qty + "/" + box;
                                    list.Add(data);
                                }
                            }

                            int count = 0;
                            foreach (var boxes in list)
                            {
                                count++;
                                customer.storageflag = "ST";

                                int maxiddd = _context.PhysicalStockTake.Any() ? _context.PhysicalStockTake.Max(e => e.Id) + 1 : 1;

                                //var splitBox = boxes.Split('-');
                                //splitBox[0] = count.ToString();

                                var physicalstock = new PhysicalStockTake();
                                physicalstock.pstid = maxiddd;
                                physicalstock.productcode = customer.productcode.ToUpper();
                                physicalstock.batchcode = customer.batchcode;
                                physicalstock.locationcode = customer.locationcode;
                                physicalstock.boxes = boxes.Trim(); 
                                //physicalstock.boxes = string.Join("-", splitBox).Trim(); 
                                physicalstock.storageflag = customer.storageflag;
                                physicalstock.grnno = customer.grnno;
                                physicalstock.physicalid = customer.physicalid;

                                try
                                {
                                    _context.Add(physicalstock);
                                    _context.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    return Json(new { success = false, message = "Catch Exception : " + ex.Message });
                                }

                            }


                        }
                        else
                        {

                            // Prepare the new PhysicalStockTake entity
                            var physicalstock = new PhysicalStockTake
                            {
                                physicalid = customer.physicalid,
                                productcode = customer.productcode.ToUpper(),
                                batchcode = customer.batchcode,
                                locationcode = customer.locationcode,
                                boxes = customer.boxes,
                                storageflag = customer.storageflag,
                                grnno = customer.grnno
                            };

                            // Check if the record already exists in the database
                            var existingStock = _context.PhysicalStockTake
                                .Where(a => a.productcode.ToUpper().Trim() == customer.productcode.ToUpper().Trim()
                                         && a.batchcode.Trim() == customer.batchcode.Trim()
                                         && a.grnno.Trim() == customer.grnno.Trim()
                                         && a.boxes.Trim() == customer.boxes.Trim())
                                .FirstOrDefault();

                            // If no existing record is found, add a new entry
                            if (existingStock == null)
                            {
                                try
                                {
                                    _context.Add(physicalstock);
                                    _context.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    return Json(new { success = false, message = "Error while saving: " + ex.Message, details = ex.StackTrace });
                                }
                            }
                            else
                            {
                                // You could update the existing record here if necessary, otherwise leave empty
                                // Example: Update existing record
                                // existingStock.locationcode = customer.locationcode;
                                // _context.SaveChanges();
                            }
                        }


                    }

                    return Json(new { success = true, message = "PST Data saved successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "No data to update, count is " + storage.Count + " only!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        //public IActionResult Pst_Excel(string id)
        //{

        //    //var physical_Stock = _context.PhysicalStockTake.Where(x => x.id == id).Select(x => x.field1).FirstOrDefault();

        //    var exporttoexcel = _context.checkphysicalstock.Where(x => x.physicalid == id).ToList();

        //    if (exporttoexcel == null)
        //    {
        //        return NotFound();
        //    }
        //    using (var excelPackage = new ExcelPackage())
        //    {
        //        // Create a worksheet
        //        ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("PST Stock");

        //        // Define headers
        //        worksheet.Cells[1, 1].Value = "PST Id";
        //        worksheet.Cells[1, 2].Value = "Product Code";
        //        worksheet.Cells[1, 3].Value = "Physical Location";
        //        worksheet.Cells[1, 4].Value = "Boxes";
        //        worksheet.Cells[1, 5].Value = "BatchCode";
        //        //worksheet.Cells[1, 6].Value = "System Meter";


        //        // Add more headers as needed
        //        // Apply bold formatting to the header row
        //        using (var range = worksheet.Cells["A1:K1"])
        //        {
        //            range.Style.Font.Bold = true;
        //        }

        //        // Populate data
        //        int row = 2;
        //        foreach (var postProcess in exporttoexcel)
        //        {
        //            worksheet.Cells[row, 1].Value = postProcess.physicalid;
        //            worksheet.Cells[row, 2].Value = postProcess.productcode;
        //            worksheet.Cells[row, 3].Value = postProcess.locationcode;
        //            worksheet.Cells[row, 4].Value = postProcess.boxes;
        //            worksheet.Cells[row, 5].Value = postProcess.batchcode;

        //            // worksheet.Cells[row, 6].Value = postProcess.shade_descript;

        //            // Add more data fields as needed
        //            row++;
        //        }

        //        // Auto-fit columns for better readability
        //        worksheet.Cells.AutoFitColumns();

        //        // Generate the file content
        //        byte[] fileContents = excelPackage.GetAsByteArray();

        //        // Return the Excel file
        //        return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Pst_Stock.xlsx");

        //    }
        //    // return View(physical_Stock);
        //}

        public IActionResult Pst_Excel(string id)
        {
            // Fetch the data from your context based on the physicalid (assuming it's the PST Id)
            var exporttoexcel = _context.checkphysicalstock.Where(x => x.physicalid == id).ToList();

            if (exporttoexcel == null || !exporttoexcel.Any())
            {
                return NotFound();
            }

            using (var excelPackage = new ExcelPackage())
            {
                // Create a worksheet
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("PST Stock");

                // Header Information
                worksheet.Cells["A1:E1"].Merge = true; // Merge cells for the title
                worksheet.Cells["A1"].Value = "EROS GENERAL AGENCIES PVT. LTD.";
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.Font.Size = 14;
                worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Apply borders to the merged title cell
                using (var range = worksheet.Cells["A1:E1"])
                {
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }

                // Add Excel generation timestamp
                worksheet.Cells["A2:E2"].Merge = true;
                worksheet.Cells["A2"].Value = "Excel generated on " + DateTime.Now.ToString("dd/MM/yyyy") + " at " + DateTime.Now.ToString("HH:mm:ss");
                worksheet.Cells["A2"].Style.Font.Bold = false;
                worksheet.Cells["A2"].Style.Font.Size = 10;
                worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Apply borders to the merged timestamp cell
                using (var range = worksheet.Cells["A2:E2"])
                {
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }

                // Define column headers for the table
                worksheet.Cells[4, 1].Value = "PST Id";
                worksheet.Cells[4, 2].Value = "Product Code";
                worksheet.Cells[4, 3].Value = "Physical Location";
                worksheet.Cells[4, 4].Value = "Boxes";
                worksheet.Cells[4, 5].Value = "BatchCode";

                // Apply bold formatting to the header row and set borders
                using (var range = worksheet.Cells["A4:E4"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Apply border to each cell in the header row
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }

                // Populate data and apply borders
                int row = 5; // Data starts from row 5
                foreach (var postProcess in exporttoexcel)
                {
                    worksheet.Cells[row, 1].Value = postProcess.physicalid;    // PST Id
                    worksheet.Cells[row, 2].Value = postProcess.productcode;   // Product Code
                    worksheet.Cells[row, 3].Value = postProcess.locationcode;  // Physical Location
                    worksheet.Cells[row, 4].Value = postProcess.boxes;         // Boxes
                    worksheet.Cells[row, 5].Value = postProcess.batchcode;     // BatchCode

                    // Center align the Product Code column (Column B) for the current row
                    worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Apply borders to each cell in the current row
                    for (int col = 1; col <= 5; col++)
                    {
                        var cell = worksheet.Cells[row, col];
                        cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }

                    row++;
                }

                // Auto-fit columns for better readability
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Generate the file content
                byte[] fileContents = excelPackage.GetAsByteArray();

                // Return the Excel file
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Pst_Stock.xlsx");
            }
        }


        public IActionResult Resumepst(string id)
        {

            var physical_Stock = _context.PhysicalStockTake.Where(x => x.physicalid == id).FirstOrDefault();
            //var physical_stock = physical_Stock.pstid;
            if (physical_Stock == null)
            {
                return NotFound();
            }
            return View(physical_Stock);
        }

        public async Task<IActionResult> Details(string? id)
        {
            if (id == null || _context.PhysicalStockTake == null)
            {
                return NotFound();
            }

            var PhysicalStockTake = await _context.PhysicalStockTake
                .Where(m => m.physicalid == id).ToListAsync();

            if (PhysicalStockTake == null)
            {
                return NotFound();
            }

            return View(PhysicalStockTake);
        }

        public async Task<IActionResult> checkphysicaldetails(string? id)
        {
            if (id == null || _context.checkphysicalstock == null)
            {
                return NotFound();
            }

            var checkphysicalstock = await _context.checkphysicalstock
                .Where(m => m.physicalid == id).ToListAsync();

            if (checkphysicalstock == null)
            {
                return NotFound();
            }

            return View(checkphysicalstock);
        }

        // GET: Storage_Operation/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PhysicalStockTake == null)
            {
                return NotFound();
            }

            var PhysicalStockTake = await _context.PhysicalStockTake.FindAsync(id);
            if (PhysicalStockTake == null)
            {
                return NotFound();
            }
            return View(PhysicalStockTake);
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (_context.PhysicalStockTake == null)
            {
                return Problem("Entity set 'AuthDbContext.PhysicalStockTake'  is null.");
            }
            var supplier = _context.PhysicalStockTake.Where(a => a.Id == id).FirstOrDefault(); ;
            if (supplier != null)
            {
                _context.PhysicalStockTake.Remove(supplier);
            }

            await _context.SaveChangesAsync();
            _notyfService.Error("Record Deleted Succesfully");
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PhysicalStockTake physicalStockTake)
        {
            if (id != physicalStockTake.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(physicalStockTake);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhysicalStockTakeExists(physicalStockTake.Id))
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
            return View(physicalStockTake);
        }
        private bool PhysicalStockTakeExists(int id)
        {
            return (_context.Storage_Operation?.Any(e => e.id == id)).GetValueOrDefault();
        }



        //public IActionResult VarianceReport()
        //{

        //    try
        //    {
        //        //Get data from the PhysicalStock and StorageOperation tables            
        //        //var physicalStockData = _context.checkphysicalstock.Where(c=>c.date==currentdate).ToList();
        //        var physicalStockData = _context.checkphysicalstock.Where(f => f.flag == 0).ToList();
        //        var storageOperationData = _context.Storage_Operation.ToList();

        //        //Create a list to store the matched data with remarks
        //        var matchedDataWithRemarks = new List<MatchedData>();

        //        foreach (var physicalStockItem in physicalStockData)
        //        {
        //            //Check if productcode in storageOperationData
        //            var matchingStorageItem = storageOperationData.Where(s => s.productcode.ToUpper() == physicalStockItem.productcode.ToUpper());

        //            if (matchingStorageItem.Any())
        //            {
        //                //var storageop = storageOperationData.Where(s => s.productcode == physicalStockItem.productcode);
        //                //Reterive the list of matchingStorageItem(storage operation)
        //                var storageop = matchingStorageItem.ToList();

        //                var boxstorage = storageop.Select(s => new { loc = s.locationcode, product = s.productcode, box = s.boxno.Split('-')[1], batch = s.batchcode });

        //                var boxCountByProductAndLocation = boxstorage
        //                          .GroupBy(item => new { Location = item.loc, Product = item.product, Batch = item.batch })
        //                          .Select(group => new
        //                          {
        //                              Location = group.Key.Location,
        //                              Product = group.Key.Product,
        //                              Batch = group.Key.Batch,
        //                              BoxCount = group.Count()
        //                          }).ToList();


        //                // Check if the product is already processed
        //                //if (matchedDataWithRemarks.Any(matchedData => matchedData.PhysicalStockItem != null && matchedData.PhysicalStockItem.productcode == physicalStockItem.productcode))
        //                if (matchedDataWithRemarks.Any(matchedData => matchedData.PhysicalStockItem.productcode.ToUpper() == physicalStockItem.productcode.ToUpper()))
        //                {
        //                    // If the product is already added, skip to the next iteration
        //                    continue;
        //                }

        //                //check existence of product in checkphysicalstock
        //                var checkpst = physicalStockData.Where(s => s.productcode.ToUpper() == physicalStockItem.productcode.ToUpper() && s.flag == 0);

        //                var checkbox = checkpst.Select(s => new { loc = s.locationcode, product = s.productcode, Box = s.boxes.Split('-')[1], Batch = s.batchcode, PSTcode = s.physicalid });

        //                var checkboxcount = checkbox.GroupBy(item => new { Location = item.loc, Product = item.product, Batch = item.Batch, PSTcode = item.PSTcode })
        //                .Select(group => new
        //                {
        //                    Location = group.Key.Location,
        //                    Product = group.Key.Product,
        //                    Batch = group.Key.Batch,
        //                    PSTcode = group.Key.PSTcode,
        //                    //date = group.Key.Date,
        //                    BoxCount = group.Count()
        //                }).ToList();


        //                // Iterate through each row in checkboxcount
        //                foreach (var checkboxRow in checkboxcount)
        //                {
        //                    // Check if the row exists in boxCountByProductAndLocation
        //                    var matchingRow = boxCountByProductAndLocation.FirstOrDefault(row =>
        //                        row.Location == checkboxRow.Location &&
        //                        row.Product == checkboxRow.Product &&
        //                        row.Batch == checkboxRow.Batch &&
        //                        row.BoxCount == checkboxRow.BoxCount);

        //                    int pstcount = checkboxRow.BoxCount;

        //                    if (matchingRow != null)
        //                    {

        //                        matchedDataWithRemarks.Add(new MatchedData
        //                        {
        //                            PhysicalStockItem = CreateCheckPhysicalStockFromCheckPhysicalStock(checkboxRow),
        //                            StorageOperationItem = null,
        //                            pstcount = pstcount,
        //                            storagecount = matchingRow.BoxCount,
        //                            stockvariance = pstcount - matchingRow.BoxCount,
        //                            Remark = "No Variance"
        //                        });
        //                    }
        //                    else
        //                    {
        //                        var matching1 = boxCountByProductAndLocation.FirstOrDefault(row =>
        //                        row.Location == checkboxRow.Location &&
        //                        row.Product == checkboxRow.Product &&
        //                        row.Batch == checkboxRow.Batch &&
        //                        row.BoxCount != checkboxRow.BoxCount);

        //                        if (matching1 != null)
        //                        {
        //                            matchedDataWithRemarks.Add(new MatchedData
        //                            {
        //                                PhysicalStockItem = CreateCheckPhysicalStockFromCheckPhysicalStock(checkboxRow),
        //                                StorageOperationItem = null,
        //                                pstcount = pstcount,
        //                                storagecount = matching1.BoxCount,
        //                                stockvariance = pstcount - matching1.BoxCount,
        //                                Remark = "Box Quantity Mismatch"

        //                            });
        //                        }
        //                        else
        //                        {
        //                            var matching2 = boxCountByProductAndLocation.FirstOrDefault(row =>
        //                            row.Location == checkboxRow.Location ||
        //                            row.Product == checkboxRow.Product ||
        //                            row.Batch == checkboxRow.Batch ||
        //                            row.BoxCount != checkboxRow.BoxCount);

        //                            matchedDataWithRemarks.Add(new MatchedData
        //                            {
        //                                PhysicalStockItem = CreateCheckPhysicalStockFromCheckPhysicalStock(checkboxRow),
        //                                StorageOperationItem = null,
        //                                pstcount = pstcount,
        //                                storagecount = matching2.BoxCount,
        //                                stockvariance = pstcount - matching2.BoxCount,
        //                                //Remark = matchingRow != null ? "Box Quantity Mismatch" : "Location is Mismatch"
        //                                Remark = "Location is Mismatch"
        //                            });
        //                        }

        //                    }
        //                }

        //            }
        //            else
        //            {
        //                var nonmatchpst = physicalStockData.Where(s => s.productcode.ToUpper() == physicalStockItem.productcode.ToUpper() && s.flag == 0);

        //                var noncheckbox = nonmatchpst.Select(s => new { location = s.locationcode, Product = s.productcode, Box = s.boxes.Split('-')[1], Batch = s.batchcode });

        //                var nonmatch = noncheckbox.GroupBy(item => new { Location = item.location, Product = item.Product, Batch = item.Batch })
        //                     .Select(group => new
        //                     {
        //                         Location = group.Key.Location,
        //                         Product = group.Key.Product,
        //                         Batch = group.Key.Batch,
        //                         BoxCount = group.Count()
        //                     }).ToList();

        //                int count = nonmatch.Any() ? nonmatch.First().BoxCount : 0;

        //                if (matchedDataWithRemarks.Any(matchedData => matchedData.PhysicalStockItem.productcode.ToUpper() == physicalStockItem.productcode.ToUpper()))
        //                {
        //                    //If the product is already added, skip to the next iteration
        //                    continue;
        //                }

        //                matchedDataWithRemarks.Add(new MatchedData
        //                {
        //                    PhysicalStockItem = new CheckPhysicalStock
        //                    {
        //                        physicalid = physicalStockItem.physicalid,
        //                        //boxes = physicalStockItem.boxes,//added
        //                        locationcode = physicalStockItem.locationcode,
        //                        productcode = physicalStockItem.productcode.ToUpper(),
        //                        batchcode = physicalStockItem.batchcode

        //                    },
        //                    StorageOperationItem = null,
        //                    pstcount = count,
        //                    stockvariance = count,
        //                    Remark = "New Product Added"
        //                });


        //            }

        //        }

        //        // You can pass the matchedDataWithRemarks to your view
        //        return View(matchedDataWithRemarks);


        //    }
        //    catch (Exception e)
        //    {
        //        return StatusCode(500, e);
        //    }

        //}
        public IActionResult VarianceReport()
        {

            try
            {
                //Get data from the PhysicalStock and StorageOperation tables            

                var physicalStockData = _context.checkphysicalstock.Where(f => f.flag == 0).ToList();
                var storageOperationData = _context.Storage_Operation.Where(f => f.statusflag.ToUpper() == "ST").ToList();
                var s = physicalStockData.FirstOrDefault()?.physicalid;
                if (s == null)
                {
                    s = "NA";
                }
                    //Create a list to store the matched data with remarks
                    var matchedDataWithRemarks = new List<MatchedData>();

                    foreach (var physicalStockItem in physicalStockData)
                    {
                        //Check if productcode in storageOperationData
                        var matchingStorageItem = storageOperationData.Where(s => s.productcode.ToUpper().Trim() == physicalStockItem.productcode.ToUpper().Trim());

                        //if product code match with storage table
                        if (matchingStorageItem.Any())
                        {

                            //var storageop = storageOperationData.Where(s => s.productcode == physicalStockItem.productcode);
                            //Reterive the list of matchingStorageItem(storage operation) and store in storageop variable
                            var storageop = matchingStorageItem.ToList();

                            //From storageop selecting location,productcode,box after split and batchcode
                            var boxstorage = storageop.Select(s => new { loc = s.locationcode.Trim(), product = s.productcode.Trim(), box = s.boxno.Split('-')[1], batch = s.batchcode.Trim() });

                            var boxCountByProductAndLocation = boxstorage
                                      .GroupBy(item => new { Location = item.loc.Trim(), Product = item.product.Trim(), Batch = item.batch.Trim(), Box = item.box })
                                      .Select(group => new
                                      {
                                          Location = group.Key.Location,
                                          Product = group.Key.Product,
                                          Batch = group.Key.Batch,
                                          Box = group.Key.Box,  // Include the box number in the output
                                          BoxCount = group.Count()
                                      }).ToList();


                            // Check if the product is already processed
                            //if (matchedDataWithRemarks.Any(matchedData => matchedData.PhysicalStockItem != null && matchedData.PhysicalStockItem.productcode == physicalStockItem.productcode))
                            if (matchedDataWithRemarks.Any(matchedData => matchedData.PhysicalStockItem.productcode.ToUpper().Trim() == physicalStockItem.productcode.ToUpper().Trim() && matchedData.PhysicalStockItem.locationcode.ToUpper().Trim() == physicalStockItem.locationcode.ToUpper().Trim() && matchedData.PhysicalStockItem.batchcode.ToUpper().Trim() == physicalStockItem.batchcode.ToUpper().Trim() && matchedData.PhysicalStockItem.boxes == physicalStockItem.boxes.Split('-')[1]))
                            {
                                // If the product is already added, skip to the next iteration
                                continue;
                            }

                            //check existence of product in checkphysicalstock
                            //Check productcode is equal to checkphysicalstock table and checkpst variable
                            var checkpst = physicalStockData.Where(s => s.productcode.ToUpper().Trim() == physicalStockItem.productcode.ToUpper().Trim() && s.flag == 0);

                            //select location,product,box(after split),batch,pstcode checkpst and store in checkbox
                            var checkbox = checkpst.Select(s => new { loc = s.locationcode.Trim(), product = s.productcode.Trim(), Box = s.boxes.Split('-')[1], Batch = s.batchcode.Trim(), PSTcode = s.physicalid.Trim() });

                            //Group by the location,product,batch,pstcode
                            var checkboxcount = checkbox.GroupBy(item => new { Location = item.loc.Trim(), Product = item.product.Trim(), Batch = item.Batch.Trim(), PSTcode = item.PSTcode.Trim(), Box = item.Box })
                            .Select(group => new
                            {
                                Location = group.Key.Location,
                                Product = group.Key.Product,
                                Batch = group.Key.Batch,
                                PSTcode = group.Key.PSTcode,
                                Box = group.Key.Box,
                                BoxCount = group.Count()
                            }).ToList();

                            int totalBoxCount = 0;
                            // Iterate through each row in checkboxcount
                            foreach (var checkboxRow in checkboxcount)
                            {
                                var boxNumber = checkboxRow.Box.Trim();
                                if (matchedDataWithRemarks.Any(matchedData => matchedData.PhysicalStockItem.productcode.ToUpper().Trim() == checkboxRow.Product.ToUpper().Trim() && matchedData.PhysicalStockItem.locationcode.ToUpper().Trim() == checkboxRow.Location.ToUpper().Trim() && matchedData.PhysicalStockItem.batchcode.ToUpper().Trim() == checkboxRow.Batch.ToUpper().Trim() && matchedData.PhysicalStockItem.boxes.Trim() == boxNumber))
                                {
                                    // If the product is already added, skip to the next iteration
                                    continue;
                                }


                                // Check if the row exists in boxCountByProductAndLocation
                                var matchingRow = boxCountByProductAndLocation.FirstOrDefault(row =>
                                    row.Location.ToUpper().Trim() == checkboxRow.Location.ToUpper().Trim() &&
                                    row.Product.ToUpper().Trim() == checkboxRow.Product.ToUpper().Trim() &&
                                    row.Batch.ToUpper().Trim() == checkboxRow.Batch.ToUpper().Trim() &&
                                    row.Box == checkboxRow.Box &&
                                    row.BoxCount == checkboxRow.BoxCount);

                                int pstcount = checkboxRow.BoxCount;

                                if (matchingRow != null)
                                {

                                    matchedDataWithRemarks.Add(new MatchedData
                                    {
                                        PhysicalStockItem = CreateCheckPhysicalStockFromCheckPhysicalStock(checkboxRow),
                                        StorageOperationItem = null,
                                        pstcount = pstcount,
                                        boxno = matchingRow.Box,
                                        storagecount = matchingRow.BoxCount,
                                        stockvariance = pstcount - matchingRow.BoxCount,
                                        Remark = "No Variance"
                                    });
                                }
                                else
                                {

                                    var matching1 = boxCountByProductAndLocation.FirstOrDefault(row =>
                                    row.Location.ToUpper().Trim() == checkboxRow.Location.ToUpper().Trim() &&
                                    row.Product.ToUpper().Trim() == checkboxRow.Product.ToUpper().Trim() &&
                                    row.Batch.ToUpper().Trim() == checkboxRow.Batch.ToUpper().Trim() &&
                                    row.Box == checkboxRow.Box &&
                                    row.BoxCount != checkboxRow.BoxCount);

                                    //var matching1 = boxCountByProductAndLocation.Where(row =>
                                    //    row.Location.ToUpper().Trim() == checkboxRow.Location.ToUpper().Trim() ||
                                    //    row.Product.ToUpper().Trim() == checkboxRow.Product.ToUpper().Trim() ||
                                    //    row.Batch.Trim() == checkboxRow.Batch.Trim() ||
                                    //    row.Box.Trim() == checkboxRow.Box.Trim() ||
                                    //    row.BoxCount! == checkboxRow.BoxCount).ToList();

                                    if (matching1 != null)
                                    {
                                        matchedDataWithRemarks.Add(new MatchedData
                                        {
                                            PhysicalStockItem = CreateCheckPhysicalStockFromCheckPhysicalStock(checkboxRow),
                                            StorageOperationItem = null,
                                            pstcount = pstcount,
                                            boxno = matching1.Box,
                                            storagecount = matching1.BoxCount,
                                            stockvariance = pstcount - matching1.BoxCount,
                                            Remark = "Box Quantity Mismatch"

                                        });
                                    }
                                    else
                                    {
                                        //var matching2 = boxCountByProductAndLocation.FirstOrDefault(row =>
                                        //row.Location.ToUpper().Trim() == checkboxRow.Location.ToUpper().Trim() ||
                                        //row.Product.ToUpper().Trim() == checkboxRow.Product.ToUpper().Trim() ||
                                        //row.Batch.Trim() == checkboxRow.Batch.Trim() ||
                                        //row.Box.Trim() == checkboxRow.Box.Trim() ||
                                        //row.BoxCount == checkboxRow.BoxCount);
                                        //matchedDataWithRemarks.Add(new MatchedData
                                        //{
                                        //    PhysicalStockItem = CreateCheckPhysicalStockFromCheckPhysicalStock(checkboxRow),
                                        //    StorageOperationItem = null,
                                        //    pstcount = pstcount,
                                        //    boxno = checkboxRow.Box,
                                        //    storagecount = matching2.Boxcount, // Use the storage count from the mismatched row
                                        //    stockvariance = pstcount - storagecount,
                                        //    Remark = "Location is Mismatch"
                                        //});

                                        var matching2 = boxCountByProductAndLocation.Where(row =>
                                        row.Location.ToUpper().Trim() == checkboxRow.Location.ToUpper().Trim() ||
                                        row.Product.ToUpper().Trim() == checkboxRow.Product.ToUpper().Trim() ||
                                        row.Batch.Trim() == checkboxRow.Batch.Trim() ||
                                        row.Box.Trim() == checkboxRow.Box.Trim() ||
                                        row.BoxCount == checkboxRow.BoxCount).ToList();

                                        if (matching2.Any())
                                        {
                                            totalBoxCount = matching2
                                           .Where(row => row.Product.ToUpper().Trim() == checkboxRow.Product.ToUpper().Trim()
                                                  && row.Batch.Trim() == checkboxRow.Batch.Trim()
                                                  && row.Box.Trim() == checkboxRow.Box.Trim())
                                                 .Sum(row => row.BoxCount);

                                            var matchedLocation = matching2
                                                .Where(row =>
                                                    row.Product.ToUpper().Trim() == checkboxRow.Product.ToUpper().Trim() &&
                                                    row.Batch.Trim() == checkboxRow.Batch.Trim() &&
                                                    row.Box.Trim() == checkboxRow.Box.Trim())
                                                .ToList();

                                            string selectedLocation = matchedLocation.Select(row => row.Location).FirstOrDefault();

                                            if (selectedLocation != null)
                                            {
                                                if (selectedLocation.ToUpper().Trim() != checkboxRow.Location.ToUpper().Trim() && totalBoxCount != checkboxRow.BoxCount)
                                                {

                                                    matchedDataWithRemarks.Add(new MatchedData
                                                    {
                                                        PhysicalStockItem = CreateCheckPhysicalStockFromCheckPhysicalStock(checkboxRow),
                                                        StorageOperationItem = null,
                                                        pstcount = pstcount,
                                                        boxno = checkboxRow.Box,
                                                        storagecount = totalBoxCount, // Use the storage count from the mismatched row
                                                        stockvariance = pstcount - totalBoxCount,
                                                        Remark = "Location and Box Count is Mismatch"
                                                    });

                                                }
                                                else if (selectedLocation.ToUpper().Trim() != checkboxRow.Location.ToUpper().Trim())
                                                {
                                                    matchedDataWithRemarks.Add(new MatchedData
                                                    {
                                                        PhysicalStockItem = CreateCheckPhysicalStockFromCheckPhysicalStock(checkboxRow),
                                                        StorageOperationItem = null,
                                                        pstcount = pstcount,
                                                        boxno = checkboxRow.Box,
                                                        storagecount = totalBoxCount, // Use the storage count from the mismatched row
                                                        stockvariance = pstcount - totalBoxCount,
                                                        Remark = "Location is Mismatch"
                                                    });

                                                }
                                            }
                                            else
                                            {
                                                matchedDataWithRemarks.Add(new MatchedData
                                                {
                                                    PhysicalStockItem = CreateCheckPhysicalStockFromCheckPhysicalStock(checkboxRow),
                                                    StorageOperationItem = null,
                                                    pstcount = pstcount,
                                                    boxno = checkboxRow.Box,
                                                    storagecount = totalBoxCount, // Use the storage count from the mismatched row
                                                    stockvariance = pstcount - totalBoxCount,
                                                    Remark = "Product Not Found"
                                                });
                                            }

                                            //var isLocationMismatch = matching2.FirstOrDefault(row =>
                                            //row.Location.ToUpper().Trim() != checkboxRow.Location.ToUpper().Trim() &&
                                            //row.Product.ToUpper().Trim() == checkboxRow.Product.ToUpper().Trim() &&
                                            //row.Box.Trim() == checkboxRow.Box.Trim() &&
                                            //row.BoxCount == checkboxRow.BoxCount);


                                            //var isloboxMismatch = matching2.FirstOrDefault(row =>
                                            //row.Location.ToUpper().Trim() != checkboxRow.Location.ToUpper().Trim() &&
                                            //row.Product.ToUpper().Trim() == checkboxRow.Product.ToUpper().Trim() &&
                                            //row.Box.Trim() == checkboxRow.Box.Trim() &&
                                            //row.BoxCount! == checkboxRow.BoxCount);

                                            //if (isLocationMismatch != null)
                                            //{



                                            //    matchedDataWithRemarks.Add(new MatchedData
                                            //    {
                                            //        PhysicalStockItem = CreateCheckPhysicalStockFromCheckPhysicalStock(checkboxRow),
                                            //        StorageOperationItem = null,
                                            //        pstcount = pstcount,
                                            //        boxno = checkboxRow.Box,
                                            //        storagecount = totalBoxCount, // Use the storage count from the mismatched row
                                            //        stockvariance = pstcount - totalBoxCount,
                                            //        Remark = "Location is Mismatch"
                                            //    });



                                            //}
                                            //else if (isloboxMismatch != null)
                                            //{

                                            //    matchedDataWithRemarks.Add(new MatchedData
                                            //    {
                                            //        PhysicalStockItem = CreateCheckPhysicalStockFromCheckPhysicalStock(checkboxRow),
                                            //        StorageOperationItem = null,
                                            //        pstcount = pstcount,
                                            //        boxno = checkboxRow.Box,
                                            //        storagecount = totalBoxCount, // Use the storage count from the mismatched row
                                            //        stockvariance = pstcount - totalBoxCount,
                                            //        Remark = "Location and Box Qty Mismatch"
                                            //    });


                                            //}

                                        }

                                    }

                                }
                            }

                        }
                        else
                        {
                            var nonmatchpst = physicalStockData.Where(s => s.productcode.ToUpper() == physicalStockItem.productcode.ToUpper() && s.flag == 0);

                            var noncheckbox = nonmatchpst.Select(s => new { location = s.locationcode, Product = s.productcode, Box = s.boxes.Split('-')[1], Batch = s.batchcode });

                            var nonmatch = noncheckbox.GroupBy(item => new { Location = item.location, Product = item.Product, Batch = item.Batch, Box = item.Box })
                                 .Select(group => new
                                 {
                                     Location = group.Key.Location,
                                     Product = group.Key.Product,
                                     Batch = group.Key.Batch,
                                     Box = group.Key.Box,
                                     BoxCount = group.Count()
                                 }).ToList();

                            int count = nonmatch.Any() ? nonmatch.First().BoxCount : 0;

                            if (matchedDataWithRemarks.Any(matchedData => matchedData.PhysicalStockItem.productcode.ToUpper() == physicalStockItem.productcode.ToUpper() && matchedData.PhysicalStockItem.batchcode.Trim() == physicalStockItem.batchcode.Trim() && matchedData.PhysicalStockItem.boxes == physicalStockItem.boxes.Split('-')[1].Trim()))
                            {
                                //If the product is already added, skip to the next iteration
                                continue;
                            }

                            matchedDataWithRemarks.Add(new MatchedData
                            {
                                PhysicalStockItem = new CheckPhysicalStock
                                {
                                    physicalid = physicalStockItem.physicalid,
                                    locationcode = physicalStockItem.locationcode,
                                    productcode = physicalStockItem.productcode.ToUpper(),
                                    batchcode = physicalStockItem.batchcode,
                                    boxes = physicalStockItem.boxes.Split('-')[1].Trim()

                                },
                                boxno = nonmatch.Any() ? nonmatch.First().Box : "-",
                                StorageOperationItem = null,
                                pstcount = count,
                                stockvariance = count,
                                Remark = "New Product Added"
                            });

                        }

                    }

                    foreach (var storageItem in storageOperationData)
                    {
                        var matchingStorageItem = physicalStockData.Where(s => s.productcode.ToUpper().Trim() == storageItem.productcode.ToUpper().Trim());

                        if (matchingStorageItem.Any())
                        {
                            if (matchedDataWithRemarks.Any(matchedData => matchedData.PhysicalStockItem.productcode.ToUpper().Trim() == storageItem.productcode.ToUpper().Trim() && matchedData.PhysicalStockItem.locationcode.ToUpper().Trim() == storageItem.locationcode.ToUpper().Trim() && matchedData.PhysicalStockItem.batchcode.ToUpper().Trim() == storageItem.batchcode.ToUpper().Trim() && matchedData.PhysicalStockItem.boxes.Trim() == storageItem.boxno.Split('-')[1].Trim()))
                            {
                                // If the product is already added, skip to the next iteration
                                continue;
                            }

                            var storageitemop = storageOperationData.Where(s => s.productcode.ToUpper().Trim() == storageItem.productcode.ToUpper().Trim() && s.locationcode.ToUpper().Trim() == storageItem.locationcode.ToUpper().Trim() && s.boxno.Split('-')[1] == storageItem.boxno.Split('-')[1] && s.batchcode.ToUpper().Trim() == storageItem.batchcode.ToUpper().Trim() && s.statusflag.ToUpper().Trim() == "ST");

                            var st = storageitemop.GroupBy(s => new
                            {
                                s.locationcode,
                                s.productcode,
                                s.batchcode,
                                BoxPart = s.boxno.Split('-')[1]
                            })
                            .Select(group => new
                            {
                                LocationCode = group.Key.locationcode,
                                ProductCode = group.Key.productcode,
                                BatchCode = group.Key.batchcode,
                                BoxPart = group.Key.BoxPart,
                                Count = group.Count()
                            }).ToList();


                            //if (matchedDataWithRemarks.Any(matchedData => matchedData.PhysicalStockItem.productcode.ToUpper().Trim() == storageItem.productcode.ToUpper().Trim() && matchedData.PhysicalStockItem.locationcode.ToUpper().Trim() == storageItem.locationcode.ToUpper().Trim() && matchedData.PhysicalStockItem.batchcode.ToUpper().Trim() == storageItem.batchcode.ToUpper().Trim() && matchedData.PhysicalStockItem.boxes.Trim() == storageItem.boxno.Split('-')[1].Trim()))
                            //{
                            //    // If the product is already added, skip to the next iteration
                            //    continue;
                            //}


                            //matchedDataWithRemarks.Add(new MatchedData
                            //{
                            //    PhysicalStockItem = new CheckPhysicalStock
                            //    {
                            //        physicalid = "-",
                            //        boxes = storageItem.boxno.Split('-')[1],
                            //        locationcode = storageItem.locationcode,
                            //        productcode = storageItem.productcode.ToUpper(),
                            //        batchcode = storageItem.batchcode
                            //    },
                            //    StorageOperationItem = null,
                            //    pstcount = 0,
                            //    boxno = st.First().BoxPart,
                            //    storagecount = st.Any() ? st.First().Count : 0,
                            //    stockvariance = 0,
                            //    Remark = "Product Not Found"
                            //});

                        }
                        else
                        {

                            if (matchedDataWithRemarks.Any(matchedData => matchedData.PhysicalStockItem.productcode.ToUpper().Trim() == storageItem.productcode.ToUpper().Trim() && matchedData.PhysicalStockItem.locationcode.ToUpper().Trim() == storageItem.locationcode.ToUpper().Trim() && matchedData.PhysicalStockItem.batchcode.ToUpper().Trim() == storageItem.batchcode.ToUpper().Trim() && matchedData.PhysicalStockItem.boxes.Trim() == storageItem.boxno.Split('-')[1].Trim()))
                            {
                                // If the product is already added, skip to the next iteration
                                continue;
                            }

                            var storageitemop = storageOperationData.Where(s => s.productcode.ToUpper().Trim() == storageItem.productcode.ToUpper().Trim() && s.locationcode.ToUpper().Trim() == storageItem.locationcode.ToUpper().Trim() && s.boxno.Split('-')[1] == storageItem.boxno.Split('-')[1] && s.batchcode.ToUpper().Trim() == storageItem.batchcode.ToUpper().Trim() && s.statusflag.ToUpper().Trim() == "ST");

                            var st = storageitemop.GroupBy(s => new
                            {
                                s.locationcode,
                                s.productcode,
                                BoxPart = s.boxno.Split('-')[1]
                            })
                            .Select(group => new
                            {
                                LocationCode = group.Key.locationcode,
                                ProductCode = group.Key.productcode,
                                BoxPart = group.Key.BoxPart,
                                Count = group.Count()
                            }).ToList();

                            if (storageitemop != null)
                            {

                                matchedDataWithRemarks.Add(new MatchedData
                                {
                                    PhysicalStockItem = new CheckPhysicalStock
                                    {
                                        physicalid = s,
                                        boxes = storageItem.boxno.Split('-')[1],
                                        locationcode = storageItem.locationcode,
                                        productcode = storageItem.productcode.ToUpper(),
                                        batchcode = storageItem.batchcode
                                    },
                                    boxno = st.First().BoxPart,
                                    StorageOperationItem = null,
                                    storagecount = st.Any() ? st.First().Count : 0,
                                    pstcount = 0,
                                    stockvariance = 0,
                                    Remark = "Product Not Found"
                                });
                            }

                        }

                    }

                    // You can pass the matchedDataWithRemarks to your view
                    return View(matchedDataWithRemarks);
                
                


            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }

        }


        //public IActionResult VarianceReport()
        //{

        //    try
        //    {
        //        var physicalStockData = _context.checkphysicalstock.Where(f => f.flag == 0).ToList();
        //        var storageOperationData = _context.Storage_Operation.Where(f => f.statusflag == "ST").ToList();
        //        var matchedDataWithRemarks = new List<MatchedData>();

        //        foreach (var physicalStockItem in physicalStockData)
        //        {
        //            var matchingStorageItem = storageOperationData.Where(s => s.productcode.ToUpper() == physicalStockItem.productcode.ToUpper());

        //            if (matchingStorageItem.Any())
        //            {
        //                var storageop = matchingStorageItem.ToList();

        //                var boxstorage = storageop.Select(s => new { loc = s.locationcode, product = s.productcode, box = s.boxno.Split('-')[1], batch = s.batchcode });

        //                var boxCountByProductAndLocation = boxstorage
        //                          .GroupBy(item => new { Location = item.loc, Product = item.product, Batch = item.batch, Box = item.box })
        //                          .Select(group => new
        //                          {
        //                              Location = group.Key.Location,
        //                              Product = group.Key.Product,
        //                              Batch = group.Key.Batch,
        //                              Box = group.Key.Box,
        //                              BoxCount = group.Count()
        //                          }).ToList();


        //                if (matchedDataWithRemarks.Any(matchedData => matchedData.PhysicalStockItem.productcode.ToUpper() == physicalStockItem.productcode.ToUpper()))
        //                {
        //                    continue;
        //                }
        //                var checkpst = physicalStockData.Where(s => s.productcode.ToUpper() == physicalStockItem.productcode.ToUpper() && s.flag == 0);

        //                var checkbox = checkpst.Select(s => new { loc = s.locationcode, product = s.productcode, Box = s.boxes.Split('-')[1], Batch = s.batchcode, PSTcode = s.physicalid });

        //                var checkboxcount = checkbox.GroupBy(item => new { Location = item.loc, Product = item.product, Batch = item.Batch, PSTcode = item.PSTcode, Box = item.Box })
        //                .Select(group => new
        //                {
        //                    Location = group.Key.Location,
        //                    Product = group.Key.Product,
        //                    Batch = group.Key.Batch,
        //                    PSTcode = group.Key.PSTcode,
        //                    Box = group.Key.Box,
        //                    BoxCount = group.Count()
        //                }).ToList();
        //                foreach (var checkboxRow in checkboxcount)
        //                {
        //                    var matchingRow = boxCountByProductAndLocation.FirstOrDefault(row =>
        //                        row.Location == checkboxRow.Location &&
        //                        row.Product == checkboxRow.Product &&
        //                        row.Batch == checkboxRow.Batch &&
        //                        row.BoxCount == checkboxRow.BoxCount 
        //                     /*   && row.Box.Split("-")[1] == checkboxRow.Box*/);
        //                    int pstcount = checkboxRow.BoxCount;
        //                    if (matchingRow != null)
        //                    {
        //                        matchedDataWithRemarks.Add(new MatchedData
        //                        {
        //                            PhysicalStockItem = CreateCheckPhysicalStockFromCheckPhysicalStock(checkboxRow),
        //                            StorageOperationItem = null,
        //                            pstcount = pstcount,
        //                            //boxno = matchingRow.Box,
        //                            storagecount = matchingRow.BoxCount,
        //                            stockvariance = pstcount - matchingRow.BoxCount,
        //                            Remark = "No Variance"
        //                        });
        //                    }
        //                    else
        //                    {
        //                        var matching1 = boxCountByProductAndLocation.FirstOrDefault(row =>
        //                        row.Location == checkboxRow.Location &&
        //                        row.Product == checkboxRow.Product &&
        //                        row.Batch == checkboxRow.Batch &&
        //                        row.BoxCount != checkboxRow.BoxCount 
        //                       /* && row.Box.Split("-")[1] == checkboxRow.Box*/);
        //                        if (matching1 != null)
        //                        {
        //                            matchedDataWithRemarks.Add(new MatchedData
        //                            {
        //                                PhysicalStockItem = CreateCheckPhysicalStockFromCheckPhysicalStock(checkboxRow),
        //                                StorageOperationItem = null,
        //                                pstcount = pstcount,
        //                                //boxno = matchingRow.Box,
        //                                storagecount = matching1.BoxCount,
        //                                stockvariance = pstcount - matching1.BoxCount,
        //                                Remark = "Box Quantity Mismatch"

        //                            });
        //                        }
        //                        else
        //                        {
        //                            var matching2 = boxCountByProductAndLocation.FirstOrDefault(row =>
        //                            row.Location == checkboxRow.Location ||
        //                            row.Product == checkboxRow.Product ||
        //                            row.Batch == checkboxRow.Batch ||
        //                            row.BoxCount != checkboxRow.BoxCount
        //                           /* || row.Box.Split("-")[1] == checkboxRow.Box*/);
        //                            matchedDataWithRemarks.Add(new MatchedData
        //                            {
        //                                PhysicalStockItem = CreateCheckPhysicalStockFromCheckPhysicalStock(checkboxRow),
        //                                StorageOperationItem = null,
        //                                pstcount = pstcount,
        //                                //boxno = matchingRow.Box,
        //                                storagecount = matching2.BoxCount,
        //                                stockvariance = pstcount - matching2.BoxCount,
        //                                Remark = "Location is Mismatch"
        //                            });
        //                        }

        //                    }
        //                }

        //            }
        //            else
        //            {
        //                var nonmatchpst = physicalStockData.Where(s => s.productcode.ToUpper() == physicalStockItem.productcode.ToUpper() && s.flag == 0);
        //                var noncheckbox = nonmatchpst.Select(s => new { location = s.locationcode, Product = s.productcode, Box = s.boxes.Split('-')[1], Batch = s.batchcode });
        //                var nonmatch = noncheckbox.GroupBy(item => new { Location = item.location, Product = item.Product, Batch = item.Batch })
        //                     .Select(group => new
        //                     {
        //                         Location = group.Key.Location,
        //                         Product = group.Key.Product,
        //                         Batch = group.Key.Batch,
        //                         BoxCount = group.Count()
        //                     }).ToList();

        //                int count = nonmatch.Any() ? nonmatch.First().BoxCount : 0;

        //                if (matchedDataWithRemarks.Any(matchedData => matchedData.PhysicalStockItem.productcode.ToUpper() == physicalStockItem.productcode.ToUpper()))
        //                {
        //                    continue;
        //                }

        //                matchedDataWithRemarks.Add(new MatchedData
        //                {
        //                    PhysicalStockItem = new CheckPhysicalStock
        //                    {
        //                        physicalid = physicalStockItem.physicalid,
        //                        locationcode = physicalStockItem.locationcode,
        //                        productcode = physicalStockItem.productcode.ToUpper(),
        //                        batchcode = physicalStockItem.batchcode,
        //                        boxes = physicalStockItem.boxes,
        //                    },
        //                    StorageOperationItem = null,
        //                    pstcount = count,
        //                    stockvariance = count,
        //                    Remark = "New Product Added"
        //                });


        //            }

        //        }
        //        return View(matchedDataWithRemarks);
        //    }
        //    catch (Exception e)
        //    {
        //        return StatusCode(500, e);
        //    }

        //}


        // Function to create a PhysicalStockTake object from CheckPhysicalStock
        CheckPhysicalStock CreateCheckPhysicalStockFromCheckPhysicalStock(object checkboxRow)
        {
            var properties = checkboxRow.GetType().GetProperties();
            var checkPhysicalStock = new CheckPhysicalStock();

            foreach (var property in properties)
            {
                var propertyName = property.Name;
                var propertyValue = property.GetValue(checkboxRow);

                // Map properties based on the actual property names in CheckPhysicalStock
                switch (propertyName)
                {
                    case "PSTcode":
                        checkPhysicalStock.physicalid = propertyValue?.ToString();
                        break;
                    case "Box":
                        checkPhysicalStock.boxes = propertyValue?.ToString();
                        break;
                    case "Location":
                        checkPhysicalStock.locationcode = propertyValue?.ToString();
                        break;
                    case "Product":
                        checkPhysicalStock.productcode = propertyValue?.ToString();
                        break;
                    case "Batch":
                        checkPhysicalStock.batchcode = propertyValue?.ToString();
                        break;
                        // Add other cases for additional properties if needed
                }
            }

            return checkPhysicalStock;
        }


        //------------------
        //Over All productcode Quantity
        public IActionResult OverAllProduct()
        {


            var physicalStockTakeData = _context.checkphysicalstock.Where(f => f.flag == 0).ToList();
            var phys = physicalStockTakeData.First().physicalid;

            var groupedProductData = physicalStockTakeData
                .GroupBy(item => new { ProductCode = item.productcode.Trim(), BoxNumber = item.boxes?.Split('-')[1].Trim() })
                .Select(group => new
                {
                    ProductCode = group.Key.ProductCode.Trim(),
                    Boxes = group.Key.BoxNumber?.Trim(),
                    phystorDenominator = group.Key.BoxNumber?.Split('/').Length > 1
                        ? int.Parse(group.Key.BoxNumber.Split('/')[1].Trim())
                        : 0,
                    GroupedItems = group.ToList() // Store grouped items for further use
                }).OrderBy(item => item.ProductCode).ToList();


            //----------------------------
            var result = groupedProductData
                        .GroupBy(item => item.ProductCode)
                        .Select(productGroup =>
                        {
                            // Parse the denominator and calculate missing boxes once for the entire product group
                            var firstGroup = productGroup.FirstOrDefault();
                            var denominator = int.TryParse(firstGroup?.phystorDenominator.ToString(), out int denomValue) ? denomValue : 0;

                            // Get all actual box numbers for this product group
                            var actualBoxNumbers = productGroup
                                .Select(item => int.TryParse(item.Boxes.Split('/')[0], out int boxNum) ? boxNum : 0)
                                .Where(boxNum => boxNum > 0)
                                .ToList();

                            // Generate the expected sequence of boxes from 1 to the denominator
                            var expectedBoxNumbers = Enumerable.Range(1, denominator).ToList();

                            // Find missing boxes
                            var missingBoxes = expectedBoxNumbers.Except(actualBoxNumbers)
                                .Select(missingBoxNum => $"{missingBoxNum}/{denominator}")
                                .ToList();

                            // Create missing box results only once per product
                            var missingBoxResults = missingBoxes.Select(missingBox => new OverAllCount
                            {
                                ProductCode = firstGroup.ProductCode,
                                Boxes = missingBox, // Add missing box in the Boxes field
                                ProductCount = actualBoxNumbers.Count, // The count for missing boxes is based on actual occurrences
                                phystorDenominator = firstGroup.phystorDenominator.ToString(),
                                IsCountEqualToDenominator = false, // Missing boxes mean the count is not equal to the denominator
                                TotalCount = 0, // Total count is zero for missing boxes
                            }).ToList();

                            // Add the existing product entries to the results
                            var existingDataResults = productGroup.Select(group => new OverAllCount
                            {
                                ProductCode = group.ProductCode,
                                Boxes = group.Boxes,
                                ProductCount = actualBoxNumbers.Count, // Same product count for the whole group
                                phystorDenominator = group.phystorDenominator.ToString(),
                                IsCountEqualToDenominator = actualBoxNumbers.Count == denominator,
                                TotalCount = group.GroupedItems.Count(),
                                MissingBoxes = missingBoxes // Attach the missing boxes, even if they are not in this entry
                            }).ToList();

                            // Combine both existing and missing results
                            var combinedResults = existingDataResults.Concat(missingBoxResults).ToList();

                            // Calculate the minimum total count across all product entries, including missing boxes
                            var minTotalCount = combinedResults.Min(group => group.TotalCount);

                            // Return the ProductCode and MinTotalCount (minimum count for that product)
                            return new
                            {
                                ProductCode = firstGroup.ProductCode,
                                MinTotalCount = minTotalCount // Store the minimum count for each product
                            };
                        })
                        .OrderBy(result => result.ProductCode) // Order by ProductCode
                        .ToList();

            // Output: result will contain ProductCode and MinTotalCount for each product



            //Storage operation
            var storageoperationData = _context.Storage_Operation.Where(f => f.statusflag.ToUpper().Trim() == "ST").ToList();

            var groupedstorageData = storageoperationData
                .GroupBy(item => new { ProductCode = item.productcode.Trim(), BoxNumber = item.boxno?.Split('-')[1].Trim() })
                .Select(group => new
                {
                    ProductCode = group.Key.ProductCode.Trim(),
                    Boxes = group.Key.BoxNumber?.Trim(),
                    phystorDenominator = group.Key.BoxNumber?.Split('/').Length > 1
                        ? int.Parse(group.Key.BoxNumber.Split('/')[1].Trim())
                        : 0,
                    GroupedItems = group.ToList() // Store grouped items for further use
                }).OrderBy(item => item.ProductCode).ToList();

            var result1 = groupedstorageData
                        .GroupBy(item => item.ProductCode)
                        .Select(productGroup =>
                        {
                            // Parse the denominator and calculate missing boxes once for the entire product group
                            var firstGroup = productGroup.FirstOrDefault();
                            var denominator = int.TryParse(firstGroup?.phystorDenominator.ToString(), out int denomValue) ? denomValue : 0;

                            // Get all actual box numbers for this product group
                            var actualBoxNumbers = productGroup
                                .Select(item => int.TryParse(item.Boxes.Split('/')[0], out int boxNum) ? boxNum : 0)
                                .Where(boxNum => boxNum > 0)
                                .ToList();

                            // Generate the expected sequence of boxes from 1 to the denominator
                            var expectedBoxNumbers = Enumerable.Range(1, denominator).ToList();

                            // Find missing boxes
                            var missingBoxes = expectedBoxNumbers.Except(actualBoxNumbers)
                                .Select(missingBoxNum => $"{missingBoxNum}/{denominator}")
                                .ToList();

                            // Create missing box results only once per product
                            var missingBoxResults = missingBoxes.Select(missingBox => new OverAllCount
                            {
                                ProductCode = firstGroup.ProductCode,
                                Boxes = missingBox, // Add missing box in the Boxes field
                                ProductCount = actualBoxNumbers.Count, // The count for missing boxes is based on actual occurrences
                                phystorDenominator = firstGroup.phystorDenominator.ToString(),
                                IsCountEqualToDenominator = false, // Missing boxes mean the count is not equal to the denominator
                                TotalCount = 0, // Total count is zero for missing boxes
                            }).ToList();

                            // Add the existing product entries to the results
                            var existingDataResults = productGroup.Select(group => new OverAllCount
                            {
                                ProductCode = group.ProductCode,
                                Boxes = group.Boxes,
                                ProductCount = actualBoxNumbers.Count, // Same product count for the whole group
                                phystorDenominator = group.phystorDenominator.ToString(),
                                IsCountEqualToDenominator = actualBoxNumbers.Count == denominator,
                                TotalCount = group.GroupedItems.Count(),
                                MissingBoxes = missingBoxes // Attach the missing boxes, even if they are not in this entry
                            }).ToList();

                            // Combine both existing and missing results
                            var combinedResults = existingDataResults.Concat(missingBoxResults).ToList();

                            // Calculate the minimum total count across all product entries, including missing boxes
                            var minTotalCount = combinedResults.Min(group => group.TotalCount);

                            // Return the ProductCode and MinTotalCount (minimum count for that product)
                            return new
                            {
                                ProductCode = firstGroup.ProductCode,
                                MinTotalCount = minTotalCount // Store the minimum count for each product
                            };
                        })
                        .OrderBy(result => result.ProductCode) // Order by ProductCode
                        .ToList();


            // Now join both storageResult and physicalStockResult by ProductCode and store the comparison in a new list
            var comparisonResult = (from storage in result1
                                    join physicalStock in result
                                    on storage.ProductCode equals physicalStock.ProductCode
                                    select new ProductComparisonViewModel
                                    {
                                        physicalid = phys,
                                        ProductCode = storage.ProductCode,
                                        StorageMinTotalCount = storage.MinTotalCount,
                                        PhysicalStockMinTotalCount = physicalStock.MinTotalCount,
                                        StockVariance = physicalStock.MinTotalCount - storage.MinTotalCount,
                                        Remark = storage.MinTotalCount == physicalStock.MinTotalCount
                                                 ? "Quantity Match"
                                                 : "Quantity Mismatch"
                                    }).OrderBy(a => a.ProductCode).ToList();



            var physicalOnly = (from physicalStock in result
                                join storage in result1
                                on physicalStock.ProductCode equals storage.ProductCode into joined
                                from subStorage in joined.DefaultIfEmpty() // Left join
                                where subStorage == null // Filter for items that do not exist in storage
                                select new ProductComparisonViewModel
                                {
                                    physicalid = phys,
                                    ProductCode = physicalStock.ProductCode,
                                    StorageMinTotalCount = 0, // Not present in storage
                                    PhysicalStockMinTotalCount = physicalStock.MinTotalCount,
                                    StockVariance = physicalStock.MinTotalCount,
                                    Remark = "Not in Storage"
                                });

            // Retrieve storage data not in physicalStock
            var storageOnly = (from storage in result1
                               join physicalStock in result
                               on storage.ProductCode equals physicalStock.ProductCode into joined
                               from subPhysical in joined.DefaultIfEmpty() // Left join
                               where subPhysical == null // Only items in storage that are not in physicalStock
                               select new ProductComparisonViewModel
                               {
                                   physicalid = phys,
                                   ProductCode = storage.ProductCode,
                                   StorageMinTotalCount = storage.MinTotalCount,
                                   PhysicalStockMinTotalCount = 0, // Not present in physicalStock
                                   StockVariance = -storage.MinTotalCount, // Negative variance
                                   Remark = "Product Not Found"
                               });

            var allResult = physicalOnly
                        .Union(storageOnly)
                        .Union(comparisonResult)
                        .OrderBy(a => a.ProductCode)
                        .ToList();



            //return View(comparisonResult);
            return Json(allResult);
        }
        private string GetDenominatorFromBoxes(string boxes)
        {
            // Assuming the format is always "n/m"
            string[] parts = boxes?.Split('/');
            return parts?.Length == 2 ? parts[1] : null;
        }

        public IActionResult RetrieveData()
        {
            try
            {
                //Clear the old data from (PhysicalStockTake) table               
                //string currentdate = DateTime.Now.Date.ToString();
                //List<PhysicalStockTake> sourceData = _context.PhysicalStockTake.Where(c=>c.date==currentdate).ToList();
                List<PhysicalStockTake> sourceData = _context.PhysicalStockTake.Where(f => f.flag == 0).ToList();
                if (sourceData != null && sourceData.Any())
                {
                    //string pstid = "PST" + DateTime.Now.Date.ToString("ddMMyyyy");
                    // Create instances of CheckPhysicalStock based on the retrieved data
                    List<CheckPhysicalStock> receivedData = sourceData.Select(p => new CheckPhysicalStock
                    {
                        physicalid = p.physicalid,
                        productcode = p.productcode.ToUpper(),
                        batchcode = p.batchcode,
                        locationcode = p.locationcode,
                        boxes = p.boxes,
                        storageflag = p.storageflag,
                        grnno = p.grnno,
                        //set flag to 0 which mean non-validate
                        flag = 0,
                        date = DateTime.Now.Date.ToString("dd-MM-yyyy")
                        // Map other properties as needed
                    }).ToList();

                    // Insert data into the destination table (CheckPhysicalStock)
                    _context.checkphysicalstock.AddRange(receivedData);
                    _context.SaveChanges();

                    // Update the flag of the retrieved data to 1
                    foreach (var item in sourceData)
                    {
                        item.flag = 1;
                    }
                    _context.SaveChanges();


                    return Json(new { success = true, message = "Data Send Successfully." });
                }


                return Json(new { error = true, message = "No data found" });
            }

            catch (Exception ex)
            {
                return Json(new { success = false, message = "Catch Exception :" + ex.Message });
            }
        }


        public async Task<IActionResult> CheckPhysicalStock()
        {
            if (_context.checkphysicalstock == null)
            {
                return Problem("Entity set 'AuthDbContext.do_allotment' is null.");
            }
            string currentdate = DateTime.Now.Date.ToString();
            //var checkphysical = await _context.checkphysicalstock.Where(c => c.date == currentdate).ToListAsync();
            //var checkphysical = await _context.checkphysicalstock.Where(f => f.flag == 0).ToListAsync();

            var checkphysical =
                      await _context.checkphysicalstock.Where(f => f.flag == 0).GroupBy(ps => ps.physicalid)
                       .Select(group => new CheckPhysicalStock
                       {
                           physicalid = group.Key,
                           boxcount = group.Count(),
                       }).ToListAsync();

            return View(checkphysical);
        }

        public ActionResult ReplaceData()
        {
            try
            {
                //Get the data from checkphysicalstock
                List<CheckPhysicalStock> checkphysicalstock = _context.checkphysicalstock.Where(s => s.flag == 0).ToList();
                //string currentdate = DateTime.Now.Date.ToString();

                //List<CheckPhysicalStock> checkphysicalstock = _context.checkphysicalstock.Where(p => p.date == currentdate).ToList();
                if (checkphysicalstock.Count > 0)
                {
                    //Clear the data from destination(Storage_Operation) table
                    _context.Storage_Operation.RemoveRange(_context.Storage_Operation);
                    _context.SaveChanges();

                    // Add new data to the Storage_Operation table
                    List<Storage_Operation> storageOperationList = checkphysicalstock
                        .Select(c => new Storage_Operation
                        {
                            // Map the properties from CheckPhysicalStock to Storage_Operation
                            id = c.id,
                            productcode = c.productcode.ToUpper().Trim(),
                            locationcode = c.locationcode.Trim(),
                            batchcode = c.batchcode.Trim(),
                            boxno = c.boxes.Trim(),
                            statusflag = c.storageflag.Trim(),
                            grnno = c.grnno.Trim(),
                            pickflag = "0",
                            //balanceqty = "-"
                        })
                        .ToList();

                    //Add new data to the destination(Storage_Operation) table
                    _context.Storage_Operation.AddRange(storageOperationList);
                    foreach (var ll in storageOperationList)
                    {
                        //maintain logs
                        var user = HttpContext.Session.GetString("User");
                        var logs = new Logs();
                        logs.pagename = "Validate PST";
                        logs.task = "Storage Data Replace";
                        logs.action = "Update";
                        logs.taskid = ll.id;
                        logs.date = DateTime.Now.ToString("dd/MM/yyyy");
                        logs.time = DateTime.Now.ToString("HH:mm:ss");
                        logs.username = user;
                        _context.Add(logs);
                        _context.SaveChanges();
                    }
                    _context.SaveChanges();

                    // Update the flag of the retrieved data to 1
                    foreach (var item in checkphysicalstock)
                    {
                        item.flag = 1;
                    }
                    _context.SaveChanges();
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "No Data to Validate" });
                }

            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "An error occurred" });
            }

        }

        public IActionResult Restart()
        {
            try
            {
                List<PhysicalStockTake> pst = _context.PhysicalStockTake.ToList();
                if (pst.Count > 0)
                {
                    _context.PhysicalStockTake.RemoveRange(_context.PhysicalStockTake);
                    _context.SaveChanges();
                    return Json(new { success = true, message = "Restart Successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Data is Already Empty" });
                }

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred" });
            }

        }
        public ActionResult Damage()
        {
            var damaged = _context.checkphysicalstock.Where(s => s.storageflag == "DMG").Select(s => new Damage
            {
                productcode = s.productcode.ToUpper(),
                locationcode = s.locationcode,
                batchcode = s.batchcode,
                boxes = s.boxes
            }).ToList();

            //int damagecount = damaged.Count;
            //// Store the count in ViewBag
            //ViewBag.damagecount = damagecount;
            return View(damaged);
        }



        public IActionResult SaveVariance(string tableDataJson, string selectedvalue)
        {

            try
            {
                if (selectedvalue.ToUpper() == "BOXES")
                {
                    var tableDataList = JsonConvert.DeserializeObject<List<SaveVariance>>(tableDataJson);
                    bool allSuccess = true; // Flag to track overall success

                    foreach (var item in tableDataList)
                    {
                        var pstid = _context.SaveVariance.FirstOrDefault(a => a.physicalid == item.physicalid);
                        if (pstid != null)
                        {
                            allSuccess = false; // Update flag if any item is not saved
                        }
                    }

                    if (allSuccess)
                    {
                        foreach (var item in tableDataList)
                        {
                            item.locationcode = item.locationcode.Replace("\n", "").Trim(); // Example
                            item.productcode = item.productcode.Replace("\n", "").Trim(); // Example
                            item.batchcode = item.batchcode.Replace("\n", "").Trim(); // Example
                            item.Remark = item.Remark.Replace("\n", "").Trim(); // Example
                            item.physicalid = item.physicalid.Replace("\n", "").Trim(); // Example
                            item.boxno = item.boxno.Replace("\n", "").Trim(); // Example
                                                                              // Create a new SaveVariance object for each iteration
                            var newSaveVariance = new SaveVariance
                            {
                                physicalid = item.physicalid,
                                locationcode = item.locationcode,
                                productcode = item.productcode.ToUpper(),
                                pstcount = item.pstcount,
                                batchcode = item.batchcode,
                                stockvariance = item.stockvariance,
                                storagecount = item.storagecount,
                                Remark = item.Remark,
                                boxno = item.boxno
                            };

                            // Add the newSaveVariance object to the context (outside the loop)
                            _context.Add(newSaveVariance);
                        }

                        // Save changes outside the loop
                        _context.SaveChanges();
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { success = false });
                    }
                }
                else if (selectedvalue.ToUpper() == "PRODUCT")
                {
                    var tableDataList = JsonConvert.DeserializeObject<List<productwise>>(tableDataJson);
                    bool allSuccess = true; // Flag to track overall success
                    int maxCslip = _context.productwisesave.Select(e => (int?)e.maxvalue).Max() ?? 0;
                    maxCslip += 1;

                    foreach (var item in tableDataList)
                    {
                        var pstid = _context.productwisesave.FirstOrDefault(a => a.physicalid == item.physicalid);
                        if (pstid != null)
                        {
                            allSuccess = false; // Update flag if any item is not saved
                        }
                    }
                    if (allSuccess)
                    {
                        foreach (var item in tableDataList)
                        {
                            item.productcode = item.productcode.Replace("\n", "").Trim(); // Example
                            item.pstcount = item.pstcount;
                            item.storagecount = item.storagecount;
                            item.stockvariance = item.stockvariance;
                            item.remark = item.remark.Replace("\n", "").Trim(); // Example
                            item.physicalid = item.physicalid.Replace("\n", "").Trim(); // Example

                            // Create a new SaveVariance object for each iteration
                            var productwise = new productwisesave
                            {
                                maxvalue = maxCslip,
                                productcode = item.productcode,

                                pstcount = item.pstcount,
                                storagecount = item.storagecount,
                                stockvariance = item.stockvariance,
                                remark = item.remark,
                                physicalid = item.physicalid,

                            };

                            // Add the newSaveVariance object to the context (outside the loop)
                            _context.Add(productwise);
                        }

                        // Save changes outside the loop
                        _context.SaveChanges();
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { success = false });
                    }
                }
                else
                {
                    return Json(new { success = false });
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions here
                return Json(new { success = false, error = ex.Message });
            }
        }
        public IActionResult detailsummary(string productCode)
        {
            var physicalStockTakeData = _context.checkphysicalstock.Where(f => f.productcode.Trim() == productCode.Trim() && f.flag == 0).ToList();


            var groupedProductData = physicalStockTakeData
                .GroupBy(item => new { ProductCode = item.productcode.Trim(), BoxNumber = item.boxes?.Split('-')[1].Trim() })
                .Select(group => new
                {
                    ProductCode = group.Key.ProductCode.Trim(),
                    Boxes = group.Key.BoxNumber?.Trim(),
                    phystorDenominator = group.Key.BoxNumber?.Split('/').Length > 1
                        ? int.Parse(group.Key.BoxNumber.Split('/')[1].Trim())
                        : 0,
                    GroupedItems = group.ToList() // Store grouped items for further use
                }).OrderBy(item => item.ProductCode).ToList();

            //----------------------------------------------------

            var result = groupedProductData
                        .GroupBy(item => item.ProductCode)
                        .SelectMany(productGroup =>
                        {
                            // Parse the denominator and calculate missing boxes once for the entire product group
                            var firstGroup = productGroup.FirstOrDefault();
                            var denominator = int.TryParse(firstGroup?.phystorDenominator.ToString(), out int denomValue) ? denomValue : 0;

                            // Get all actual box numbers for this product group
                            var actualBoxNumbers = productGroup
                                .Select(item => int.TryParse(item.Boxes.Split('/')[0], out int boxNum) ? boxNum : 0)
                                .Where(boxNum => boxNum > 0)
                                .ToList();

                            // Generate the expected sequence of boxes from 1 to the denominator
                            var expectedBoxNumbers = Enumerable.Range(1, denominator).ToList();

                            // Find missing boxes
                            var missingBoxes = expectedBoxNumbers.Except(actualBoxNumbers)
                                .Select(missingBoxNum => $"{missingBoxNum}/{denominator}")
                                .ToList();

                            // Create missing box results only once per product
                            var missingBoxResults = missingBoxes.Select(missingBox => new OverAllCount
                            {
                                ProductCode = firstGroup.ProductCode,
                                Boxes = missingBox, // Add missing box in the Boxes field
                                ProductCount = actualBoxNumbers.Count, // The count for missing boxes is based on actual occurrences
                                phystorDenominator = firstGroup.phystorDenominator.ToString(),
                                IsCountEqualToDenominator = false, // Missing boxes mean the count is not equal to the denominator
                                TotalCount = 0, // Total count is zero for missing boxes
                            }).ToList();

                            // Add the existing product entries to the results
                            var existingDataResults = productGroup.Select(group => new OverAllCount
                            {
                                ProductCode = group.ProductCode,
                                Boxes = group.Boxes,
                                ProductCount = actualBoxNumbers.Count, // Same product count for the whole group
                                phystorDenominator = group.phystorDenominator.ToString(),
                                IsCountEqualToDenominator = actualBoxNumbers.Count == denominator,
                                TotalCount = group.GroupedItems.Count(),
                                MissingBoxes = missingBoxes // Attach the missing boxes, even if they are not in this entry
                            }).ToList();

                            // Calculate the minimum total count across all product entries, including missing boxes
                            var minTotalCount = existingDataResults.Concat(missingBoxResults).Min(group => group.TotalCount);

                            // Update all results (both existing and missing) to include the minimum total count
                            return existingDataResults.Concat(missingBoxResults)
                                .Select(group => new OverAllCount
                                {
                                    ProductCode = group.ProductCode,
                                    Boxes = group.Boxes,
                                    ProductCount = group.ProductCount,
                                    phystorDenominator = group.phystorDenominator,
                                    TotalCount = group.TotalCount,
                                    MinTotalCount = minTotalCount, // Store the minimum count
                                    MissingBoxes = group.MissingBoxes,
                                    IsCountEqualToDenominator = group.IsCountEqualToDenominator,
                                })
                                .OrderBy(item => item.ProductCode)
                                .ThenBy(item =>
                                {
                                    var boxParts = item.Boxes.Split('/');
                                    return int.TryParse(boxParts[0], out int boxNumber) ? boxNumber : 0;
                                }).ToList();
                        }).ToList();



            var storageoperationData = _context.Storage_Operation.Where(f => f.productcode.Trim() == productCode.Trim() && f.statusflag.ToUpper().Trim() == "ST").ToList();

            var groupedstorageData = storageoperationData
                .GroupBy(item => new { ProductCode = item.productcode.Trim(), BoxNumber = item.boxno?.Split('-')[1].Trim() })
                .Select(group => new
                {
                    ProductCode = group.Key.ProductCode.Trim(),
                    Boxes = group.Key.BoxNumber?.Trim(),
                    phystorDenominator = group.Key.BoxNumber?.Split('/').Length > 1
                        ? int.Parse(group.Key.BoxNumber.Split('/')[1].Trim())
                        : 0,
                    GroupedItems = group.ToList() // Store grouped items for further use
                }).OrderBy(item => item.ProductCode).ToList();

            var result1 = groupedstorageData
                        .GroupBy(group => group.ProductCode) // Group by ProductCode to ensure we process each product code only once
                        .SelectMany(productGroup =>
                        {
                            var firstGroup = productGroup.First(); // Take the first group to get general details

                            // Parse the denominator from the box number
                            var denominator = int.TryParse(firstGroup.phystorDenominator.ToString(), out int denomValue) ? denomValue : 0;

                            // Get all the actual box numbers for this product across the grouped data
                            var actualBoxNumbers = productGroup
                                .Select(item => int.TryParse(item.Boxes.Split('/')[0], out int boxNum) ? boxNum : 0)
                                .Where(boxNum => boxNum > 0)
                                .ToList();

                            // Generate the expected sequence of boxes from 1 to the denominator
                            var expectedBoxNumbers = Enumerable.Range(1, denominator).ToList();

                            // Find missing boxes by comparing expected and actual boxes
                            var missingBoxes = expectedBoxNumbers.Except(actualBoxNumbers)
                                .Select(missingBoxNum => $"{missingBoxNum}/{denominator}") // Format the missing boxes as "x/denominator"
                                .ToList();

                            // Create the result for the existing data (for each box that exists)
                            var existingDataResults = productGroup.Select(group => new OverAllCount
                            {
                                ProductCode = group.ProductCode.Trim(),
                                Boxes = group.Boxes,
                                ProductCount = actualBoxNumbers.Count, // Use actual count of the boxes
                                phystorDenominator = group.phystorDenominator.ToString(),
                                IsCountEqualToDenominator = actualBoxNumbers.Count == denominator,
                                TotalCount = group.GroupedItems.Count(),
                                MissingBoxes = missingBoxes // List of missing boxes (if any)
                            }).ToList();

                            // Add each missing box individually as a separate entry with TotalCount = 0
                            var missingBoxResults = missingBoxes.Select(missingBox => new OverAllCount
                            {
                                ProductCode = firstGroup.ProductCode.Trim(), // Use the product code from the group
                                Boxes = missingBox, // Add missing box in the Boxes field
                                ProductCount = actualBoxNumbers.Count, // Same product count for all missing entries
                                phystorDenominator = firstGroup.phystorDenominator.ToString(), // Same denominator
                                IsCountEqualToDenominator = false, // Mark it false since boxes are missing
                                TotalCount = 0, // Total count for missing boxes is zero
                            }).ToList();

                            // Return the existing data and missing boxes combined
                            return existingDataResults.Concat(missingBoxResults);
                        })
                        .OrderBy(item => item.ProductCode.Trim())
                        .ThenBy(item =>
                        {
                            // Split box number to order correctly
                            var boxParts = item.Boxes.Split('/');
                            return int.TryParse(boxParts[0], out int boxNumber) ? boxNumber : 0;
                        }).ToList();

            var comparisonresult = (from storage in result1
                                    join physicalStock in result
                                    on new { storage.ProductCode, storage.Boxes } equals new { physicalStock.ProductCode, physicalStock.Boxes }
                                    select new detailsummary
                                    {
                                        ProductCode = storage.ProductCode,
                                        storageboxes = storage.Boxes,
                                        storagecount = storage.TotalCount,
                                        pstboxes = physicalStock.Boxes,
                                        pstcount = physicalStock.TotalCount,
                                    }).OrderBy(a => a.ProductCode).ToList();


            var physicalOnly = (from physicalStock in result
                                join storage in result1
                                on physicalStock.ProductCode equals storage.ProductCode into joined
                                from subStorage in joined.DefaultIfEmpty() // Left join
                                where subStorage == null // Filter for items that do not exist in storage
                                select new detailsummary
                                {
                                    ProductCode = physicalStock.ProductCode, // Use physicalStock's ProductCode
                                    storageboxes = "0", // No boxes in storage
                                    storagecount = 0, // No count in storage
                                    pstboxes = physicalStock.Boxes,
                                    pstcount = physicalStock.TotalCount,
                                });

            // Retrieve storage data not in physicalStock
            var storageOnly = (from storage in result1
                               join physicalStock in result
                               on storage.ProductCode equals physicalStock.ProductCode into joined
                               from subPhysical in joined.DefaultIfEmpty() // Left join
                               where subPhysical == null // Only items in storage that are not in physicalStock
                               select new detailsummary
                               {
                                   ProductCode = storage.ProductCode, // Use storage's ProductCode
                                   storageboxes = storage.Boxes,
                                   storagecount = storage.TotalCount,
                                   pstboxes = "0", // No boxes in physicalStock
                                   pstcount = 0, // No count in physicalStock
                               });

            var allResult = physicalOnly
                       .Union(storageOnly)
                       .Union(comparisonresult)
                       .OrderBy(a => a.ProductCode)
                       .ToList();

            return Json(allResult);
        }


        public IActionResult VarianceReporttt()
        {

            try
            {
                //Get data from the PhysicalStock and StorageOperation tables            

                var physicalStockData = _context.checkphysicalstock.Where(f => f.flag == 0).ToList();
                var storageOperationData = _context.Storage_Operation.Where(f => f.statusflag.ToUpper() == "ST").ToList();
                var s = physicalStockData.First().physicalid;
                //Create a list to store the matched data with remarks
                var matchedDataWithRemarks = new List<MatchedData>();

                foreach (var physicalStockItem in physicalStockData)
                {
                    //Check if productcode in storageOperationData
                    var matchingStorageItem = storageOperationData.Where(s => s.productcode.ToUpper().Trim() == physicalStockItem.productcode.ToUpper().Trim());

                    //if product code match with storage table
                    if (matchingStorageItem.Any())
                    {

                        //var storageop = storageOperationData.Where(s => s.productcode == physicalStockItem.productcode);
                        //Reterive the list of matchingStorageItem(storage operation) and store in storageop variable
                        var storageop = matchingStorageItem.ToList();

                        //From storageop selecting location,productcode,box after split and batchcode
                        var boxstorage = storageop.Select(s => new { loc = s.locationcode.Trim(), product = s.productcode.Trim(), box = s.boxno.Split('-')[1], batch = s.batchcode.Trim() });

                        var boxCountByProductAndLocation = boxstorage
                                  .GroupBy(item => new { Location = item.loc.Trim(), Product = item.product.Trim(), Batch = item.batch.Trim(), Box = item.box })
                                  .Select(group => new
                                  {
                                      Location = group.Key.Location,
                                      Product = group.Key.Product,
                                      Batch = group.Key.Batch,
                                      Box = group.Key.Box,  // Include the box number in the output
                                      BoxCount = group.Count()
                                  }).ToList();


                        // Check if the product is already processed
                        //if (matchedDataWithRemarks.Any(matchedData => matchedData.PhysicalStockItem != null && matchedData.PhysicalStockItem.productcode == physicalStockItem.productcode))
                        if (matchedDataWithRemarks.Any(matchedData => matchedData.PhysicalStockItem.productcode.ToUpper().Trim() == physicalStockItem.productcode.ToUpper().Trim() && matchedData.PhysicalStockItem.locationcode.ToUpper().Trim() == physicalStockItem.locationcode.ToUpper().Trim() && matchedData.PhysicalStockItem.batchcode.ToUpper().Trim() == physicalStockItem.batchcode.ToUpper().Trim() && matchedData.PhysicalStockItem.boxes == physicalStockItem.boxes.Split('-')[1]))
                        {
                            // If the product is already added, skip to the next iteration
                            continue;
                        }

                        //check existence of product in checkphysicalstock
                        //Check productcode is equal to checkphysicalstock table and checkpst variable
                        var checkpst = physicalStockData.Where(s => s.productcode.ToUpper().Trim() == physicalStockItem.productcode.ToUpper().Trim() && s.flag == 0);

                        //select location,product,box(after split),batch,pstcode checkpst and store in checkbox
                        var checkbox = checkpst.Select(s => new { loc = s.locationcode.Trim(), product = s.productcode.Trim(), Box = s.boxes.Split('-')[1], Batch = s.batchcode.Trim(), PSTcode = s.physicalid.Trim() });

                        //Group by the location,product,batch,pstcode
                        var checkboxcount = checkbox.GroupBy(item => new { Location = item.loc.Trim(), Product = item.product.Trim(), Batch = item.Batch.Trim(), PSTcode = item.PSTcode.Trim(), Box = item.Box })
                        .Select(group => new
                        {
                            Location = group.Key.Location,
                            Product = group.Key.Product,
                            Batch = group.Key.Batch,
                            PSTcode = group.Key.PSTcode,
                            Box = group.Key.Box,
                            BoxCount = group.Count()
                        }).ToList();

                        int totalBoxCount = 0;
                        // Iterate through each row in checkboxcount
                        foreach (var checkboxRow in checkboxcount)
                        {
                            var boxNumber = checkboxRow.Box.Trim();
                            if (matchedDataWithRemarks.Any(matchedData => matchedData.PhysicalStockItem.productcode.ToUpper().Trim() == checkboxRow.Product.ToUpper().Trim() && matchedData.PhysicalStockItem.locationcode.ToUpper().Trim() == checkboxRow.Location.ToUpper().Trim() && matchedData.PhysicalStockItem.batchcode.ToUpper().Trim() == checkboxRow.Batch.ToUpper().Trim() && matchedData.PhysicalStockItem.boxes.Trim() == boxNumber))
                            {
                                // If the product is already added, skip to the next iteration
                                continue;
                            }


                            // Check if the row exists in boxCountByProductAndLocation
                            var matchingRow = boxCountByProductAndLocation.FirstOrDefault(row =>
                                row.Location.ToUpper().Trim() == checkboxRow.Location.ToUpper().Trim() &&
                                row.Product.ToUpper().Trim() == checkboxRow.Product.ToUpper().Trim() &&
                                row.Batch.ToUpper().Trim() == checkboxRow.Batch.ToUpper().Trim() &&
                                row.Box == checkboxRow.Box &&
                                row.BoxCount == checkboxRow.BoxCount);

                            int pstcount = checkboxRow.BoxCount;

                            if (matchingRow != null)
                            {

                                matchedDataWithRemarks.Add(new MatchedData
                                {
                                    PhysicalStockItem = CreateCheckPhysicalStockFromCheckPhysicalStock(checkboxRow),
                                    StorageOperationItem = null,
                                    pstcount = pstcount,
                                    boxno = matchingRow.Box,
                                    storagecount = matchingRow.BoxCount,
                                    stockvariance = pstcount - matchingRow.BoxCount,
                                    Remark = "No Variance"
                                });
                            }
                            else
                            {

                                var matching1 = boxCountByProductAndLocation.FirstOrDefault(row =>
                                row.Location.ToUpper().Trim() == checkboxRow.Location.ToUpper().Trim() &&
                                row.Product.ToUpper().Trim() == checkboxRow.Product.ToUpper().Trim() &&
                                row.Batch.ToUpper().Trim() == checkboxRow.Batch.ToUpper().Trim() &&
                                row.Box == checkboxRow.Box &&
                                row.BoxCount != checkboxRow.BoxCount);

                                //var matching1 = boxCountByProductAndLocation.Where(row =>
                                //    row.Location.ToUpper().Trim() == checkboxRow.Location.ToUpper().Trim() ||
                                //    row.Product.ToUpper().Trim() == checkboxRow.Product.ToUpper().Trim() ||
                                //    row.Batch.Trim() == checkboxRow.Batch.Trim() ||
                                //    row.Box.Trim() == checkboxRow.Box.Trim() ||
                                //    row.BoxCount! == checkboxRow.BoxCount).ToList();

                                if (matching1 != null)
                                {
                                    matchedDataWithRemarks.Add(new MatchedData
                                    {
                                        PhysicalStockItem = CreateCheckPhysicalStockFromCheckPhysicalStock(checkboxRow),
                                        StorageOperationItem = null,
                                        pstcount = pstcount,
                                        boxno = matching1.Box,
                                        storagecount = matching1.BoxCount,
                                        stockvariance = pstcount - matching1.BoxCount,
                                        Remark = "Box Quantity Mismatch"

                                    });
                                }
                                else
                                {
                                    //var matching2 = boxCountByProductAndLocation.FirstOrDefault(row =>
                                    //row.Location.ToUpper().Trim() == checkboxRow.Location.ToUpper().Trim() ||
                                    //row.Product.ToUpper().Trim() == checkboxRow.Product.ToUpper().Trim() ||
                                    //row.Batch.Trim() == checkboxRow.Batch.Trim() ||
                                    //row.Box.Trim() == checkboxRow.Box.Trim() ||
                                    //row.BoxCount == checkboxRow.BoxCount);
                                    //matchedDataWithRemarks.Add(new MatchedData
                                    //{
                                    //    PhysicalStockItem = CreateCheckPhysicalStockFromCheckPhysicalStock(checkboxRow),
                                    //    StorageOperationItem = null,
                                    //    pstcount = pstcount,
                                    //    boxno = checkboxRow.Box,
                                    //    storagecount = matching2.Boxcount, // Use the storage count from the mismatched row
                                    //    stockvariance = pstcount - storagecount,
                                    //    Remark = "Location is Mismatch"
                                    //});

                                    var matching2 = boxCountByProductAndLocation.Where(row =>
                                    row.Location.ToUpper().Trim() == checkboxRow.Location.ToUpper().Trim() ||
                                    row.Product.ToUpper().Trim() == checkboxRow.Product.ToUpper().Trim() ||
                                    row.Batch.Trim() == checkboxRow.Batch.Trim() ||
                                    row.Box.Trim() == checkboxRow.Box.Trim() ||
                                    row.BoxCount == checkboxRow.BoxCount).ToList();

                                    if (matching2.Any())
                                    {
                                        totalBoxCount = matching2
                                       .Where(row => row.Product.ToUpper().Trim() == checkboxRow.Product.ToUpper().Trim()
                                              && row.Batch.Trim() == checkboxRow.Batch.Trim()
                                              && row.Box.Trim() == checkboxRow.Box.Trim())
                                             .Sum(row => row.BoxCount);

                                        var matchedLocation = matching2
                                            .Where(row =>
                                                row.Product.ToUpper().Trim() == checkboxRow.Product.ToUpper().Trim() &&
                                                row.Batch.Trim() == checkboxRow.Batch.Trim() &&
                                                row.Box.Trim() == checkboxRow.Box.Trim())
                                            .ToList();

                                        string selectedLocation = matchedLocation.Select(row => row.Location).FirstOrDefault();

                                        if (selectedLocation != null)
                                        {
                                            if (selectedLocation.ToUpper().Trim() != checkboxRow.Location.ToUpper().Trim() && totalBoxCount != checkboxRow.BoxCount)
                                            {

                                                matchedDataWithRemarks.Add(new MatchedData
                                                {
                                                    PhysicalStockItem = CreateCheckPhysicalStockFromCheckPhysicalStock(checkboxRow),
                                                    StorageOperationItem = null,
                                                    pstcount = pstcount,
                                                    boxno = checkboxRow.Box,
                                                    storagecount = totalBoxCount, // Use the storage count from the mismatched row
                                                    stockvariance = pstcount - totalBoxCount,
                                                    Remark = "Location and Box Count is Mismatch"
                                                });

                                            }
                                            else if (selectedLocation.ToUpper().Trim() != checkboxRow.Location.ToUpper().Trim())
                                            {
                                                matchedDataWithRemarks.Add(new MatchedData
                                                {
                                                    PhysicalStockItem = CreateCheckPhysicalStockFromCheckPhysicalStock(checkboxRow),
                                                    StorageOperationItem = null,
                                                    pstcount = pstcount,
                                                    boxno = checkboxRow.Box,
                                                    storagecount = totalBoxCount, // Use the storage count from the mismatched row
                                                    stockvariance = pstcount - totalBoxCount,
                                                    Remark = "Location is Mismatch of boxes"
                                                });

                                            }
                                        }
                                        else
                                        {
                                            matchedDataWithRemarks.Add(new MatchedData
                                            {
                                                PhysicalStockItem = CreateCheckPhysicalStockFromCheckPhysicalStock(checkboxRow),
                                                StorageOperationItem = null,
                                                pstcount = pstcount,
                                                boxno = checkboxRow.Box,
                                                storagecount = totalBoxCount, // Use the storage count from the mismatched row
                                                stockvariance = pstcount - totalBoxCount,
                                                Remark = "Boxes Not Found"
                                            });
                                        }

                                        //var isLocationMismatch = matching2.FirstOrDefault(row =>
                                        //row.Location.ToUpper().Trim() != checkboxRow.Location.ToUpper().Trim() &&
                                        //row.Product.ToUpper().Trim() == checkboxRow.Product.ToUpper().Trim() &&
                                        //row.Box.Trim() == checkboxRow.Box.Trim() &&
                                        //row.BoxCount == checkboxRow.BoxCount);


                                        //var isloboxMismatch = matching2.FirstOrDefault(row =>
                                        //row.Location.ToUpper().Trim() != checkboxRow.Location.ToUpper().Trim() &&
                                        //row.Product.ToUpper().Trim() == checkboxRow.Product.ToUpper().Trim() &&
                                        //row.Box.Trim() == checkboxRow.Box.Trim() &&
                                        //row.BoxCount! == checkboxRow.BoxCount);

                                        //if (isLocationMismatch != null)
                                        //{



                                        //    matchedDataWithRemarks.Add(new MatchedData
                                        //    {
                                        //        PhysicalStockItem = CreateCheckPhysicalStockFromCheckPhysicalStock(checkboxRow),
                                        //        StorageOperationItem = null,
                                        //        pstcount = pstcount,
                                        //        boxno = checkboxRow.Box,
                                        //        storagecount = totalBoxCount, // Use the storage count from the mismatched row
                                        //        stockvariance = pstcount - totalBoxCount,
                                        //        Remark = "Location is Mismatch"
                                        //    });



                                        //}
                                        //else if (isloboxMismatch != null)
                                        //{

                                        //    matchedDataWithRemarks.Add(new MatchedData
                                        //    {
                                        //        PhysicalStockItem = CreateCheckPhysicalStockFromCheckPhysicalStock(checkboxRow),
                                        //        StorageOperationItem = null,
                                        //        pstcount = pstcount,
                                        //        boxno = checkboxRow.Box,
                                        //        storagecount = totalBoxCount, // Use the storage count from the mismatched row
                                        //        stockvariance = pstcount - totalBoxCount,
                                        //        Remark = "Location and Box Qty Mismatch"
                                        //    });


                                        //}

                                    }

                                }

                            }
                        }

                    }
                    else
                    {
                        var nonmatchpst = physicalStockData.Where(s => s.productcode.ToUpper() == physicalStockItem.productcode.ToUpper() && s.flag == 0);

                        var noncheckbox = nonmatchpst.Select(s => new { location = s.locationcode, Product = s.productcode, Box = s.boxes.Split('-')[1], Batch = s.batchcode });

                        var nonmatch = noncheckbox.GroupBy(item => new { Location = item.location, Product = item.Product, Batch = item.Batch, Box = item.Box })
                             .Select(group => new
                             {
                                 Location = group.Key.Location,
                                 Product = group.Key.Product,
                                 Batch = group.Key.Batch,
                                 Box = group.Key.Box,
                                 BoxCount = group.Count()
                             }).ToList();

                        int count = nonmatch.Any() ? nonmatch.First().BoxCount : 0;

                        if (matchedDataWithRemarks.Any(matchedData => matchedData.PhysicalStockItem.productcode.ToUpper() == physicalStockItem.productcode.ToUpper() && matchedData.PhysicalStockItem.batchcode.Trim() == physicalStockItem.batchcode.Trim() && matchedData.PhysicalStockItem.boxes == physicalStockItem.boxes.Split('-')[1].Trim()))
                        {
                            //If the product is already added, skip to the next iteration
                            continue;
                        }

                        matchedDataWithRemarks.Add(new MatchedData
                        {
                            PhysicalStockItem = new CheckPhysicalStock
                            {
                                physicalid = physicalStockItem.physicalid,
                                locationcode = physicalStockItem.locationcode,
                                productcode = physicalStockItem.productcode.ToUpper(),
                                batchcode = physicalStockItem.batchcode,
                                boxes = physicalStockItem.boxes.Split('-')[1].Trim()

                            },
                            boxno = nonmatch.Any() ? nonmatch.First().Box : "-",
                            StorageOperationItem = null,
                            pstcount = count,
                            stockvariance = count,
                            Remark = "New Boxes Added"
                        });

                    }

                }

                foreach (var storageItem in storageOperationData)
                {
                    var matchingStorageItem = physicalStockData.Where(s => s.productcode.ToUpper().Trim() == storageItem.productcode.ToUpper().Trim());

                    if (matchingStorageItem.Any())
                    {
                        if (matchedDataWithRemarks.Any(matchedData => matchedData.PhysicalStockItem.productcode.ToUpper().Trim() == storageItem.productcode.ToUpper().Trim() && matchedData.PhysicalStockItem.locationcode.ToUpper().Trim() == storageItem.locationcode.ToUpper().Trim() && matchedData.PhysicalStockItem.batchcode.ToUpper().Trim() == storageItem.batchcode.ToUpper().Trim() && matchedData.PhysicalStockItem.boxes.Trim() == storageItem.boxno.Split('-')[1].Trim()))
                        {
                            // If the product is already added, skip to the next iteration
                            continue;
                        }

                        var storageitemop = storageOperationData.Where(s => s.productcode.ToUpper().Trim() == storageItem.productcode.ToUpper().Trim() && s.locationcode.ToUpper().Trim() == storageItem.locationcode.ToUpper().Trim() && s.boxno.Split('-')[1] == storageItem.boxno.Split('-')[1] && s.batchcode.ToUpper().Trim() == storageItem.batchcode.ToUpper().Trim() && s.statusflag.ToUpper().Trim() == "ST");

                        var st = storageitemop.GroupBy(s => new
                        {
                            s.locationcode,
                            s.productcode,
                            s.batchcode,
                            BoxPart = s.boxno.Split('-')[1]
                        })
                        .Select(group => new
                        {
                            LocationCode = group.Key.locationcode,
                            ProductCode = group.Key.productcode,
                            BatchCode = group.Key.batchcode,
                            BoxPart = group.Key.BoxPart,
                            Count = group.Count()
                        }).ToList();


                        //if (matchedDataWithRemarks.Any(matchedData => matchedData.PhysicalStockItem.productcode.ToUpper().Trim() == storageItem.productcode.ToUpper().Trim() && matchedData.PhysicalStockItem.locationcode.ToUpper().Trim() == storageItem.locationcode.ToUpper().Trim() && matchedData.PhysicalStockItem.batchcode.ToUpper().Trim() == storageItem.batchcode.ToUpper().Trim() && matchedData.PhysicalStockItem.boxes.Trim() == storageItem.boxno.Split('-')[1].Trim()))
                        //{
                        //    // If the product is already added, skip to the next iteration
                        //    continue;
                        //}


                        //matchedDataWithRemarks.Add(new MatchedData
                        //{
                        //    PhysicalStockItem = new CheckPhysicalStock
                        //    {
                        //        physicalid = "-",
                        //        boxes = storageItem.boxno.Split('-')[1],
                        //        locationcode = storageItem.locationcode,
                        //        productcode = storageItem.productcode.ToUpper(),
                        //        batchcode = storageItem.batchcode
                        //    },
                        //    StorageOperationItem = null,
                        //    pstcount = 0,
                        //    boxno = st.First().BoxPart,
                        //    storagecount = st.Any() ? st.First().Count : 0,
                        //    stockvariance = 0,
                        //    Remark = "Product Not Found"
                        //});

                    }
                    else
                    {

                        if (matchedDataWithRemarks.Any(matchedData => matchedData.PhysicalStockItem.productcode.ToUpper().Trim() == storageItem.productcode.ToUpper().Trim() && matchedData.PhysicalStockItem.locationcode.ToUpper().Trim() == storageItem.locationcode.ToUpper().Trim() && matchedData.PhysicalStockItem.batchcode.ToUpper().Trim() == storageItem.batchcode.ToUpper().Trim() && matchedData.PhysicalStockItem.boxes.Trim() == storageItem.boxno.Split('-')[1].Trim()))
                        {
                            // If the product is already added, skip to the next iteration
                            continue;
                        }

                        var storageitemop = storageOperationData.Where(s => s.productcode.ToUpper().Trim() == storageItem.productcode.ToUpper().Trim() && s.locationcode.ToUpper().Trim() == storageItem.locationcode.ToUpper().Trim() && s.boxno.Split('-')[1] == storageItem.boxno.Split('-')[1] && s.batchcode.ToUpper().Trim() == storageItem.batchcode.ToUpper().Trim() && s.statusflag.ToUpper().Trim() == "ST");

                        var st = storageitemop.GroupBy(s => new
                        {
                            s.locationcode,
                            s.productcode,
                            BoxPart = s.boxno.Split('-')[1]
                        })
                        .Select(group => new
                        {
                            LocationCode = group.Key.locationcode,
                            ProductCode = group.Key.productcode,
                            BoxPart = group.Key.BoxPart,
                            Count = group.Count()
                        }).ToList();

                        if (storageitemop != null)
                        {

                            matchedDataWithRemarks.Add(new MatchedData
                            {
                                PhysicalStockItem = new CheckPhysicalStock
                                {
                                    physicalid = s,
                                    boxes = storageItem.boxno.Split('-')[1],
                                    locationcode = storageItem.locationcode,
                                    productcode = storageItem.productcode.ToUpper(),
                                    batchcode = storageItem.batchcode
                                },
                                boxno = st.First().BoxPart,
                                StorageOperationItem = null,
                                storagecount = st.Any() ? st.First().Count : 0,
                                pstcount = 0,
                                stockvariance = 0,
                                Remark = "Product Not Found"
                            });
                        }

                    }

                }

                // You can pass the matchedDataWithRemarks to your view
                //return View(matchedDataWithRemarks);
                return Json(matchedDataWithRemarks);


            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }

        }
    }
}
