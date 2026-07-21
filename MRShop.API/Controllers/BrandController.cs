using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MRShop.API.Models;
using MRShop.API.Services;

namespace MRShop.API.Controllers;

[ApiController]
[Route("api/brands")]
public class BrandController : ControllerBase
{
    private readonly MongoDbService _mongoDb;

    public BrandController(MongoDbService mongoDb)
    {
        _mongoDb = mongoDb;
    }

    // Public - get all active brands
    [HttpGet]
    public async Task<IActionResult> GetBrands()
    {
        var brands = await _mongoDb.Brands
            .Find(b => b.IsActive)
            .SortBy(b => b.Name)
            .ToListAsync();

        return Ok(brands);
    }

    // Public - get brand by id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBrand(string id)
    {
        var brand = await _mongoDb.Brands.Find(b => b.Id == id).FirstOrDefaultAsync();
        if (brand == null) return NotFound(new { message = "Brand not found." });
        return Ok(brand);
    }

    // Admin - get all brands including inactive
    [Authorize(Roles = "admin")]
    [HttpGet("admin/all")]
    public async Task<IActionResult> GetAllBrands()
    {
        var brands = await _mongoDb.Brands
            .Find(_ => true)
            .SortBy(b => b.Name)
            .ToListAsync();

        return Ok(brands);
    }

    // Admin - create brand
    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> CreateBrand([FromBody] CreateBrandRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Brand name is required." });

        var slug = request.Name.ToLower().Replace(" ", "-").Replace("&", "and");
        var existing = await _mongoDb.Brands.Find(b => b.Slug == slug).FirstOrDefaultAsync();
        if (existing != null) return Conflict(new { message = "Brand already exists." });

        var brand = new Brand
        {
            Name = request.Name.Trim(),
            Slug = slug,
            Logo = request.Logo,
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _mongoDb.Brands.InsertOneAsync(brand);
        return Ok(new { message = "Brand created.", brand });
    }

    // Admin - update brand
    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBrand(string id, [FromBody] UpdateBrandRequest request)
    {
        var brand = await _mongoDb.Brands.Find(b => b.Id == id).FirstOrDefaultAsync();
        if (brand == null) return NotFound(new { message = "Brand not found." });

        var update = Builders<Brand>.Update.Set(b => b.UpdatedAt, DateTime.UtcNow);
        if (request.Name != null) update = update.Set(b => b.Name, request.Name.Trim());
        if (request.Logo != null) update = update.Set(b => b.Logo, request.Logo);
        if (request.Description != null) update = update.Set(b => b.Description, request.Description);
        if (request.IsActive.HasValue) update = update.Set(b => b.IsActive, request.IsActive.Value);

        await _mongoDb.Brands.UpdateOneAsync(b => b.Id == id, update);
        return Ok(new { message = "Brand updated." });
    }

    // Admin - delete brand
    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBrand(string id)
    {
        var result = await _mongoDb.Brands.DeleteOneAsync(b => b.Id == id);
        if (result.DeletedCount == 0) return NotFound(new { message = "Brand not found." });
        return Ok(new { message = "Brand deleted." });
    }
}

public class CreateBrandRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? Description { get; set; }
}

public class UpdateBrandRequest
{
    public string? Name { get; set; }
    public string? Logo { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}
