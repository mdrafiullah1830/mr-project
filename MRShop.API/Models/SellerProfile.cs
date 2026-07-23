using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MRShop.API.Models;

public class SellerProfile
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("shopName")]
    public string ShopName { get; set; } = string.Empty;

    [BsonElement("shopDescription")]
    public string? ShopDescription { get; set; }

    [BsonElement("shopLogo")]
    public string? ShopLogo { get; set; }

    [BsonElement("shopBanner")]
    public string? ShopBanner { get; set; }

    [BsonElement("phone")]
    public string? Phone { get; set; }

    [BsonElement("address")]
    public string? Address { get; set; }

    [BsonElement("city")]
    public string? City { get; set; }

    [BsonElement("country")]
    public string? Country { get; set; }

    [BsonElement("latitude")]
    public string? Latitude { get; set; }

    [BsonElement("longitude")]
    public string? Longitude { get; set; }

    [BsonElement("paymentMethod")]
    public string? PaymentMethod { get; set; }

    [BsonElement("bankName")]
    public string? BankName { get; set; }

    [BsonElement("accountNumber")]
    public string? AccountNumber { get; set; }

    [BsonElement("categories")]
    public List<string> Categories { get; set; } = new();

    [BsonElement("totalSales")]
    public decimal TotalSales { get; set; }

    [BsonElement("totalOrders")]
    public int TotalOrders { get; set; }

    [BsonElement("totalProducts")]
    public int TotalProducts { get; set; }

    [BsonElement("rating")]
    public double Rating { get; set; }

    [BsonElement("isVerified")]
    public bool IsVerified { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("sellerUsername")]
    public string? SellerUsername { get; set; }

    [BsonElement("sellerPasswordHash")]
    public string? SellerPasswordHash { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
