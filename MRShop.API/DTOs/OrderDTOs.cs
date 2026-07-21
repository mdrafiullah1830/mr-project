namespace MRShop.API.DTOs;

// ==================== CART DTOs ====================
public class CartItemResponse
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string Image { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string SellerId { get; set; } = string.Empty;
    public string? SellerName { get; set; }
    public int StockQuantity { get; set; }
    public bool SavedForLater { get; set; }
    public decimal LineTotal => (DiscountPrice ?? Price) * Quantity;
}

public class CartSummary
{
    public List<CartItemResponse> Items { get; set; } = new();
    public List<CartItemResponse> SavedForLater { get; set; } = new();
    public int ItemCount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal CouponDiscount { get; set; }
    public string? CouponCode { get; set; }
    public decimal ShippingCharge { get; set; }
    public decimal Tax { get; set; }
    public decimal GrandTotal { get; set; }
}

public class AddToCartRequest
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
}

public class UpdateCartRequest
{
    public int Quantity { get; set; }
}

public class ApplyCouponRequest
{
    public string Code { get; set; } = string.Empty;
}

// ==================== ADDRESS DTOs ====================
public class AddressResponse
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Country { get; set; } = "Bangladesh";
    public string? Division { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string? Area { get; set; }
    public string? PostalCode { get; set; }
    public string AddressLine { get; set; } = string.Empty;
    public string AddressType { get; set; } = "home";
    public bool IsDefault { get; set; }
}

public class CreateAddressRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Country { get; set; } = "Bangladesh";
    public string? Division { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string? Area { get; set; }
    public string? PostalCode { get; set; }
    public string AddressLine { get; set; } = string.Empty;
    public string AddressType { get; set; } = "home";
    public bool IsDefault { get; set; }
}

// ==================== CHECKOUT DTOs ====================
public class CheckoutRequest
{
    public string ShippingAddressId { get; set; } = string.Empty;
    public string? BillingAddressId { get; set; }
    public string PaymentMethod { get; set; } = "cod";
    public string DeliveryMethod { get; set; } = "standard";
    public string? CouponCode { get; set; }
    public string? Notes { get; set; }
}

public class CheckoutSummary
{
    public List<CartItemResponse> Items { get; set; } = new();
    public AddressResponse? ShippingAddress { get; set; }
    public AddressResponse? BillingAddress { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal CouponDiscount { get; set; }
    public string? CouponCode { get; set; }
    public decimal ShippingCharge { get; set; }
    public decimal Tax { get; set; }
    public decimal GrandTotal { get; set; }
    public string DeliveryMethod { get; set; } = "standard";
    public string PaymentMethod { get; set; } = "cod";
}

// ==================== ORDER DTOs ====================
public class OrderResponse
{
    public string Id { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string SellerId { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
    public List<OrderItemResponse> Items { get; set; } = new();
    public OrderAddressResponse? ShippingAddress { get; set; }
    public OrderAddressResponse? BillingAddress { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = "pending";
    public string Status { get; set; } = string.Empty;
    public string DeliveryMethod { get; set; } = "standard";
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public string? CouponCode { get; set; }
    public decimal CouponDiscount { get; set; }
    public decimal ShippingCharge { get; set; }
    public decimal Tax { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Notes { get; set; }
    public List<OrderTimelineResponse> Timeline { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class OrderItemResponse
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Quantity { get; set; }
    public string? Image { get; set; }
    public string SellerId { get; set; } = string.Empty;
}

public class OrderAddressResponse
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Country { get; set; } = "Bangladesh";
    public string? Division { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string? Area { get; set; }
    public string? PostalCode { get; set; }
    public string AddressLine { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
}

public class OrderTimelineResponse
{
    public string Status { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public string? Remarks { get; set; }
}

public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }
    public string? Remarks { get; set; }
}

public class OrderSearchRequest
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? PaymentStatus { get; set; }
    public string? SellerId { get; set; }
    public string? CustomerId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Sort { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}

// ==================== INVOICE DTOs ====================
public class InvoiceResponse
{
    public string Id { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public List<InvoiceItemResponse> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal CouponDiscount { get; set; }
    public decimal ShippingCharge { get; set; }
    public decimal Tax { get; set; }
    public decimal GrandTotal { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class InvoiceItemResponse
{
    public string ProductName { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Total { get; set; }
    public string? Image { get; set; }
}
