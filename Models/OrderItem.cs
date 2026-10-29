using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegaciesPrintersApi.Models
{
    [Table("order_items")]
    public class OrderItem
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("order_id")]
        public int OrderId { get; set; }

        [MaxLength(150)]
        [Column("item_name")]
        public string ItemName { get; set; } = string.Empty;

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("unit_price", TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [MaxLength(50)]
        [Column("print_color")]
        public string? PrintColor { get; set; }

        [MaxLength(50)]
        [Column("size")]
        public string? Size { get; set; }

        // Navigation
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }
    }
}