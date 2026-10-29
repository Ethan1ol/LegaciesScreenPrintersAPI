using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LegaciesPrintersApi.Data;
using LegaciesPrintersApi.Models;
using LegaciesPrintersApi.DTOs;

namespace LegaciesPrintersApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound(new { message = "Order not found." });

            return Ok(order);
        }

        [HttpGet("track/{trackingNumber}")]
        public async Task<IActionResult> TrackOrderByNumber(string trackingNumber)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.TrackingNumber.ToUpper() == trackingNumber.ToUpper());

            if (order == null) return NotFound(new { message = "Tracking number not found." });

            return Ok(order);
        }

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetUserOrders(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return Ok(orders);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            if (dto.Items == null || !dto.Items.Any())
            {
                return BadRequest(new { message = "Order must contain at least one item." });
            }

            var trackingCode = "LP-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            var total = dto.Items.Sum(i => i.UnitPrice * i.Quantity);

            var order = new Order
            {
                UserId = dto.UserId,
                TrackingNumber = trackingCode,
                Status = "Pending",
                TotalAmount = total,
                DeliveryAddress = dto.DeliveryAddress,
                CreatedAt = DateTime.UtcNow,
                OrderItems = dto.Items.Select(item => new OrderItem
                {
                    ItemName = item.ItemName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    PrintColor = item.PrintColor,
                    Size = item.Size
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
        }

        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string newStatus)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound(new { message = "Order not found." });

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(order);
        }
    }
}