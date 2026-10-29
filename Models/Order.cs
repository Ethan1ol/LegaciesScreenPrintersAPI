using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegaciesPrintersApi.Models
{
    [Table("orders")]
    public class Order
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [MaxLength(50)]
        [Column("tracking_number")]
        public string TrackingNumber { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = "Pending"; // Pending, In Production, Completed, Delivered

        [Column("total_amount", TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(255)]
        [Column("delivery_address")]
        public string? DeliveryAddress { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}