using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MRShop.API.DTOs;
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
            .Find(p => p.SellerId == userId || p.CreatedBy == userId)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(products.Select(MapToProductResponse));
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            OriginalPrice = request.OriginalPrice,
            Category = request.Category,
            Subcategory = request.Subcategory,
            Image = request.Image,
            Images = request.Images,
            Stock = request.Stock,
            Rating = 0,
            ReviewCount = 0,
            IsActive = true,
            SellerId = userId,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _mongoDb.Products.InsertOneAsync(product);

        return Ok(MapToProductResponse(product));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] UpdateProductRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var existing = await _mongoDb.Products
            .Find(p => p.Id == id && (p.SellerId == userId || p.CreatedBy == userId))
            .FirstOrDefaultAsync();

        if (existing == null)
        {
            return NotFound(new { message = "Product not found." });
        }

        var update = Builders<Product>.Update.Set(p => p.UpdatedAt, DateTime.UtcNow);

        if (request.Name != null) update = update.Set(p => p.Name, request.Name);
        if (request.Description != null) update = update.Set(p => p.Description, request.Description);
        if (request.Price.HasValue) update = update.Set(p => p.Price, request.Price.Value);
        if (request.OriginalPrice.HasValue) update = update.Set(p => p.OriginalPrice, request.OriginalPrice.Value);
        if (request.Category != null) update = update.Set(p => p.Category, request.Category);
        if (request.Subcategory != null) update = update.Set(p => p.Subcategory, request.Subcategory);
        if (request.Image != null) update = update.Set(p => p.Image, request.Image);
        if (request.Images != null) update = update.Set(p => p.Images, request.Images);
        if (request.Stock.HasValue) update = update.Set(p => p.Stock, request.Stock.Value);

        await _mongoDb.Products.UpdateOneAsync(p => p.Id == id, update);

        var product = await _mongoDb.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
        return Ok(MapToProductResponse(product!));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _mongoDb.Products
            .DeleteOneAsync(p => p.Id == id && (p.SellerId == userId || p.CreatedBy == userId));

        if (result.DeletedCount == 0)
        {
            return NotFound(new { message = "Product not found." });
        }

        return Ok(new { message = "Product deleted." });
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetSellerStats()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var products = await _mongoDb.Products
            .Find(p => p.SellerId == userId || p.CreatedBy == userId)
            .ToListAsync();

        var orders = await _mongoDb.Orders
            .Find(o => o.Items.Any(i => products.Any(p => p.Id == i.ProductId)))
            .ToListAsync();

        var totalRevenue = orders.Sum(o => o.TotalAmount);
        var totalProducts = products.Count;
        var totalOrders = orders.Count;
        var activeProducts = products.Count(p => p.IsActive && p.Stock > 0);

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

    private static ProductResponse MapToProductResponse(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            OriginalPrice = product.OriginalPrice,
            Category = product.Category,
            Subcategory = product.Subcategory,
            Image = product.Image,
            Images = product.Images,
            Stock = product.Stock,
            Rating = product.Rating,
            ReviewCount = product.ReviewCount
        };
    }
}
