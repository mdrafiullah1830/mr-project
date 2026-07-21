using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MRShop.API.Models;
using MRShop.API.Services;

namespace MRShop.API.Controllers;

[Authorize(Roles = "seller,admin")]
[ApiController]
[Route("api/sellerproducts")]
public class SellerProductsController : ControllerBase
{
    private readonly MongoDbService _mongoDb;

    public SellerProductsController(MongoDbService mongoDb)
    {
        _mongoDb = mongoDb;
    }

    [HttpGet]
    public async Task<IActionResult> GetSellerProducts()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var products = await _mongoDb.Products
            .Find(p => p.SellerId == userId)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(products.Select(p => new
        {
            id = p.Id,
            name = p.Name,
            slug = p.Slug,
            price = p.Price,
            discountPrice = p.DiscountPrice,
            stockQuantity = p.StockQuantity,
            status = p.Status,
            approvalStatus = p.ApprovalStatus,
            thumbnailImage = p.ThumbnailImage,
            averageRating = p.AverageRating,
            soldCount = p.SoldCount,
            viewCount = p.ViewCount,
            createdAt = p.CreatedAt
        }));
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetSellerStats()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var products = await _mongoDb.Products
            .Find(p => p.SellerId == userId)
            .ToListAsync();

        var orders = await _mongoDb.Orders
            .Find(o => o.Items.Any(i => products.Any(p => p.Id == i.ProductId)))
            .ToListAsync();

        var totalRevenue = orders.Where(o => o.Status == "delivered").Sum(o => o.TotalAmount);
        var totalProducts = products.Count;
        var totalOrders = orders.Count;
        var activeProducts = products.Count(p => p.Status == "published" && p.StockQuantity > 0);

        return Ok(new
        {
            totalRevenue,
            totalProducts,
            totalOrders,
            activeProducts,
            pendingOrders = orders.Count(o => o.Status == "pending"),
            deliveredOrders = orders.Count(o => o.Status == "delivered")
        });
    }

    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
