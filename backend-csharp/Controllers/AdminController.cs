using MRShop.Admin.Models;
using MRShop.Admin.Services;
using Microsoft.AspNetCore.Mvc;

namespace MRShop.Admin.Controllers
{
    /// <summary>
    /// Admin panel management endpoints
    /// </summary>
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;
        private readonly IWebHostEnvironment _environment;

        public AdminController(
            IAdminService adminService,
            ILogger<AdminController> logger,
            IWebHostEnvironment environment)
        {
            _adminService = adminService;
            _logger = logger;
            _environment = environment;
        }

        // ==================== CATEGORIES ====================

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <returns>List of categories</returns>
        [HttpGet("categories")]
        public async Task<ActionResult<AdminResponse<List<Category>>>> GetCategories()
        {
            _logger.LogInformation("Get all categories request");

            try
            {
                var categories = await _adminService.GetCategoriesAsync();
                return Ok(new AdminResponse<List<Category>>
                {
                    Success = true,
                    Message = "Categories retrieved successfully",
                    Data = categories
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return StatusCode(500, new AdminResponse<List<Category>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get specific category
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Category data</returns>
        [HttpGet("categories/{id}")]
        public async Task<ActionResult<AdminResponse<Category>>> GetCategory(int id)
        {
            _logger.LogInformation("Get category request for ID {Id}", id);

            try
            {
                var category = await _adminService.GetCategoryAsync(id);

                if (category == null)
                {
                    return NotFound(new AdminResponse<Category>
                    {
                        Success = false,
                        Message = "Category not found",
                        Data = null
                    });
                }

                return Ok(new AdminResponse<Category>
                {
                    Success = true,
                    Message = "Category retrieved successfully",
                    Data = category
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category");
                return StatusCode(500, new AdminResponse<Category>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Create new category
        /// </summary>
        /// <param name="request">Category request data</param>
        /// <returns>Created category</returns>
        [HttpPost("categories")]
        public async Task<ActionResult<AdminResponse<Category>>> CreateCategory([FromForm] CreateCategoryRequest request)
        {
            _logger.LogInformation("Create category request: {Name}", request.Name);

            if (string.IsNullOrEmpty(request.Name))
            {
                return BadRequest(new AdminResponse<Category>
                {
                    Success = false,
                    Message = "Category name is required",
                    Data = null
                });
            }

            try
            {
                string? imagePath = null;

                // Handle image upload
                if (Request.Form.Files.Count > 0)
                {
                    var imageFile = Request.Form.Files[0];
                    imagePath = await SaveUploadedFileAsync(imageFile, "categories");
                }

                var (success, message, category) = await _adminService.CreateCategoryAsync(request, imagePath);

                if (!success)
                {
                    return BadRequest(new AdminResponse<Category>
                    {
                        Success = false,
                        Message = message,
                        Data = null
                    });
                }

                return Created($"/api/admin/categories/{category?.Id}", new AdminResponse<Category>
                {
                    Success = true,
                    Message = message,
                    Data = category
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, new AdminResponse<Category>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Delete category
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("categories/{id}")]
        public async Task<ActionResult<AdminResponse<string>>> DeleteCategory(int id)
        {
            _logger.LogInformation("Delete category request for ID {Id}", id);

            try
            {
                var (success, message) = await _adminService.DeleteCategoryAsync(id);

                if (!success)
                {
                    return NotFound(new AdminResponse<string>
                    {
                        Success = false,
                        Message = message,
                        Data = null
                    });
                }

                return Ok(new AdminResponse<string>
                {
                    Success = true,
                    Message = message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category");
                return StatusCode(500, new AdminResponse<string>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                });
            }
        }

        // ==================== PRODUCTS ====================

        /// <summary>
        /// Get all products
        /// </summary>
        /// <returns>List of products</returns>
        [HttpGet("products")]
        public async Task<ActionResult<AdminResponse<List<Product>>>> GetProducts()
        {
            _logger.LogInformation("Get all products request");

            try
            {
                var products = await _adminService.GetProductsAsync();
                return Ok(new AdminResponse<List<Product>>
                {
                    Success = true,
                    Message = "Products retrieved successfully",
                    Data = products
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return StatusCode(500, new AdminResponse<List<Product>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Create new product
        /// </summary>
        /// <param name="request">Product request data</param>
        /// <returns>Created product</returns>
        [HttpPost("products")]
        public async Task<ActionResult<AdminResponse<Product>>> CreateProduct([FromForm] CreateProductRequest request)
        {
            _logger.LogInformation("Create product request: {Name}", request.Name);

            if (string.IsNullOrEmpty(request.Name))
            {
                return BadRequest(new AdminResponse<Product>
                {
                    Success = false,
                    Message = "Product name is required",
                    Data = null
                });
            }

            try
            {
                string? imagePath = null;

                // Handle image upload
                if (Request.Form.Files.Count > 0)
                {
                    var imageFile = Request.Form.Files[0];
                    imagePath = await SaveUploadedFileAsync(imageFile, "products");
                }

                var (success, message, product) = await _adminService.CreateProductAsync(request, imagePath);

                if (!success)
                {
                    return BadRequest(new AdminResponse<Product>
                    {
                        Success = false,
                        Message = message,
                        Data = null
                    });
                }

                return Created($"/api/admin/products/{product?.Id}", new AdminResponse<Product>
                {
                    Success = true,
                    Message = message,
                    Data = product
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new AdminResponse<Product>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Delete product
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("products/{id}")]
        public async Task<ActionResult<AdminResponse<string>>> DeleteProduct(int id)
        {
            _logger.LogInformation("Delete product request for ID {Id}", id);

            try
            {
                var (success, message) = await _adminService.DeleteProductAsync(id);

                if (!success)
                {
                    return NotFound(new AdminResponse<string>
                    {
                        Success = false,
                        Message = message,
                        Data = null
                    });
                }

                return Ok(new AdminResponse<string>
                {
                    Success = true,
                    Message = message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                return StatusCode(500, new AdminResponse<string>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                });
            }
        }

        // ==================== ORDERS ====================

        /// <summary>
        /// Get all orders
        /// </summary>
        /// <param name="status">Optional status filter</param>
        /// <returns>List of orders</returns>
        [HttpGet("orders")]
        public async Task<ActionResult<AdminResponse<List<AdminOrder>>>> GetOrders([FromQuery] string? status = null)
        {
            _logger.LogInformation("Get orders request, status filter: {Status}", status ?? "none");

            try
            {
                List<AdminOrder> orders;

                if (!string.IsNullOrEmpty(status))
                {
                    orders = await _adminService.GetOrdersByStatusAsync(status);
                }
                else
                {
                    orders = await _adminService.GetOrdersAsync();
                }

                return Ok(new AdminResponse<List<AdminOrder>>
                {
                    Success = true,
                    Message = "Orders retrieved successfully",
                    Data = orders
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders");
                return StatusCode(500, new AdminResponse<List<AdminOrder>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                });
            }
        }

        // ==================== USERS ====================

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>List of users</returns>
        [HttpGet("users")]
        public async Task<ActionResult<AdminResponse<List<AdminUser>>>> GetUsers()
        {
            _logger.LogInformation("Get users request");

            try
            {
                var users = await _adminService.GetUsersAsync();
                return Ok(new AdminResponse<List<AdminUser>>
                {
                    Success = true,
                    Message = "Users retrieved successfully",
                    Data = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, new AdminResponse<List<AdminUser>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Search users
        /// </summary>
        /// <param name="q">Search query</param>
        /// <returns>Matching users</returns>
        [HttpGet("users/search")]
        public async Task<ActionResult<AdminResponse<List<AdminUser>>>> SearchUsers([FromQuery] string q)
        {
            _logger.LogInformation("Search users request: {Query}", q);

            if (string.IsNullOrEmpty(q) || q.Length < 2)
            {
                return BadRequest(new AdminResponse<List<AdminUser>>
                {
                    Success = false,
                    Message = "Search query must be at least 2 characters",
                    Data = null
                });
            }

            try
            {
                var users = await _adminService.SearchUsersAsync(q);
                return Ok(new AdminResponse<List<AdminUser>>
                {
                    Success = true,
                    Message = "Search completed",
                    Data = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users");
                return StatusCode(500, new AdminResponse<List<AdminUser>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                });
            }
        }

        // ==================== SETTINGS ====================

        /// <summary>
        /// Get site settings
        /// </summary>
        /// <returns>Site settings</returns>
        [HttpGet("settings")]
        public async Task<ActionResult<AdminResponse<SiteSettings>>> GetSettings()
        {
            _logger.LogInformation("Get settings request");

            try
            {
                var settings = await _adminService.GetSettingsAsync();
                return Ok(new AdminResponse<SiteSettings>
                {
                    Success = true,
                    Message = "Settings retrieved successfully",
                    Data = settings
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting settings");
                return StatusCode(500, new AdminResponse<SiteSettings>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Save site settings
        /// </summary>
        /// <param name="settings">Settings to save</param>
        /// <returns>Success message</returns>
        [HttpPost("settings")]
        public async Task<ActionResult<AdminResponse<string>>> SaveSettings([FromBody] SiteSettings settings)
        {
            _logger.LogInformation("Save settings request");

            try
            {
                var (success, message) = await _adminService.SaveSettingsAsync(settings);

                if (!success)
                {
                    return BadRequest(new AdminResponse<string>
                    {
                        Success = false,
                        Message = message,
                        Data = null
                    });
                }

                return Ok(new AdminResponse<string>
                {
                    Success = true,
                    Message = message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving settings");
                return StatusCode(500, new AdminResponse<string>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                });
            }
        }

        // ==================== BACKUP & MAINTENANCE ====================

        /// <summary>
        /// Get backup of all admin data
        /// </summary>
        /// <returns>Complete backup data</returns>
        [HttpGet("backup")]
        public async Task<ActionResult<object>> GetBackup()
        {
            _logger.LogInformation("Get backup request");

            try
            {
                var backup = await _adminService.GetBackupAsync();
                return Ok(backup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting backup");
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Clear all admin data (categories and products only)
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPost("clear-data")]
        public async Task<ActionResult<AdminResponse<string>>> ClearData()
        {
            _logger.LogWarning("Clear all data request");

            try
            {
                var (success, message) = await _adminService.ClearAllDataAsync();

                if (!success)
                {
                    return BadRequest(new AdminResponse<string>
                    {
                        Success = false,
                        Message = message,
                        Data = null
                    });
                }

                return Ok(new AdminResponse<string>
                {
                    Success = true,
                    Message = message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing data");
                return StatusCode(500, new AdminResponse<string>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                });
            }
        }

        // ==================== HELPERS ====================

        /// <summary>
        /// Save uploaded file to wwwroot/uploads directory
        /// </summary>
        /// <param name="file">File to save</param>
        /// <param name="folder">Subfolder name (e.g., "categories", "products")</param>
        /// <returns>Relative file path</returns>
        private async Task<string> SaveUploadedFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "admin", folder);

            // Create directory if it doesn't exist
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Return relative path for storage in JSON
            return $"/uploads/admin/{folder}/{fileName}";
        }
    }
}
