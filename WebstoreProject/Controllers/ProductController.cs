using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebstoreProject.Data;
using WebstoreProject.Models;

namespace WebstoreProject.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _db;

        public ProductController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            Product? product = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.IsActive);

            if (product == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(product);
        }

        [HttpPost]
        public IActionResult ChangeQty(int id, int qty, string actionType)
        {
            if (actionType == "minus") qty--;
            if (actionType == "plus") qty++;

            if (qty < 1) qty = 1;

            return RedirectToAction(nameof(Details), new { id = id, qty = qty });
        }


    }
}
