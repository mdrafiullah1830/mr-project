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
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly MongoDbService _mongoDb;

    public CartController(MongoDbService mongoDb)
    {
        _mongoDb = mongoDb;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var items = await _mongoDb.CartItems
            .Find(c => c.UserId == userId)
            .SortByDescending(c => c.CreatedAt)
            .ToListAsync();

        var total = items.Sum(i => i.Price * i.Quantity);

        return Ok(new
        {
            items = items.Select(MapToCartItemResponse),
            total,
            itemCount = items.Sum(i => i.Quantity)
        });
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var existingItem = await _mongoDb.CartItems
            .Find(c => c.UserId == userId && c.ProductId == request.ProductId)
            .FirstOrDefaultAsync();

        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
            await _mongoDb.CartItems.ReplaceOneAsync(
                c => c.Id == existingItem.Id,
                existingItem
            );
        }
        else
        {
            var cartItem = new CartItem
            {
                UserId = userId,
                ProductId = request.ProductId,
                ProductName = request.ProductName,
                Price = request.Price,
                Image = request.Image,
                Quantity = request.Quantity,
                CreatedAt = DateTime.UtcNow
            };

            await _mongoDb.CartItems.InsertOneAsync(cartItem);
        }

        return Ok(new { message = "Item added to cart." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveFromCart(string id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _mongoDb.CartItems
            .DeleteOneAsync(c => c.Id == id && c.UserId == userId);

        if (result.DeletedCount == 0)
        {
            return NotFound(new { message = "Cart item not found." });
        }

        return Ok(new { message = "Item removed from cart." });
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        await _mongoDb.CartItems.DeleteManyAsync(c => c.UserId == userId);

        return Ok(new { message = "Cart cleared." });
    }

    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private static CartItemResponse MapToCartItemResponse(CartItem item)
    {
        return new CartItemResponse
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Price = item.Price,
            Image = item.Image,
            Quantity = item.Quantity
        };
    }
}
