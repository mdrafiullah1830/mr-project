using Newtonsoft.Json;

namespace MRShop.Admin.Models
{
    /// <summary>
    /// Category model for admin panel
    /// </summary>
    public class Category
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("image_path")]
        public string? ImagePath { get; set; }

        [JsonProperty("display_order")]
        public int DisplayOrder { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; } = "active"; // active, inactive

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; } = DateTime.UtcNow.ToString("o");
    }

    /// <summary>
    /// Product model for admin panel
    /// </summary>
    public class Product
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("category_id")]
        public int CategoryId { get; set; }

        [JsonProperty("category")]
        public string? Category { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("stock")]
        public int Stock { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("image_path")]
        public string? ImagePath { get; set; }

        [JsonProperty("discount")]
        public decimal Discount { get; set; } // Percentage

        [JsonProperty("status")]
        public string Status { get; set; } = "active";

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; } = DateTime.UtcNow.ToString("o");
    }

    /// <summary>
    /// Order model for admin dashboard
    /// </summary>
    public class AdminOrder
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("customer_name")]
        public string CustomerName { get; set; } = string.Empty;

        [JsonProperty("total")]
        public decimal Total { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; } = "pending"; // pending, processing, completed, cancelled

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; } = DateTime.UtcNow.ToString("o");
    }

    /// <summary>
    /// User summary for admin dashboard
    /// </summary>
    public class AdminUser
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("username")]
        public string Username { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonProperty("role")]
        public string Role { get; set; } = "user";

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");
    }

    /// <summary>
    /// Request model for creating category
    /// </summary>
    public class CreateCategoryRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("display_order")]
        public int DisplayOrder { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; } = "active";
    }

    /// <summary>
    /// Request model for creating product
    /// </summary>
    public class CreateProductRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("category_id")]
        public int CategoryId { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("stock")]
        public int Stock { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("discount")]
        public decimal Discount { get; set; }
    }

    /// <summary>
    /// Site settings model
    /// </summary>
    public class SiteSettings
    {
        [JsonProperty("site_name")]
        public string SiteName { get; set; } = "MR Shop";

        [JsonProperty("site_description")]
        public string SiteDescription { get; set; } = string.Empty;

        [JsonProperty("contact_email")]
        public string ContactEmail { get; set; } = string.Empty;

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; } = DateTime.UtcNow.ToString("o");
    }

    /// <summary>
    /// Response model for admin operations
    /// </summary>
    public class AdminResponse<T>
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("data")]
        public T? Data { get; set; }
    }
}
