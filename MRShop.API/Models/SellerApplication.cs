using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MRShop.API.Models;

public class SellerApplication
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("phone")]
    public string? Phone { get; set; }

    [BsonElement("shopName")]
    public string ShopName { get; set; } = string.Empty;

    [BsonElement("businessType")]
    public string? BusinessType { get; set; }

    [BsonElement("paymentMethod")]
    public string? PaymentMethod { get; set; }

    [BsonElement("bankName")]
    public string? BankName { get; set; }

    [BsonElement("accountNumber")]
    public string? AccountNumber { get; set; }

    [BsonElement("latitude")]
    public string? Latitude { get; set; }

    [BsonElement("longitude")]
    public string? Longitude { get; set; }

    [BsonElement("categories")]
    public string? Categories { get; set; }

    [BsonElement("additionalInfo")]
    public string? AdditionalInfo { get; set; }

    [BsonElement("docType")]
    public string? DocType { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = "pending"; // pending, approved, rejected

    [BsonElement("sellerUsername")]
    public string? SellerUsername { get; set; }

    [BsonElement("sellerPasswordHash")]
    public string? SellerPasswordHash { get; set; }

    [BsonElement("sellerPasswordPlain")]
    public string? SellerPasswordPlain { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
