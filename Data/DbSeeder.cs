using Microsoft.EntityFrameworkCore;
using LegaciesPrintersApi.Models;

namespace LegaciesPrintersApi.Data
{
    /// <summary>Fills an empty database with demo data so the app and website have something to show.</summary>
    // Reference: Microsoft (2025) Data seeding (EF Core) - https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding
    public static class DbSeeder
    {
        /// <summary>Password for the demo customer accounts.</summary>
        public const string DemoCustomerPassword = "Password123!";

        public static async Task SeedAsync(AppDbContext db, IConfiguration config, ILogger logger)
        {
            await SeedUsersAsync(db, config, logger);
            await SeedMaterialsAsync(db, logger);
            await SeedOrdersAsync(db, logger);
        }

        private static async Task SeedUsersAsync(AppDbContext db, IConfiguration config, ILogger logger)
        {
            if (await db.Users.AnyAsync()) return;

            // The admin account is only created when an admin password is configured.
            var adminPassword = config["Seed:AdminPassword"];
            if (!string.IsNullOrWhiteSpace(adminPassword))
            {
                db.Users.Add(NewUser("Legacies Admin", "admin@example.com", "0820000000",
                    adminPassword, UserRoles.Admin));
            }
            else
            {
                logger.LogWarning("Seed:AdminPassword is not set; no admin account was seeded.");
            }

            var customers = new (string Name, string Email, string Phone)[]
            {
                ("Demo Customer One", "customer1@example.com", "0820000001"),
                ("Demo Customer Two", "customer2@example.com", "0820000002"),
                ("Demo Customer Three", "customer3@example.com", "0820000003"),
                ("Demo Customer Four", "customer4@example.com", "0820000004"),
                ("Demo Customer Five", "customer5@example.com", "0820000005"),
            };
            foreach (var c in customers)
            {
                db.Users.Add(NewUser(c.Name, c.Email, c.Phone, DemoCustomerPassword, UserRoles.Customer));
            }

            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} users.", await db.Users.CountAsync());
        }

        private static User NewUser(string name, string email, string phone, string password, string role) => new()
        {
            FullName = name,
            Email = email,
            PhoneNumber = phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
        };

        private static async Task SeedMaterialsAsync(AppDbContext db, ILogger logger)
        {
            if (await db.Materials.AnyAsync()) return;

            // Same catalogue as the team's original SQL script.
            db.Materials.AddRange(
                Mat("Gloss Vinyl Standard", "Gloss White", 45.00m, 120, 20),
                Mat("Gloss Vinyl Standard", "Jet Black", 45.00m, 85, 20),
                Mat("Matte Vinyl Premium", "Matte Gold", 65.00m, 8, 15),
                Mat("Matte Vinyl Premium", "Matte Black", 60.00m, 110, 20),
                Mat("Heavy Duty Banner Vinyl (10oz)", "White / Gold", 75.00m, 42, 10),
                Mat("Reflective Safety Vinyl", "Silver Grey", 95.00m, 5, 10),
                Mat("Fluorescent Neon Vinyl", "Neon Green", 55.00m, 30, 10),
                Mat("Heat Transfer Garment Vinyl", "Navy Blue", 50.00m, 60, 15),
                Mat("Frosted Window Etch Film", "Translucent", 85.00m, 18, 10),
                Mat("Holographic Prism Vinyl", "Multi-color", 110.00m, 12, 15));

            await db.SaveChangesAsync();
            logger.LogInformation("Seeded 10 materials.");
        }

        private static Material Mat(string name, string color, decimal price, int stock, int threshold) => new()
        {
            MaterialName = name,
            Color = color,
            UnitPrice = price,
            StockQuantity = stock,
            LowStockThreshold = threshold,
        };

        private static async Task SeedOrdersAsync(AppDbContext db, ILogger logger)
        {
            if (await db.Orders.AnyAsync()) return;

            var customers = await db.Users.Where(u => u.Role == UserRoles.Customer)
                .OrderBy(u => u.UserId).ToListAsync();
            var materials = await db.Materials.OrderBy(m => m.MaterialId).ToListAsync();
            if (customers.Count == 0 || materials.Count == 0) return;

            // (order ref, customer index, type, status, design, material index, quantity)
            var seed = new (string Ref, int Cust, string Type, string Status, string Design, int Mat, int Qty)[]
            {
                ("ORD5824", 0, OrderTypes.Mobile, OrderStatus.CuttingAndPrep, "Glass Block Back Logo", 0, 10),
                ("ORD9624", 1, OrderTypes.Mobile, OrderStatus.Printing, "Club Rugby Crest Emblem", 2, 9),
                ("ORD1001", 2, OrderTypes.WalkIn, OrderStatus.ReadyForCollection, "Storefront Window Lettering", 1, 5),
                ("ORD1002", 3, OrderTypes.Mobile, OrderStatus.Received, "Fleet Delivery Vehicle Decal", 6, 10),
                ("ORD1003", 4, OrderTypes.WalkIn, OrderStatus.Completed, "Gym Member Hoodies Transfer", 7, 7),
                ("ORD1004", 0, OrderTypes.Mobile, OrderStatus.Completed, "Varsity Jacket Chest Badges", 4, 8),
            };

            foreach (var s in seed)
            {
                var material = materials[s.Mat % materials.Count];
                var subtotal = material.UnitPrice * s.Qty;
                db.Orders.Add(new Order
                {
                    OrderRef = s.Ref,
                    UserId = customers[s.Cust % customers.Count].UserId,
                    OrderType = s.Type,
                    Status = s.Status,
                    TotalAmount = subtotal,
                    OrderItems =
                    {
                        new OrderItem
                        {
                            DesignName = s.Design,
                            MaterialId = material.MaterialId,
                            Quantity = s.Qty,
                            UnitPrice = material.UnitPrice,
                            Subtotal = subtotal,
                        }
                    }
                });
            }

            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} orders.", seed.Length);
        }
    }
}
