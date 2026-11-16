using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegaciesPrintersApi.Models
{
    /// <summary>A vinyl material the shop prints with.</summary>
    [Table("materials")]
    public class Material
    {
        [Key]
        [Column("material_id")]
        public int MaterialId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("material_name")]
        public string MaterialName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("color")]
        public string Color { get; set; } = string.Empty;

        [Column("unit_price", TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        [Column("stock_quantity")]
        public int StockQuantity { get; set; }

        [MaxLength(20)]
        [Column("unit_measure")]
        public string UnitMeasure { get; set; } = "meters";

        [Column("low_stock_threshold")]
        public int LowStockThreshold { get; set; } = 15;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
