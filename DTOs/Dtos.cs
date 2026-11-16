using System.ComponentModel.DataAnnotations;

namespace LegaciesPrintersApi.DTOs
{
    // Data Transfer Objects: the JSON shapes the API sends and receives.

    // ---------- Auth ----------

    public class RegisterDto
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(120)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, MinLength(6), MaxLength(100)]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>Body for Google sign-in: the ID token the app or website got from Google.</summary>
    public class GoogleLoginDto
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }

        /// <summary>The API's signed JWT.</summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>When the token stops working (UTC).</summary>
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>The signed-in user's profile, returned by GET api/auth/me.</summary>
    public class MeDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public bool GoogleLinked { get; set; }
    }

    // ---------- Users (staff portal) ----------

    public class UserSummaryDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // ---------- Materials ----------

    public class MaterialDto
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int StockQuantity { get; set; }
        public string UnitMeasure { get; set; } = "meters";
        public int LowStockThreshold { get; set; }
    }

    /// <summary>Body for creating or updating a material (staff only).</summary>
    public class SaveMaterialDto
    {
        [Required, MaxLength(100)]
        public string MaterialName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Color { get; set; } = string.Empty;

        [Range(0.01, 100000)]
        public decimal UnitPrice { get; set; }

        [Range(0, 1000000)]
        public int StockQuantity { get; set; }

        [MaxLength(20)]
        public string UnitMeasure { get; set; } = "meters";

        [Range(0, 1000000)]
        public int LowStockThreshold { get; set; } = 15;
    }

    /// <summary>Body for adjusting stock up or down.</summary>
    public class AdjustStockDto
    {
        [Range(-1000000, 1000000)]
        public int Change { get; set; }
    }

    // ---------- Orders ----------

    public class CreateOrderItemDto
    {
        [Required, MaxLength(100)]
        public string DesignName { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int MaterialId { get; set; }

        [Range(1, 10000)]
        public int Quantity { get; set; }

        [MaxLength(255)]
        public string? ArtworkFileUrl { get; set; }
    }

    public class CreateOrderDto
    {
        // Ignored for customers: the order always belongs to the signed-in user (from the JWT).
        public int UserId { get; set; }

        // "mobile"/"Remote" from the app, "walk-in" from the website; normalised server-side.
        public string? OrderType { get; set; }

        [Required, MinLength(1)]
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    public class CreateOrderResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public string OrderRef { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public decimal Total { get; set; }
    }

    public class OrderItemDto
    {
        public int ItemId { get; set; }
        public int OrderId { get; set; }
        public string DesignName { get; set; } = string.Empty;
        public string? ArtworkFileUrl { get; set; }
        public int MaterialId { get; set; }
        public string? MaterialName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class OrderDto
    {
        public int OrderId { get; set; }
        public string OrderRef { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public string OrderType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class UpdateStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
