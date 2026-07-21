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

        var activeItems = items.Where(c => !c.SavedForLater).ToList();
        var savedItems = items.Where(c => c.SavedForLater).ToList();

        decimal subtotal = 0;
        decimal discount = 0;

        foreach (var item in activeItems)
        {
            var product = await _mongoDb.Products.Find(p => p.Id == item.ProductId).FirstOrDefaultAsync();
            if (product != null)
            {
                item.Price = product.Price;
                item.DiscountPrice = product.DiscountPrice;
                item.StockQuantity = product.StockQuantity;
                item.ProductName = product.Name;
                item.Image = product.ThumbnailImage ?? item.Image;

                var effectivePrice = product.DiscountPrice ?? product.Price;
                subtotal += product.Price * item.Quantity;
                discount += (product.Price - effectivePrice) * item.Quantity;
            }
        }

        return Ok(new CartSummary
        {
            Items = activeItems.Select(MapToResponse).ToList(),
            SavedForLater = savedItems.Select(MapToResponse).ToList(),
            ItemCount = activeItems.Sum(i => i.Quantity),
            Subtotal = subtotal,
            Discount = discount,
            GrandTotal = subtotal - discount
        });
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (request.Quantity <= 0)
            return BadRequest(new { message = "Quantity must be at least 1." });

        var product = await _mongoDb.Products
            .Find(p => p.Id == request.ProductId && p.Status == "published")
            .FirstOrDefaultAsync();
        if (product == null)
            return BadRequest(new { message = "Product not found." });

        if (product.StockQuantity < request.Quantity)
            return BadRequest(new { message = $"Insufficient stock. Only {product.StockQuantity} available." });

        var existingItem = await _mongoDb.CartItems
            .Find(c => c.UserId == userId && c.ProductId == request.ProductId && !c.SavedForLater)
            .FirstOrDefaultAsync();

        if (existingItem != null)
        {
            var newQty = existingItem.Quantity + request.Quantity;
            if (newQty > product.StockQuantity)
                return BadRequest(new { message = $"Cannot add more. Only {product.StockQuantity} in stock." });

            await _mongoDb.CartItems.UpdateOneAsync(
                c => c.Id == existingItem.Id,
                Builders<CartItem>.Update.Set(c => c.Quantity, newQty)
            );
        }
        else
        {
            var sellerProfile = await _mongoDb.SellerProfiles.Find(s => s.UserId == product.SellerId).FirstOrDefaultAsync();
            var cartItem = new CartItem
            {
                UserId = userId,
                ProductId = request.ProductId,
                ProductName = product.Name,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                Image = product.ThumbnailImage ?? "",
                Quantity = request.Quantity,
                SellerId = product.SellerId,
                SellerName = sellerProfile?.ShopName,
                StockQuantity = product.StockQuantity,
                SavedForLater = false,
                CreatedAt = DateTime.UtcNow
            };
            await _mongoDb.CartItems.InsertOneAsync(cartItem);
        }

        return Ok(new { message = "Item added to cart." });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateQuantity(string id, [FromBody] UpdateCartRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var item = await _mongoDb.CartItems
            .Find(c => c.Id == id && c.UserId == userId)
            .FirstOrDefaultAsync();
        if (item == null) return NotFound(new { message = "Cart item not found." });

        if (request.Quantity <= 0)
        {
            await _mongoDb.CartItems.DeleteOneAsync(c => c.Id == id);
            return Ok(new { message = "Item removed from cart." });
        }

        var product = await _mongoDb.Products.Find(p => p.Id == item.ProductId).FirstOrDefaultAsync();
        if (product == null) return BadRequest(new { message = "Product no longer available." });

        if (request.Quantity > product.StockQuantity)
            return BadRequest(new { message = $"Only {product.StockQuantity} available in stock." });

        await _mongoDb.CartItems.UpdateOneAsync(
            c => c.Id == id,
            Builders<CartItem>.Update.Set(c => c.Quantity, request.Quantity)
        );

        return Ok(new { message = "Cart updated." });
    }

    [HttpPost("{id}/save-for-later")]
    public async Task<IActionResult> SaveForLater(string id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _mongoDb.CartItems.UpdateOneAsync(
            c => c.Id == id && c.UserId == userId,
            Builders<CartItem>.Update.Set(c => c.SavedForLater, true)
        );
        if (result.MatchedCount == 0) return NotFound(new { message = "Cart item not found." });
        return Ok(new { message = "Item saved for later." });
    }

    [HttpPost("{id}/move-to-cart")]
    public async Task<IActionResult> MoveToCart(string id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _mongoDb.CartItems.UpdateOneAsync(
            c => c.Id == id && c.UserId == userId,
            Builders<CartItem>.Update.Set(c => c.SavedForLater, false)
        );
        if (result.MatchedCount == 0) return NotFound(new { message = "Cart item not found." });
        return Ok(new { message = "Item moved to cart." });
    }

    [HttpPost("{id}/move-to-wishlist")]
    public async Task<IActionResult> MoveToWishlist(string id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var item = await _mongoDb.CartItems
            .Find(c => c.Id == id && c.UserId == userId)
            .FirstOrDefaultAsync();
        if (item == null) return NotFound(new { message = "Cart item not found." });

        var existingWish = await _mongoDb.WishlistItems
            .Find(w => w.UserId == userId && w.ProductId == item.ProductId)
            .FirstOrDefaultAsync();
        if (existingWish == null)
        {
            await _mongoDb.WishlistItems.InsertOneAsync(new WishlistItem
            {
                UserId = userId,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Price = item.Price,
                Image = item.Image,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _mongoDb.CartItems.DeleteOneAsync(c => c.Id == id);
        return Ok(new { message = "Item moved to wishlist." });
    }

    [HttpPost("merge")]
    public async Task<IActionResult> MergeGuestCart([FromBody] List<AddToCartRequest> guestItems)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        foreach (var gi in guestItems)
        {
            var existing = await _mongoDb.CartItems
                .Find(c => c.UserId == userId && c.ProductId == gi.ProductId && !c.SavedForLater)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                // Validate stock before incrementing
                var product = await _mongoDb.Products.Find(p => p.Id == gi.ProductId && p.Status == "published").FirstOrDefaultAsync();
                var newQuantity = existing.Quantity + gi.Quantity;
                if (product != null && newQuantity > product.StockQuantity)
                    newQuantity = product.StockQuantity;

                await _mongoDb.CartItems.UpdateOneAsync(
                    c => c.Id == existing.Id,
                    Builders<CartItem>.Update.Set(c => c.Quantity, newQuantity)
                );
            }
            else
            {
                var product = await _mongoDb.Products.Find(p => p.Id == gi.ProductId && p.Status == "published").FirstOrDefaultAsync();
                if (product != null)
                {
                    await _mongoDb.CartItems.InsertOneAsync(new CartItem
                    {
                        UserId = userId,
                        ProductId = gi.ProductId,
                        ProductName = product.Name,
                        Price = product.Price,
                        DiscountPrice = product.DiscountPrice,
                        Image = product.ThumbnailImage ?? "",
                        Quantity = gi.Quantity,
                        SellerId = product.SellerId,
                        StockQuantity = product.StockQuantity,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        return Ok(new { message = "Guest cart merged." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveFromCart(string id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _mongoDb.CartItems.DeleteOneAsync(c => c.Id == id && c.UserId == userId);
        if (result.DeletedCount == 0) return NotFound(new { message = "Cart item not found." });
        return Ok(new { message = "Item removed from cart." });
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        await _mongoDb.CartItems.DeleteManyAsync(c => c.UserId == userId && !c.SavedForLater);
        return Ok(new { message = "Cart cleared." });
    }

    private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    private static CartItemResponse MapToResponse(CartItem item) => new()
    {
        Id = item.Id,
        ProductId = item.ProductId,
        ProductName = item.ProductName,
        Price = item.Price,
        DiscountPrice = item.DiscountPrice,
        Image = item.Image,
        Quantity = item.Quantity,
        SellerId = item.SellerId,
        SellerName = item.SellerName,
        StockQuantity = item.StockQuantity,
        SavedForLater = item.SavedForLater
    };
}
