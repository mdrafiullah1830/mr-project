using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MRShop.API.Models;

public class Invoice
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("orderId")]
    public string OrderId { get; set; } = string.Empty;

    [BsonElement("invoiceNumber")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [BsonElement("customerId")]
    public string CustomerId { get; set; } = string.Empty;

    [BsonElement("customerName")]
    public string CustomerName { get; set; } = string.Empty;

    [BsonElement("customerEmail")]
    public string? CustomerEmail { get; set; }

    [BsonElement("sellerId")]
    public string SellerId { get; set; } = string.Empty;

    [BsonElement("sellerName")]
    public string SellerName { get; set; } = string.Empty;

    [BsonElement("items")]
    public List<InvoiceItem> Items { get; set; } = new();

    [BsonElement("subtotal")]
    public decimal Subtotal { get; set; }

    [BsonElement("discount")]
    public decimal Discount { get; set; }

    [BsonElement("couponDiscount")]
    public decimal CouponDiscount { get; set; }

    [BsonElement("shippingCharge")]
    public decimal ShippingCharge { get; set; }

    [BsonElement("tax")]
    public decimal Tax { get; set; }

    [BsonElement("grandTotal")]
    public decimal GrandTotal { get; set; }

    [BsonElement("shippingAddress")]
    public string ShippingAddress { get; set; } = string.Empty;

    [BsonElement("paymentMethod")]
    public string PaymentMethod { get; set; } = string.Empty;

    [BsonElement("paymentStatus")]
    public string PaymentStatus { get; set; } = "pending";

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class InvoiceItem
{
    [BsonElement("productId")]
    public string ProductId { get; set; } = string.Empty;

    [BsonElement("productName")]
    public string ProductName { get; set; } = string.Empty;

    [BsonElement("sku")]
    public string? Sku { get; set; }

    [BsonElement("price")]
    public decimal Price { get; set; }

    [BsonElement("quantity")]
    public int Quantity { get; set; }

    [BsonElement("total")]
    public decimal Total { get; set; }

    [BsonElement("image")]
    public string? Image { get; set; }
}
