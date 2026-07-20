using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MRShop.API.Models;

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("price")]
    public decimal Price { get; set; }

    [BsonElement("originalPrice")]
    public decimal? OriginalPrice { get; set; }

    [BsonElement("category")]
    public string Category { get; set; } = string.Empty;

    [BsonElement("subcategory")]
    public string? Subcategory { get; set; }

    [BsonElement("image")]
    public string Image { get; set; } = string.Empty;

    [BsonElement("images")]
    public List<string> Images { get; set; } = new();

    [BsonElement("stock")]
    public int Stock { get; set; }

    [BsonElement("rating")]
    public double Rating { get; set; }

    [BsonElement("reviewCount")]
    public int ReviewCount { get; set; }

    [BsonElement("sellerId")]
    public string? SellerId { get; set; }

    [BsonElement("createdBy")]
    public string? CreatedBy { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
