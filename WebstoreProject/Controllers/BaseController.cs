using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebstoreProject.Data;
using WebstoreProject.Models;

namespace WebstoreProject.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly AppDbContext _db;

        protected BaseController(AppDbContext db)
        {
            _db = db;
        }

        //loads categories
        protected async Task LoadCategoriesForMenu()
        {
            List<Category> categories = await _db.Categories
                .Where(c => c.ParentId == null && c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .Include(c => c.Children.Where(x => x.IsActive).OrderBy(x => x.SortOrder).ThenBy(x => x.Name))
                    .ThenInclude(c2 => c2.Children.Where(x => x.IsActive).OrderBy(x => x.SortOrder).ThenBy(x => x.Name))
                .ToListAsync();

            ViewBag.MenuCategories = categories;
        }
    }
}
