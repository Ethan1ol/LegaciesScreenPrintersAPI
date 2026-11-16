using Microsoft.EntityFrameworkCore;
using LegaciesPrintersApi.Data;
using LegaciesPrintersApi.Models;

namespace LegaciesPrintersApi.Services
{
    /// <summary>Finds or creates the local account for a verified Google identity.</summary>
    public class UserService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<UserService> _logger;

        public UserService(AppDbContext context, IConfiguration config, ILogger<UserService> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        /// <summary>Finds the user by Google id, then by email, or creates a new customer.</summary>
        public async Task<User> UpsertGoogleUserAsync(string googleSub, string email, string? name)
        {
            email = email.Trim().ToLowerInvariant();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleSub == googleSub);
            if (user != null)
            {
                _logger.LogInformation("Google sign-in matched linked user {UserId}.", user.UserId);
            }
            else
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user != null)
                {
                    user.GoogleSub = googleSub;
                    _logger.LogInformation("Linked Google identity to existing user {UserId}.", user.UserId);
                }
                else
                {
                    user = new User
                    {
                        FullName = string.IsNullOrWhiteSpace(name) ? email : name.Trim(),
                        Email = email,
                        GoogleSub = googleSub,
                        Role = UserRoles.Customer,
                    };
                    _context.Users.Add(user);
                    _logger.LogInformation("Creating new account from Google sign-in.");
                }
            }

            if (IsConfiguredAdmin(email) && user.Role != UserRoles.Admin)
            {
                user.Role = UserRoles.Admin;
                _logger.LogInformation("Granted admin role from Admin:Emails configuration.");
            }

            await _context.SaveChangesAsync();
            return user;
        }

        private bool IsConfiguredAdmin(string email)
        {
            var admins = _config.GetSection("Admin:Emails").Get<string[]>() ?? Array.Empty<string>();
            return admins.Any(a => string.Equals(a.Trim(), email, StringComparison.OrdinalIgnoreCase));
        }
    }
}
