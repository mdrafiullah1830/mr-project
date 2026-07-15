using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MRShop.API.DTOs;
using MRShop.API.Models;
using MRShop.API.Services;

namespace MRShop.API.Controllers;

[Authorize]
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly MongoDbService _mongoDb;

    public OrdersController(MongoDbService mongoDb)
    {
        _mongoDb = mongoDb;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var orders = await _mongoDb.Orders
            .Find(o => o.UserId == userId)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(new
        {
            orders = orders.Select(MapToOrderResponse),
            totalCount = orders.Count
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(string id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var order = await _mongoDb.Orders
            .Find(o => o.Id == id && o.UserId == userId)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return NotFound(new { message = "Order not found." });
        }

        return Ok(MapToOrderResponse(order));
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var cartItems = await _mongoDb.CartItems
            .Find(c => c.UserId == userId)
            .ToListAsync();

        if (cartItems.Count == 0)
        {
            return BadRequest(new { message = "Cart is empty." });
        }

        var orderItems = cartItems.Select(ci => new OrderItem
        {
            ProductId = ci.ProductId,
            ProductName = ci.ProductName,
            Price = ci.Price,
            Quantity = ci.Quantity,
            Image = ci.Image
        }).ToList();

        var totalAmount = orderItems.Sum(i => i.Price * i.Quantity);

        var order = new Order
        {
            UserId = userId,
            Items = orderItems,
            TotalAmount = totalAmount,
            ShippingAddress = request.ShippingAddress,
            PaymentMethod = request.PaymentMethod,
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _mongoDb.Orders.InsertOneAsync(order);

        // Clear cart after order
        await _mongoDb.CartItems.DeleteManyAsync(c => c.UserId == userId);

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, MapToOrderResponse(order));
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "admin,seller")]
    public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] string status)
    {
        var validStatuses = new[] { "pending", "confirmed", "shipped", "delivered", "cancelled" };
        if (!validStatuses.Contains(status))
        {
            return BadRequest(new { message = "Invalid status." });
        }

        var result = await _mongoDb.Orders.UpdateOneAsync(
            o => o.Id == id,
            Builders<Order>.Update
                .Set(o => o.Status, status)
                .Set(o => o.UpdatedAt, DateTime.UtcNow)
        );

        if (result.MatchedCount == 0)
        {
            return NotFound(new { message = "Order not found." });
        }

        return Ok(new { message = $"Order status updated to {status}." });
    }

    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private static OrderResponse MapToOrderResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            Items = order.Items.Select(i => new OrderItemResponse
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Price = i.Price,
                Quantity = i.Quantity,
                Image = i.Image
            }).ToList(),
            TotalAmount = order.TotalAmount,
            ShippingAddress = order.ShippingAddress,
            PaymentMethod = order.PaymentMethod,
            Status = order.Status,
            CreatedAt = order.CreatedAt
        };
    }
}
