using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegaciesPrintersApi.Models
{
    /// <summary>A customer print order.</summary>
    [Table("orders")]
    public class Order
    {
        [Key]
        [Column("order_id")]
        public int OrderId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("order_ref")]
        public string OrderRef { get; set; } = string.Empty;

        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("order_type")]
        public string OrderType { get; set; } = OrderTypes.Mobile;

        [Column("total_amount", TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = OrderStatus.Received;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    /// <summary>The five production stages, in order.</summary>
    public static class OrderStatus
    {
        public const string Received = "Order Received & Verified";
        public const string CuttingAndPrep = "Vinyl Cutting & Screen Prep";
        public const string Printing = "Screen Printing";
        public const string ReadyForCollection = "Ready for Collection";
        public const string Completed = "Completed";

        public static readonly string[] All =
            { Received, CuttingAndPrep, Printing, ReadyForCollection, Completed };

        public static bool IsValid(string? status) => status != null && All.Contains(status);
    }

    /// <summary>Where an order was placed: the mobile app, the website, or at the shop counter.</summary>
    public static class OrderTypes
    {
        public const string Mobile = "mobile";
        public const string Web = "web";
        public const string WalkIn = "walk_in";
        public static readonly string[] All = { Mobile, Web, WalkIn };

        /// <summary>Turns the order type a client sends into mobile, web or walk_in.</summary>
        public static string Normalise(string? value)
        {
            if (value != null && value.Contains("walk", StringComparison.OrdinalIgnoreCase)) return WalkIn;
            if (string.Equals(value?.Trim(), Web, StringComparison.OrdinalIgnoreCase)) return Web;
            return Mobile;
        }
    }
}
