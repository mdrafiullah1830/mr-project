using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MRShop.AdminPanel
{
    /// <summary>
    /// Product model for admin panel
    /// </summary>
    public class Product
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("stock")]
        public int Stock { get; set; }

        [JsonPropertyName("seller")]
        public string Seller { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("imageBase64")]
        public string ImageBase64 { get; set; }

        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [JsonPropertyName("rating")]
        public double Rating { get; set; } = 5.0;

        [JsonPropertyName("reviews")]
        public int Reviews { get; set; } = 0;
    }

    /// <summary>
    /// Category model
    /// </summary>
    public class Category
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("productCount")]
        public int ProductCount { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }
    }

    /// <summary>
    /// Admin Product Service - Handles all product operations
    /// </summary>
    public class AdminProductService
    {
        private readonly string _productsPath;
        private readonly string _dataPath;

        public AdminProductService(string dataPath = null)
        {
            _dataPath = dataPath ?? Path.Combine(Directory.GetCurrentDirectory(), "data");
            _productsPath = Path.Combine(_dataPath, "products.json");
            
            // Ensure data directory exists
            Directory.CreateDirectory(_dataPath);
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        public List<Category> GetAllCategories()
        {
            var categories = new List<Category>
            {
                new Category { Name = "Clothing", Icon = "👕" },
                new Category { Name = "Sarees", Icon = "👗" },
                new Category { Name = "Salwar", Icon = "👔" },
                new Category { Name = "Panjabi", Icon = "🥻" },
                new Category { Name = "Lungi", Icon = "👖" },
                new Category { Name = "Handicrafts", Icon = "🎨" },
                new Category { Name = "Pottery", Icon = "🏺" },
                new Category { Name = "Nakshi", Icon = "🧵" },
                new Category { Name = "Jute", Icon = "🌾" },
                new Category { Name = "Wood", Icon = "🪵" },
                new Category { Name = "Organic Food", Icon = "🥬" },
                new Category { Name = "Sweets & Dairy", Icon = "🍶" },
                new Category { Name = "Food & Natural", Icon = "🥗" },
                new Category { Name = "Honey", Icon = "🍯" },
                new Category { Name = "Milk", Icon = "🥛" },
                new Category { Name = "Antiques", Icon = "🏛️" },
                new Category { Name = "Books", Icon = "📚" }
            };

            return categories;
        }

        /// <summary>
        /// Get all products
        /// </summary>
        public List<Product> GetAllProducts()
        {
            if (!File.Exists(_productsPath))
                return new List<Product>();

            try
            {
                var json = File.ReadAllText(_productsPath);
                var products = JsonSerializer.Deserialize<List<Product>>(json, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return products ?? new List<Product>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading products: {ex.Message}");
                return new List<Product>();
            }
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        public List<Product> GetProductsByCategory(string category)
        {
            var allProducts = GetAllProducts();
            return allProducts.Where(p => p.Category?.ToLower() == category?.ToLower()).ToList();
        }

        /// <summary>
        /// Add new product
        /// </summary>
        public Product AddProduct(Product product)
        {
            if (string.IsNullOrEmpty(product.Name))
                throw new ArgumentException("Product name is required");

            if (product.Price < 0)
                throw new ArgumentException("Price cannot be negative");

            product.Id = Guid.NewGuid().ToString();
            product.CreatedAt = DateTime.Now;
            product.UpdatedAt = DateTime.Now;

            var products = GetAllProducts();
            products.Add(product);
            SaveProducts(products);

            Console.WriteLine($"✅ Product '{product.Name}' added to category '{product.Category}'");
            return product;
        }

        /// <summary>
        /// Update product
        /// </summary>
        public Product UpdateProduct(string productId, Product updatedProduct)
        {
            var products = GetAllProducts();
            var existingProduct = products.FirstOrDefault(p => p.Id == productId);

            if (existingProduct == null)
                throw new InvalidOperationException($"Product with ID {productId} not found");

            existingProduct.Name = updatedProduct.Name ?? existingProduct.Name;
            existingProduct.Description = updatedProduct.Description ?? existingProduct.Description;
            existingProduct.Price = updatedProduct.Price > 0 ? updatedProduct.Price : existingProduct.Price;
            existingProduct.Stock = updatedProduct.Stock >= 0 ? updatedProduct.Stock : existingProduct.Stock;
            existingProduct.Seller = updatedProduct.Seller ?? existingProduct.Seller;
            existingProduct.Category = updatedProduct.Category ?? existingProduct.Category;
            existingProduct.ImageBase64 = updatedProduct.ImageBase64 ?? existingProduct.ImageBase64;
            existingProduct.UpdatedAt = DateTime.Now;

            SaveProducts(products);
            Console.WriteLine($"✅ Product '{existingProduct.Name}' updated");
            return existingProduct;
        }

        /// <summary>
        /// Delete product
        /// </summary>
        public bool DeleteProduct(string productId)
        {
            var products = GetAllProducts();
            var product = products.FirstOrDefault(p => p.Id == productId);

            if (product == null)
                return false;

            products.Remove(product);
            SaveProducts(products);
            Console.WriteLine($"✅ Product '{product.Name}' deleted");
            return true;
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        public Product GetProductById(string productId)
        {
            var products = GetAllProducts();
            return products.FirstOrDefault(p => p.Id == productId);
        }

        /// <summary>
        /// Search products
        /// </summary>
        public List<Product> SearchProducts(string query)
        {
            var allProducts = GetAllProducts();
            return allProducts
                .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           p.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();
        }

        /// <summary>
        /// Get category statistics
        /// </summary>
        public Dictionary<string, int> GetCategoryStats()
        {
            var products = GetAllProducts();
            return products
                .GroupBy(p => p.Category)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Save products to JSON file
        /// </summary>
        private void SaveProducts(List<Product> products)
        {
            try
            {
                var json = JsonSerializer.Serialize(products, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(_productsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving products: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get total revenue (all products * price)
        /// </summary>
        public decimal GetTotalRevenue()
        {
            var products = GetAllProducts();
            return products.Sum(p => p.Price * p.Stock);
        }

        /// <summary>
        /// Get total inventory value
        /// </summary>
        public decimal GetInventoryValue()
        {
            var products = GetAllProducts();
            return products.Sum(p => p.Price * p.Stock);
        }

        /// <summary>
        /// Get low stock products
        /// </summary>
        public List<Product> GetLowStockProducts(int threshold = 10)
        {
            var products = GetAllProducts();
            return products.Where(p => p.Stock <= threshold).ToList();
        }
    }

    /// <summary>
    /// Example usage and testing
    /// </summary>
    public class AdminPanelDemo
    {
        public static void Main()
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   MR SHOP - Admin Panel API (C#)                          ║");
            Console.WriteLine("║   Product Management System                               ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

            try
            {
                // Initialize service
                var adminService = new AdminProductService();

                // Display available categories
                Console.WriteLine("📂 Available Categories:");
                var categories = adminService.GetAllCategories();
                foreach (var cat in categories)
                {
                    Console.WriteLine($"   {cat.Icon} {cat.Name}");
                }

                Console.WriteLine("\n" + new string('=', 60));

                // Example: Add a new product
                Console.WriteLine("\n➕ Adding New Product...\n");
                var newProduct = new Product
                {
                    Name = "Traditional Bengali Saree",
                    Description = "Beautiful handcrafted saree with authentic patterns",
                    Price = 5500m,
                    Stock = 25,
                    Seller = "Rafi's Textile House",
                    Category = "Sarees",
                    ImageBase64 = "", // Would be image data in real scenario
                    Rating = 4.8,
                    Reviews = 150
                };

                var addedProduct = adminService.AddProduct(newProduct);
                Console.WriteLine($"   Product ID: {addedProduct.Id}");
                Console.WriteLine($"   Name: {addedProduct.Name}");
                Console.WriteLine($"   Price: ৳{addedProduct.Price}");
                Console.WriteLine($"   Stock: {addedProduct.Stock} units");

                Console.WriteLine("\n" + new string('=', 60));

                // Get products by category
                Console.WriteLine("\n📦 Products in 'Sarees' Category:\n");
                var sareeProducts = adminService.GetProductsByCategory("Sarees");
                foreach (var product in sareeProducts)
                {
                    Console.WriteLine($"   • {product.Name} - ৳{product.Price} ({product.Stock} in stock)");
                }

                Console.WriteLine("\n" + new string('=', 60));

                // Statistics
                Console.WriteLine("\n📊 Admin Dashboard Statistics:\n");
                Console.WriteLine($"   Total Products: {adminService.GetAllProducts().Count}");
                Console.WriteLine($"   Total Inventory Value: ৳{adminService.GetInventoryValue()}");
                Console.WriteLine($"   Total Revenue: ৳{adminService.GetTotalRevenue()}");
                
                var stats = adminService.GetCategoryStats();
                Console.WriteLine("\n   Products per Category:");
                foreach (var stat in stats)
                {
                    Console.WriteLine($"      {stat.Key}: {stat.Value} products");
                }

                Console.WriteLine("\n" + new string('=', 60));
                Console.WriteLine("\n✅ Admin Panel API is ready for use!");
                Console.WriteLine("   Use this service with the HTML admin panel (adminrafi.html)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
            }
        }
    }
}
