using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MRShop.API.Models;
using MRShop.API.Services;

namespace MRShop.API.Controllers;

[Authorize(Roles = "seller")]
[ApiController]
[Route("api/seller")]
public class SellerDashboardController : ControllerBase
{
    private readonly MongoDbService _mongoDb;

    public SellerDashboardController(MongoDbService mongoDb)
    {
        _mongoDb = mongoDb;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var profile = await _mongoDb.SellerProfiles
            .Find(p => p.UserId == userId)
            .FirstOrDefaultAsync();

        if (profile == null)
        {
            return NotFound(new { message = "Seller profile not found." });
        }

        return Ok(profile);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateSellerProfileRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var profile = await _mongoDb.SellerProfiles
            .Find(p => p.UserId == userId)
            .FirstOrDefaultAsync();

        if (profile == null)
        {
            return NotFound(new { message = "Seller profile not found." });
        }

        var update = Builders<SellerProfile>.Update.Set(p => p.UpdatedAt, DateTime.UtcNow);

        if (request.ShopName != null) update = update.Set(p => p.ShopName, request.ShopName);
        if (request.ShopDescription != null) update = update.Set(p => p.ShopDescription, request.ShopDescription);
        if (request.ShopLogo != null) update = update.Set(p => p.ShopLogo, request.ShopLogo);
        if (request.ShopBanner != null) update = update.Set(p => p.ShopBanner, request.ShopBanner);
        if (request.Phone != null) update = update.Set(p => p.Phone, request.Phone);
        if (request.Address != null) update = update.Set(p => p.Address, request.Address);
        if (request.City != null) update = update.Set(p => p.City, request.City);
        if (request.Country != null) update = update.Set(p => p.Country, request.Country);
        if (request.PaymentMethod != null) update = update.Set(p => p.PaymentMethod, request.PaymentMethod);
        if (request.BankName != null) update = update.Set(p => p.BankName, request.BankName);
        if (request.AccountNumber != null) update = update.Set(p => p.AccountNumber, request.AccountNumber);
        if (request.Categories != null) update = update.Set(p => p.Categories, request.Categories);

        await _mongoDb.SellerProfiles.UpdateOneAsync(p => p.UserId == userId, update);

        var updatedProfile = await _mongoDb.SellerProfiles
            .Find(p => p.UserId == userId)
            .FirstOrDefaultAsync();

        return Ok(updatedProfile);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var products = await _mongoDb.Products
            .Find(p => p.SellerId == userId)
            .ToListAsync();

        var productIds = products.Select(p => p.Id).ToList();

        var orders = await _mongoDb.Orders
            .Find(o => o.Items.Any(i => productIds.Contains(i.ProductId)))
            .ToListAsync();

        var sellerOrders = orders.Where(o =>
            o.Items.Any(i => productIds.Contains(i.ProductId))
        ).ToList();

        decimal totalRevenue = 0;
        int totalSalesCount = 0;
        foreach (var order in sellerOrders)
        {
            var sellerItems = order.Items.Where(i => productIds.Contains(i.ProductId));
            totalRevenue += sellerItems.Sum(i => i.Price * i.Quantity);
            totalSalesCount += sellerItems.Sum(i => i.Quantity);
        }

        return Ok(new
        {
            totalOrders = sellerOrders.Count,
            totalSales = totalSalesCount,
            totalRevenue = totalRevenue,
            totalProducts = products.Count,
            activeProducts = products.Count(p => p.Status == "published" && p.StockQuantity > 0),
            pendingOrders = sellerOrders.Count(o => o.Status == "pending"),
            confirmedOrders = sellerOrders.Count(o => o.Status == "confirmed"),
            shippedOrders = sellerOrders.Count(o => o.Status == "shipped"),
            deliveredOrders = sellerOrders.Count(o => o.Status == "delivered"),
            cancelledOrders = sellerOrders.Count(o => o.Status == "cancelled"),
            recentOrders = sellerOrders
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .Select(o => new
                {
                    id = o.Id,
                    totalAmount = o.TotalAmount,
                    status = o.Status,
                    createdAt = o.CreatedAt,
                    items = o.Items.Count,
                    customerName = o.ShippingAddress?.Split(',').FirstOrDefault()?.Trim() ?? "Customer"
                })
        });
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var products = await _mongoDb.Products
            .Find(p => p.SellerId == userId)
            .ToListAsync();

        var productIds = products.Select(p => p.Id).ToList();

        var orders = await _mongoDb.Orders
            .Find(o => o.Items.Any(i => productIds.Contains(i.ProductId)))
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders.Select(o => new
        {
            id = o.Id,
            userId = o.UserId,
            totalAmount = o.TotalAmount,
            shippingAddress = o.ShippingAddress,
            paymentMethod = o.PaymentMethod,
            status = o.Status,
            createdAt = o.CreatedAt,
            items = o.Items.Where(i => productIds.Contains(i.ProductId)).Select(i => new
            {
                productId = i.ProductId,
                productName = i.ProductName,
                price = i.Price,
                quantity = i.Quantity,
                image = i.Image
            })
        }));
    }

    [HttpPut("orders/{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] UpdateOrderStatusRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var order = await _mongoDb.Orders
            .Find(o => o.Id == id)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return NotFound(new { message = "Order not found." });
        }

        // Verify seller owns products in this order
        var products = await _mongoDb.Products
            .Find(p => p.SellerId == userId)
            .ToListAsync();

        var productIds = products.Select(p => p.Id).ToList();
        var hasSellerProduct = order.Items.Any(i => productIds.Contains(i.ProductId));

        if (!hasSellerProduct)
        {
            return Forbid();
        }

        var validStatuses = new[] { "confirmed", "shipped", "delivered", "cancelled" };
        if (!validStatuses.Contains(request.Status))
        {
            return BadRequest(new { message = "Invalid status. Must be: confirmed, shipped, delivered, or cancelled." });
        }

        await _mongoDb.Orders.UpdateOneAsync(
            o => o.Id == id,
            Builders<Order>.Update
                .Set(o => o.Status, request.Status)
                .Set(o => o.UpdatedAt, DateTime.UtcNow)
        );

        return Ok(new { message = $"Order status updated to {request.Status}." });
    }

    [HttpGet("earnings")]
    public async Task<IActionResult> GetEarnings()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var products = await _mongoDb.Products
            .Find(p => p.SellerId == userId)
            .ToListAsync();

        var productIds = products.Select(p => p.Id).ToList();

        var orders = await _mongoDb.Orders
            .Find(o => o.Items.Any(i => productIds.Contains(i.ProductId)) && o.Status == "delivered")
            .ToListAsync();

        decimal totalEarnings = 0;
        decimal monthlyEarnings = 0;
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        foreach (var order in orders)
        {
            var sellerItems = order.Items.Where(i => productIds.Contains(i.ProductId));
            var orderTotal = sellerItems.Sum(i => i.Price * i.Quantity);
            totalEarnings += orderTotal;

            if (order.CreatedAt >= monthStart)
            {
                monthlyEarnings += orderTotal;
            }
        }

        var pendingOrders = await _mongoDb.Orders
            .Find(o => o.Items.Any(i => productIds.Contains(i.ProductId)) && o.Status != "delivered" && o.Status != "cancelled")
            .ToListAsync();

        decimal pendingAmount = 0;
        foreach (var order in pendingOrders)
        {
            var sellerItems = order.Items.Where(i => productIds.Contains(i.ProductId));
            pendingAmount += sellerItems.Sum(i => i.Price * i.Quantity);
        }

        return Ok(new
        {
            totalEarnings,
            monthlyEarnings,
            pendingAmount,
            deliveredOrders = orders.Count,
            totalProducts = products.Count
        });
    }

    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}

public class UpdateSellerProfileRequest
{
    public string? ShopName { get; set; }
    public string? ShopDescription { get; set; }
    public string? ShopLogo { get; set; }
    public string? ShopBanner { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PaymentMethod { get; set; }
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
    public List<string>? Categories { get; set; }
}

public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
