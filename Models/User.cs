using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegaciesPrintersApi.Models
{
    /// <summary>A customer or staff account.</summary>
    [Table("users")]
    public class User
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("full_name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(120)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        // Nullable because Google sign-in accounts may not have a phone number.
        [MaxLength(20)]
        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        // BCrypt hash. Nullable because Google-only accounts have no local password.
        [MaxLength(255)]
        [Column("password_hash")]
        public string? PasswordHash { get; set; }

        // Google account subject ("sub" claim).
        [MaxLength(64)]
        [Column("google_sub")]
        public string? GoogleSub { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("role")]
        public string Role { get; set; } = UserRoles.Customer;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }

    /// <summary>Allowed roles (match the database CHECK constraint).</summary>
    public static class UserRoles
    {
        public const string Customer = "customer";
        public const string Admin = "admin";
        public static readonly string[] All = { Customer, Admin };
    }
}
