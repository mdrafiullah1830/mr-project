using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MRShop.API.Models;
using MRShop.API.Services;

namespace MRShop.API.Controllers;

[ApiController]
[Route("api/sellers")]
public class SellersController : ControllerBase
{
    private readonly MongoDbService _mongoDb;

    public SellersController(MongoDbService mongoDb)
    {
        _mongoDb = mongoDb;
    }

    // Public endpoint - anyone can submit a seller application
    [HttpPost("applications")]
    public async Task<IActionResult> SubmitApplication([FromBody] SubmitApplicationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.ShopName))
        {
            return BadRequest(new { message = "Name, email, and shop name are required." });
        }

        var existing = await _mongoDb.SellerApplications
            .Find(a => a.Email == request.Email.ToLower().Trim() && a.Status == "pending")
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            return Conflict(new { message = "You already have a pending application." });
        }

        var application = new SellerApplication
        {
            Name = request.Name.Trim(),
            Email = request.Email.ToLower().Trim(),
            Phone = request.Phone,
            ShopName = request.ShopName.Trim(),
            BusinessType = request.BusinessType,
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _mongoDb.SellerApplications.InsertOneAsync(application);

        return Ok(new { message = "Application submitted successfully! We will review it shortly." });
    }

    // Admin endpoints
    [Authorize(Roles = "admin")]
    [HttpGet("applications")]
    public async Task<IActionResult> GetApplications()
    {
        var applications = await _mongoDb.SellerApplications
            .Find(_ => true)
            .SortByDescending(a => a.CreatedAt)
            .ToListAsync();

        return Ok(applications);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("applications/{id}/approve")]
    public async Task<IActionResult> ApproveApplication(string id)
    {
        var application = await _mongoDb.SellerApplications
            .Find(a => a.Id == id)
            .FirstOrDefaultAsync();

        if (application == null)
        {
            return NotFound(new { message = "Application not found." });
        }

        await _mongoDb.SellerApplications.UpdateOneAsync(
            a => a.Id == id,
            Builders<SellerApplication>.Update
                .Set(a => a.Status, "approved")
                .Set(a => a.UpdatedAt, DateTime.UtcNow)
        );

        var existingUser = await _mongoDb.Users
            .Find(u => u.Email == application.Email.ToLower().Trim())
            .FirstOrDefaultAsync();

        if (existingUser == null)
        {
            var sellerUser = new User
            {
                Name = application.Name,
                Email = application.Email.ToLower().Trim(),
                PasswordHash = "",
                Phone = application.Phone,
                Role = "seller",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _mongoDb.Users.InsertOneAsync(sellerUser);
        }
        else
        {
            await _mongoDb.Users.UpdateOneAsync(
                u => u.Id == existingUser.Id,
                Builders<User>.Update.Set(u => u.Role, "seller")
            );
        }

        return Ok(new { message = "Seller application approved." });
    }

    [Authorize(Roles = "admin")]
    [HttpPut("applications/{id}/reject")]
    public async Task<IActionResult> RejectApplication(string id)
    {
        var result = await _mongoDb.SellerApplications.UpdateOneAsync(
            a => a.Id == id,
            Builders<SellerApplication>.Update
                .Set(a => a.Status, "rejected")
                .Set(a => a.UpdatedAt, DateTime.UtcNow)
        );

        if (result.MatchedCount == 0)
        {
            return NotFound(new { message = "Application not found." });
        }

        return Ok(new { message = "Seller application rejected." });
    }

    [Authorize(Roles = "admin")]
    [HttpGet]
    public async Task<IActionResult> GetSellers()
    {
        var sellers = await _mongoDb.Users
            .Find(u => u.Role == "seller")
            .ToListAsync();

        return Ok(sellers.Select(s => new
        {
            id = s.Id,
            name = s.Name,
            email = s.Email,
            phone = s.Phone,
            createdAt = s.CreatedAt
        }));
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveSeller(string id)
    {
        var result = await _mongoDb.Users.UpdateOneAsync(
            u => u.Id == id,
            Builders<User>.Update.Set(u => u.Role, "customer")
        );

        if (result.MatchedCount == 0)
        {
            return NotFound(new { message = "Seller not found." });
        }

        return Ok(new { message = "Seller role removed." });
    }
}

public class SubmitApplicationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string? BusinessType { get; set; }
}
