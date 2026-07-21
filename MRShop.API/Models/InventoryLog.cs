using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MRShop.API.Models;

public class InventoryLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("productId")]
    public string ProductId { get; set; } = string.Empty;

    [BsonElement("productName")]
    public string ProductName { get; set; } = string.Empty;

    [BsonElement("sellerId")]
    public string SellerId { get; set; } = string.Empty;

    [BsonElement("variantId")]
    public string? VariantId { get; set; }

    [BsonElement("action")]
    public string Action { get; set; } = string.Empty; // stock_in, stock_out, adjustment, sale, return

    [BsonElement("quantity")]
    public int Quantity { get; set; }

    [BsonElement("previousStock")]
    public int PreviousStock { get; set; }

    [BsonElement("newStock")]
    public int NewStock { get; set; }

    [BsonElement("reason")]
    public string? Reason { get; set; }

    [BsonElement("orderId")]
    public string? OrderId { get; set; }

    [BsonElement("performedBy")]
    public string PerformedBy { get; set; } = string.Empty; // admin or seller userId

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
