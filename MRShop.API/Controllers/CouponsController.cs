using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MRShop.API.Models;
using MRShop.API.Services;

namespace MRShop.API.Controllers;

[ApiController]
[Route("api/coupons")]
public class CouponsController : ControllerBase
{
    private readonly MongoDbService _mongoDb;

    public CouponsController(MongoDbService mongoDb)
    {
        _mongoDb = mongoDb;
    }

    // Public - validate a coupon code
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            return BadRequest(new { message = "Coupon code is required." });

        var coupon = await _mongoDb.Coupons
            .Find(c => c.Code == request.Code.ToUpper().Trim() && c.IsActive)
            .FirstOrDefaultAsync();

        if (coupon == null)
            return BadRequest(new { message = "Invalid coupon code." });

        if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt.Value < DateTime.UtcNow)
            return BadRequest(new { message = "Coupon has expired." });

        if (coupon.UsageCount >= coupon.MaxUsage)
            return BadRequest(new { message = "Coupon usage limit reached." });

        if (request.Subtotal < coupon.MinOrder)
            return BadRequest(new { message = $"Minimum order ৳{coupon.MinOrder} required." });

        decimal discount = coupon.DiscountType switch
        {
            "percent" => Math.Round(request.Subtotal * coupon.DiscountValue / 100),
            "fixed" => coupon.DiscountValue,
            "shipping" => 0,
            _ => 0
        };

        return Ok(new
        {
            code = coupon.Code,
            type = coupon.DiscountType,
            value = coupon.DiscountValue,
            discount,
            message = coupon.DiscountType == "percent" ? $"{coupon.DiscountValue}% off" :
                      coupon.DiscountType == "fixed" ? $"৳{coupon.DiscountValue} off" : "Free shipping"
        });
    }

    // Admin - get all coupons
    [Authorize(Roles = "admin")]
    [HttpGet]
    public async Task<IActionResult> GetCoupons()
    {
        var coupons = await _mongoDb.Coupons
            .Find(_ => true)
            .SortByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Ok(coupons);
    }

    // Admin - create coupon
    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            return BadRequest(new { message = "Coupon code is required." });

        var existing = await _mongoDb.Coupons
            .Find(c => c.Code == request.Code.ToUpper().Trim())
            .FirstOrDefaultAsync();

        if (existing != null)
            return Conflict(new { message = "Coupon code already exists." });

        var coupon = new Coupon
        {
            Code = request.Code.ToUpper().Trim(),
            DiscountType = request.DiscountType ?? "percent",
            DiscountValue = request.DiscountValue,
            MinOrder = request.MinOrder,
            MaxUsage = request.MaxUsage > 0 ? request.MaxUsage : 1000,
            ExpiresAt = request.ExpiresAt,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _mongoDb.Coupons.InsertOneAsync(coupon);

        return Ok(new { message = "Coupon created.", coupon });
    }

    // Admin - update coupon
    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCoupon(string id, [FromBody] UpdateCouponRequest request)
    {
        var existing = await _mongoDb.Coupons.Find(c => c.Id == id).FirstOrDefaultAsync();
        if (existing == null) return NotFound(new { message = "Coupon not found." });

        var update = Builders<Coupon>.Update.Set(c => c.IsActive, request.IsActive ?? true);
        if (request.DiscountValue.HasValue) update = update.Set(c => c.DiscountValue, request.DiscountValue.Value);
        if (request.MaxUsage.HasValue) update = update.Set(c => c.MaxUsage, request.MaxUsage.Value);
        if (request.ExpiresAt.HasValue) update = update.Set(c => c.ExpiresAt, request.ExpiresAt);

        await _mongoDb.Coupons.UpdateOneAsync(c => c.Id == id, update);

        return Ok(new { message = "Coupon updated." });
    }

    // Admin - delete coupon
    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCoupon(string id)
    {
        var result = await _mongoDb.Coupons.DeleteOneAsync(c => c.Id == id);
        if (result.DeletedCount == 0) return NotFound(new { message = "Coupon not found." });
        return Ok(new { message = "Coupon deleted." });
    }

    // Admin - get dashboard stats
    [Authorize(Roles = "admin")]
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var orders = await _mongoDb.Orders.Find(_ => true).ToListAsync();
        var users = await _mongoDb.Users.Find(u => u.Role == "customer").ToListAsync();
        var products = await _mongoDb.Products.Find(_ => true).ToListAsync();

        var totalRevenue = orders.Sum(o => o.TotalAmount);
        var totalOrders = orders.Count;
        var totalUsers = users.Count;
        var pendingOrders = orders.Count(o => o.Status == "pending");

        var recentOrders = orders.OrderByDescending(o => o.CreatedAt).Take(5).Select(o => new
        {
            id = o.Id,
            totalAmount = o.TotalAmount,
            status = o.Status,
            createdAt = o.CreatedAt,
            items = o.Items.Count
        });

        return Ok(new
        {
            totalRevenue,
            totalOrders,
            totalUsers,
            pendingOrders,
            recentOrders
        });
    }
}

public class ValidateCouponRequest
{
    public string Code { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
}

public class CreateCouponRequest
{
    public string Code { get; set; } = string.Empty;
    public string? DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal MinOrder { get; set; }
    public int MaxUsage { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class UpdateCouponRequest
{
    public decimal? DiscountValue { get; set; }
    public int? MaxUsage { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool? IsActive { get; set; }
}
