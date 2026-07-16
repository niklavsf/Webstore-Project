using Microsoft.AspNetCore.Identity;
using WebstoreProject.Models;

namespace WebstoreProject.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider services)
        {
            RoleManager<IdentityRole> roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            IdentityRole? adminRole = await roleManager.FindByNameAsync("Admin");
            if (adminRole == null)
            {
                IdentityRole role = new IdentityRole("Admin");
                await roleManager.CreateAsync(role);
            }
        }

        // For adding adminb
        public static async Task EnsureAdminUserAsync(IServiceProvider services, string email)
        {
            UserManager<ApplicationUser> userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            ApplicationUser? user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return;
            }

            bool isAdmin = await userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}
