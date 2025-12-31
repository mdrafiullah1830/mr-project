using MRShop.Search.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MRShop.Services
{
    /// <summary>
    /// Service for handling product and category search with fuzzy matching
    /// </summary>
    public interface ISearchService
    {
        Task<SearchResponse> SearchAsync(SearchRequest request);
        Task<SearchSuggestionsResponse> GetSuggestionsAsync(string query);
        Task<List<string>> GetCategoriesAsync();
    }

    public class SearchService : ISearchService
    {
        private readonly string _dataDirectory;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private static List<SearchProduct> _cachedProducts = new();
        private static DateTime _lastCacheRefresh = DateTime.MinValue;
        private const int CACHE_DURATION_MINUTES = 5;

        public SearchService()
        {
            _dataDirectory = "/Users/mdrafiullah/Desktop/mr project /data";
        }

        /// <summary>
        /// Main search method with filtering and relevance scoring
        /// </summary>
        public async Task<SearchResponse> SearchAsync(SearchRequest request)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.Query))
                {
                    return new SearchResponse
                    {
                        Query = request.Query,
                        TotalResults = 0,
                        Page = request.Page,
                        PageSize = request.PageSize,
                        TotalPages = 0
                    };
                }

                // Load products
                var allProducts = await LoadProductsAsync();

                // Perform search
                var searchQuery = request.Query.ToLower().Trim();
                var matches = new List<SearchProduct>();

                foreach (var product in allProducts)
                {
                    var score = CalculateRelevanceScore(product, searchQuery);

                    if (score > 0)
                    {
                        product.RelevanceScore = score;
                        matches.Add(product);
                    }
                }

                // Apply category filter
                if (!string.IsNullOrWhiteSpace(request.Category))
                {
                    matches = matches
                        .Where(p => p.Category?.Equals(request.Category, StringComparison.OrdinalIgnoreCase) ?? false)
                        .ToList();
                }

                // Apply price filter
                if (request.MinPrice.HasValue)
                {
                    matches = matches.Where(p => p.FinalPrice >= request.MinPrice.Value).ToList();
                }

                if (request.MaxPrice.HasValue)
                {
                    matches = matches.Where(p => p.FinalPrice <= request.MaxPrice.Value).ToList();
                }

                // Sort based on request
                matches = SortResults(matches, request.SortBy);

                // Get total count
                int totalResults = matches.Count;

                // Apply pagination
                int skipCount = (request.Page - 1) * request.PageSize;
                var pageResults = matches
                    .Skip(skipCount)
                    .Take(request.PageSize)
                    .ToList();

                // Get suggestions for the query
                var suggestions = await GetTopSuggestionsAsync(searchQuery, allProducts, 5);

                stopwatch.Stop();

                return new SearchResponse
                {
                    Query = request.Query,
                    TotalResults = totalResults,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalResults / request.PageSize),
                    Results = pageResults,
                    Suggestions = suggestions,
                    TookMs = stopwatch.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Search error: {ex.Message}");
                return new SearchResponse
                {
                    Query = request.Query,
                    TotalResults = 0,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = 0,
                    Results = new()
                };
            }
        }

        /// <summary>
        /// Get search suggestions/autocomplete
        /// </summary>
        public async Task<SearchSuggestionsResponse> GetSuggestionsAsync(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                {
                    return new SearchSuggestionsResponse();
                }

                var allProducts = await LoadProductsAsync();
                var searchQuery = query.ToLower().Trim();
                var suggestions = new List<SuggestionItem>();
                var categoryMap = new Dictionary<string, int>();

                // Get matching products
                var matchingProducts = allProducts
                    .Where(p => p.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                               p.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(p => p.Name.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase))
                    .Take(8)
                    .ToList();

                // Add product suggestions
                foreach (var product in matchingProducts)
                {
                    suggestions.Add(new SuggestionItem
                    {
                        Text = product.Name,
                        Type = "product",
                        Icon = "📦"
                    });

                    // Count by category
                    if (!categoryMap.ContainsKey(product.Category))
                    {
                        categoryMap[product.Category] = 0;
                    }
                    categoryMap[product.Category]++;
                }

                // Get category suggestions
                var categorySuggestions = categoryMap
                    .Select(c => new CategorySuggestion
                    {
                        Name = c.Key,
                        Icon = GetCategoryIcon(c.Key),
                        Count = c.Value
                    })
                    .OrderByDescending(c => c.Count)
                    .Take(5)
                    .ToList();

                return new SearchSuggestionsResponse
                {
                    Suggestions = suggestions.Take(5).ToList(),
                    Categories = categorySuggestions
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Suggestions error: {ex.Message}");
                return new SearchSuggestionsResponse();
            }
        }

        /// <summary>
        /// Get all available categories
        /// </summary>
        public async Task<List<string>> GetCategoriesAsync()
        {
            try
            {
                var allProducts = await LoadProductsAsync();
                return allProducts
                    .Select(p => p.Category)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(c => c)
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Calculate relevance score for a product based on search query
        /// Higher score = more relevant
        /// </summary>
        private decimal CalculateRelevanceScore(SearchProduct product, string query)
        {
            decimal score = 0;
            var productName = product.Name.ToLower();
            var productDesc = product.Description.ToLower();
            var productCategory = product.Category.ToLower();

            // Split product name into words for better matching
            var productWords = productName.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            
            // 1. Exact full name match (highest priority)
            if (productName == query)
            {
                score = 1500; // Highest possible score
            }
            // 2. Exact word match in product name (very high priority)
            else if (productWords.Any(w => w == query))
            {
                score = 1200;
            }
            // 3. Exact match in category
            else if (productCategory == query)
            {
                score = 1000;
            }
            // 4. Multiple word match or phrase at start
            else if (productName.StartsWith(query))
            {
                score = 800;
            }
            // 5. Word starts with query
            else if (productWords.Any(w => w.StartsWith(query)))
            {
                score = 700;
            }
            // 6. Category contains query
            else if (productCategory.Contains(query))
            {
                score = 500;
            }
            // 7. Name contains query
            else if (productName.Contains(query))
            {
                score = 400;
            }
            // 8. Description contains query
            else if (productDesc.Contains(query))
            {
                score = 200;
            }
            // 9. Fuzzy match (for typos)
            else
            {
                int fuzzyScore = FuzzyMatchScore(productName, query);
                if (fuzzyScore > 0)
                {
                    score = fuzzyScore + 50;
                }
            }

            // Only apply boosts if we have a match
            if (score > 0)
            {
                // Boost score for highly rated products (max +50)
                score += (product.Rating * 5);
                
                // Boost for in-stock items
                if (product.Stock > 0)
                {
                    score += 30;
                }
                
                // Boost for discounted items (max +50)
                if (product.Discount > 0)
                {
                    score += (product.Discount / 2);
                }
            }

            return score;
        }

        /// <summary>
        /// Implement fuzzy matching using Levenshtein-like distance
        /// Returns score > 0 if match found, higher = better match
        /// </summary>
        private int FuzzyMatchScore(string text, string pattern)
        {
            // Early exit if pattern is too long or too short
            if (pattern.Length == 0 || text.Length == 0)
                return 0;
            
            if (pattern.Length > text.Length * 2)
                return 0; // Pattern too long relative to text

            int matches = 0;
            int patternIndex = 0;
            int textIndex = 0;
            int consecutiveMatches = 0;
            int maxConsecutiveMatches = 0;

            // Character-by-character matching
            while (textIndex < text.Length && patternIndex < pattern.Length)
            {
                if (text[textIndex] == pattern[patternIndex])
                {
                    matches++;
                    patternIndex++;
                    consecutiveMatches++;
                    maxConsecutiveMatches = Math.Max(maxConsecutiveMatches, consecutiveMatches);
                }
                else
                {
                    consecutiveMatches = 0;
                }
                textIndex++;
            }

            // Calculate coverage - need at least 60% of pattern matched
            double matchPercentage = (double)matches / pattern.Length;
            
            if (matchPercentage < 0.60)
            {
                return 0; // Not enough matches
            }

            // Base score for matches found
            int score = matches * 8;
            
            // Bonus for consecutive matches (indicates better partial match)
            score += maxConsecutiveMatches * 5;
            
            // Bonus if pattern is matched from the start
            if (text.StartsWith(pattern.Substring(0, Math.Min(2, pattern.Length))))
            {
                score += 20;
            }

            return Math.Max(score, 50); // Minimum 50 for valid fuzzy matches
        }

        /// <summary>
        /// Get top related search suggestions
        /// </summary>
        private async Task<List<string>> GetTopSuggestionsAsync(string query, List<SearchProduct> products, int limit)
        {
            try
            {
                var suggestions = products
                    .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                               p.Category.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.Name)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(limit)
                    .ToList();

                return suggestions;
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Sort results based on sort criteria
        /// </summary>
        private List<SearchProduct> SortResults(List<SearchProduct> results, string sortBy)
        {
            return sortBy?.ToLower() switch
            {
                "price_low" => results.OrderBy(r => r.FinalPrice).ToList(),
                "price_high" => results.OrderByDescending(r => r.FinalPrice).ToList(),
                "rating" => results.OrderByDescending(r => r.Rating).ToList(),
                "newest" => results.OrderByDescending(r => r.Id).ToList(),
                _ => results.OrderByDescending(r => r.RelevanceScore).ToList() // relevance (default)
            };
        }

        /// <summary>
        /// Load products from JSON with caching
        /// </summary>
        private async Task<List<SearchProduct>> LoadProductsAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                // Return cached data if fresh
                if (_cachedProducts.Count > 0 &&
                    DateTime.UtcNow.Subtract(_lastCacheRefresh).TotalMinutes < CACHE_DURATION_MINUTES)
                {
                    return _cachedProducts;
                }

                var productsPath = Path.Combine(_dataDirectory, "products.json");

                if (!File.Exists(productsPath))
                {
                    return new List<SearchProduct>();
                }

                var json = await File.ReadAllTextAsync(productsPath);
                var products = JsonConvert.DeserializeObject<List<SearchProduct>>(json) ?? new();

                _cachedProducts = products;
                _lastCacheRefresh = DateTime.UtcNow;

                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading products: {ex.Message}");
                return new List<SearchProduct>();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Get appropriate icon for category
        /// </summary>
        private string GetCategoryIcon(string category)
        {
            return category.ToLower() switch
            {
                "electronics" => "💻",
                "clothing" => "👕",
                "food" => "🍔",
                "books" => "📚",
                "beauty" => "💄",
                "sports" => "⚽",
                "home" => "🏠",
                "toys" => "🧸",
                _ => "📦"
            };
        }
    }
}
