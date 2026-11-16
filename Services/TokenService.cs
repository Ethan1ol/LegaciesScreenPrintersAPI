using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using LegaciesPrintersApi.Models;

namespace LegaciesPrintersApi.Services
{
    /// <summary>Settings for issuing and validating the API's own JWTs (config section "Jwt").</summary>
    public class JwtOptions
    {
        public const string Section = "Jwt";

        /// <summary>HMAC-SHA256 signing key.</summary>
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = "LegaciesPrintersApi";
        public string Audience { get; set; } = "LegaciesPrintersClients";
        public int ExpiryMinutes { get; set; } = 1440; // 24 hours

        /// <summary>Prefix that marks the throwaway key in appsettings.Development.json.</summary>
        public const string DevKeyPrefix = "DEV-ONLY";

        public SymmetricSecurityKey SigningKey() => new(Encoding.UTF8.GetBytes(Key));

        /// <summary>Stops the API starting with a missing, short or development-only key.</summary>
        public void Validate(bool isDevelopment)
        {
            if (Encoding.UTF8.GetByteCount(Key) < 32)
                throw new InvalidOperationException("Jwt:Key must be set and at least 32 bytes long.");
            if (!isDevelopment && Key.StartsWith(DevKeyPrefix, StringComparison.Ordinal))
                throw new InvalidOperationException("The development Jwt:Key must not be used outside Development.");
        }
    }

    /// <summary>Issues the API's own signed session token (a JWT) after a successful email/password or Google sign-in.</summary>
    // Reference: Microsoft (2025) Configure JWT bearer authentication in ASP.NET Core - https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication
    // Reference: Jones, M., Bradley, J. and Sakimura, N. (2015) RFC 7519: JSON Web Token (JWT) - https://www.rfc-editor.org/rfc/rfc7519
    public class TokenService
    {
        private readonly JwtOptions _options;

        public TokenService(JwtOptions options)
        {
            _options = options;
        }

        /// <summary>Creates a signed JWT for the user and returns it with its expiry time.</summary>
        public (string Token, DateTime ExpiresAtUtc) CreateToken(User user)
        {
            var expires = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _options.Issuer,
                Audience = _options.Audience,
                Expires = expires,
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Name, user.FullName),
                    new Claim(AuthClaims.Role, user.Role),
                }),
                SigningCredentials = new SigningCredentials(_options.SigningKey(), SecurityAlgorithms.HmacSha256),
            };

            return (new JsonWebTokenHandler().CreateToken(descriptor), expires);
        }
    }

    /// <summary>Claim names used in the API's tokens, and helpers to read them in controllers.</summary>
    public static class AuthClaims
    {
        public const string Role = "role";
        public const string Name = JwtRegisteredClaimNames.Name;

        /// <summary>The signed-in user's id from the "sub" claim, or null if not signed in.</summary>
        public static int? GetUserId(this ClaimsPrincipal principal) =>
            int.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : null;

        public static bool IsAdmin(this ClaimsPrincipal principal) => principal.IsInRole(UserRoles.Admin);
    }
}
