namespace MRShop.API.DTOs;

public class WishlistItemResponse
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Image { get; set; } = string.Empty;
}

public class AddToWishlistRequest
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Image { get; set; } = string.Empty;
}
