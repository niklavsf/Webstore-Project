using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebstoreProject.Data;
using WebstoreProject.Models;

namespace WebstoreProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            //top categroeis for home
            List<Category> topCategories = await _db.Categories
                .Where(c => c.ParentId == null && c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .Take(6)
                .ToListAsync();

            List<Product> allProducts = await _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();

            List<HomeSection> sections = new List<HomeSection>();

            foreach (Category top in topCategories)
            {
                List<int> categoryIds = await GetCategoryIdsUnderTop(top.CategoryId);

                List<Product> sectionProducts = allProducts
                    .Where(p => categoryIds.Contains(p.CategoryId))
                    .Take(12)
                    .ToList();

                HomeSection section = new HomeSection
                {
                    TopCategory = top,
                    Products = sectionProducts
                };

                sections.Add(section);
            }

            HomePage model = new HomePage
            {
                Sections = sections
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Info()
        {
            return View();
        }


        private async Task<List<int>> GetCategoryIdsUnderTop(int topCategoryId)
        {
            List<Category> categories = await _db.Categories
                .Where(c => c.IsActive)
                .ToListAsync();

            List<int> result = new List<int>();
            result.Add(topCategoryId);

            // children
            List<Category> children = categories.Where(c => c.ParentId == topCategoryId).ToList();
            foreach (Category c1 in children)
            {
                result.Add(c1.CategoryId);

                // grandchildren
                List<Category> grandChildren = categories.Where(c => c.ParentId == c1.CategoryId).ToList();
                foreach (Category c2 in grandChildren)
                {
                    result.Add(c2.CategoryId);
                }
            }

            return result;
        }
    }
}
