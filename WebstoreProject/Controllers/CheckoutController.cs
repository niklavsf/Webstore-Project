using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebstoreProject.Data;
using WebstoreProject.Models;

namespace WebstoreProject.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private const string CartSessionKey = "CART";

        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            List<CartLine> cart = GetCart();
            if (cart.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            List<int> ids = cart.Select(x => x.ProductId).ToList();

            List<Product> products = await _db.Products
                .Where(p => ids.Contains(p.ProductId))
                .ToListAsync();

            List<DeliveryType> deliveryTypes = await _db.DeliveryTypes
                .Where(d => d.IsActive)
                .OrderBy(d => d.DeliveryTypeId)
                .ToListAsync();

            CheckoutPage model = BuildCheckoutModel(cart, products, deliveryTypes);

            string address = BuildUserAddress(user);
            if (!string.IsNullOrWhiteSpace(address))
            {
                model.DeliveryAddress = address;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(int deliveryTypeId, string deliveryAddress)
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            List<CartLine> cart = GetCart();
            if (cart.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            DeliveryType? deliveryType = await _db.DeliveryTypes
                .FirstOrDefaultAsync(d => d.DeliveryTypeId == deliveryTypeId && d.IsActive);

            if (deliveryType == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(deliveryAddress))
            {
                deliveryAddress = "-";
            }

            List<int> ids = cart.Select(x => x.ProductId).ToList();

            List<Product> products = await _db.Products
                .Where(p => ids.Contains(p.ProductId))
                .ToListAsync();

            decimal itemsTotal = 0m;

            foreach (CartLine line in cart)
            {
                Product? p = products.FirstOrDefault(x => x.ProductId == line.ProductId);
                if (p == null) continue;

                decimal unitPrice = p.HasDiscount ? p.DiscountedPrice : p.Price;
                decimal lineTotal = unitPrice * line.Qty;

                itemsTotal += lineTotal;
            }

            decimal deliveryFee = deliveryType.Price;
            decimal totalPrice = itemsTotal + deliveryFee;

            Order order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.UtcNow,
                Status = "New",
                DeliveryTypeId = deliveryType.DeliveryTypeId,
                DeliveryFee = Round2(deliveryFee),
                TotalPrice = Round2(totalPrice),
                DeliveryAddress = deliveryAddress.Trim()


            };

            order.PaymentMethod = Request.Form["PaymentMethod"].ToString();
            if (string.IsNullOrWhiteSpace(order.PaymentMethod))
            {
                order.PaymentMethod = "Cash";
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            foreach (CartLine line in cart)
            {
                Product? p = products.FirstOrDefault(x => x.ProductId == line.ProductId);
                if (p == null) continue;

                decimal unitPrice = p.HasDiscount ? p.DiscountedPrice : p.Price;
                decimal lineTotal = unitPrice * line.Qty;

                OrderItem item = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = p.ProductId,
                    Quantity = line.Qty,
                    UnitPrice = Round2(unitPrice),
                    LineTotal = Round2(lineTotal)
                };

                _db.OrderItems.Add(item);
            }

            await _db.SaveChangesAsync();

            HttpContext.Session.Remove(CartSessionKey);

            return RedirectToAction("Index", "User");
        }

        // helper functs

        private CheckoutPage BuildCheckoutModel(List<CartLine> cart, List<Product> products, List<DeliveryType> deliveryTypes)
        {
            CheckoutPage model = new CheckoutPage
            {
                Lines = new List<CheckoutLine>(),
                DeliveryTypes = deliveryTypes,
                DeliveryTypeId = deliveryTypes.Count > 0 ? deliveryTypes[0].DeliveryTypeId : 0,
                DeliveryAddress = ""
            };

            decimal itemsTotal = 0m;

            foreach (CartLine line in cart)
            {
                Product? p = products.FirstOrDefault(x => x.ProductId == line.ProductId);
                if (p == null) continue;

                decimal unitPrice = p.HasDiscount ? p.DiscountedPrice : p.Price;
                decimal lineTotal = unitPrice * line.Qty;

                CheckoutLine vm = new CheckoutLine
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Qty = line.Qty,
                    UnitPrice = Round2(unitPrice),
                    LineTotal = Round2(lineTotal)
                };

                model.Lines.Add(vm);
                itemsTotal += lineTotal;
            }

            model.ItemsTotal = Round2(itemsTotal);
            model.DeliveryFee = 0m;
            model.Total = Round2(itemsTotal);

            return model;
        }

        private string BuildUserAddress(ApplicationUser user)
        {
            string addr = user.Address ?? "";
            string city = user.City ?? "";
            string pc = user.PostalCode ?? "";

            string combined = (addr + ", " + city + ", " + pc).Trim().Trim(',', ' ');
            return combined;
        }

        private decimal Round2(decimal x)
        {
            return Math.Round(x, 2, MidpointRounding.AwayFromZero);
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

        private class CartLine
        {
            public int ProductId { get; set; }
            public int Qty { get; set; }
        }
    }

    // classes as pseudo vms

    public class CheckoutLine
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class CheckoutPage
    {
        public List<CheckoutLine> Lines { get; set; } = new List<CheckoutLine>();
        public List<DeliveryType> DeliveryTypes { get; set; } = new List<DeliveryType>();

        public int DeliveryTypeId { get; set; }
        public string DeliveryAddress { get; set; } = "";

        public decimal ItemsTotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total { get; set; }
    }
}
