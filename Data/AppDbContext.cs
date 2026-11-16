using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using LegaciesPrintersApi.Models;

namespace LegaciesPrintersApi.Data
{
    /// <summary>Entity Framework Core database context for the Legacies Screen Printers database.</summary>
    // Reference: Microsoft (2025) Creating and configuring a model (EF Core) - https://learn.microsoft.com/en-us/ef/core/modeling/
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Material> Materials => Set<Material>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---- users ----
            modelBuilder.Entity<User>(e =>
            {
                e.HasIndex(u => u.Email).IsUnique();

                // A Google identity can be linked to at most one account.
                e.HasIndex(u => u.GoogleSub).IsUnique().HasFilter("[google_sub] IS NOT NULL");

                e.Property(u => u.Role).HasDefaultValue(UserRoles.Customer);
                e.Property(u => u.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                e.ToTable(t => t.HasCheckConstraint("CK_users_role",
                    $"[role] IN ({SqlList(UserRoles.All)})"));
            });

            // ---- materials ----
            modelBuilder.Entity<Material>(e =>
            {
                e.Property(m => m.UnitMeasure).HasDefaultValue("meters");
                e.Property(m => m.LowStockThreshold).HasDefaultValue(15);
                e.Property(m => m.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                e.ToTable(t => t.HasCheckConstraint("CK_materials_stock", "[stock_quantity] >= 0"));
            });

            // ---- orders ----
            modelBuilder.Entity<Order>(e =>
            {
                e.HasIndex(o => o.OrderRef).IsUnique();

                e.HasOne(o => o.User)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.Property(o => o.OrderType).HasDefaultValue(OrderTypes.Mobile);
                e.Property(o => o.Status).HasDefaultValue(OrderStatus.Received);
                e.Property(o => o.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

                e.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_orders_order_type", $"[order_type] IN ({SqlList(OrderTypes.All)})");
                    t.HasCheckConstraint("CK_orders_status", $"[status] IN ({SqlList(OrderStatus.All)})");
                });
            });

            // ---- order_items ----
            modelBuilder.Entity<OrderItem>(e =>
            {
                e.HasOne(i => i.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(i => i.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                // A material that appears on past orders can't be deleted.
                e.HasOne(i => i.Material)
                    .WithMany()
                    .HasForeignKey(i => i.MaterialId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.ToTable(t => t.HasCheckConstraint("CK_order_items_quantity", "[quantity] > 0"));
            });

            // All timestamps are stored in UTC.
            var utc = new ValueConverter<DateTime, DateTime>(
                toDb => toDb.Kind == DateTimeKind.Utc ? toDb : toDb.ToUniversalTime(),
                fromDb => DateTime.SpecifyKind(fromDb, DateTimeKind.Utc));

            foreach (var property in modelBuilder.Model.GetEntityTypes()
                         .SelectMany(t => t.GetProperties())
                         .Where(p => p.ClrType == typeof(DateTime)))
            {
                property.SetValueConverter(utc);
            }
        }

        /// <summary>Builds a SQL list like 'a', 'b' for CHECK constraints from a set of constants.</summary>
        private static string SqlList(IEnumerable<string> values) =>
            string.Join(", ", values.Select(v => $"'{v.Replace("'", "''")}'"));
    }
}
