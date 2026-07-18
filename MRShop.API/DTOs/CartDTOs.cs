namespace MRShop.API.DTOs;

public class CartItemResponse
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Image { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class AddToCartRequest
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Image { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
}

public class UpdateCartRequest
{
    public int Quantity { get; set; } = 1;
}
