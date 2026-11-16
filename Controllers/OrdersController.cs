using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LegaciesPrintersApi.Data;
using LegaciesPrintersApi.DTOs;
using LegaciesPrintersApi.Models;
using LegaciesPrintersApi.Services;

namespace LegaciesPrintersApi.Controllers
{
    /// <summary>Print orders: placing them (app and website), tracking them by reference.</summary>
    // Reference: Microsoft (2025) Transactions (EF Core) - https://learn.microsoft.com/en-us/ef/core/saving/transactions
    // Reference: Microsoft (2025) ExecuteUpdate and ExecuteDelete (EF Core) - https://learn.microsoft.com/en-us/ef/core/saving/execute-insert-update-delete
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(AppDbContext context, ILogger<OrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>Lists all orders, newest first.</summary>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
        {
            var orders = await OrdersWithDetails()
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            return Ok(orders.Select(o => ToDto(o)));
        }

        /// <summary>Lists the signed-in customer's own orders, newest first.</summary>
        [Authorize]
        [HttpGet("mine")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyOrders()
        {
            var userId = User.GetUserId();
            var orders = await OrdersWithDetails()
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            return Ok(orders.Select(o => ToDto(o)));
        }

        /// <summary>Gets one order by its database id.</summary>
        [Authorize]
        [HttpGet("{orderId:int}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(int orderId)
        {
            var order = await OrdersWithDetails().FirstOrDefaultAsync(o => o.OrderId == orderId);

            // 404 rather than 403 for someone else's order, so ids can't be probed.
            if (order == null || !CanSeeFullDetails(order)) return NotFound(new { message = "Order not found." });
            return Ok(ToDto(order));
        }

        /// <summary>Tracks an order by its reference (no sign-in needed).</summary>
        [AllowAnonymous]
        [HttpGet("track/{orderRef}")]
        public async Task<ActionResult<OrderDto>> TrackOrder(string orderRef)
        {
            var reference = orderRef.Trim().ToUpperInvariant();
            var order = await OrdersWithDetails().FirstOrDefaultAsync(o => o.OrderRef == reference);

            if (order == null)
            {
                _logger.LogInformation("Track lookup found no order for {OrderRef}.", reference);
                return NotFound(new { message = "Order reference not found." });
            }
            return Ok(ToDto(order, masked: !CanSeeFullDetails(order)));
        }

        /// <summary>Places an order.</summary>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(CreateOrderResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            // The order belongs to the signed-in user.
            var signedInId = User.GetUserId()!.Value;
            dto.UserId = User.IsAdmin() && dto.UserId > 0 ? dto.UserId : signedInId;

            if (!await _context.Users.AnyAsync(u => u.UserId == dto.UserId))
            {
                return BadRequest(new { message = "Unknown customer." });
            }

            var materialIds = dto.Items.Select(i => i.MaterialId).Distinct().ToList();
            var materials = await _context.Materials
                .Where(m => materialIds.Contains(m.MaterialId))
                .ToDictionaryAsync(m => m.MaterialId);

            var missing = materialIds.Where(id => !materials.ContainsKey(id)).ToList();
            if (missing.Count > 0)
            {
                return BadRequest(new { message = $"Unknown material id(s): {string.Join(", ", missing)}." });
            }

            // Run the transaction inside the retry strategy.
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(() => PlaceOrderAsync(dto, materials));
        }

        /// <summary>Reserves stock and saves the order in one transaction.</summary>
        private async Task<IActionResult> PlaceOrderAsync(CreateOrderDto dto, Dictionary<int, Material> materials)
        {
            _context.ChangeTracker.Clear();
            await using var transaction = await _context.Database.BeginTransactionAsync();

            // Reduce stock atomically: the UPDATE only succeeds if enough stock remains.
            foreach (var group in dto.Items.GroupBy(i => i.MaterialId))
            {
                var needed = group.Sum(i => i.Quantity);
                var updated = await _context.Materials
                    .Where(m => m.MaterialId == group.Key && m.StockQuantity >= needed)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(m => m.StockQuantity, m => m.StockQuantity - needed)
                        .SetProperty(m => m.UpdatedAt, DateTime.UtcNow));

                if (updated == 0)
                {
                    var m = materials[group.Key];
                    _logger.LogWarning("Order rejected: not enough stock of material {MaterialId}.", group.Key);
                    return Conflict(new
                    {
                        message = $"Not enough {m.MaterialName} ({m.Color}) in stock for {needed} {m.UnitMeasure}."
                    });
                }
            }

            var order = new Order
            {
                OrderRef = await GenerateOrderRefAsync(),
                UserId = dto.UserId,
                OrderType = OrderTypes.Normalise(dto.OrderType),
                Status = OrderStatus.Received,
                OrderItems = dto.Items.Select(i =>
                {
                    var price = materials[i.MaterialId].UnitPrice;
                    return new OrderItem
                    {
                        DesignName = i.DesignName.Trim(),
                        ArtworkFileUrl = i.ArtworkFileUrl,
                        MaterialId = i.MaterialId,
                        Quantity = i.Quantity,
                        UnitPrice = price,
                        Subtotal = price * i.Quantity,
                    };
                }).ToList(),
            };
            order.TotalAmount = order.OrderItems.Sum(i => i.Subtotal);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Order {OrderRef} placed by user {UserId}: {Count} item(s), total R{Total}.",
                order.OrderRef, order.UserId, order.OrderItems.Count, order.TotalAmount);

            return CreatedAtAction(nameof(GetOrderById), new { orderId = order.OrderId }, new CreateOrderResponseDto
            {
                Message = "Order placed successfully.",
                OrderRef = order.OrderRef,
                OrderId = order.OrderId,
                Total = order.TotalAmount,
            });
        }

        /// <summary>Moves an order to another production stage.</summary>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut("{orderId:int}/status")]
        public async Task<ActionResult<OrderDto>> UpdateOrderStatus(int orderId, [FromBody] UpdateStatusDto dto)
        {
            if (!OrderStatus.IsValid(dto.Status))
            {
                return BadRequest(new
                {
                    message = "Invalid status.",
                    allowed = OrderStatus.All,
                });
            }

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound(new { message = "Order not found." });

            var previous = order.Status;
            order.Status = dto.Status;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {OrderRef} status: '{From}' -> '{To}'.", order.OrderRef, previous, dto.Status);

            var updated = await OrdersWithDetails().FirstAsync(o => o.OrderId == orderId);
            return Ok(ToDto(updated));
        }

        /// <summary>Generates a unique public reference like ORD482913.</summary>
        private async Task<string> GenerateOrderRefAsync()
        {
            while (true)
            {
                var candidate = "ORD" + RandomNumberGenerator.GetInt32(100000, 1000000);
                if (!await _context.Orders.AnyAsync(o => o.OrderRef == candidate)) return candidate;
            }
        }

        private IQueryable<Order> OrdersWithDetails() => _context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.OrderItems).ThenInclude(i => i.Material);

        /// <summary>True for the order's owner and for staff.</summary>
        private bool CanSeeFullDetails(Order order) =>
            User.IsAdmin() || (User.GetUserId() is int id && id == order.UserId);

        /// <summary>"0820000001" becomes "082*****01"; anything short is fully hidden.</summary>
        private static string MaskContact(string contact) =>
            contact.Length <= 5 ? new string('*', contact.Length)
                : contact[..3] + new string('*', contact.Length - 5) + contact[^2..];

        private static OrderDto ToDto(Order o, bool masked = false) => new()
        {
            OrderId = o.OrderId,
            OrderRef = o.OrderRef,
            UserId = masked ? 0 : o.UserId,
            ClientName = masked
                ? (o.User?.FullName ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty
                : o.User?.FullName ?? string.Empty,
            Contact = masked
                ? MaskContact(o.User?.PhoneNumber ?? string.Empty)
                : o.User?.PhoneNumber ?? o.User?.Email ?? string.Empty,
            OrderType = o.OrderType,
            Status = o.Status,
            TotalAmount = o.TotalAmount,
            CreatedAt = o.CreatedAt,
            Items = o.OrderItems.OrderBy(i => i.ItemId).Select(i => new OrderItemDto
            {
                ItemId = i.ItemId,
                OrderId = i.OrderId,
                DesignName = i.DesignName,
                ArtworkFileUrl = i.ArtworkFileUrl,
                MaterialId = i.MaterialId,
                MaterialName = i.Material == null ? null : $"{i.Material.MaterialName} ({i.Material.Color})",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Subtotal = i.Subtotal,
            }).ToList(),
        };
    }
}
