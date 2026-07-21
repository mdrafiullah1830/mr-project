using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MRShop.API.Models;

public class Order
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("orderNumber")]
    public string OrderNumber { get; set; } = string.Empty;

    [BsonElement("customerId")]
    public string CustomerId { get; set; } = string.Empty;

    [BsonElement("customerName")]
    public string CustomerName { get; set; } = string.Empty;

    [BsonElement("customerEmail")]
    public string? CustomerEmail { get; set; }

    [BsonElement("customerPhone")]
    public string? CustomerPhone { get; set; }

    [BsonElement("sellerId")]
    public string SellerId { get; set; } = string.Empty;

    [BsonElement("sellerName")]
    public string SellerName { get; set; } = string.Empty;

    [BsonElement("items")]
    public List<OrderItem> Items { get; set; } = new();

    [BsonElement("shippingAddress")]
    public OrderAddress ShippingAddress { get; set; } = new();

    [BsonElement("billingAddress")]
    public OrderAddress? BillingAddress { get; set; }

    [BsonElement("paymentMethod")]
    public string PaymentMethod { get; set; } = string.Empty;

    [BsonElement("paymentStatus")]
    public string PaymentStatus { get; set; } = "pending"; // pending, paid, failed, refunded

    [BsonElement("status")]
    public string Status { get; set; } = "pending";

    [BsonElement("deliveryMethod")]
    public string DeliveryMethod { get; set; } = "standard"; // standard, express, pickup

    [BsonElement("trackingNumber")]
    public string? TrackingNumber { get; set; }

    [BsonElement("carrier")]
    public string? Carrier { get; set; }

    [BsonElement("subtotal")]
    public decimal Subtotal { get; set; }

    [BsonElement("discount")]
    public decimal Discount { get; set; }

    [BsonElement("couponCode")]
    public string? CouponCode { get; set; }

    [BsonElement("couponDiscount")]
    public decimal CouponDiscount { get; set; }

    [BsonElement("shippingCharge")]
    public decimal ShippingCharge { get; set; }

    [BsonElement("tax")]
    public decimal Tax { get; set; }

    [BsonElement("grandTotal")]
    public decimal GrandTotal { get; set; }

    [BsonElement("notes")]
    public string? Notes { get; set; }

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

    [BsonElement("sku")]
    public string? Sku { get; set; }

    [BsonElement("price")]
    public decimal Price { get; set; }

    [BsonElement("discountPrice")]
    public decimal? DiscountPrice { get; set; }

    [BsonElement("quantity")]
    public int Quantity { get; set; }

    [BsonElement("image")]
    public string? Image { get; set; }

    [BsonElement("sellerId")]
    public string SellerId { get; set; } = string.Empty;
}

public class OrderAddress
{
    [BsonElement("fullName")]
    public string FullName { get; set; } = string.Empty;

    [BsonElement("phone")]
    public string Phone { get; set; } = string.Empty;

    [BsonElement("email")]
    public string? Email { get; set; }

    [BsonElement("country")]
    public string Country { get; set; } = "Bangladesh";

    [BsonElement("division")]
    public string? Division { get; set; }

    [BsonElement("district")]
    public string? District { get; set; }

    [BsonElement("city")]
    public string? City { get; set; }

    [BsonElement("area")]
    public string? Area { get; set; }

    [BsonElement("postalCode")]
    public string? PostalCode { get; set; }

    [BsonElement("addressLine")]
    public string AddressLine { get; set; } = string.Empty;

    public string ToDisplayString()
    {
        var parts = new List<string> { AddressLine };
        if (!string.IsNullOrEmpty(Area)) parts.Add(Area);
        if (!string.IsNullOrEmpty(City)) parts.Add(City);
        if (!string.IsNullOrEmpty(District)) parts.Add(District);
        if (!string.IsNullOrEmpty(Division)) parts.Add(Division);
        if (!string.IsNullOrEmpty(PostalCode)) parts.Add(PostalCode);
        parts.Add(Country);
        return string.Join(", ", parts);
    }
}
