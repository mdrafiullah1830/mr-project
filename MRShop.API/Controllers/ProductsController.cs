using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
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

    // Public - get published products with search, filter, sort, pagination
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] string? subCategory,
        [FromQuery] string? brand,
        [FromQuery] string? sellerId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] double? minRating,
        [FromQuery] bool? inStock,
        [FromQuery] string? tags,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.Status, "published");

        if (!string.IsNullOrWhiteSpace(search))
        {
            var escaped = System.Text.RegularExpressions.Regex.Escape(search);
            filter = Builders<Product>.Filter.And(filter,
                Builders<Product>.Filter.Or(
                    Builders<Product>.Filter.Regex(p => p.Name, new BsonRegularExpression(escaped, "i")),
                    Builders<Product>.Filter.Regex(p => p.Description, new BsonRegularExpression(escaped, "i")),
                    Builders<Product>.Filter.Regex(p => p.Sku, new BsonRegularExpression(escaped, "i")),
                    Builders<Product>.Filter.AnyIn(p => p.Tags, search.ToLower().Split(',').Select(t => t.Trim()).ToList())
                ));
        }

        if (!string.IsNullOrWhiteSpace(category))
            filter = Builders<Product>.Filter.And(filter, Builders<Product>.Filter.Eq(p => p.CategoryId, category));

        if (!string.IsNullOrWhiteSpace(subCategory))
            filter = Builders<Product>.Filter.And(filter, Builders<Product>.Filter.Eq(p => p.SubCategoryId, subCategory));

        if (!string.IsNullOrWhiteSpace(brand))
            filter = Builders<Product>.Filter.And(filter, Builders<Product>.Filter.Eq(p => p.BrandId, brand));

        if (!string.IsNullOrWhiteSpace(sellerId))
            filter = Builders<Product>.Filter.And(filter, Builders<Product>.Filter.Eq(p => p.SellerId, sellerId));

        if (minPrice.HasValue)
            filter = Builders<Product>.Filter.And(filter, Builders<Product>.Filter.Gte(p => p.Price, minPrice.Value));

        if (maxPrice.HasValue)
            filter = Builders<Product>.Filter.And(filter, Builders<Product>.Filter.Lte(p => p.Price, maxPrice.Value));

        if (minRating.HasValue)
            filter = Builders<Product>.Filter.And(filter, Builders<Product>.Filter.Gte(p => p.AverageRating, minRating.Value));

        if (inStock.HasValue)
        {
            if (inStock.Value)
                filter = Builders<Product>.Filter.And(filter, Builders<Product>.Filter.Gt(p => p.StockQuantity, 0));
            else
                filter = Builders<Product>.Filter.And(filter, Builders<Product>.Filter.Eq(p => p.StockQuantity, 0));
        }

        var totalCount = await _mongoDb.Products.CountDocumentsAsync(filter);

        var sortDef = sort switch
        {
            "price_asc" => Builders<Product>.Sort.Ascending(p => p.Price),
            "price_desc" => Builders<Product>.Sort.Descending(p => p.Price),
            "rating" => Builders<Product>.Sort.Descending(p => p.AverageRating),
            "newest" => Builders<Product>.Sort.Descending(p => p.CreatedAt),
            "oldest" => Builders<Product>.Sort.Ascending(p => p.CreatedAt),
            "best_selling" => Builders<Product>.Sort.Descending(p => p.SoldCount),
            "most_viewed" => Builders<Product>.Sort.Descending(p => p.ViewCount),
            "name_asc" => Builders<Product>.Sort.Ascending(p => p.Name),
            "name_desc" => Builders<Product>.Sort.Descending(p => p.Name),
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
            products = products.Select(MapToPublicProduct),
            totalCount,
            page,
            totalPages = (int)Math.Ceiling((double)totalCount / limit),
            hasMore = page * limit < totalCount
        });
    }

    // Public - get single product by id or slug
    [HttpGet("{idOrSlug}")]
    public async Task<IActionResult> GetProduct(string idOrSlug)
    {
        var product = await _mongoDb.Products
            .Find(p => p.Id == idOrSlug || p.Slug == idOrSlug)
            .FirstOrDefaultAsync();

        if (product == null)
            return NotFound(new { message = "Product not found." });

        // Increment view count
        await _mongoDb.Products.UpdateOneAsync(
            p => p.Id == product.Id,
            Builders<Product>.Update.Inc(p => p.ViewCount, 1)
        );

        // Get seller info
        var seller = await _mongoDb.Users.Find(u => u.Id == product.SellerId).FirstOrDefaultAsync();
        var sellerProfile = await _mongoDb.SellerProfiles.Find(p => p.UserId == product.SellerId).FirstOrDefaultAsync();

        // Get category info
        var category = product.CategoryId != null
            ? await _mongoDb.Categories.Find(c => c.Id == product.CategoryId).FirstOrDefaultAsync()
            : null;

        // Get brand info
        var brand = product.BrandId != null
            ? await _mongoDb.Brands.Find(b => b.Id == product.BrandId).FirstOrDefaultAsync()
            : null;

        return Ok(new
        {
            product = MapToPublicProduct(product),
            seller = seller != null ? new { id = seller.Id, name = seller.Name, shopName = sellerProfile?.ShopName } : null,
            category = category != null ? new { id = category.Id, name = category.Name, slug = category.Slug } : null,
            brand = brand != null ? new { id = brand.Id, name = brand.Name, logo = brand.Logo } : null
        });
    }

    // Seller - get own products (any status)
    [Authorize(Roles = "seller,admin")]
    [HttpGet("manage")]
    public async Task<IActionResult> GetManageProducts([FromQuery] string? status)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var filter = Builders<Product>.Filter.Eq(p => p.SellerId, userId);
        if (!string.IsNullOrWhiteSpace(status))
            filter = Builders<Product>.Filter.And(filter, Builders<Product>.Filter.Eq(p => p.Status, status));

        var products = await _mongoDb.Products
            .Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(products.Select(MapToManageProduct));
    }

    // Seller - create product
    [Authorize(Roles = "seller,admin")]
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Product name is required." });

        if (request.Price <= 0)
            return BadRequest(new { message = "Price must be greater than 0." });

        var slug = GenerateSlug(request.Name);

        // Check slug uniqueness
        var existingSlug = await _mongoDb.Products.Find(p => p.Slug == slug).FirstOrDefaultAsync();
        if (existingSlug != null)
            slug = $"{slug}-{DateTime.UtcNow.Ticks}";

        var product = new Product
        {
            SellerId = userId,
            CategoryId = request.CategoryId,
            SubCategoryId = request.SubCategoryId,
            BrandId = request.BrandId,
            Name = request.Name.Trim(),
            Slug = slug,
            ShortDescription = request.ShortDescription,
            Description = request.Description ?? "",
            Sku = request.Sku,
            Barcode = request.Barcode,
            Price = request.Price,
            DiscountPrice = request.DiscountPrice,
            CostPrice = request.CostPrice,
            Currency = request.Currency ?? "BDT",
            StockQuantity = request.StockQuantity,
            ReservedStock = 0,
            MinimumStockLevel = request.MinimumStockLevel > 0 ? request.MinimumStockLevel : 5,
            Weight = request.Weight,
            Dimensions = request.Dimensions,
            Status = request.Status ?? "draft",
            ApprovalStatus = "pending",
            ThumbnailImage = request.ThumbnailImage,
            ImageGallery = request.ImageGallery ?? new(),
            Tags = request.Tags?.Select(t => t.ToLower().Trim()).ToList() ?? new(),
            Variants = request.Variants ?? new(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _mongoDb.Products.InsertOneAsync(product);

        // Log inventory
        if (product.StockQuantity > 0)
        {
            await LogInventory(product.Id, product.Name, userId, "stock_in", product.StockQuantity, 0, product.StockQuantity, "Initial stock");
        }

        return Ok(MapToManageProduct(product));
    }

    // Seller - update own product
    [Authorize(Roles = "seller,admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] UpdateProductRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var product = await _mongoDb.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
        if (product == null) return NotFound(new { message = "Product not found." });

        // Ownership check (admin can edit any)
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "admin" && product.SellerId != userId)
            return Forbid();

        var update = Builders<Product>.Update.Set(p => p.UpdatedAt, DateTime.UtcNow);

        if (request.Name != null)
        {
            update = update.Set(p => p.Name, request.Name.Trim());
            update = update.Set(p => p.Slug, GenerateSlug(request.Name));
        }
        if (request.ShortDescription != null) update = update.Set(p => p.ShortDescription, request.ShortDescription);
        if (request.Description != null) update = update.Set(p => p.Description, request.Description);
        if (request.CategoryId != null) update = update.Set(p => p.CategoryId, request.CategoryId);
        if (request.SubCategoryId != null) update = update.Set(p => p.SubCategoryId, request.SubCategoryId);
        if (request.BrandId != null) update = update.Set(p => p.BrandId, request.BrandId);
        if (request.Sku != null) update = update.Set(p => p.Sku, request.Sku);
        if (request.Barcode != null) update = update.Set(p => p.Barcode, request.Barcode);
        if (request.Price.HasValue) update = update.Set(p => p.Price, request.Price.Value);
        if (request.DiscountPrice.HasValue) update = update.Set(p => p.DiscountPrice, request.DiscountPrice);
        if (request.CostPrice.HasValue) update = update.Set(p => p.CostPrice, request.CostPrice);
        if (request.Weight.HasValue) update = update.Set(p => p.Weight, request.Weight);
        if (request.Dimensions != null) update = update.Set(p => p.Dimensions, request.Dimensions);
        if (request.ThumbnailImage != null) update = update.Set(p => p.ThumbnailImage, request.ThumbnailImage);
        if (request.ImageGallery != null) update = update.Set(p => p.ImageGallery, request.ImageGallery);
        if (request.Tags != null) update = update.Set(p => p.Tags, request.Tags.Select(t => t.ToLower().Trim()).ToList());
        if (request.Variants != null) update = update.Set(p => p.Variants, request.Variants);
        if (request.MinimumStockLevel.HasValue) update = update.Set(p => p.MinimumStockLevel, request.MinimumStockLevel.Value);

        // Stock update with inventory logging
        if (request.StockQuantity.HasValue && request.StockQuantity.Value != product.StockQuantity)
        {
            var oldStock = product.StockQuantity;
            var newStock = request.StockQuantity.Value;
            update = update.Set(p => p.StockQuantity, newStock);
            await LogInventory(id, product.Name, userId, "adjustment", newStock - oldStock, oldStock, newStock, "Stock updated by seller");
        }

        // Status update
        if (request.Status != null)
        {
            update = update.Set(p => p.Status, request.Status);
            if (request.Status == "pending")
                update = update.Set(p => p.ApprovalStatus, "pending");
        }

        await _mongoDb.Products.UpdateOneAsync(p => p.Id == id, update);

        var updated = await _mongoDb.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
        return Ok(MapToManageProduct(updated!));
    }

    // Seller - delete own product
    [Authorize(Roles = "seller,admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var product = await _mongoDb.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
        if (product == null) return NotFound(new { message = "Product not found." });

        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "admin" && product.SellerId != userId)
            return Forbid();

        await _mongoDb.Products.DeleteOneAsync(p => p.Id == id);
        return Ok(new { message = "Product deleted." });
    }

    // Seller - update stock
    [Authorize(Roles = "seller,admin")]
    [HttpPut("{id}/stock")]
    public async Task<IActionResult> UpdateStock(string id, [FromBody] UpdateStockRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var product = await _mongoDb.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
        if (product == null) return NotFound(new { message = "Product not found." });

        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "admin" && product.SellerId != userId)
            return Forbid();

        var oldStock = product.StockQuantity;
        var newStock = request.Quantity;

        await _mongoDb.Products.UpdateOneAsync(
            p => p.Id == id,
            Builders<Product>.Update
                .Set(p => p.StockQuantity, newStock)
                .Set(p => p.UpdatedAt, DateTime.UtcNow)
        );

        var action = newStock > oldStock ? "stock_in" : "stock_out";
        await LogInventory(id, product.Name, userId, action, newStock - oldStock, oldStock, newStock, request.Reason ?? "Stock updated");

        return Ok(new { message = "Stock updated.", stockQuantity = newStock });
    }

    // Seller - upload image (returns placeholder URL)
    [Authorize(Roles = "seller,admin")]
    [HttpPost("{id}/images")]
    public async Task<IActionResult> UploadImage(string id, [FromBody] UploadImageRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var product = await _mongoDb.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
        if (product == null) return NotFound(new { message = "Product not found." });

        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "admin" && product.SellerId != userId)
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.ImageUrl))
            return BadRequest(new { message = "Image URL is required." });

        var gallery = product.ImageGallery;
        gallery.Add(request.ImageUrl);

        await _mongoDb.Products.UpdateOneAsync(
            p => p.Id == id,
            Builders<Product>.Update
                .Set(p => p.ImageGallery, gallery)
                .Set(p => p.UpdatedAt, DateTime.UtcNow)
        );

        return Ok(new { message = "Image added.", imageGallery = gallery });
    }

    // Seller - delete image
    [Authorize(Roles = "seller,admin")]
    [HttpDelete("{id}/images/{index}")]
    public async Task<IActionResult> DeleteImage(string id, int index)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var product = await _mongoDb.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
        if (product == null) return NotFound(new { message = "Product not found." });

        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "admin" && product.SellerId != userId)
            return Forbid();

        if (index < 0 || index >= product.ImageGallery.Count)
            return BadRequest(new { message = "Invalid image index." });

        var gallery = product.ImageGallery;
        gallery.RemoveAt(index);

        await _mongoDb.Products.UpdateOneAsync(
            p => p.Id == id,
            Builders<Product>.Update
                .Set(p => p.ImageGallery, gallery)
                .Set(p => p.UpdatedAt, DateTime.UtcNow)
        );

        return Ok(new { message = "Image deleted.", imageGallery = gallery });
    }

    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLower()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace(".", "-")
            .Replace(",", "-")
            .Replace("--", "-")
            .Trim('-');
    }

    private async Task LogInventory(string productId, string productName, string sellerId, string action, int quantity, int previousStock, int newStock, string reason)
    {
        var log = new InventoryLog
        {
            ProductId = productId,
            ProductName = productName,
            SellerId = sellerId,
            Action = action,
            Quantity = Math.Abs(quantity),
            PreviousStock = previousStock,
            NewStock = newStock,
            Reason = reason,
            PerformedBy = sellerId,
            CreatedAt = DateTime.UtcNow
        };
        await _mongoDb.InventoryLogs.InsertOneAsync(log);
    }

    private static object MapToPublicProduct(Product p) => new
    {
        id = p.Id,
        name = p.Name,
        slug = p.Slug,
        shortDescription = p.ShortDescription,
        description = p.Description,
        price = p.Price,
        discountPrice = p.DiscountPrice,
        currency = p.Currency,
        stockQuantity = p.StockQuantity,
        averageRating = p.AverageRating,
        reviewCount = p.ReviewCount,
        soldCount = p.SoldCount,
        viewCount = p.ViewCount,
        thumbnailImage = p.ThumbnailImage,
        imageGallery = p.ImageGallery,
        tags = p.Tags,
        variants = p.Variants,
        categoryId = p.CategoryId,
        subCategoryId = p.SubCategoryId,
        brandId = p.BrandId,
        sellerId = p.SellerId,
        createdAt = p.CreatedAt
    };

    private static object MapToManageProduct(Product p) => new
    {
        id = p.Id,
        name = p.Name,
        slug = p.Slug,
        shortDescription = p.ShortDescription,
        description = p.Description,
        sku = p.Sku,
        barcode = p.Barcode,
        price = p.Price,
        discountPrice = p.DiscountPrice,
        costPrice = p.CostPrice,
        currency = p.Currency,
        stockQuantity = p.StockQuantity,
        reservedStock = p.ReservedStock,
        minimumStockLevel = p.MinimumStockLevel,
        weight = p.Weight,
        dimensions = p.Dimensions,
        status = p.Status,
        approvalStatus = p.ApprovalStatus,
        averageRating = p.AverageRating,
        reviewCount = p.ReviewCount,
        soldCount = p.SoldCount,
        viewCount = p.ViewCount,
        thumbnailImage = p.ThumbnailImage,
        imageGallery = p.ImageGallery,
        tags = p.Tags,
        variants = p.Variants,
        categoryId = p.CategoryId,
        subCategoryId = p.SubCategoryId,
        brandId = p.BrandId,
        sellerId = p.SellerId,
        createdAt = p.CreatedAt,
        updatedAt = p.UpdatedAt
    };
}

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? CategoryId { get; set; }
    public string? SubCategoryId { get; set; }
    public string? BrandId { get; set; }
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public string? Currency { get; set; }
    public int StockQuantity { get; set; }
    public int MinimumStockLevel { get; set; }
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public string? Status { get; set; }
    public string? ThumbnailImage { get; set; }
    public List<string>? ImageGallery { get; set; }
    public List<string>? Tags { get; set; }
    public List<ProductVariant>? Variants { get; set; }
}

public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? CategoryId { get; set; }
    public string? SubCategoryId { get; set; }
    public string? BrandId { get; set; }
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public decimal? Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public int? StockQuantity { get; set; }
    public int? MinimumStockLevel { get; set; }
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public string? Status { get; set; }
    public string? ThumbnailImage { get; set; }
    public List<string>? ImageGallery { get; set; }
    public List<string>? Tags { get; set; }
    public List<ProductVariant>? Variants { get; set; }
}

public class UpdateStockRequest
{
    public int Quantity { get; set; }
    public string? Reason { get; set; }
}

public class UploadImageRequest
{
    public string ImageUrl { get; set; } = string.Empty;
}
