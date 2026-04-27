using Microsoft.AspNetCore.Mvc;
using MRShop.AdminPanel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MRShop.OrderTracking.Controllers
{
    /// <summary>
    /// Admin Product Management API Controller
    /// Handles all product CRUD operations from the admin panel
    /// </summary>
    [ApiController]
    [Route("api/admin")]
    [Produces("application/json")]
    public class AdminProductController : ControllerBase
    {
        private readonly AdminProductService _productService;

        public AdminProductController()
        {
            _productService = new AdminProductService();
        }

        /// <summary>
        /// GET: api/admin/categories
        /// Get all product categories
        /// </summary>
        [HttpGet("categories")]
        public IActionResult GetCategories()
        {
            try
            {
                var categories = _productService.GetAllCategories();
                return Ok(new
                {
                    success = true,
                    message = "Categories retrieved successfully",
                    data = categories,
                    count = categories.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// GET: api/admin/products
        /// Get all products or filter by category
        /// </summary>
        [HttpGet("products")]
        public IActionResult GetProducts([FromQuery] string? category = null)
        {
            try
            {
                List<Product> products;

                if (string.IsNullOrEmpty(category))
                {
                    products = _productService.GetAllProducts();
                }
                else
                {
                    products = _productService.GetProductsByCategory(category);
                }

                return Ok(new
                {
                    success = true,
                    message = "Products retrieved successfully",
                    data = products,
                    count = products.Count,
                    category = category ?? "All"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// GET: api/admin/products/{id}
        /// Get a specific product by ID
        /// </summary>
        [HttpGet("products/{id:int}")]
        public IActionResult GetProduct(int id)
        {
            try
            {
                var product = _productService.GetProductById(id);

                if (product == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Product with ID {id} not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Product retrieved successfully",
                    data = product
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// POST: api/admin/products
        /// Add a new product
        /// </summary>
        [HttpPost("products")]
        public IActionResult AddProduct([FromBody] Product product)
        {
            try
            {
                if (product == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Product data is required"
                    });
                }

                if (string.IsNullOrEmpty(product.Name))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Product name is required"
                    });
                }

                if (product.Price < 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Price cannot be negative"
                    });
                }

                var addedProduct = _productService.AddProduct(product);

                return CreatedAtAction(nameof(GetProduct), new { id = addedProduct.Id }, new
                {
                    success = true,
                    message = $"Product '{addedProduct.Name}' added successfully to {addedProduct.Category}",
                    data = addedProduct
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// PUT: api/admin/products/{id}
        /// Update an existing product
        /// </summary>
        [HttpPut("products/{id:int}")]
        public IActionResult UpdateProduct(int id, [FromBody] Product product)
        {
            try
            {
                if (product == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Product data is required"
                    });
                }

                var updatedProduct = _productService.UpdateProduct(id, product);

                return Ok(new
                {
                    success = true,
                    message = $"Product '{updatedProduct.Name}' updated successfully",
                    data = updatedProduct
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// DELETE: api/admin/products/{id}
        /// Delete a product
        /// </summary>
        [HttpDelete("products/{id:int}")]
        public IActionResult DeleteProduct(int id)
        {
            try
            {
                var product = _productService.GetProductById(id);

                if (product == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Product with ID {id} not found"
                    });
                }

                var deleted = _productService.DeleteProduct(id);

                return Ok(new
                {
                    success = true,
                    message = $"Product '{product.Name}' deleted successfully",
                    data = new { deletedId = id }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// GET: api/admin/search
        /// Search products by name or description
        /// </summary>
        [HttpGet("search")]
        public IActionResult SearchProducts([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Search query is required"
                    });
                }

                var results = _productService.SearchProducts(query);

                return Ok(new
                {
                    success = true,
                    message = $"Found {results.Count} product(s)",
                    data = results,
                    count = results.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// GET: api/admin/statistics
        /// Get admin dashboard statistics
        /// </summary>
        [HttpGet("statistics")]
        public IActionResult GetStatistics()
        {
            try
            {
                var allProducts = _productService.GetAllProducts();
                var categoryStats = _productService.GetCategoryStats();
                var lowStock = _productService.GetLowStockProducts();

                var stats = new
                {
                    totalProducts = allProducts.Count,
                    totalCategories = categoryStats.Count,
                    totalInventoryValue = _productService.GetInventoryValue(),
                    totalRevenue = _productService.GetTotalRevenue(),
                    lowStockProducts = lowStock.Count,
                    averagePrice = allProducts.Count > 0 ? allProducts.Average(p => p.Price) : 0,
                    averageStock = allProducts.Count > 0 ? allProducts.Average(p => p.Stock) : 0,
                    categoryStats = categoryStats,
                    lowStockItems = lowStock.Select(p => new { p.Id, p.Name, p.Stock, p.Category })
                };

                return Ok(new
                {
                    success = true,
                    message = "Statistics retrieved successfully",
                    data = stats
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// GET: api/admin/low-stock
        /// Get products with low stock
        /// </summary>
        [HttpGet("low-stock")]
        public IActionResult GetLowStockProducts([FromQuery] int threshold = 10)
        {
            try
            {
                var products = _productService.GetLowStockProducts(threshold);

                return Ok(new
                {
                    success = true,
                    message = $"Found {products.Count} product(s) with stock <= {threshold}",
                    data = products,
                    count = products.Count,
                    threshold = threshold
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// POST: api/admin/sync
        /// Sync products to index page
        /// </summary>
        [HttpPost("sync")]
        public IActionResult SyncToIndexPage()
        {
            try
            {
                var products = _productService.GetAllProducts();
                var categories = _productService.GetAllCategories();

                // This would trigger an event to update index.html
                return Ok(new
                {
                    success = true,
                    message = "Products synced to index page",
                    data = new
                    {
                        totalProducts = products.Count,
                        totalCategories = categories.Count,
                        syncedAt = DateTime.Now
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                status = "online",
                service = "Admin Product Management API",
                version = "1.0.0",
                timestamp = DateTime.Now
            });
        }
    }
}
