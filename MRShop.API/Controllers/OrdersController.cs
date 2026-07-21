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

        if (string.IsNullOrWhiteSpace(request.ShippingAddress) || request.ShippingAddress.Length < 10)
            return BadRequest(new { message = "Valid shipping address is required (min 10 characters)." });

        var cartItems = await _mongoDb.CartItems
            .Find(c => c.UserId == userId)
            .ToListAsync();

        if (cartItems.Count == 0)
        {
            return BadRequest(new { message = "Cart is empty." });
        }

        // Validate stock for each item and use server-side prices
        var orderItems = new List<OrderItem>();
        foreach (var ci in cartItems)
        {
            var product = await _mongoDb.Products
                .Find(p => p.Id == ci.ProductId && p.Status == "published")
                .FirstOrDefaultAsync();

            if (product == null)
                return BadRequest(new { message = $"Product '{ci.ProductName}' is no longer available." });

            if (product.StockQuantity < ci.Quantity)
                return BadRequest(new { message = $"Insufficient stock for '{product.Name}'. Only {product.StockQuantity} available." });

            orderItems.Add(new OrderItem
            {
                ProductId = ci.ProductId,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = ci.Quantity,
                Image = product.ThumbnailImage
            });

            // Decrease stock
            await _mongoDb.Products.UpdateOneAsync(
                p => p.Id == ci.ProductId,
                Builders<Product>.Update.Inc(p => p.StockQuantity, -ci.Quantity)
            );
        }

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

        var order = await _mongoDb.Orders.Find(o => o.Id == id).FirstOrDefaultAsync();
        if (order == null)
        {
            return NotFound(new { message = "Order not found." });
        }

        // Sellers can only update orders containing their products
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole == "seller")
        {
            var userId = GetUserId();
            var sellerProducts = await _mongoDb.Products
                .Find(p => p.SellerId == userId)
                .ToListAsync();
            var sellerProductIds = sellerProducts.Select(p => p.Id).ToHashSet();
            var hasSellerProduct = order.Items.Any(i => sellerProductIds.Contains(i.ProductId));
            if (!hasSellerProduct)
                return Forbid();
        }

        var result = await _mongoDb.Orders.UpdateOneAsync(
            o => o.Id == id,
            Builders<Order>.Update
                .Set(o => o.Status, status)
                .Set(o => o.UpdatedAt, DateTime.UtcNow)
        );

        return Ok(new { message = $"Order status updated to {status}." });
    }

    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    // Admin - get all orders
    [Authorize(Roles = "admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _mongoDb.Orders
            .Find(_ => true)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(new
        {
            orders = orders.Select(MapToOrderResponse),
            totalCount = orders.Count
        });
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
