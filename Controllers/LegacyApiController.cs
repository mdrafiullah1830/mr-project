using Microsoft.AspNetCore.Mvc;
using MRShop.AdminPanel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MRShop.OrderTracking.Controllers
{
    [ApiController]
    [Route("api.php")]
    [Produces("application/json")]
    public class LegacyApiController : ControllerBase
    {
        private readonly AdminProductService _productService;

        public LegacyApiController(AdminProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public IActionResult HandleGet(
            [FromQuery] string action = "",
            [FromQuery] string? category = null,
            [FromQuery] string? search = null,
            [FromQuery] string? query = null,
            [FromQuery] int limit = 4)
        {
            var normalizedAction = NormalizeAction(action);

            return normalizedAction switch
            {
                "getproducts" => GetProducts(category),
                "getcategories" => GetCategories(),
                "getfoodnaturalsection" => GetFoodNaturalSection(limit),
                "searchproducts" => SearchProducts(search ?? query),
                _ => BadRequest(new
                {
                    success = false,
                    error = "Invalid request. Use a supported action such as getProducts, getCategories, getFoodNaturalSection, addProduct, updateProduct, deleteProduct, or searchProducts"
                })
            };
        }

        [HttpPost]
        public async Task<IActionResult> HandlePost([FromQuery] string action = "")
        {
            var normalizedAction = NormalizeAction(action);
            var payload = await ReadProductPayloadAsync();

            return normalizedAction switch
            {
                "addproduct" => AddProduct(payload),
                "updateproduct" => UpdateProduct(payload),
                "deleteproduct" => DeleteProduct(payload),
                _ => BadRequest(new
                {
                    success = false,
                    error = "Invalid request. Use a supported action such as getProducts, getCategories, getFoodNaturalSection, addProduct, updateProduct, deleteProduct, or searchProducts"
                })
            };
        }

        private IActionResult GetProducts(string? category)
        {
            var requestedCategory = string.IsNullOrWhiteSpace(category) ? "all" : category.Trim();
            var products = requestedCategory.Equals("all", StringComparison.OrdinalIgnoreCase)
                ? _productService.GetAllProducts().Where(IsActiveProduct).Select(MapProduct).ToList()
                : _productService.GetProductsByCategory(requestedCategory).Select(MapProduct).ToList();

            return Ok(new
            {
                success = true,
                data = products,
                count = products.Count
            });
        }

        private IActionResult GetCategories()
        {
            var categories = _productService.GetAllCategories()
                .Select((category, index) => new LegacyCategoryDto
                {
                    Id = index + 1,
                    Name = category.Name,
                    Slug = NormalizeCategorySlug(category.Name),
                    Icon = category.Icon
                })
                .ToList();

            return Ok(new
            {
                success = true,
                data = categories,
                count = categories.Count
            });
        }

        private IActionResult GetFoodNaturalSection(int limit)
        {
            var sectionLimit = Math.Max(1, limit);
            var products = _productService.GetProductsByCategory("food")
                .Take(sectionLimit)
                .Select(MapProduct)
                .ToList();

            return Ok(new
            {
                success = true,
                message = "Food & Natural section retrieved successfully",
                data = new
                {
                    section = new
                    {
                        title = "Food & Natural",
                        subtitle = "Fresh, organic and authentic products updated from the live catalog.",
                        badge = "Live updates",
                        ctaLabel = "Open full collection",
                        featuredLimit = sectionLimit,
                        category = "food"
                    },
                    products
                },
                count = products.Count
            });
        }

        private IActionResult SearchProducts(string? searchTerm)
        {
            var normalizedSearch = (searchTerm ?? string.Empty).Trim();

            if (normalizedSearch.Length < 2)
            {
                return BadRequest(new
                {
                    success = false,
                    error = "Search term must be at least 2 characters"
                });
            }

            var products = _productService.SearchProducts(normalizedSearch)
                .Where(IsActiveProduct)
                .Take(20)
                .Select(MapProduct)
                .ToList();

            return Ok(new
            {
                success = true,
                data = products,
                count = products.Count
            });
        }

        private IActionResult AddProduct(Product? payload)
        {
            if (payload == null)
            {
                return BadRequest(new { success = false, error = "Product data is required" });
            }

            if (string.IsNullOrWhiteSpace(payload.Name))
            {
                return BadRequest(new { success = false, error = "Product name is required" });
            }

            if (payload.Price < 0)
            {
                return BadRequest(new { success = false, error = "Price is required and must be non-negative" });
            }

            var addedProduct = _productService.AddProduct(payload);

            return StatusCode(201, new
            {
                success = true,
                message = "Product added successfully",
                data = MapProduct(addedProduct)
            });
        }

        private IActionResult UpdateProduct(Product? payload)
        {
            if (payload == null)
            {
                return BadRequest(new { success = false, error = "Product data is required" });
            }

            var productId = payload.Id > 0 ? payload.Id : GetProductIdFromQuery();
            if (productId <= 0)
            {
                return BadRequest(new { success = false, error = "Product ID is required" });
            }

            try
            {
                var updatedProduct = _productService.UpdateProduct(productId, payload);

                return Ok(new
                {
                    success = true,
                    message = "Product updated successfully",
                    data = MapProduct(updatedProduct)
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { success = false, error = ex.Message });
            }
        }

        private IActionResult DeleteProduct(Product? payload)
        {
            var productId = payload?.Id > 0 ? payload.Id : GetProductIdFromQuery();
            if (productId <= 0)
            {
                return BadRequest(new { success = false, error = "Product ID is required" });
            }

            var existingProduct = _productService.GetProductById(productId);
            if (existingProduct == null)
            {
                return NotFound(new { success = false, error = $"Product not found: {productId}" });
            }

            var deleted = _productService.DeleteProduct(productId);
            if (!deleted)
            {
                return StatusCode(500, new { success = false, error = "Delete failed" });
            }

            return Ok(new
            {
                success = true,
                message = "Product deleted successfully",
                data = new
                {
                    deletedId = productId,
                    deletedProduct = MapProduct(existingProduct)
                }
            });
        }

        private async Task<Product?> ReadProductPayloadAsync()
        {
            if (!Request.ContentLength.HasValue || Request.ContentLength.Value == 0)
            {
                return null;
            }

            try
            {
                return await JsonSerializer.DeserializeAsync<Product>(
                    Request.Body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private int GetProductIdFromQuery()
        {
            var queryValue = Request.Query["id"].ToString();
            return int.TryParse(queryValue, out var productId) ? productId : 0;
        }

        private static LegacyProductDto MapProduct(Product product)
        {
            var imagePath = ResolveImagePath(product);

            return new LegacyProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Category = product.Category,
                Seller = string.IsNullOrWhiteSpace(product.Seller) ? "Admin" : product.Seller,
                ImagePath = imagePath,
                ImageBase64 = string.IsNullOrWhiteSpace(product.ImageBase64) ? imagePath : product.ImageBase64,
                ImageUrl = string.IsNullOrWhiteSpace(product.ImageUrl) ? imagePath : product.ImageUrl,
                Discount = product.Discount,
                FinalPrice = product.FinalPrice > 0 ? product.FinalPrice : CalculateFinalPrice(product.Price, product.Discount),
                Rating = product.Rating,
                Stock = product.Stock,
                Status = product.Status,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        private static string ResolveImagePath(Product product)
        {
            if (!string.IsNullOrWhiteSpace(product.ImagePath))
            {
                return product.ImagePath.Trim();
            }

            if (!string.IsNullOrWhiteSpace(product.ImageBase64))
            {
                return product.ImageBase64.Trim();
            }

            if (!string.IsNullOrWhiteSpace(product.ImageUrl))
            {
                return product.ImageUrl.Trim();
            }

            return string.Empty;
        }

        private static decimal CalculateFinalPrice(decimal price, int discount)
        {
            if (discount <= 0)
            {
                return price;
            }

            return Math.Max(0, price - ((price * discount) / 100m));
        }

        private static string NormalizeAction(string action)
        {
            return (action ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static bool IsActiveProduct(Product product)
        {
            return string.Equals(product.Status, "active", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeCategorySlug(string? category)
        {
            var normalizedCategory = (category ?? string.Empty).Trim().ToLowerInvariant();
            var compactCategory = new string(normalizedCategory.Where(char.IsLetterOrDigit).ToArray());

            return compactCategory switch
            {
                "food" or "foodnatural" or "foodnaturalproducts" => "food",
                "sweets" or "sweetsdairy" => "sweets",
                "handicrafts" => "handicrafts",
                "clothing" => "clothing",
                "books" => "books",
                "antique" or "antiquecollectibles" or "antiques" => "antique",
                _ => compactCategory
            };
        }
    }

    public sealed class LegacyProductDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("seller")]
        public string Seller { get; set; } = string.Empty;

        [JsonPropertyName("image_path")]
        public string ImagePath { get; set; } = string.Empty;

        [JsonPropertyName("imageBase64")]
        public string ImageBase64 { get; set; } = string.Empty;

        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; } = string.Empty;

        [JsonPropertyName("discount")]
        public int Discount { get; set; }

        [JsonPropertyName("final_price")]
        public decimal FinalPrice { get; set; }

        [JsonPropertyName("rating")]
        public decimal Rating { get; set; }

        [JsonPropertyName("stock")]
        public int Stock { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    public sealed class LegacyCategoryDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;

        [JsonPropertyName("icon")]
        public string Icon { get; set; } = string.Empty;
    }
}