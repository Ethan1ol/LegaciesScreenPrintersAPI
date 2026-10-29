namespace LegaciesPrintersApi.DTOs
{
    public class RegisterDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    public class CreateOrderItemDto
    {
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? PrintColor { get; set; }
        public string? Size { get; set; }
    }

    public class CreateOrderDto
    {
        public int UserId { get; set; }
        public string? DeliveryAddress { get; set; }
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }
}