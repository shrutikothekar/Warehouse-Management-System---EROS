using AuthSystem.Data;
using DocumentFormat.OpenXml.Office.CustomUI;
using eros.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace eros.Controllers
{
    public class InStockController : Controller
    {
        private readonly ErosDbContext _context;

        public InStockController(ErosDbContext context)
        {
            _context = context;
        }

        public class InStockQtyModel
        {
            public string productcode { get; set; }
            public int currentqty { get; set; }
        }
        public IActionResult Index()
        {
            return RedirectToAction("InStockQty", "inwards");
        }
        //public IActionResult Index()
        //{
        //    var storageOperations = GetStorageOperationsFromDataSource();

        //    var inStockQuantities = storageOperations
        //        .GroupBy(op => op.productcode)
        //        .Select(group =>
        //        {
        //            var productCode = group.Key;
        //            var currentQty = 0;

        //            foreach (var operation in group)
        //            {
        //                var totalBoxes = int.Parse(operation.balanceqty.Split('/')[1]);
        //                var boxFraction = int.Parse(operation.balanceqty.Split('/')[0]);

        //                currentQty += totalBoxes * boxFraction;
        //            }

        //            return new InStockQtyModel
        //            {
        //                productcode = productCode,
        //                currentqty = currentQty
        //            };
        //        })
        //        .ToList();

        //    var anotherTableData = _context.Storage_Operation.ToList();

        //    return Ok(inStockQuantities);
        //}


    }
}
