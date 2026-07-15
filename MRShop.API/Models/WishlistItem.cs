using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MRShop.API.Models;

public class WishlistItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("productId")]
    public string ProductId { get; set; } = string.Empty;

    [BsonElement("productName")]
    public string ProductName { get; set; } = string.Empty;

    [BsonElement("price")]
    public decimal Price { get; set; }

    [BsonElement("image")]
    public string Image { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
