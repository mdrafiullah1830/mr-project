using Microsoft.AspNetCore.Mvc;
using MRShop.Search;
using System.Text.Json.Serialization;

namespace MRShop.Controllers
{
    /// <summary>
    /// Response wrapper for API consistency
    /// </summary>
    public class ApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }
    }

    /// <summary>
    /// Product DTO for API responses
    /// </summary>
    public class ProductDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("final_price")]
        public decimal FinalPrice { get; set; }

        [JsonPropertyName("discount")]
        public int Discount { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("image_path")]
        public string ImagePath { get; set; }

        [JsonPropertyName("rating")]
        public decimal Rating { get; set; }

        [JsonPropertyName("stock")]
        public int Stock { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    /// <summary>
    /// Search results container
    /// </summary>
    public class SearchResultsDTO
    {
        [JsonPropertyName("query")]
        public string Query { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("total_results")]
        public int TotalResults { get; set; }

        [JsonPropertyName("results")]
        public List<ProductDTO> Results { get; set; } = new List<ProductDTO>();
    }

    /// <summary>
    /// Search API Controller
    /// Provides endpoints for product search functionality
    /// </summary>
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        private readonly SearchService _searchService;

        public SearchController(SearchService searchService)
        {
            _searchService = searchService;
        }

        /// <summary>
        /// Search products by query string
        /// GET: /api/search?query=smartphone&category=electronics&pageSize=8&sortBy=relevance
        /// </summary>
        [HttpGet]
        public IActionResult Search(
            [FromQuery] string query,
            [FromQuery] string category = null,
            [FromQuery] int pageSize = 8,
            [FromQuery] string sortBy = "relevance")
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new ApiResponse<SearchResultsDTO>
                {
                    Success = false,
                    Message = "Search query is required",
                    Data = null
                });
            }

            try
            {
                var searchResult = _searchService.Search(query, category);

                // Map to DTO format
                var results = searchResult.Results
                    .Take(pageSize)
                    .Select(r => new ProductDTO
                    {
                        Id = r.Product.Id,
                        Name = r.Product.Name,
                        Price = r.Product.Price,
                        FinalPrice = r.Product.FinalPrice,
                        Discount = r.Product.Discount,
                        Category = r.Product.Category,
                        Description = r.Product.Description,
                        ImagePath = r.Product.ImagePath,
                        Rating = r.Product.Rating,
                        Stock = r.Product.Stock,
                        Status = r.Product.Status
                    })
                    .ToList();

                var response = new ApiResponse<SearchResultsDTO>
                {
                    Success = true,
                    Message = $"Found {results.Count} products",
                    Data = new SearchResultsDTO
                    {
                        Query = query,
                        Category = category,
                        TotalResults = results.Count,
                        Results = results
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<SearchResultsDTO>
                {
                    Success = false,
                    Message = $"Search error: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get search suggestions for autocomplete
        /// GET: /api/search/suggestions?q=smartphone
        /// </summary>
        [HttpGet("suggestions")]
        public IActionResult GetSuggestions([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { suggestions = new List<string>() }
                });
            }

            try
            {
                var allProducts = _searchService.GetAllProducts(true);
                var suggestions = allProducts
                    .Where(p => p.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.Name)
                    .Distinct()
                    .Take(5)
                    .ToList();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { suggestions = suggestions }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get products by category
        /// GET: /api/search/category/electronics
        /// </summary>
        [HttpGet("category/{category}")]
        public IActionResult GetByCategory(string category)
        {
            try
            {
                var result = _searchService.GetByCategory(category);
                
                var products = result.Results
                    .Select(r => new ProductDTO
                    {
                        Id = r.Product.Id,
                        Name = r.Product.Name,
                        Price = r.Product.Price,
                        FinalPrice = r.Product.FinalPrice,
                        Discount = r.Product.Discount,
                        Category = r.Product.Category,
                        Description = r.Product.Description,
                        ImagePath = r.Product.ImagePath,
                        Rating = r.Product.Rating,
                        Stock = r.Product.Stock,
                        Status = r.Product.Status
                    })
                    .ToList();

                return Ok(new ApiResponse<SearchResultsDTO>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = new SearchResultsDTO
                    {
                        Category = category,
                        TotalResults = products.Count,
                        Results = products
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<SearchResultsDTO>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get all available categories
        /// GET: /api/search/categories
        /// </summary>
        [HttpGet("categories")]
        public IActionResult GetCategories()
        {
            try
            {
                var categories = _searchService.GetCategories();
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { categories = categories }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get product by ID
        /// GET: /api/search/product/201
        /// </summary>
        [HttpGet("product/{id}")]
        public IActionResult GetProductById(int id)
        {
            try
            {
                var product = _searchService.GetProductById(id);
                if (product == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Product not found",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<ProductDTO>
                {
                    Success = true,
                    Data = new ProductDTO
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Price = product.Price,
                        FinalPrice = product.FinalPrice,
                        Discount = product.Discount,
                        Category = product.Category,
                        Description = product.Description,
                        ImagePath = product.ImagePath,
                        Rating = product.Rating,
                        Stock = product.Stock,
                        Status = product.Status
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get all products
        /// GET: /api/search/all?active=true
        /// </summary>
        [HttpGet("all")]
        public IActionResult GetAllProducts([FromQuery] bool active = true)
        {
            try
            {
                var products = _searchService.GetAllProducts(active)
                    .Select(p => new ProductDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price,
                        FinalPrice = p.FinalPrice,
                        Discount = p.Discount,
                        Category = p.Category,
                        Description = p.Description,
                        ImagePath = p.ImagePath,
                        Rating = p.Rating,
                        Stock = p.Stock,
                        Status = p.Status
                    })
                    .ToList();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { products = products, total = products.Count }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Reload products data from JSON
        /// POST: /api/search/reload
        /// </summary>
        [HttpPost("reload")]
        public IActionResult ReloadData()
        {
            try
            {
                _searchService.ReloadProducts();
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Product data reloaded successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error reloading data: {ex.Message}",
                    Data = null
                });
            }
        }
    }
}
