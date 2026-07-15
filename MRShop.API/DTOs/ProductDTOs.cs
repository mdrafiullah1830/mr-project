namespace MRShop.API.DTOs;

public class ProductResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Subcategory { get; set; }
    public string Image { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new();
    public int Stock { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
}

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Subcategory { get; set; }
    public string Image { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new();
    public int Stock { get; set; }
}

public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string? Category { get; set; }
    public string? Subcategory { get; set; }
    public string? Image { get; set; }
    public List<string>? Images { get; set; }
    public int? Stock { get; set; }
}
