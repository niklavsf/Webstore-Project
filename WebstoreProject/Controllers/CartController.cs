using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebstoreProject.Data;
using WebstoreProject.Models;

namespace WebstoreProject.Controllers
{
    public class CartController : Controller
    {
        private const string CartSessionKey = "CART";
        private readonly AppDbContext _db;

        public CartController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<CartLine> cart = GetCart();

            // loads products info
            List<int> productIds = cart.Select(x => x.ProductId).ToList();

            List<Product> products = await _db.Products
                .Where(p => productIds.Contains(p.ProductId))
                .Include(p => p.Brand)
                .ToListAsync();

            CartPage model = new CartPage
            {
                Lines = new List<CartLineView>(),
                Total = 0m
            };

            foreach (CartLine line in cart)
            {
                Product? p = products.FirstOrDefault(x => x.ProductId == line.ProductId);
                if (p == null) continue;

                decimal unitPrice = p.HasDiscount ? p.DiscountedPrice : p.Price;
                decimal lineTotal = unitPrice * line.Qty;

                CartLineView vm = new CartLineView
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    ImageUrl = p.ImageUrl,
                    UnitText = p.UnitText,
                    Qty = line.Qty,
                    UnitPrice = unitPrice,
                    LineTotal = lineTotal
                };

                model.Lines.Add(vm);
                model.Total += lineTotal;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int id, int qty)
        {
            if (qty < 1) qty = 1;

            Product? product = await _db.Products
                .FirstOrDefaultAsync(p => p.ProductId == id && p.IsActive);

            if (product == null)
            {
                return RedirectToAction("Index", "Home");
            }

            List<CartLine> cart = GetCart();

            CartLine? existing = cart.FirstOrDefault(x => x.ProductId == id);
            if (existing != null)
            {
                existing.Qty += qty;
            }
            else
            {
                cart.Add(new CartLine { ProductId = id, Qty = qty });
            }

            SaveCart(cart);

            // returns user to last pos
            string? referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrWhiteSpace(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction("Index", "Home");
        }

        
        [HttpPost]
        public IActionResult Update(int id, int qty)
        {
            if (qty < 1) qty = 1;

            List<CartLine> cart = GetCart();
            CartLine? line = cart.FirstOrDefault(x => x.ProductId == id);
            if (line != null)
            {
                line.Qty = qty;
                SaveCart(cart);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Remove(int id)
        {
            List<CartLine> cart = GetCart();
            CartLine? line = cart.FirstOrDefault(x => x.ProductId == id);
            if (line != null)
            {
                cart.Remove(line);
                SaveCart(cart);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CartSessionKey);
            return RedirectToAction(nameof(Index));
        }

        private List<CartLine> GetCart()
        {
            string? json = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<CartLine>();
            }

            try
            {
                List<CartLine>? cart = JsonSerializer.Deserialize<List<CartLine>>(json);
                if (cart == null) return new List<CartLine>();
                return cart;
            }
            catch
            {
                return new List<CartLine>();
            }
        }

        private void SaveCart(List<CartLine> cart)
        {
            string json = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CartSessionKey, json);
        }
    }

    //classes as pseudo vms
    public class CartLine
    {
        public int ProductId { get; set; }
        public int Qty { get; set; }
    }

    public class CartLineView
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public string? ImageUrl { get; set; }
        public string UnitText { get; set; } = "";
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class CartPage
    {
        public List<CartLineView> Lines { get; set; } = new List<CartLineView>();
        public decimal Total { get; set; }
    }
}
