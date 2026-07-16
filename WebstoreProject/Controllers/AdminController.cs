using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WebstoreProject.Data;
using WebstoreProject.Models;

namespace WebstoreProject.Controllers
{
    //adds role
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;

        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string tab = "Products")
        {
            Admin model = new Admin();
            model.Tab = string.IsNullOrWhiteSpace(tab) ? "Products" : tab;

            List<Category> categories = await _db.Categories
                .OrderBy(c => c.ParentId)
                .ThenBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            List<Brand> brands = await _db.Brands
                .OrderBy(b => b.Name)
                .ToListAsync();

            List<Product> products = await _db.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToListAsync();

            List<DeliveryType> deliveryTypes = await _db.DeliveryTypes
                .OrderBy(d => d.DeliveryTypeId)
                .ToListAsync();

            List<Order> orders = await _db.Orders
                .AsNoTracking()
                .Include(o => o.DeliveryType)
                .OrderByDescending(o => o.OrderId)
                .Take(200)
                .ToListAsync();

            model.Orders = orders;


            model.Categories = categories;
            model.Brands = brands;
            model.Products = products;
            model.DeliveryTypes = deliveryTypes;

            model.NewCategory = new Category { IsActive = true, SortOrder = 0 };
            model.NewBrand = new Brand();
            model.NewProduct = new Product { IsActive = true, StockQty = 0, DiscountPercent = 0 };
            model.NewDeliveryType = new DeliveryType { IsActive = true };

            return View(model);
        }



        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status, string tab = "Orders")
        {
            Order? order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null)
            {
                return RedirectToAction(nameof(Index), new { tab = tab });
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                return RedirectToAction(nameof(Index), new { tab = tab });
            }

            
            List<string> allowed = new List<string> { "New", "Processing", "Shipped", "Delivered", "Cancelled" };
            if (!allowed.Contains(status))
            {
                TempData["Error"] = "Invalid status.";
                return RedirectToAction(nameof(Index), new { tab = tab });
            }

            order.Status = status;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { tab = tab });
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            Order? order = await _db.Orders
                .AsNoTracking()
                .Include(o => o.DeliveryType)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return RedirectToAction(nameof(Index), new { tab = "Orders" });
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> AddBrand(string name, string tab = "Brands")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return RedirectToAction(nameof(Index), new { tab = tab });
            }

            string trimmed = name.Trim();

            Brand? existing = await _db.Brands.FirstOrDefaultAsync(b => b.Name == trimmed);
            if (existing != null)
            {
                TempData["Error"] = "Brand already exists.";
                return RedirectToAction(nameof(Index), new { tab = tab });
            }

            Brand brand = new Brand();
            brand.Name = trimmed;

            _db.Brands.Add(brand);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Could not add brand (DB constraint).";
            }

            return RedirectToAction(nameof(Index), new { tab = tab });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBrand(int brandId, string name, string tab = "Brands")
        {
            Brand? brand = await _db.Brands.FirstOrDefaultAsync(b => b.BrandId == brandId);
            if (brand == null)
            {
                return RedirectToAction(nameof(Index), new { tab = tab });
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Brand name cannot be empty.";
                return RedirectToAction(nameof(Index), new { tab = tab });
            }

            brand.Name = name.Trim();

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Could not update brand (DB constraint).";
            }

            return RedirectToAction(nameof(Index), new { tab = tab });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBrand(int id, string tab = "Brands")
        {
            Brand? brand = await _db.Brands.FirstOrDefaultAsync(b => b.BrandId == id);
            if (brand == null)
            {
                return RedirectToAction(nameof(Index), new { tab = tab });
            }

            _db.Brands.Remove(brand);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Cannot delete brand (likely used by products).";
            }

            return RedirectToAction(nameof(Index), new { tab = tab });
        }

        //categories

        [HttpPost]
        public async Task<IActionResult> AddCategory(Category newCategory, string tab = "Categories")
        {
            if (string.IsNullOrWhiteSpace(newCategory.Name))
            {
                TempData["Error"] = "Category name is required.";
                return RedirectToAction(nameof(Index), new { tab = tab });
            }

            _db.Categories.Add(newCategory);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Could not add category (DB constraint).";
            }

            return RedirectToAction(nameof(Index), new { tab = tab });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory(int categoryId, int? parentId, string name, int sortOrder, bool isActive, string? description, string? iconUrl, string tab = "Categories")
        {
            Category? category = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
            if (category == null)
            {
                return RedirectToAction(nameof(Index), new { tab = tab });
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Category name cannot be empty.";
                return RedirectToAction(nameof(Index), new { tab = tab });
            }

            category.ParentId = parentId;
            category.Name = name.Trim();
            category.SortOrder = sortOrder;
            category.IsActive = isActive;
            category.Description = description;
            category.IconUrl = iconUrl;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Could not update category (DB constraint).";
            }

            return RedirectToAction(nameof(Index), new { tab = tab });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id, string tab = "Categories")
        {
            Category? category = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null)
            {
                return RedirectToAction(nameof(Index), new { tab = tab });
            }

            _db.Categories.Remove(category);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Cannot delete category (has products or subcategories).";
            }

            return RedirectToAction(nameof(Index), new { tab = tab });
        }

        //product stufsa

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product newProduct, string tab = "Products")
        {
            newProduct.Price = ParseDecimal(Request.Form["Price"].ToString(), 0m);
            newProduct.DiscountPercent = ParseInt(Request.Form["DiscountPercent"].ToString(), 0);

            if (newProduct.DiscountPercent < 0) newProduct.DiscountPercent = 0;
            if (newProduct.DiscountPercent > 90) newProduct.DiscountPercent = 90;

            if (string.IsNullOrWhiteSpace(newProduct.Name))
            {
                TempData["Error"] = "Product name is required.";
                return RedirectToAction(nameof(Index), new { tab = tab });
            }

            _db.Products.Add(newProduct);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Could not add product (DB constraint).";
            }

            return RedirectToAction(nameof(Index), new { tab = tab });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct(
            int productId,
            string name,
            string? description,
            int categoryId,
            int? brandId,
            int stockQty,
            bool isActive,
            string? imageUrl,
            string? amountText,
            string? unitType,
            decimal? unitSize,
            string tab = "Products")
        {
            Product? product = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product == null)
            {
                return RedirectToAction(nameof(Index), new { tab = tab });
            }

            product.Name = string.IsNullOrWhiteSpace(name) ? product.Name : name.Trim();
            product.Description = description;
            product.CategoryId = categoryId;
            product.BrandId = brandId;
            product.StockQty = stockQty;
            product.IsActive = isActive;
            product.ImageUrl = imageUrl;
            //product.AmountText = amountText;
            product.UnitType = unitType;
            product.UnitSize = Convert.ToDecimal(unitSize);

            product.Price = ParseDecimal(Request.Form["Price"].ToString(), product.Price);
            product.DiscountPercent = ParseInt(Request.Form["DiscountPercent"].ToString(), product.DiscountPercent);

            if (product.DiscountPercent < 0) product.DiscountPercent = 0;
            if (product.DiscountPercent > 90) product.DiscountPercent = 90;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Could not update product (DB constraint).";
            }

            return RedirectToAction(nameof(Index), new { tab = tab });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id, string tab = "Products")
        {
            Product? product = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null)
            {
                return RedirectToAction(nameof(Index), new { tab = tab });
            }

            _db.Products.Remove(product);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Could not delete product (DB constraint).";
            }

            return RedirectToAction(nameof(Index), new { tab = tab });
        }

        //delivery stuff

        [HttpPost]
        public async Task<IActionResult> AddDeliveryType(string name, string price, bool isActive, string tab = "DeliveryTypes")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Delivery type name is required.";
                return RedirectToAction(nameof(Index), new { tab = tab });
            }

            DeliveryType d = new DeliveryType();
            d.Name = name.Trim();
            d.Price = ParseDecimal(price, 0m);
            d.IsActive = isActive;

            _db.DeliveryTypes.Add(d);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Could not add delivery type (DB constraint).";
            }

            return RedirectToAction(nameof(Index), new { tab = tab });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDeliveryType(int deliveryTypeId, string name, string price, bool isActive, string tab = "DeliveryTypes")
        {
            DeliveryType? d = await _db.DeliveryTypes.FirstOrDefaultAsync(x => x.DeliveryTypeId == deliveryTypeId);
            if (d == null)
            {
                return RedirectToAction(nameof(Index), new { tab = tab });
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Delivery type name cannot be empty.";
                return RedirectToAction(nameof(Index), new { tab = tab });
            }

            d.Name = name.Trim();
            d.Price = ParseDecimal(price, d.Price);
            d.IsActive = isActive;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Could not update delivery type (DB constraint).";
            }

            return RedirectToAction(nameof(Index), new { tab = tab });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDeliveryType(int id, string tab = "DeliveryTypes")
        {
            DeliveryType? d = await _db.DeliveryTypes.FirstOrDefaultAsync(x => x.DeliveryTypeId == id);
            if (d == null)
            {
                return RedirectToAction(nameof(Index), new { tab = tab });
            }

            _db.DeliveryTypes.Remove(d);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Cannot delete delivery type (likely used by orders).";
            }

            return RedirectToAction(nameof(Index), new { tab = tab });
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            Product? product = await _db.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return RedirectToAction(nameof(Index), new { tab = "Products" });
            }

            List<Brand> brands = await _db.Brands
                .OrderBy(b => b.Name)
                .ToListAsync();

            List<Category> categories = await _db.Categories
                .OrderBy(c => c.ParentId)
                .ThenBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            ViewBag.Brands = brands;
            ViewBag.AllCategories = categories;

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(Product formProduct)
        {
            Product? product = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == formProduct.ProductId);
            if (product == null)
            {
                return RedirectToAction(nameof(Index), new { tab = "Products" });
            }

            // fields basic
            product.Name = formProduct.Name ?? "";
            product.Description = formProduct.Description;
            product.ImageUrl = formProduct.ImageUrl;

            product.StockQty = formProduct.StockQty;
            product.IsActive = formProduct.IsActive;

            product.CategoryId = formProduct.CategoryId;
            product.BrandId = formProduct.BrandId;

            // unit stuffs
            product.UnitType = formProduct.UnitType;
            product.UnitSize = formProduct.UnitSize;
            //product.AmountText = formProduct.AmountText;

            // nutrition stuffs
            product.EnergyKj = formProduct.EnergyKj;
            product.EnergyKcal = formProduct.EnergyKcal;
            product.FatG = ParseNullableDecimal(Request.Form["FatG"].ToString());
            product.SatFatG = ParseNullableDecimal(Request.Form["SatFatG"].ToString());
            product.CarbsG = ParseNullableDecimal(Request.Form["CarbsG"].ToString());
            product.SugarG = ParseNullableDecimal(Request.Form["SugarG"].ToString());
            product.ProteinG = ParseNullableDecimal(Request.Form["ProteinG"].ToString());
            product.SaltG = ParseNullableDecimal(Request.Form["SaltG"].ToString());

            //for price and discount 
            product.Price = ParseDecimal(Request.Form["Price"].ToString(), product.Price);
            product.DiscountPercent = ParseInt(Request.Form["DiscountPercent"].ToString(), product.DiscountPercent);

            if (product.DiscountPercent < 0) product.DiscountPercent = 0;
            if (product.DiscountPercent > 90) product.DiscountPercent = 90;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { tab = "Products" });
        }


        // helper functs
        private decimal ParseDecimal(string input, decimal fallback)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return fallback;
            }

            string normalized = input.Replace(',', '.');

            bool ok = decimal.TryParse(
                normalized,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out decimal value
            );

            if (!ok)
            {
                return fallback;
            }

            return value;
        }

        private decimal? ParseNullableDecimal(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            string normalized = input.Replace(',', '.');

            bool ok = decimal.TryParse(
                normalized,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out decimal value
            );

            if (!ok)
            {
                return null;
            }

            return value;
        }

        private int ParseInt(string input, int fallback)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return fallback;
            }

            bool ok = int.TryParse(input, out int value);
            if (!ok)
            {
                return fallback;
            }

            return value;
        }
    }
}
