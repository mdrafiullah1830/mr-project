using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MRShop.API.Models;

public class Order
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("items")]
    public List<OrderItem> Items { get; set; } = new();

    [BsonElement("totalAmount")]
    public decimal TotalAmount { get; set; }

    [BsonElement("shippingAddress")]
    public string ShippingAddress { get; set; } = string.Empty;

    [BsonElement("paymentMethod")]
    public string PaymentMethod { get; set; } = string.Empty; // bkash, nagad, cod, card

    [BsonElement("status")]
    public string Status { get; set; } = "pending"; // pending, confirmed, shipped, delivered, cancelled

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class OrderItem
{
    [BsonElement("productId")]
    public string ProductId { get; set; } = string.Empty;

    [BsonElement("productName")]
    public string ProductName { get; set; } = string.Empty;

    [BsonElement("price")]
    public decimal Price { get; set; }

    [BsonElement("quantity")]
    public int Quantity { get; set; }

    [BsonElement("image")]
    public string Image { get; set; } = string.Empty;
}
