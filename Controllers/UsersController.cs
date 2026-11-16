using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LegaciesPrintersApi.Data;
using LegaciesPrintersApi.DTOs;
using LegaciesPrintersApi.Models;

namespace LegaciesPrintersApi.Controllers
{
    /// <summary>Customer list for the website's staff "Customers" page.</summary>
    [ApiController]
    [Authorize(Roles = UserRoles.Admin)]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Lists all accounts, newest first.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserSummaryDto>>> GetUsers()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UserSummaryDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                })
                .ToListAsync();
            return Ok(users);
        }
    }
}
