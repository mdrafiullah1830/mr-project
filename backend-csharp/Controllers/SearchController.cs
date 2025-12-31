using Microsoft.AspNetCore.Mvc;
using MRShop.Search.Models;
using MRShop.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRShop.Controllers
{
    /// <summary>
    /// Controller for search operations
    /// Endpoints for product search, suggestions, and filtering
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(ISearchService searchService, ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        /// <summary>
        /// Search products with filtering and pagination
        /// GET /api/search?query=phone&page=1&pageSize=10&sortBy=relevance
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Search(
            [FromQuery] string? query,
            [FromQuery] string? category,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "relevance")
        {
            try
            {
                // Validate query
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Query parameter is required",
                        data = (object?)null
                    });
                }

                // Validate pagination
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var searchRequest = new SearchRequest
                {
                    Query = query,
                    Category = category,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    Page = page,
                    PageSize = pageSize,
                    SortBy = sortBy
                };

                _logger.LogInformation($"Search initiated: query='{query}', page={page}, size={pageSize}");

                var result = await _searchService.SearchAsync(searchRequest);

                return Ok(new
                {
                    success = true,
                    message = $"Found {result.TotalResults} results",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Search error: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while searching",
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Get search suggestions for autocomplete dropdown
        /// GET /api/search/suggestions?q=pho
        /// </summary>
        [HttpGet("suggestions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSuggestions([FromQuery] string? q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No suggestions",
                        data = new SearchSuggestionsResponse()
                    });
                }

                var suggestions = await _searchService.GetSuggestionsAsync(q);

                return Ok(new
                {
                    success = true,
                    message = "Suggestions retrieved",
                    data = suggestions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Suggestions error: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error fetching suggestions",
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Get all available product categories
        /// GET /api/search/categories
        /// </summary>
        [HttpGet("categories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _searchService.GetCategoriesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Categories retrieved",
                    data = new
                    {
                        categories = categories,
                        total = categories.Count
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Categories error: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error fetching categories",
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Advanced search with multiple filters
        /// POST /api/search/advanced
        /// </summary>
        [HttpPost("advanced")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AdvancedSearch([FromBody] SearchRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Query))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid search request",
                        data = (object?)null
                    });
                }

                // Validate pagination
                if (request.Page < 1) request.Page = 1;
                if (request.PageSize < 1 || request.PageSize > 100) request.PageSize = 10;

                _logger.LogInformation($"Advanced search: query='{request.Query}'");

                var result = await _searchService.SearchAsync(request);

                return Ok(new
                {
                    success = true,
                    message = $"Found {result.TotalResults} results",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Advanced search error: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error processing advanced search",
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Quick search for specific product type
        /// GET /api/search/quick?q=phone&type=product
        /// </summary>
        [HttpGet("quick")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> QuickSearch([FromQuery] string? q, [FromQuery] string? type)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No results",
                        data = new List<SearchProduct>()
                    });
                }

                var searchRequest = new SearchRequest
                {
                    Query = q,
                    PageSize = 5,
                    Page = 1
                };

                var result = await _searchService.SearchAsync(searchRequest);

                return Ok(new
                {
                    success = true,
                    message = "Quick search completed",
                    data = result.Results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Quick search error: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error in quick search",
                    data = (object?)null
                });
            }
        }
    }
}
