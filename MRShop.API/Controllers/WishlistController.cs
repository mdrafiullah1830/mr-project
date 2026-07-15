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
[Route("api/wishlist")]
public class WishlistController : ControllerBase
{
    private readonly MongoDbService _mongoDb;

    public WishlistController(MongoDbService mongoDb)
    {
        _mongoDb = mongoDb;
    }

    [HttpGet]
    public async Task<IActionResult> GetWishlist()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var items = await _mongoDb.WishlistItems
            .Find(w => w.UserId == userId)
            .SortByDescending(w => w.CreatedAt)
            .ToListAsync();

        return Ok(new
        {
            items = items.Select(MapToWishlistItemResponse),
            itemCount = items.Count
        });
    }

    [HttpPost]
    public async Task<IActionResult> AddToWishlist([FromBody] AddToWishlistRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var existingItem = await _mongoDb.WishlistItems
            .Find(w => w.UserId == userId && w.ProductId == request.ProductId)
            .FirstOrDefaultAsync();

        if (existingItem != null)
        {
            return Ok(new { message = "Item already in wishlist." });
        }

        var wishlistItem = new WishlistItem
        {
            UserId = userId,
            ProductId = request.ProductId,
            ProductName = request.ProductName,
            Price = request.Price,
            Image = request.Image,
            CreatedAt = DateTime.UtcNow
        };

        await _mongoDb.WishlistItems.InsertOneAsync(wishlistItem);

        return Ok(new { message = "Item added to wishlist." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveFromWishlist(string id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _mongoDb.WishlistItems
            .DeleteOneAsync(w => w.Id == id && w.UserId == userId);

        if (result.DeletedCount == 0)
        {
            return NotFound(new { message = "Wishlist item not found." });
        }

        return Ok(new { message = "Item removed from wishlist." });
    }

    [HttpDelete("product/{productId}")]
    public async Task<IActionResult> RemoveByProductId(string productId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _mongoDb.WishlistItems
            .DeleteOneAsync(w => w.ProductId == productId && w.UserId == userId);

        if (result.DeletedCount == 0)
        {
            return NotFound(new { message = "Item not found in wishlist." });
        }

        return Ok(new { message = "Item removed from wishlist." });
    }

    [HttpDelete]
    public async Task<IActionResult> ClearWishlist()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        await _mongoDb.WishlistItems.DeleteManyAsync(w => w.UserId == userId);

        return Ok(new { message = "Wishlist cleared." });
    }

    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private static WishlistItemResponse MapToWishlistItemResponse(WishlistItem item)
    {
        return new WishlistItemResponse
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Price = item.Price,
            Image = item.Image
        };
    }
}
