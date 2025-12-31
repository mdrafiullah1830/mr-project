using MRShop.Admin.Models;
using Newtonsoft.Json;

namespace MRShop.Admin.Services
{
    public interface IAdminService
    {
        Task<List<Category>> GetCategoriesAsync();
        Task<Category?> GetCategoryAsync(int id);
        Task<(bool success, string message, Category? category)> CreateCategoryAsync(CreateCategoryRequest request, string? imagePath = null);
        Task<(bool success, string message)> DeleteCategoryAsync(int id);

        Task<List<Product>> GetProductsAsync();
        Task<Product?> GetProductAsync(int id);
        Task<(bool success, string message, Product? product)> CreateProductAsync(CreateProductRequest request, string? imagePath = null);
        Task<(bool success, string message)> DeleteProductAsync(int id);

        Task<List<AdminOrder>> GetOrdersAsync();
        Task<List<AdminOrder>> GetOrdersByStatusAsync(string status);

        Task<List<AdminUser>> GetUsersAsync();
        Task<List<AdminUser>> SearchUsersAsync(string query);

        Task<(bool success, string message)> SaveSettingsAsync(SiteSettings settings);
        Task<SiteSettings?> GetSettingsAsync();

        Task<object> GetBackupAsync();
        Task<(bool success, string message)> ClearAllDataAsync();
    }

    public class AdminService : IAdminService
    {
        private readonly string _dataDirectory;
        private readonly string _categoriesFilePath;
        private readonly string _productsFilePath;
        private readonly string _settingsFilePath;
        private readonly SemaphoreSlim _fileLock = new(1, 1);
        private readonly ILogger<AdminService> _logger;

        public AdminService(IConfiguration configuration, ILogger<AdminService> logger)
        {
            _dataDirectory = configuration["DataDirectory"] ?? "/Users/mdrafiullah/Desktop/mr project /data";
            _categoriesFilePath = Path.Combine(_dataDirectory, "categories.json");
            _productsFilePath = Path.Combine(_dataDirectory, "products.json");
            _settingsFilePath = Path.Combine(_dataDirectory, "settings.json");
            _logger = logger;

            InitializeFiles();
        }

        private void InitializeFiles()
        {
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
                _logger.LogInformation("Created data directory: {Directory}", _dataDirectory);
            }

            if (!File.Exists(_categoriesFilePath))
            {
                File.WriteAllText(_categoriesFilePath, "[]");
                _logger.LogInformation("Created categories.json file");
            }

            if (!File.Exists(_productsFilePath))
            {
                File.WriteAllText(_productsFilePath, "[]");
                _logger.LogInformation("Created products.json file");
            }

            if (!File.Exists(_settingsFilePath))
            {
                var defaultSettings = new SiteSettings();
                File.WriteAllText(_settingsFilePath, JsonConvert.SerializeObject(defaultSettings, Formatting.Indented));
                _logger.LogInformation("Created settings.json file");
            }
        }

        // ==================== CATEGORIES ====================

        public async Task<List<Category>> GetCategoriesAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                var json = await File.ReadAllTextAsync(_categoriesFilePath);
                return JsonConvert.DeserializeObject<List<Category>>(json) ?? new List<Category>();
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<Category?> GetCategoryAsync(int id)
        {
            var categories = await GetCategoriesAsync();
            return categories.FirstOrDefault(c => c.Id == id);
        }

        public async Task<(bool success, string message, Category? category)> CreateCategoryAsync(CreateCategoryRequest request, string? imagePath = null)
        {
            await _fileLock.WaitAsync();
            try
            {
                var categories = await GetCategoriesAsync();

                var category = new Category
                {
                    Id = categories.Count > 0 ? categories.Max(c => c.Id) + 1 : 1,
                    Name = request.Name,
                    Description = request.Description,
                    ImagePath = imagePath,
                    DisplayOrder = request.DisplayOrder,
                    Status = request.Status,
                    CreatedAt = DateTime.UtcNow.ToString("o"),
                    UpdatedAt = DateTime.UtcNow.ToString("o")
                };

                categories.Add(category);
                await WriteCategoriesAsync(categories);

                _logger.LogInformation("Created category: {Name} (ID: {Id})", category.Name, category.Id);
                return (true, "Category created successfully", category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return (false, $"Error creating category: {ex.Message}", null);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<(bool success, string message)> DeleteCategoryAsync(int id)
        {
            await _fileLock.WaitAsync();
            try
            {
                var categories = await GetCategoriesAsync();
                var category = categories.FirstOrDefault(c => c.Id == id);

                if (category == null)
                {
                    return (false, "Category not found");
                }

                categories.Remove(category);
                await WriteCategoriesAsync(categories);

                _logger.LogInformation("Deleted category: {Name} (ID: {Id})", category.Name, id);
                return (true, "Category deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category");
                return (false, $"Error deleting category: {ex.Message}");
            }
            finally
            {
                _fileLock.Release();
            }
        }

        // ==================== PRODUCTS ====================

        public async Task<List<Product>> GetProductsAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                var json = await File.ReadAllTextAsync(_productsFilePath);
                return JsonConvert.DeserializeObject<List<Product>>(json) ?? new List<Product>();
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<Product?> GetProductAsync(int id)
        {
            var products = await GetProductsAsync();
            return products.FirstOrDefault(p => p.Id == id);
        }

        public async Task<(bool success, string message, Product? product)> CreateProductAsync(CreateProductRequest request, string? imagePath = null)
        {
            await _fileLock.WaitAsync();
            try
            {
                var products = await GetProductsAsync();
                var categories = await GetCategoriesAsync();

                var category = categories.FirstOrDefault(c => c.Id == request.CategoryId);
                if (category == null)
                {
                    return (false, "Category not found", null);
                }

                var product = new Product
                {
                    Id = products.Count > 0 ? products.Max(p => p.Id) + 1 : 1,
                    Name = request.Name,
                    CategoryId = request.CategoryId,
                    Category = category.Name,
                    Price = request.Price,
                    Stock = request.Stock,
                    Description = request.Description,
                    ImagePath = imagePath,
                    Discount = request.Discount,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow.ToString("o"),
                    UpdatedAt = DateTime.UtcNow.ToString("o")
                };

                products.Add(product);
                await WriteProductsAsync(products);

                _logger.LogInformation("Created product: {Name} (ID: {Id})", product.Name, product.Id);
                return (true, "Product created successfully", product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return (false, $"Error creating product: {ex.Message}", null);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<(bool success, string message)> DeleteProductAsync(int id)
        {
            await _fileLock.WaitAsync();
            try
            {
                var products = await GetProductsAsync();
                var product = products.FirstOrDefault(p => p.Id == id);

                if (product == null)
                {
                    return (false, "Product not found");
                }

                products.Remove(product);
                await WriteProductsAsync(products);

                _logger.LogInformation("Deleted product: {Name} (ID: {Id})", product.Name, id);
                return (true, "Product deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                return (false, $"Error deleting product: {ex.Message}");
            }
            finally
            {
                _fileLock.Release();
            }
        }

        // ==================== ORDERS ====================

        public async Task<List<AdminOrder>> GetOrdersAsync()
        {
            var ordersPath = Path.Combine(_dataDirectory, "orders.json");
            if (!File.Exists(ordersPath))
            {
                return new List<AdminOrder>();
            }

            await _fileLock.WaitAsync();
            try
            {
                var json = await File.ReadAllTextAsync(ordersPath);
                return JsonConvert.DeserializeObject<List<AdminOrder>>(json) ?? new List<AdminOrder>();
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<List<AdminOrder>> GetOrdersByStatusAsync(string status)
        {
            var orders = await GetOrdersAsync();
            return orders.Where(o => o.Status == status).ToList();
        }

        // ==================== USERS ====================

        public async Task<List<AdminUser>> GetUsersAsync()
        {
            var usersPath = Path.Combine(_dataDirectory, "users.json");
            if (!File.Exists(usersPath))
            {
                return new List<AdminUser>();
            }

            await _fileLock.WaitAsync();
            try
            {
                var json = await File.ReadAllTextAsync(usersPath);
                var users = JsonConvert.DeserializeObject<List<dynamic>>(json) ?? new List<dynamic>();
                
                var adminUsers = users.Select(u => new AdminUser
                {
                    Id = u["id"]?.ToString() ?? string.Empty,
                    Username = u["username"]?.ToString() ?? string.Empty,
                    Email = u["email"]?.ToString() ?? string.Empty,
                    Role = u["role"]?.ToString() ?? "user",
                    CreatedAt = u["created_at"]?.ToString() ?? DateTime.UtcNow.ToString("o")
                }).ToList();

                return adminUsers;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<List<AdminUser>> SearchUsersAsync(string query)
        {
            var users = await GetUsersAsync();
            var lowerQuery = query.ToLower();
            
            return users.Where(u => 
                u.Username.ToLower().Contains(lowerQuery) || 
                u.Email.ToLower().Contains(lowerQuery)
            ).ToList();
        }

        // ==================== SETTINGS ====================

        public async Task<(bool success, string message)> SaveSettingsAsync(SiteSettings settings)
        {
            await _fileLock.WaitAsync();
            try
            {
                settings.UpdatedAt = DateTime.UtcNow.ToString("o");
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                await File.WriteAllTextAsync(_settingsFilePath, json);

                _logger.LogInformation("Saved site settings");
                return (true, "Settings saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving settings");
                return (false, $"Error saving settings: {ex.Message}");
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<SiteSettings?> GetSettingsAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    return new SiteSettings();
                }

                var json = await File.ReadAllTextAsync(_settingsFilePath);
                return JsonConvert.DeserializeObject<SiteSettings>(json);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        // ==================== BACKUP & MAINTENANCE ====================

        public async Task<object> GetBackupAsync()
        {
            var categories = await GetCategoriesAsync();
            var products = await GetProductsAsync();
            var orders = await GetOrdersAsync();
            var users = await GetUsersAsync();
            var settings = await GetSettingsAsync();

            return new
            {
                timestamp = DateTime.UtcNow.ToString("o"),
                categories,
                products,
                orders,
                users,
                settings
            };
        }

        public async Task<(bool success, string message)> ClearAllDataAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                File.WriteAllText(_categoriesFilePath, "[]");
                File.WriteAllText(_productsFilePath, "[]");

                _logger.LogWarning("Cleared all admin data");
                return (true, "All data cleared successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing data");
                return (false, $"Error clearing data: {ex.Message}");
            }
            finally
            {
                _fileLock.Release();
            }
        }

        // ==================== PRIVATE HELPERS ====================

        private async Task WriteCategoriesAsync(List<Category> categories)
        {
            var json = JsonConvert.SerializeObject(categories, Formatting.Indented);
            await File.WriteAllTextAsync(_categoriesFilePath, json);
        }

        private async Task WriteProductsAsync(List<Product> products)
        {
            var json = JsonConvert.SerializeObject(products, Formatting.Indented);
            await File.WriteAllTextAsync(_productsFilePath, json);
        }
    }
}
