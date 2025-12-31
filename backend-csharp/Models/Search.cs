using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MRShop.Search.Models
{
    /// <summary>
    /// Represents a product for search results
    /// </summary>
    public class SearchProduct
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("category")]
        public string Category { get; set; } = string.Empty;

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("discount")]
        public int Discount { get; set; }

        [JsonProperty("final_price")]
        public decimal FinalPrice { get; set; }

        [JsonProperty("image_path")]
        public string ImagePath { get; set; } = string.Empty;

        [JsonProperty("rating")]
        public decimal Rating { get; set; }

        [JsonProperty("stock")]
        public int Stock { get; set; }

        [JsonProperty("relevance_score")]
        public decimal RelevanceScore { get; set; }
    }

    /// <summary>
    /// Search request model
    /// </summary>
    public class SearchRequest
    {
        [JsonProperty("query")]
        public string Query { get; set; } = string.Empty;

        [JsonProperty("category")]
        public string? Category { get; set; }

        [JsonProperty("min_price")]
        public decimal? MinPrice { get; set; }

        [JsonProperty("max_price")]
        public decimal? MaxPrice { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; } = 1;

        [JsonProperty("page_size")]
        public int PageSize { get; set; } = 10;

        [JsonProperty("sort_by")]
        public string SortBy { get; set; } = "relevance"; // relevance, price_low, price_high, newest, rating
    }

    /// <summary>
    /// Search response with pagination
    /// </summary>
    public class SearchResponse
    {
        [JsonProperty("query")]
        public string Query { get; set; } = string.Empty;

        [JsonProperty("total_results")]
        public int TotalResults { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("page_size")]
        public int PageSize { get; set; }

        [JsonProperty("total_pages")]
        public int TotalPages { get; set; }

        [JsonProperty("results")]
        public List<SearchProduct> Results { get; set; } = new();

        [JsonProperty("suggestions")]
        public List<string> Suggestions { get; set; } = new();

        [JsonProperty("took_ms")]
        public long TookMs { get; set; }
    }

    /// <summary>
    /// Search filter for advanced queries
    /// </summary>
    public class SearchFilter
    {
        [JsonProperty("category")]
        public List<string> Categories { get; set; } = new();

        [JsonProperty("price_range")]
        public PriceRange? PriceRange { get; set; }

        [JsonProperty("rating_min")]
        public decimal? MinRating { get; set; }

        [JsonProperty("in_stock")]
        public bool? InStockOnly { get; set; }

        [JsonProperty("discount_min")]
        public int? MinDiscount { get; set; }
    }

    /// <summary>
    /// Price range filter
    /// </summary>
    public class PriceRange
    {
        [JsonProperty("min")]
        public decimal Min { get; set; }

        [JsonProperty("max")]
        public decimal Max { get; set; }
    }

    /// <summary>
    /// Search suggestions response
    /// </summary>
    public class SearchSuggestionsResponse
    {
        [JsonProperty("suggestions")]
        public List<SuggestionItem> Suggestions { get; set; } = new();

        [JsonProperty("categories")]
        public List<CategorySuggestion> Categories { get; set; } = new();
    }

    /// <summary>
    /// Individual suggestion item
    /// </summary>
    public class SuggestionItem
    {
        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty; // "product", "category", "search"

        [JsonProperty("icon")]
        public string Icon { get; set; } = string.Empty;
    }

    /// <summary>
    /// Category suggestion for dropdown
    /// </summary>
    public class CategorySuggestion
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("icon")]
        public string Icon { get; set; } = string.Empty;

        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
