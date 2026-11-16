using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegaciesPrintersApi.Models
{
    /// <summary>One design line within an order (a design printed on a material, in a quantity).</summary>
    [Table("order_items")]
    public class OrderItem
    {
        [Key]
        [Column("item_id")]
        public int ItemId { get; set; }

        [Column("order_id")]
        public int OrderId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("design_name")]
        public string DesignName { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("artwork_file_url")]
        public string? ArtworkFileUrl { get; set; }

        [Column("material_id")]
        public int MaterialId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("unit_price", TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        [Column("subtotal", TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        public Order? Order { get; set; }

        public Material? Material { get; set; }
    }
}
