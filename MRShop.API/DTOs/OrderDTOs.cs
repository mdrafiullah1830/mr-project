namespace MRShop.API.DTOs;

public class OrderResponse
{
    public string Id { get; set; } = string.Empty;
    public List<OrderItemResponse> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class OrderItemResponse
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string Image { get; set; } = string.Empty;
}

public class CreateOrderRequest
{
    public string ShippingAddress { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
}
