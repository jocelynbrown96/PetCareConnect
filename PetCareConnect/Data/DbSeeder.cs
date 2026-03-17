using Microsoft.AspNetCore.Identity;
using PetCareConnect.Models;
using System.Threading.Tasks;

namespace PetCareConnect.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdmin(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            // Seed roles
            string[] roles = { "Pet Owner", "Clinic Staff", "System Administrator" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create admin if it doesn't exist
            string adminEmail = "kimpetras@petcareconnect.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Kim",
                    LastName = "Petras",
                    Role = "System Administrator",
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(admin, "Admin@123!"); // meets password requirements
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "System Administrator");
                }
            }

        }
    }
}
