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
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("discount")]
        public int Discount { get; set; }

        [JsonPropertyName("final_price")]
        public decimal FinalPrice { get; set; }

        [JsonPropertyName("rating")]
        public decimal Rating { get; set; } = 5.0m;

        [JsonPropertyName("stock")]
        public int Stock { get; set; }

        [JsonPropertyName("seller")]
        public string Seller { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = "active";

        [JsonPropertyName("image_path")]
        public string ImagePath { get; set; } = string.Empty;

        [JsonPropertyName("imageBase64")]
        public string ImageBase64 { get; set; } = string.Empty;

        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [JsonPropertyName("reviews")]
        public int Reviews { get; set; } = 0;
    }

    /// <summary>
    /// Category model
    /// </summary>
    public class Category
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("productCount")]
        public int ProductCount { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; } = string.Empty;
    }

    /// <summary>
    /// Admin Product Service - Handles all product operations
    /// </summary>
    public class AdminProductService
    {
        private readonly string _productsPath;
        private readonly string _dataPath;

        public AdminProductService(string? dataPath = null)
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
                new Category { Name = "🍯 Food & Natural", Icon = "🍯" },
                new Category { Name = "🎨 Handicrafts", Icon = "🎨" },
                new Category { Name = "📚 Books", Icon = "📚" },
                new Category { Name = "🍰 Sweets & Dairy", Icon = "🍰" },
                new Category { Name = "👗 Clothing", Icon = "👗" },
                new Category { Name = "🪙 Antique & Collectibles", Icon = "🪙" }
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
            var normalizedCategory = NormalizeCategorySlug(category);

            return allProducts
                .Where(p => p.Status.Equals("active", StringComparison.OrdinalIgnoreCase))
                .Where(p => NormalizeCategorySlug(p.Category) == normalizedCategory)
                .ToList();
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

            var products = GetAllProducts();
            product.Id = product.Id > 0 ? product.Id : GetNextProductId(products);
            product.Category = NormalizeCategorySlug(product.Category);
            product.Seller = string.IsNullOrWhiteSpace(product.Seller) ? "Admin" : product.Seller.Trim();
            product.Status = NormalizeStatus(product.Status);
            product.ImagePath = ResolveImagePath(product);
            product.ImageBase64 = string.IsNullOrWhiteSpace(product.ImageBase64) ? product.ImagePath : product.ImageBase64;
            product.ImageUrl = string.IsNullOrWhiteSpace(product.ImageUrl) ? product.ImagePath : product.ImageUrl;
            product.FinalPrice = product.FinalPrice > 0 ? product.FinalPrice : CalculateFinalPrice(product.Price, product.Discount);
            product.CreatedAt = DateTime.Now;
            product.UpdatedAt = DateTime.Now;

            products.Add(product);
            SaveProducts(products);

            Console.WriteLine($"✅ Product '{product.Name}' added to category '{product.Category}'");
            return product;
        }

        /// <summary>
        /// Update product
        /// </summary>
        public Product UpdateProduct(int productId, Product updatedProduct)
        {
            var products = GetAllProducts();
            var existingProduct = products.FirstOrDefault(p => p.Id == productId);

            if (existingProduct == null)
                throw new InvalidOperationException($"Product with ID {productId} not found");

            existingProduct.Name = string.IsNullOrWhiteSpace(updatedProduct.Name) ? existingProduct.Name : updatedProduct.Name.Trim();
            existingProduct.Description = updatedProduct.Description ?? existingProduct.Description;
            existingProduct.Price = updatedProduct.Price > 0 ? updatedProduct.Price : existingProduct.Price;
            existingProduct.Discount = updatedProduct.Discount >= 0 ? updatedProduct.Discount : existingProduct.Discount;
            existingProduct.FinalPrice = updatedProduct.FinalPrice > 0 ? updatedProduct.FinalPrice : CalculateFinalPrice(existingProduct.Price, existingProduct.Discount);
            existingProduct.Rating = updatedProduct.Rating > 0 ? updatedProduct.Rating : existingProduct.Rating;
            existingProduct.Stock = updatedProduct.Stock >= 0 ? updatedProduct.Stock : existingProduct.Stock;
            existingProduct.Seller = string.IsNullOrWhiteSpace(updatedProduct.Seller) ? existingProduct.Seller : updatedProduct.Seller.Trim();
            existingProduct.Category = string.IsNullOrWhiteSpace(updatedProduct.Category) ? existingProduct.Category : NormalizeCategorySlug(updatedProduct.Category);

            var resolvedImagePath = ResolveImagePath(updatedProduct);
            if (!string.IsNullOrWhiteSpace(resolvedImagePath))
            {
                existingProduct.ImagePath = resolvedImagePath;
            }

            if (!string.IsNullOrWhiteSpace(updatedProduct.ImageBase64))
            {
                existingProduct.ImageBase64 = updatedProduct.ImageBase64;
            }

            if (!string.IsNullOrWhiteSpace(updatedProduct.ImageUrl))
            {
                existingProduct.ImageUrl = updatedProduct.ImageUrl;
            }

            existingProduct.Status = NormalizeStatus(string.IsNullOrWhiteSpace(updatedProduct.Status) ? existingProduct.Status : updatedProduct.Status);
            existingProduct.UpdatedAt = DateTime.Now;

            SaveProducts(products);
            Console.WriteLine($"✅ Product '{existingProduct.Name}' updated");
            return existingProduct;
        }

        /// <summary>
        /// Delete product
        /// </summary>
        public bool DeleteProduct(int productId)
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
        public Product? GetProductById(int productId)
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
                .GroupBy(p => NormalizeCategorySlug(p.Category))
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

        private static int GetNextProductId(List<Product> products)
        {
            if (products.Count == 0)
            {
                return 201;
            }

            return products.Max(p => p.Id) + 1;
        }

        private static decimal CalculateFinalPrice(decimal price, int discount)
        {
            if (discount <= 0)
            {
                return price;
            }

            return Math.Max(0, price - ((price * discount) / 100m));
        }

        private static string NormalizeStatus(string? status)
        {
            var normalizedStatus = (status ?? string.Empty).Trim().ToLowerInvariant();

            return normalizedStatus switch
            {
                "inactive" => "inactive",
                "discontinued" => "discontinued",
                _ => "active"
            };
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
                    Category = "clothing",
                    ImagePath = "", // Would be image data in real scenario
                    Status = "active",
                    Rating = 4.8m,
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
                var sareeProducts = adminService.GetProductsByCategory("clothing");
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
