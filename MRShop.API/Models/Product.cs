using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MRShop.API.Models;

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("sellerId")]
    public string SellerId { get; set; } = string.Empty;

    [BsonElement("categoryId")]
    public string? CategoryId { get; set; }

    [BsonElement("subCategoryId")]
    public string? SubCategoryId { get; set; }

    [BsonElement("brandId")]
    public string? BrandId { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("slug")]
    public string Slug { get; set; } = string.Empty;

    [BsonElement("shortDescription")]
    public string? ShortDescription { get; set; }

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("sku")]
    public string? Sku { get; set; }

    [BsonElement("barcode")]
    public string? Barcode { get; set; }

    [BsonElement("price")]
    public decimal Price { get; set; }

    [BsonElement("discountPrice")]
    public decimal? DiscountPrice { get; set; }

    [BsonElement("originalPrice")]
    public decimal? OriginalPrice { get; set; }

    [BsonElement("costPrice")]
    public decimal? CostPrice { get; set; }

    [BsonElement("currency")]
    public string Currency { get; set; } = "BDT";

    [BsonElement("stockQuantity")]
    public int StockQuantity { get; set; }

    [BsonElement("reservedStock")]
    public int ReservedStock { get; set; }

    [BsonElement("minimumStockLevel")]
    public int MinimumStockLevel { get; set; } = 5;

    [BsonElement("weight")]
    public decimal? Weight { get; set; }

    [BsonElement("dimensions")]
    public string? Dimensions { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = "draft"; // draft, pending, published, hidden, archived

    [BsonElement("approvalStatus")]
    public string ApprovalStatus { get; set; } = "pending"; // pending, approved, rejected

    [BsonElement("averageRating")]
    public double AverageRating { get; set; }

    [BsonElement("reviewCount")]
    public int ReviewCount { get; set; }

    [BsonElement("soldCount")]
    public int SoldCount { get; set; }

    [BsonElement("viewCount")]
    public int ViewCount { get; set; }

    [BsonElement("thumbnailImage")]
    public string? ThumbnailImage { get; set; }

    [BsonElement("imageGallery")]
    public List<string> ImageGallery { get; set; } = new();

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    [BsonElement("variants")]
    public List<ProductVariant> Variants { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class ProductVariant
{
    [BsonElement("variantId")]
    public string VariantId { get; set; } = Guid.NewGuid().ToString("N")[..12];

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty; // e.g. "Color", "Size"

    [BsonElement("value")]
    public string Value { get; set; } = string.Empty; // e.g. "Red", "XL"

    [BsonElement("sku")]
    public string? Sku { get; set; }

    [BsonElement("price")]
    public decimal? Price { get; set; }

    [BsonElement("stock")]
    public int Stock { get; set; }

    [BsonElement("image")]
    public string? Image { get; set; }
}
