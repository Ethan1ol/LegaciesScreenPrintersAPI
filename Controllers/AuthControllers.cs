using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LegaciesPrintersApi.Data;
using LegaciesPrintersApi.Models;
using LegaciesPrintersApi.DTOs;

namespace LegaciesPrintersApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
            {
                return BadRequest(new { message = "Email already registered." });
            }

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Phone = dto.Phone,
                Role = "Customer",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDto
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                Token = "dummy-jwt-token" // Or inject standard JWT token generator
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            return Ok(new AuthResponseDto
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                Token = "dummy-jwt-token"
            });
        }
    }
}