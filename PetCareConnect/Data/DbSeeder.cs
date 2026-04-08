using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
                    await roleManager.CreateAsync(new IdentityRole(role));
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

                var result = await userManager.CreateAsync(admin, "Admin@123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "System Administrator");
            }
        }

        public static async Task SeedProducts(ApplicationDbContext db)
        {
            if (await db.Products.AnyAsync()) return;

            var products = new List<Product>
            {
                // ─── FOOD ───
                new Product { Name = "Premium Dry Dog Food", Description = "High-protein kibble made with real chicken. Supports healthy joints and a shiny coat. Grain-free formula.", Price = 34.99m, StockQuantity = 50, Category = "Food", ImageEmoji = "🦴" },
                new Product { Name = "Wet Cat Food Variety Pack", Description = "12 cans of mixed flavours including tuna, salmon, and chicken. No artificial preservatives.", Price = 24.99m, StockQuantity = 40, Category = "Food", ImageEmoji = "🐟" },
                new Product { Name = "Senior Dog Food", Description = "Specially formulated for dogs 7+ years. Supports joint health and digestive comfort.", Price = 39.99m, StockQuantity = 30, Category = "Food", ImageEmoji = "🍗" },
                new Product { Name = "Kitten Starter Pack", Description = "Complete nutrition for kittens 0–12 months. Includes wet and dry food samples.", Price = 19.99m, StockQuantity = 25, Category = "Food", ImageEmoji = "🐾" },
                new Product { Name = "Freeze-Dried Salmon Treats", Description = "Single-ingredient treats made from 100% wild-caught salmon. Perfect for training.", Price = 14.99m, StockQuantity = 60, Category = "Food", ImageEmoji = "🐠" },
                new Product { Name = "Rabbit Food Pellets", Description = "Balanced nutrition for adult rabbits. High fibre, no added sugars.", Price = 12.99m, StockQuantity = 20, Category = "Food", ImageEmoji = "🌿" },

                // ─── TOYS ───
                new Product { Name = "Rope Chew Toy", Description = "Durable braided rope toy for medium to large dogs. Helps clean teeth and massage gums.", Price = 9.99m, StockQuantity = 45, Category = "Toys", ImageEmoji = "🪢" },
                new Product { Name = "Feather Wand Cat Toy", Description = "Interactive feather wand that mimics bird movement. Extendable 90cm handle.", Price = 7.99m, StockQuantity = 55, Category = "Toys", ImageEmoji = "🪶" },
                new Product { Name = "Puzzle Treat Dispenser", Description = "Slow feeder and mental stimulation toy. Adjustable difficulty. Suitable for all dogs.", Price = 18.99m, StockQuantity = 30, Category = "Toys", ImageEmoji = "🧩" },
                new Product { Name = "Squeaky Plush Fox", Description = "Super soft plush fox toy with a built-in squeaker. Machine washable.", Price = 11.99m, StockQuantity = 35, Category = "Toys", ImageEmoji = "🦊" },
                new Product { Name = "Laser Pointer Set", Description = "Safe laser pointer for cats with 3 interchangeable tips. Auto-off safety feature.", Price = 8.99m, StockQuantity = 50, Category = "Toys", ImageEmoji = "🔴" },
                new Product { Name = "Catnip Mice 3-Pack", Description = "Set of 3 catnip-filled mice toys. Made with organic Canadian catnip.", Price = 6.99m, StockQuantity = 65, Category = "Toys", ImageEmoji = "🐭" },

                // ─── HEALTH ───
                new Product { Name = "Omega-3 Fish Oil Supplement", Description = "Daily supplement for healthy skin, coat, and joints. 180 soft chews. Bacon flavoured.", Price = 22.99m, StockQuantity = 40, Category = "Health", ImageEmoji = "💊" },
                new Product { Name = "Dental Water Additive", Description = "Add to your pet's water bowl to fight plaque and freshen breath. Tasteless and odourless.", Price = 16.99m, StockQuantity = 30, Category = "Health", ImageEmoji = "🦷" },
                new Product { Name = "Flea & Tick Prevention Collar", Description = "8-month protection against fleas and ticks. Waterproof. Fits necks up to 45cm.", Price = 29.99m, StockQuantity = 25, Category = "Health", ImageEmoji = "🐕" },
                new Product { Name = "Probiotic Digestive Support", Description = "Daily probiotic chews to support gut health and immune function. 60 count.", Price = 26.99m, StockQuantity = 20, Category = "Health", ImageEmoji = "🌱" },
                new Product { Name = "Pet First Aid Kit", Description = "Complete kit including bandages, antiseptic wipes, tweezers, and emergency guide.", Price = 34.99m, StockQuantity = 15, Category = "Health", ImageEmoji = "🩺" },
                new Product { Name = "Calming Anxiety Treats", Description = "Natural calming chews with chamomile and L-theanine. Ideal for storms and travel.", Price = 19.99m, StockQuantity = 35, Category = "Health", ImageEmoji = "😌" },
            };

            await db.Products.AddRangeAsync(products);
            await db.SaveChangesAsync();
        }
    }
}
