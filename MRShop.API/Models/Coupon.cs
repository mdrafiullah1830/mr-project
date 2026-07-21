using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MRShop.API.Models;

public class Coupon
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("code")]
    public string Code { get; set; } = string.Empty;

    [BsonElement("discountType")]
    public string DiscountType { get; set; } = "percent"; // percent, fixed, shipping

    [BsonElement("discountValue")]
    public decimal DiscountValue { get; set; }

    [BsonElement("minOrder")]
    public decimal MinOrder { get; set; }

    [BsonElement("maxUsage")]
    public int MaxUsage { get; set; } = 1000;

    [BsonElement("usageCount")]
    public int UsageCount { get; set; }

    [BsonElement("expiresAt")]
    public DateTime? ExpiresAt { get; set; }

    [BsonElement("maxDiscount")]
    public decimal MaxDiscount { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
