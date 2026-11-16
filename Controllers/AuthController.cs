using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LegaciesPrintersApi.Data;
using LegaciesPrintersApi.DTOs;
using LegaciesPrintersApi.Models;
using LegaciesPrintersApi.Services;

namespace LegaciesPrintersApi.Controllers
{
    /// <summary>Sign-in for the Android app and the website: email/password, and Google single sign-on.</summary>
    // Reference: Microsoft (2025) Create web APIs with ASP.NET Core - https://learn.microsoft.com/en-us/aspnet/core/web-api/
    // Reference: BCrypt.Net-Next contributors (2024) BCrypt.Net-Next - https://github.com/BcryptNet/bcrypt.net
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokens;
        private readonly GoogleTokenValidator _google;
        private readonly UserService _users;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext context, TokenService tokens, GoogleTokenValidator google,
            UserService users, ILogger<AuthController> logger)
        {
            _context = context;
            _tokens = tokens;
            _google = google;
            _users = users;
            _logger = logger;
        }

        /// <summary>Creates a customer account and signs it in.</summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                _logger.LogInformation("Registration rejected: email already registered.");
                return BadRequest(new { message = "Email already registered." });
            }

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = email,
                PhoneNumber = dto.PhoneNumber.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = UserRoles.Customer,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Registered new customer {UserId}.", user.UserId);
            return Ok(SignIn(user));
        }

        /// <summary>Signs in with email and password.</summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // Same message for a wrong email or password, so emails can't be guessed.
            if (user?.PasswordHash == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogInformation("Login failed.");
                return Unauthorized(new { message = "Invalid email or password." });
            }

            _logger.LogInformation("User {UserId} logged in with password.", user.UserId);
            return Ok(SignIn(user));
        }

        /// <summary>Google single sign-on.</summary>
        [HttpPost("google")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GoogleSignIn([FromBody] GoogleLoginDto dto)
        {
            if (!_google.IsConfigured)
            {
                _logger.LogError("Google sign-in attempted but Google:WebClientId is not configured.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { message = "Google sign-in is not configured on the server." });
            }

            var identity = await _google.ValidateAsync(dto.IdToken);
            if (identity == null)
            {
                return Unauthorized(new { message = "Google sign-in could not be verified." });
            }

            var user = await _users.UpsertGoogleUserAsync(identity.Subject, identity.Email, identity.Name);
            _logger.LogInformation("User {UserId} signed in with Google.", user.UserId);
            return Ok(SignIn(user));
        }

        /// <summary>Returns the signed-in user's profile.</summary>
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<MeDto>> Me()
        {
            var userId = User.GetUserId();
            var user = userId == null ? null : await _context.Users.FindAsync(userId.Value);

            // A valid token for an account that has since been deleted.
            if (user == null) return Unauthorized(new { message = "Account no longer exists." });

            return Ok(new MeDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                PhoneNumber = user.PhoneNumber,
                GoogleLinked = user.GoogleSub != null,
            });
        }

        private AuthResponseDto SignIn(User user)
        {
            var (token, expires) = _tokens.CreateToken(user);
            return new AuthResponseDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                PhoneNumber = user.PhoneNumber,
                Token = token,
                ExpiresAt = expires,
            };
        }
    }
}
