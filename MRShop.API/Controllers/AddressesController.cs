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
[Route("api/addresses")]
public class AddressesController : ControllerBase
{
    private readonly MongoDbService _mongoDb;

    public AddressesController(MongoDbService mongoDb)
    {
        _mongoDb = mongoDb;
    }

    [HttpGet]
    public async Task<IActionResult> GetAddresses()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var addresses = await _mongoDb.Addresses
            .Find(a => a.UserId == userId)
            .SortByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();

        return Ok(addresses.Select(MapToResponse));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAddress(string id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var address = await _mongoDb.Addresses
            .Find(a => a.Id == id && a.UserId == userId)
            .FirstOrDefaultAsync();

        if (address == null) return NotFound(new { message = "Address not found." });
        return Ok(MapToResponse(address));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAddress([FromBody] CreateAddressRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.AddressLine))
            return BadRequest(new { message = "Full name, phone, and address line are required." });

        if (request.IsDefault)
        {
            await _mongoDb.Addresses.UpdateManyAsync(
                a => a.UserId == userId,
                Builders<Address>.Update.Set(a => a.IsDefault, false)
            );
        }

        var address = new Address
        {
            UserId = userId,
            FullName = request.FullName.Trim(),
            Phone = request.Phone.Trim(),
            Email = request.Email?.Trim(),
            Country = request.Country ?? "Bangladesh",
            Division = request.Division?.Trim(),
            District = request.District?.Trim(),
            City = request.City?.Trim(),
            Area = request.Area?.Trim(),
            PostalCode = request.PostalCode?.Trim(),
            AddressLine = request.AddressLine.Trim(),
            AddressType = request.AddressType ?? "home",
            IsDefault = request.IsDefault,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _mongoDb.Addresses.InsertOneAsync(address);
        return Ok(new { message = "Address created.", address = MapToResponse(address) });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAddress(string id, [FromBody] CreateAddressRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var existing = await _mongoDb.Addresses.Find(a => a.Id == id && a.UserId == userId).FirstOrDefaultAsync();
        if (existing == null) return NotFound(new { message = "Address not found." });

        if (request.IsDefault)
        {
            await _mongoDb.Addresses.UpdateManyAsync(
                a => a.UserId == userId && a.Id != id,
                Builders<Address>.Update.Set(a => a.IsDefault, false)
            );
        }

        var update = Builders<Address>.Update
            .Set(a => a.FullName, request.FullName.Trim())
            .Set(a => a.Phone, request.Phone.Trim())
            .Set(a => a.Email, request.Email?.Trim())
            .Set(a => a.Country, request.Country ?? "Bangladesh")
            .Set(a => a.Division, request.Division?.Trim())
            .Set(a => a.District, request.District?.Trim())
            .Set(a => a.City, request.City?.Trim())
            .Set(a => a.Area, request.Area?.Trim())
            .Set(a => a.PostalCode, request.PostalCode?.Trim())
            .Set(a => a.AddressLine, request.AddressLine.Trim())
            .Set(a => a.AddressType, request.AddressType ?? "home")
            .Set(a => a.IsDefault, request.IsDefault)
            .Set(a => a.UpdatedAt, DateTime.UtcNow);

        await _mongoDb.Addresses.UpdateOneAsync(a => a.Id == id, update);
        return Ok(new { message = "Address updated." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAddress(string id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _mongoDb.Addresses.DeleteOneAsync(a => a.Id == id && a.UserId == userId);
        if (result.DeletedCount == 0) return NotFound(new { message = "Address not found." });
        return Ok(new { message = "Address deleted." });
    }

    [HttpPut("{id}/default")]
    public async Task<IActionResult> SetDefaultAddress(string id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var address = await _mongoDb.Addresses.Find(a => a.Id == id && a.UserId == userId).FirstOrDefaultAsync();
        if (address == null) return NotFound(new { message = "Address not found." });

        await _mongoDb.Addresses.UpdateManyAsync(
            a => a.UserId == userId,
            Builders<Address>.Update.Set(a => a.IsDefault, false)
        );
        await _mongoDb.Addresses.UpdateOneAsync(
            a => a.Id == id,
            Builders<Address>.Update.Set(a => a.IsDefault, true)
        );

        return Ok(new { message = "Default address set." });
    }

    private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    private static AddressResponse MapToResponse(Address a) => new()
    {
        Id = a.Id,
        FullName = a.FullName,
        Phone = a.Phone,
        Email = a.Email,
        Country = a.Country,
        Division = a.Division,
        District = a.District,
        City = a.City,
        Area = a.Area,
        PostalCode = a.PostalCode,
        AddressLine = a.AddressLine,
        AddressType = a.AddressType,
        IsDefault = a.IsDefault
    };
}
