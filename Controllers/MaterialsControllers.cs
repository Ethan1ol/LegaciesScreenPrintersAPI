using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LegaciesPrintersApi.Data;
using LegaciesPrintersApi.Models;

namespace LegaciesPrintersApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MaterialsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMaterials()
        {
            return Ok(await _context.Materials.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> AddMaterial([FromBody] Material material)
        {
            _context.Materials.Add(material);
            await _context.SaveChangesAsync();
            return Ok(material);
        }
    }
}