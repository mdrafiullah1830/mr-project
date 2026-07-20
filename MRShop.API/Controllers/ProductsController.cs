using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MRShop.API.DTOs;
using MRShop.API.Models;
using MRShop.API.Services;

namespace MRShop.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly MongoDbService _mongoDb;

    public ProductsController(MongoDbService mongoDb)
    {
        _mongoDb = mongoDb;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProducts(
        [FromQuery] string? category,
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.IsActive, true);

        if (!string.IsNullOrWhiteSpace(category))
        {
            filter = Builders<Product>.Filter.And(filter,
                Builders<Product>.Filter.Eq(p => p.Category, category));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var escapedSearch = System.Text.RegularExpressions.Regex.Escape(search);
            filter = Builders<Product>.Filter.And(filter,
                Builders<Product>.Filter.Regex(p => p.Name,
                    new MongoDB.Bson.BsonRegularExpression(escapedSearch, "i")));
        }

        var totalCount = await _mongoDb.Products.CountDocumentsAsync(filter);

        var sortDef = sort switch
        {
            "price_asc" => Builders<Product>.Sort.Ascending(p => p.Price),
            "price_desc" => Builders<Product>.Sort.Descending(p => p.Price),
            "rating" => Builders<Product>.Sort.Descending(p => p.Rating),
            "newest" => Builders<Product>.Sort.Descending(p => p.CreatedAt),
            _ => Builders<Product>.Sort.Descending(p => p.CreatedAt)
        };

        var products = await _mongoDb.Products
            .Find(filter)
            .Sort(sortDef)
            .Skip((page - 1) * limit)
            .Limit(limit)
            .ToListAsync();

        return Ok(new
        {
            products = products.Select(MapToProductResponse),
            totalCount,
            page,
            totalPages = (int)Math.Ceiling((double)totalCount / limit)
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(string id)
    {
        var product = await _mongoDb.Products
            .Find(p => p.Id == id)
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound(new { message = "Product not found." });
        }

        return Ok(MapToProductResponse(product));
    }

    [Authorize(Roles = "admin,seller")]
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _mongoDb.Products.InsertOneAsync(product);

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, MapToProductResponse(product));
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] UpdateProductRequest request)
    {
        var existing = await _mongoDb.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
        if (existing == null)
        {
            return NotFound(new { message = "Product not found." });
        }

        var update = Builders<Product>.Update
            .Set(p => p.UpdatedAt, DateTime.UtcNow);

        if (request.Name != null)
            update = update.Set(p => p.Name, request.Name);
        if (request.Description != null)
            update = update.Set(p => p.Description, request.Description);
        if (request.Price.HasValue && request.Price.Value > 0)
            update = update.Set(p => p.Price, request.Price.Value);
        if (request.OriginalPrice.HasValue)
            update = update.Set(p => p.OriginalPrice, request.OriginalPrice.Value);
        if (request.Category != null)
            update = update.Set(p => p.Category, request.Category);
        if (request.Subcategory != null)
            update = update.Set(p => p.Subcategory, request.Subcategory);
        if (request.Image != null)
            update = update.Set(p => p.Image, request.Image);
        if (request.Images != null)
            update = update.Set(p => p.Images, request.Images);
        if (request.Stock.HasValue && request.Stock.Value >= 0)
            update = update.Set(p => p.Stock, request.Stock.Value);

        await _mongoDb.Products.UpdateOneAsync(p => p.Id == id, update);

        var product = await _mongoDb.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
        return Ok(MapToProductResponse(product!));
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        var result = await _mongoDb.Products.DeleteOneAsync(p => p.Id == id);

        if (result.DeletedCount == 0)
        {
            return NotFound(new { message = "Product not found." });
        }

        return Ok(new { message = "Product deleted successfully." });
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
