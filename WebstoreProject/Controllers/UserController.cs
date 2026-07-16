using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebstoreProject.Data;
using WebstoreProject.Models;

namespace WebstoreProject.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(AppDbContext db, UserManager<ApplicationUser> userManager)
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
            List<Order> orders = await _db.Orders
            .Where(o => o.UserId == user.Id)
            .Include(o => o.DeliveryType)
            .OrderByDescending(o => o.OrderId)
            .ToListAsync();

            List<OrderItem> items = await _db.OrderItems
            .Where(oi => orders.Select(o => o.OrderId).Contains(oi.OrderId))
            .Include(oi => oi.Product)
            .ToListAsync();

            ViewBag.Orders = orders;
            ViewBag.OrderItems = items;


            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string firstName, string lastName, string phoneNumber, string addressLine1, string city, string postalCode)
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Index));
            }

            user.FirstName = firstName;
            user.LastName = lastName;
            user.PhoneNumber = phoneNumber;
            user.Address = addressLine1;
            user.City = city;
            user.PostalCode = postalCode;

            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }
    }
}
