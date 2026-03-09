using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MRShop.Search
{
    /// <summary>
    /// Product model matching the JSON structure
    /// </summary>
    public class Product
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("image_path")]
        public string ImagePath { get; set; }

        [JsonPropertyName("discount")]
        public int Discount { get; set; }

        [JsonPropertyName("final_price")]
        public decimal FinalPrice { get; set; }

        [JsonPropertyName("rating")]
        public decimal Rating { get; set; }

        [JsonPropertyName("stock")]
        public int Stock { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    /// <summary>
    /// Search result item
    /// </summary>
    public class SearchResult
    {
        [JsonPropertyName("product")]
        public Product Product { get; set; }

        [JsonPropertyName("relevance_score")]
        public decimal RelevanceScore { get; set; }

        [JsonPropertyName("matched_fields")]
        public List<string> MatchedFields { get; set; } = new List<string>();
    }

    /// <summary>
    /// Search response wrapper
    /// </summary>
    public class SearchResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("query")]
        public string Query { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("total_results")]
        public int TotalResults { get; set; }

        [JsonPropertyName("results")]
        public List<SearchResult> Results { get; set; } = new List<SearchResult>();

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    /// <summary>
    /// Search service for products
    /// Reads from JSON files and performs intelligent search
    /// </summary>
    public class SearchService
    {
        private readonly string _dataPath;
        private List<Product> _products;
        private readonly JsonSerializerOptions _jsonOptions;

        public SearchService(string dataPath = "./data/products.json")
        {
            _dataPath = dataPath;
            _jsonOptions = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
            LoadProducts();
        }

        /// <summary>
        /// Load products from JSON file
        /// </summary>
        private void LoadProducts()
        {
            try
            {
                if (!File.Exists(_dataPath))
                {
                    throw new FileNotFoundException($"Products file not found: {_dataPath}");
                }

                string jsonContent = File.ReadAllText(_dataPath);
                _products = JsonSerializer.Deserialize<List<Product>>(jsonContent, _jsonOptions) 
                    ?? new List<Product>();

                Console.WriteLine($"✓ Loaded {_products.Count} products from {_dataPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error loading products: {ex.Message}");
                _products = new List<Product>();
            }
        }

        /// <summary>
        /// Reload products from JSON file (call when data is updated)
        /// </summary>
        public void ReloadProducts()
        {
            LoadProducts();
        }

        /// <summary>
        /// Search products by query and optional category
        /// </summary>
        public SearchResponse Search(string query, string category = null)
        {
            var response = new SearchResponse
            {
                Query = query,
                Category = category
            };

            if (string.IsNullOrWhiteSpace(query))
            {
                response.Success = false;
                response.Message = "Search query cannot be empty";
                return response;
            }

            try
            {
                var searchTerms = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                
                var results = _products
                    .Where(p => p.Status == "active") // Only active products
                    .Where(p => string.IsNullOrEmpty(category) || p.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                    .Select(p => new
                    {
                        Product = p,
                        Score = CalculateRelevance(p, searchTerms),
                        MatchedFields = GetMatchedFields(p, searchTerms)
                    })
                    .Where(x => x.Score > 0) // Only include products with matches
                    .OrderByDescending(x => x.Score)
                    .ToList();

                response.Results = results.Select(r => new SearchResult
                {
                    Product = r.Product,
                    RelevanceScore = r.Score,
                    MatchedFields = r.MatchedFields
                }).ToList();

                response.TotalResults = response.Results.Count;
                response.Success = true;
                response.Message = response.TotalResults > 0 
                    ? $"Found {response.TotalResults} product(s)" 
                    : "No products found matching your search";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Search error: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Calculate relevance score based on keyword matches
        /// </summary>
        private decimal CalculateRelevance(Product product, string[] searchTerms)
        {
            decimal score = 0;

            foreach (var term in searchTerms)
            {
                // Exact match in name - highest priority
                if (product.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    score += 10;
                }

                // Match in category
                if (product.Category.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    score += 7;
                }

                // Match in description
                if (product.Description.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    score += 3;
                }

                // Partial match in name
                if (product.Name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    score += 2;
                }
            }

            return score;
        }

        /// <summary>
        /// Get which fields matched the search
        /// </summary>
        private List<string> GetMatchedFields(Product product, string[] searchTerms)
        {
            var matched = new List<string>();

            foreach (var term in searchTerms)
            {
                if (product.Name.Contains(term, StringComparison.OrdinalIgnoreCase) && !matched.Contains("name"))
                    matched.Add("name");

                if (product.Category.Contains(term, StringComparison.OrdinalIgnoreCase) && !matched.Contains("category"))
                    matched.Add("category");

                if (product.Description.Contains(term, StringComparison.OrdinalIgnoreCase) && !matched.Contains("description"))
                    matched.Add("description");
            }

            return matched;
        }

        /// <summary>
        /// Get all products in a category
        /// </summary>
        public SearchResponse GetByCategory(string category)
        {
            var response = new SearchResponse
            {
                Category = category
            };

            try
            {
                var products = _products
                    .Where(p => p.Status == "active" && p.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                response.Results = products.Select(p => new SearchResult
                {
                    Product = p,
                    RelevanceScore = 100,
                    MatchedFields = new List<string> { "category" }
                }).ToList();

                response.TotalResults = response.Results.Count;
                response.Success = true;
                response.Message = $"Found {response.TotalResults} products in {category}";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Get all unique categories
        /// </summary>
        public List<string> GetCategories()
        {
            return _products
                .Where(p => p.Status == "active")
                .Select(p => p.Category)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c)
                .ToList();
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        public Product GetProductById(int id)
        {
            return _products.FirstOrDefault(p => p.Id == id);
        }

        /// <summary>
        /// Get all products (optionally filtered by status)
        /// </summary>
        public List<Product> GetAllProducts(bool activeOnly = true)
        {
            return activeOnly 
                ? _products.Where(p => p.Status == "active").ToList()
                : _products;
        }
    }
}
