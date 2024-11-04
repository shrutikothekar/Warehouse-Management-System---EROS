using AuthSystem.Data;
using AuthSystem.Models;
using eros.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using static eros.Controllers.Product_MasterController;

namespace eros.Controllers.Api
{
    //[Produces("application/json")]
    [Route("api/ProductMaster")]
    public class ProductMasterController : Controller
    {
        private readonly ErosDbContext _context;

        public ProductMasterController(ErosDbContext context)
        {
            _context = context;   
        }
        [HttpGet("products")]
        //Product Master - Read API
        public IActionResult GetProduct()
        {
            var data = _context.Product_Master.ToList();
            return Ok("Data Product Master ! :"+data); 
        }

        [HttpGet("inventory")]
        //Current Inventory Stock at Warehouse - Read API
        public IActionResult GetInventoryDetails()
        {
            List<InStockQty> inStockQuantities = new List<InStockQty>();

            var productcodes = _context.Storage_Operation
                .Where(a => a.statusflag == "PI" || a.statusflag == "ST")
                .Select(a => a.productcode.Trim().ToUpper())
                .Distinct()
                .ToList();

            foreach (var b in productcodes)
            {
                var productMaster = _context.Product_Master
                    .FirstOrDefault(a => a.productcode.Trim().ToUpper() == b.Trim().ToUpper());

                if (productMaster != null)
                {
                    var productName = productMaster.productdescription.Trim().ToUpper();
                    var category = productMaster.categoryname.Trim().ToUpper();

                    var storeData = _context.Storage_Operation
                        .Where(a => a.productcode.Trim().ToUpper() == b && (a.statusflag == "PI" || a.statusflag == "ST"))
                        .ToList();

                    var result = new List<KeyValuePair<string, int>>();
                    int minCount = int.MaxValue;

                    var box = storeData.Select(a => a.boxno.Trim()).FirstOrDefault();
                    var splitbox = GetSpliBox(box);

                    var groupedData = storeData
                        .GroupBy(q => GetSecondDigit(q.boxno))
                        .ToDictionary(group => group.Key, group => group.Count());
                    var possibleBoxes = new List<string>();
                    if (groupedData.Count == 0)
                    {
                        minCount = 0;
                    }
                    else
                    {
                        possibleBoxes = GetPossibleBoxes(splitbox);
                    }
                    foreach (var item in possibleBoxes)
                    {
                        int count = 0;
                        foreach (var kvp in groupedData)
                        {
                            if (kvp.Key.Contains(item))
                            {
                                count = kvp.Value;
                                break;
                            }
                        }
                        result.Add(new KeyValuePair<string, int>(item, count));
                    }
                    if (result.Count > 0)
                    {
                        minCount = result.Min(kvp => kvp.Value);
                    }
                    else
                    {
                        minCount = 0;
                    }

                    var instcok = new InStockQty
                    {
                        productcode = b,
                        stcokallocate = storeData.Count(a => a.statusflag == "PI"),
                        currentqty = minCount,
                        productname = productName,
                        category = category,
                    };

                    inStockQuantities.Add(instcok);
                }
            }

            inStockQuantities = inStockQuantities.OrderBy(item => item.id).ToList();

            return Ok("Success :Data of Inventory Stock Level ! : " + inStockQuantities);
        }
        private int GetSpliBox(string boxno)
        {
            var dashParts = boxno.Split('-');
            if (dashParts.Length == 2)
            {
                var slashParts = dashParts[1].Split('/');
                if (slashParts.Length == 2 && int.TryParse(slashParts[1].Trim(), out int totalBoxes))
                {
                    return totalBoxes;
                }
            }
            return 0; // Or handle error case appropriately
        }
        private List<string> GetPossibleBoxes(int totalBoxes)
        {
            var boxes = new List<string>();
            for (int i = 1; i <= totalBoxes; i++)
            {
                boxes.Add($"{i}/{totalBoxes}");
            }
            return boxes;
        }
        private string GetSecondDigit(string boxno)
        {
            string[] parts = boxno.Split('-');
            if (parts.Length == 2)
            {
                return parts[1];
            }
            return string.Empty;
        }

        //Despatch Details against SO - Read API  
        [HttpGet("dispatchdetails")]
        public IActionResult InOutStock(string date)
        {
            List<PurchaseOrder> PurchaseOrders = new List<PurchaseOrder>();
            List<SalesOrder> SalesOrders = new List<SalesOrder>();

            PurchaseOrders.Clear();
            SalesOrders.Clear();

            //PONO DATA
            var inwardData = _context.inwardPacket
                .Where(a => a.date.Trim() == date.Trim() && a.flag == 1)
                .ToList();
            foreach (var item in inwardData)
            {
                var category = _context.Product_Master
                    .FirstOrDefault(a => a.productcode.Trim().ToUpper() == item.productcode.Trim().ToUpper());

                if (category != null)
                {
                    item.Category = category.categoryname.Trim();
                }

                var check = _context.inward
                    .FirstOrDefault(a => a.pono.Trim() == item.pono.Trim());

                if (check != null)
                {
                    item.Customer = check.partyname.Trim();

                    var purchaseOrder = new PurchaseOrder
                    {
                        pono = item.pono,
                        poqty = item.quantity,
                        supplier = item.Customer,
                        product = item.productcode,
                        category = item.Category
                    };

                    PurchaseOrders.Add(purchaseOrder);
                }
            }

            //SONO DTAA 
            var loadingData = _context.Loading_Dispatch_Operation
             .Where(a => a.currentdate.Trim() == date.Trim())
             .ToList()
             .DistinctBy(a => a.sono.Trim())
             .ToList();


            var picklistdata = _context.Picklist_Generation.ToList();
            picklistdata.Clear();
            foreach (var item in loadingData)
            {
                var picklist = _context.Picklist_Generation.Where(a => a.sono.Trim() == item.sono.Trim()).FirstOrDefault();

                var category = _context.Product_Master
                    .FirstOrDefault(a => a.productcode.Trim().ToUpper() == item.productcode.Trim().ToUpper());

                if (category != null)
                {
                    item.Category = category.categoryname.Trim();
                }

                var check = _context.so_inward
                    .FirstOrDefault(a => a.sono.Trim() == item.sono.Trim());

                if (check != null)
                {
                    item.customer = check.customername.Trim();
                }

                if (picklist != null)
                {
                    picklist.category = category.categoryname.Trim();
                    picklist.date = date;
                    picklist.customer = item.customer;
                    picklist.category = item.Category;
                }
                picklistdata.Add(picklist);
            }
            foreach (var item in picklistdata)
            {
                var salesOrder = new SalesOrder
                {
                    sono = item.sono,
                    soqty = Convert.ToInt32(item.pickingqty),
                    customer = item.customer,
                    product = item.prdcode,
                    category = item.category // Assuming 'category1' was a typo
                };
                SalesOrders.Add(salesOrder);
            }

            // Set ViewBag variables
            ViewBag.PurchaseOrder = PurchaseOrders;
            ViewBag.SalesOrder = SalesOrders;
            ViewBag.NitCountSale = SalesOrders.Count;
            ViewBag.NitCountPurchase = PurchaseOrders.Count;

            // Parse and format date
            DateTime parsedDate;
            if (DateTime.TryParseExact(date.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                ViewBag.Date = parsedDate.ToString("dd-MM-yyyy");
            }
            else
            {
                ViewBag.Date = "Invalid Date";
            }

            return Ok("Success: "+SalesOrders);
        }

        //Inward Details against Sale Return - Read API
        [HttpGet("SaleReturnDetails")]
        public IActionResult Details(string refno, string ordertype)
        {
            var getdata = _context.inward.Where(a => a.sono.Trim() == refno.Trim() && a.ordertype.Trim() == ordertype.Trim()).FirstOrDefault();
            if(getdata!= null)
            {
                var inwardSupplier_Master = _context.inward
                .Include(e => e.inwardPacket)
                .FirstOrDefault(a => a.inward_id == getdata.inward_id);
                if (inwardSupplier_Master == null)
                {
                    return NotFound();
                }
                return Ok(inwardSupplier_Master);
            }
            else
            {
                return Ok("Success :inward Data Not Found agaisnst ref.no : " + refno);
            }            
        }

        //Sale Order (From Quickbill) - Write API 
        [HttpPost("CreateSaleOrder")]
        public IActionResult CreateSaleOrder([FromBody] so_inward saleOrder)
        {
            if (saleOrder == null)
            {
                return BadRequest("Sale Order is null.");
            }

            var found = _context.so_inward.Where(a => a.sono.Trim() == saleOrder.sono.Trim()).FirstOrDefault();
            if(found != null)
            {
                return Ok("Warning :Please enter a different reference order number, because the entered reference number already exists in the database!");
            }

            int maxId = _context.so_inward.Any() ? _context.so_inward.Max(e => e.id) + 1 : 1;
            int maxId1 = _context.so_product.Any() ? _context.so_product.Max(e => e.id) + 1 : 1;

            // Create new so_inward object
            so_inward so_Inward = new so_inward()
            {
                id = maxId,
                sono = saleOrder.sono ?? "-",
                dono = saleOrder.dono ?? "-",
                sodate = saleOrder.sodate ?? "-",
                customername = saleOrder.customername ?? "-",
                contactno = saleOrder.contactno ?? "-",
                emailid = saleOrder.emailid ?? "-",
                address = saleOrder.address ?? "-",
                city = saleOrder.city ?? "-",
                state = saleOrder.state ?? "-",
                dispatchdate = saleOrder.dispatchdate ?? "-",
                status = "Pending"
            };

            // Add so_inward to context
            _context.so_inward.Add(so_Inward);

            foreach (var item in saleOrder.soProduct_details)
            {
                so_product so_product = new so_product()
                {
                    id = maxId1,
                    orderid = maxId,
                    productcode = item.productcode ?? "-",
                    description = item.description ?? "-",
                    brand = item.brand ?? "-",
                    hsncode = item.hsncode ?? "NA",
                    quantity = item.quantity,
                    uom = item.uom ?? "-"
                };

                _context.so_product.Add(so_product);
                maxId1++;
            }

            _context.SaveChanges();

            var saleOrder1 = _context.so_inward.Where(a=>a.id == maxId).FirstOrDefault();
            if (saleOrder1 == null)
            {
                return NotFound();
            }
            return Ok("Success : Data Added for Sale Order !  ");
        }

        //Sale Order (From Quickbill) - Write API 
        [HttpPost("SaleOrderReturn")]
        public IActionResult SaleReturnedOrder([FromBody] so_inward saleOrder)
        {
            if (saleOrder == null)
            {
                return BadRequest("Sale Order is null.");
            }

            var check = _context.so_inward.FirstOrDefault(a => a.sono.Trim() == saleOrder.sono.Trim());

            var checkInPicklist = _context.Picklist_Generation.Where(a => a.sono.Trim() == saleOrder.sono.Trim()).ToList();
            var checkInLoading = _context.Loading_Dispatch_Operation.Where(a => a.sono.Trim() == saleOrder.sono.Trim()).ToList();

            if (check != null && checkInPicklist.Count > 0 && checkInLoading.Count > 0)
            {
                check.status = "Return";
                _context.so_inward.Update(check);
                _context.SaveChanges();

                var checkedData = checkInPicklist.Any(picklistItem => saleOrder.soProduct_details.Any(saleOrderItem => picklistItem.prdcode.Trim() == saleOrderItem.productcode.Trim()));

                if (checkedData)
                {
                    int maxId = _context.so_inwardReturn.Any() ? _context.so_inwardReturn.Max(e => e.id) + 1 : 1;

                    // Create new so_inwardReturn object
                    so_inwardReturn so_Inward = new so_inwardReturn()
                    {
                        id = maxId,
                        sono = saleOrder.sono.Trim() + "/Return" ?? "-",
                        dono = saleOrder.dono.Trim() ?? "-",
                        sodate = saleOrder.sodate.Trim() ?? "-",
                        customername = saleOrder.customername.Trim() ?? "-",
                        contactno = saleOrder.contactno.Trim() ?? "-",
                        emailid = saleOrder.emailid.Trim() ?? "-",
                        address = saleOrder.address.Trim() ?? "-",
                        city = saleOrder.city.Trim() ?? "-",
                        state = saleOrder.state.Trim() ?? "-",
                        dispatchdate = saleOrder.dispatchdate.Trim() ?? "-",
                        status = "Return",
                        dDate = "-",
                        dcustomername = "-",
                    };
                    

                    // Add so_inwardReturn to context
                    _context.so_inwardReturn.Add(so_Inward);
                    _context.SaveChanges();

                    foreach (var item in saleOrder.soProduct_details)
                    {
                        int maxId1 = _context.so_productReturn.Any() ? _context.so_productReturn.Max(e => e.id) + 1 : 1;

                        so_productReturn so_product = new so_productReturn()
                        {
                            id = maxId1,
                            orderid = maxId,
                            productcode = item.productcode.Trim() ?? "-",
                            description = item.description.Trim() ?? "-",
                            brand = item.brand.Trim() ?? "-",
                            hsncode = item.hsncode.Trim() ?? "NA",
                            quantity = item.quantity,
                            uom = item.uom.Trim() ?? "-",
                            customername="-",
                            dispatchdate="-",
                            pickqty="-",
                            sono="-",
                        };

                        _context.so_productReturn.Add(so_product);
                        _context.SaveChanges();
                    }

                    // Save all changes at once
                    //try
                    //{
                    //}
                    //catch (DbUpdateException ex)
                    //{
                    //    // Handle exception, e.g., log the error or return a specific error message
                    //    return StatusCode(500, "An error occurred while saving the sale return order: " + ex.InnerException?.Message);
                    //}

                    return Ok("Success: Data Added for Sale Return order!");
                }
                else
                {
                    return Ok("Warning: The product you have entered is not against that ref. order no " + saleOrder.sono + ". You cannot process any sale return against this order number " + saleOrder.sono + ".");
                }
            }
            else
            {
                return Ok("Warning: Loading dispatch details not found for sale order number " + saleOrder.sono + ". You cannot process any sale return against this order number " + saleOrder.sono + ".");
            }
        }

        // Purchase Order (From Quickbill) - Write API 
        [HttpPost("CreatePurchaseOrder")]
        public IActionResult CreatePurchaseOrder([FromBody] purchase purchaseOrder)
        {
            if (purchaseOrder == null)
            {
                return BadRequest("Purchase Order is null.");
            }

            var existingPurchaseOrder = _context.purchase
                .Where(a => a.pono.Trim() == purchaseOrder.pono.Trim())
                .FirstOrDefault();

            if (existingPurchaseOrder != null)
            {
                return Ok("Warning: Please enter a different Purchase Order number, because the entered PO number already exists in the database!");
            }

            int maxPurchaseId = _context.purchase.Any() ? _context.purchase.Max(e => e.id) + 1 : 1;
            int maxProductId = _context.poProduct_details.Any() ? _context.poProduct_details.Max(e => e.id) + 1 : 1;

            // Create new purchase object
            var newPurchaseOrder = new purchase
            {
                id = maxPurchaseId,
                pono = purchaseOrder.pono ?? "-",
                podate = purchaseOrder.podate ?? DateTime.Now.ToString("yyyy-MM-dd"),
                suppliername = purchaseOrder.suppliername ?? "-",
                contactno = purchaseOrder.contactno ?? "-",
                gstinno = purchaseOrder.gstinno ?? "-",
                address = purchaseOrder.address ?? "-",
                status =  "Pending",
                poProduct_details = new List<purchase_product>(),
                purchase_subcomponent = new List<purchase_subcomponent>()
            };

            // Add purchase order to context
            _context.purchase.Add(newPurchaseOrder);

            foreach (var item in purchaseOrder.poProduct_details)
            {
                var newProduct = new purchase_product
                {
                    id = maxProductId,
                    porderid = maxPurchaseId,
                    productcode = item.productcode ?? "-",
                    description = item.description ?? "-",
                    brand = item.brand ?? "-",
                    quantity = item.quantity,
                    uom = item.uom ?? "-"
                };

                _context.poProduct_details.Add(newProduct);
                maxProductId++;
            }

            _context.SaveChanges();

            var createdPurchaseOrder = _context.purchase.Where(a => a.id == maxPurchaseId).FirstOrDefault();
            if (createdPurchaseOrder == null)
            {
                return NotFound();
            }

            return Ok("Success: Data added for Purchase Order!");
        }


        //Purchase Order (From Quickbill) - Write API 
        [HttpPost("PurchaseOrderReturn")]
        public IActionResult PurchaseReturnedOrder([FromBody] purchaseReturn purchaseOrder)
        {
            if (purchaseOrder == null)
            {
                return BadRequest("Purchase Order is null.");
            }
            var check = _context.purchase
                       .Where(a => a.pono.Trim() == purchaseOrder.pono.Trim() && (a.status.Trim() == "Completed" || a.status == "Pending")).FirstOrDefault();

            var check1 = _context.purchaseReturn
                       .Where(a => a.pono.Trim().StartsWith(purchaseOrder.pono.Trim()) && a.status.Trim() != "Return").FirstOrDefault();

            var inward = _context.inward.FirstOrDefault(a => a.pono.Trim() == purchaseOrder.pono.Trim());
            if(inward != null)
            {

                var founddata = _context.Storage_Operation
                            .Where(a => a.productcode.Trim() == a.productcode.Trim() && a.batchcode.Trim() == inward.batchcode && a.statusflag.Trim() == "ST")
                            .ToList();


                if(founddata.Count > 0)
                {
                    if (check1 == null)
                    {

                        if (check != null)
                        {
                            check.status = "Return";
                            _context.purchase.Update(check);
                            _context.SaveChanges();

                            if (inward != null && founddata.Count > 0)
                            {
                                int maxId = _context.purchaseReturn.Any() ? _context.purchaseReturn.Max(e => e.id) + 1 : 1;

                                // Create new PurchaseReturn object
                                var purchaseReturn = new purchaseReturn()
                                {
                                    id = maxId,
                                    pono = purchaseOrder.pono.Trim() + "/Return" ?? "-",
                                    podate = purchaseOrder.podate.Trim() ?? "-",
                                    suppliername = purchaseOrder.suppliername.Trim() ?? "-",
                                    contactno = purchaseOrder.contactno.Trim() ?? "-",
                                    gstinno = purchaseOrder.gstinno.Trim() ?? "-",
                                    address = purchaseOrder.address.Trim() ?? "-",
                                    status = "Return"
                                };

                                // Add PurchaseReturn to context
                                _context.purchaseReturn.Add(purchaseReturn);
                                _context.SaveChanges();

                                foreach (var item in purchaseOrder.poProduct_detailsReturn)
                                {
                                    int maxId1 = _context.poProduct_detailsReturn.Any() ? _context.poProduct_detailsReturn.Max(e => e.id) + 1 : 1;

                                    var poProduct_detailsReturn1 = new purchase_productReturn()
                                    {
                                        id = maxId1,
                                        porderid = maxId,
                                        productcode = item.productcode.Trim() ?? "-",
                                        description = item.description.Trim() ?? "-",
                                        brand = item.brand.Trim() ?? "-",
                                        quantity = item.quantity,
                                        uom = item.uom.Trim() ?? "-"
                                    };
                                    _context.poProduct_detailsReturn.Add(poProduct_detailsReturn1);
                                    _context.SaveChanges();
                                }
                                return Ok("Success: Data Added for Purchase Return order!");
                            }
                            else
                            {
                                return Ok("Warning: The product are not inwarded against the reference order number "
                                          + purchaseOrder.pono + ". You cannot process any purchase return against this order number "
                                          + purchaseOrder.pono + ".");
                            }
                        }
                        else
                        {
                            return Ok("Warning: Odr refno is alredy in return for purchase order number "
                                      + purchaseOrder.pono + ". You cannot process any purchase return against this order number "
                                      + purchaseOrder.pono + ".");
                        }

                    }
                    else
                    {
                        return Ok("Warning: Alredy a purchase return is generated for the purchase order number "
                              + purchaseOrder.pono + ". You cannot process any purchase return against this order number "
                              + purchaseOrder.pono + ".");
                    }
                    
                }
                else
                {

                    return Ok("Warning: Please do the storage before purchase retirn order of Ref.Order no "
                              + purchaseOrder.pono + ". You cannot process any purchase return against this order number "
                              + purchaseOrder.pono + ".");
                }
            }
            else
            {
                return Ok("Warning: Inwarding not complete agasint for the purchase order number "
                              + purchaseOrder.pono + ". You cannot process any purchase return against this order number "
                              + purchaseOrder.pono + ".");
            }
        }

    }
}
