using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LegaciesPrintersApi.Data;
using LegaciesPrintersApi.DTOs;
using LegaciesPrintersApi.Models;

namespace LegaciesPrintersApi.Controllers
{
    /// <summary>The vinyl material catalogue.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MaterialsController> _logger;

        public MaterialsController(AppDbContext context, ILogger<MaterialsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>Lists every material with its price and stock level.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MaterialDto>>> GetMaterials()
        {
            var materials = await _context.Materials
                .OrderBy(m => m.MaterialName).ThenBy(m => m.Color)
                .Select(m => ToDto(m))
                .ToListAsync();
            return Ok(materials);
        }

        /// <summary>Gets one material.</summary>
        [HttpGet("{materialId:int}")]
        public async Task<ActionResult<MaterialDto>> GetMaterial(int materialId)
        {
            var material = await _context.Materials.FindAsync(materialId);
            return material == null ? NotFound(new { message = "Material not found." }) : Ok(ToDto(material));
        }

        /// <summary>Adds a material to the catalogue.</summary>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        public async Task<ActionResult<MaterialDto>> AddMaterial([FromBody] SaveMaterialDto dto)
        {
            var material = new Material();
            Apply(material, dto);
            _context.Materials.Add(material);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Material {MaterialId} added: {Name} ({Color}).",
                material.MaterialId, material.MaterialName, material.Color);
            return CreatedAtAction(nameof(GetMaterial), new { materialId = material.MaterialId }, ToDto(material));
        }

        /// <summary>Replaces a material's details, price and stock.</summary>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut("{materialId:int}")]
        public async Task<ActionResult<MaterialDto>> UpdateMaterial(int materialId, [FromBody] SaveMaterialDto dto)
        {
            var material = await _context.Materials.FindAsync(materialId);
            if (material == null) return NotFound(new { message = "Material not found." });

            Apply(material, dto);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Material {MaterialId} updated.", materialId);
            return Ok(ToDto(material));
        }

        /// <summary>Adds to or removes from stock.</summary>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPatch("{materialId:int}/stock")]
        public async Task<ActionResult<MaterialDto>> AdjustStock(int materialId, [FromBody] AdjustStockDto dto)
        {
            var material = await _context.Materials.FindAsync(materialId);
            if (material == null) return NotFound(new { message = "Material not found." });

            if (material.StockQuantity + dto.Change < 0)
            {
                return BadRequest(new { message = $"Only {material.StockQuantity} {material.UnitMeasure} in stock." });
            }

            material.StockQuantity += dto.Change;
            material.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Material {MaterialId} stock changed by {Change} to {Stock}.",
                materialId, dto.Change, material.StockQuantity);
            if (material.StockQuantity <= material.LowStockThreshold)
            {
                _logger.LogWarning("Material {MaterialId} is low on stock ({Stock}).", materialId, material.StockQuantity);
            }
            return Ok(ToDto(material));
        }

        private static void Apply(Material material, SaveMaterialDto dto)
        {
            material.MaterialName = dto.MaterialName.Trim();
            material.Color = dto.Color.Trim();
            material.UnitPrice = dto.UnitPrice;
            material.StockQuantity = dto.StockQuantity;
            material.UnitMeasure = string.IsNullOrWhiteSpace(dto.UnitMeasure) ? "meters" : dto.UnitMeasure.Trim();
            material.LowStockThreshold = dto.LowStockThreshold;
            material.UpdatedAt = DateTime.UtcNow;
        }

        private static MaterialDto ToDto(Material m) => new()
        {
            MaterialId = m.MaterialId,
            MaterialName = m.MaterialName,
            Color = m.Color,
            UnitPrice = m.UnitPrice,
            StockQuantity = m.StockQuantity,
            UnitMeasure = m.UnitMeasure,
            LowStockThreshold = m.LowStockThreshold,
        };
    }
}
