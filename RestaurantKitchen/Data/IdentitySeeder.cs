using Microsoft.AspNetCore.Identity;
using RestaurantKitchen.Models;

namespace RestaurantKitchen.Data
{
    /// <summary>
    /// Створює ролі Admin і User та обліковий запис адміністратора,
    /// якщо їх ще немає в базі.
    /// </summary>
    public static class IdentitySeeder
    {
        public const string AdminEmail = "admin@kitchen.ua";
        public const string AdminPassword = "admin123";

        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in new[] { Roles.Admin, Roles.User })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            var admin = await userManager.FindByEmailAsync(AdminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = AdminEmail,
                    Email = AdminEmail,
                    FullName = "Адміністратор",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(admin, AdminPassword);
            }

            if (!await userManager.IsInRoleAsync(admin, Roles.Admin))
                await userManager.AddToRoleAsync(admin, Roles.Admin);
        }
    }
}