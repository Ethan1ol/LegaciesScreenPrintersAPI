using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegaciesPrintersApi.Models
{
    [Table("materials")]
    public class Material
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("category")]
        public string Category { get; set; } = string.Empty; // e.g. Ink, T-Shirt, Vinyl

        [Column("stock_quantity")]
        public int StockQuantity { get; set; }

        [Column("unit_price", TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
    }
}