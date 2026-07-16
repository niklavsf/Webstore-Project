using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebstoreProject.Data;
using WebstoreProject.Models;

namespace WebstoreProject.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _db;

        public ProductsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? categoryId, string q, string sort, decimal? min, decimal? max, int page = 1)
        {
            int pageSize = 16; 

            IQueryable<Product> query = _db.Products
                .AsNoTracking()
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            Category? category = null;
            if (categoryId.HasValue)
            {
                category = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryId == categoryId.Value);

                List<Category> allCats = await _db.Categories
                    .AsNoTracking()
                    .Select(c => new Category
                    {
                        CategoryId = c.CategoryId,
                        ParentId = c.ParentId,
                        Name = c.Name
                    })
                    .ToListAsync();

                Dictionary<int, List<int>> childrenMap = new Dictionary<int, List<int>>();
                foreach (Category c in allCats)
                {
                    if (c.ParentId.HasValue)
                    {
                        int pid = c.ParentId.Value;

                        if (!childrenMap.ContainsKey(pid))
                        {
                            childrenMap[pid] = new List<int>();
                        }

                        childrenMap[pid].Add(c.CategoryId);
                    }
                }

                List<int> allowedCategoryIds = new List<int>();
                Queue<int> queue = new Queue<int>();

                allowedCategoryIds.Add(categoryId.Value);
                queue.Enqueue(categoryId.Value);

                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();

                    if (childrenMap.ContainsKey(current))
                    {
                        List<int> kids = childrenMap[current];
                        for (int i = 0; i < kids.Count; i++)
                        {
                            int childId = kids[i];
                            if (!allowedCategoryIds.Contains(childId))
                            {
                                allowedCategoryIds.Add(childId);
                                queue.Enqueue(childId);
                            }
                        }
                    }
                }

                query = query.Where(p => allowedCategoryIds.Contains(p.CategoryId));
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                string term = q.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(term));
            }

            if (min.HasValue)
            {
                decimal minValue = min.Value;
                query = query.Where(p => (p.DiscountPercent > 0 ? (p.Price * (100 - p.DiscountPercent) / 100) : p.Price) >= minValue);
            }

            if (max.HasValue)
            {
                decimal maxValue = max.Value;
                query = query.Where(p => (p.DiscountPercent > 0 ? (p.Price * (100 - p.DiscountPercent) / 100) : p.Price) <= maxValue);
            }

            if (string.IsNullOrWhiteSpace(sort))
            {
                sort = "name_asc";
            }

            if (sort == "price_asc")
            {
                query = query.OrderBy(p => (p.DiscountPercent > 0 ? (p.Price * (100 - p.DiscountPercent) / 100) : p.Price));
            }
            else if (sort == "price_desc")
            {
                query = query.OrderByDescending(p => (p.DiscountPercent > 0 ? (p.Price * (100 - p.DiscountPercent) / 100) : p.Price));
            }
            else if (sort == "name_desc")
            {
                query = query.OrderByDescending(p => p.Name);
            }
            else
            {
                query = query.OrderBy(p => p.Name);
            }

            int totalCount = await query.CountAsync();
            int totalPages = (int)System.Math.Ceiling(totalCount / (double)pageSize);

            if (page < 1) page = 1;
            if (totalPages > 0 && page > totalPages) page = totalPages;

            List<Product> items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Category = category;
            ViewBag.CategoryId = categoryId;
            ViewBag.Q = q ?? "";
            ViewBag.Sort = sort;
            ViewBag.Min = min;
            ViewBag.Max = max;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;

            return View(items);
        }

        private HashSet<int> GetDescendantCategoryIds(List<Category> all, int rootId)
        {
            HashSet<int> ids = new HashSet<int>();
            Queue<int> q = new Queue<int>();

            ids.Add(rootId);
            q.Enqueue(rootId);

            while (q.Count > 0)
            {
                int current = q.Dequeue();

                List<Category> children = all
                    .Where(c => c.ParentId.HasValue && c.ParentId.Value == current && c.IsActive)
                    .ToList();

                foreach (Category child in children)
                {
                    if (!ids.Contains(child.CategoryId))
                    {
                        ids.Add(child.CategoryId);
                        q.Enqueue(child.CategoryId);
                    }
                }
            }

            return ids;
        }
    }

    public class ProductsPageModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";

        public List<Product> Products { get; set; } = new List<Product>();

        public int Page { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
    }
}
