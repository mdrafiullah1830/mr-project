using System.Net.Security;
using System.Security.Authentication;
using MongoDB.Driver;
using MRShop.API.Models;

namespace MRShop.API.Services;

public class MongoDbService
{
    private readonly IMongoDatabase _database;

    public MongoDbService(IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING")
            ?? configuration["MongoDB:ConnectionString"]
            ?? throw new InvalidOperationException(
                "MongoDB connection string not configured. " +
                "Set MONGODB_CONNECTION_STRING environment variable.");

        var databaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME")
            ?? configuration["MongoDB:DatabaseName"]
            ?? "mrshop";

        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.SslSettings = new SslSettings
        {
            EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
        };

        var client = new MongoClient(settings);
        _database = client.GetDatabase(databaseName);

        // Create indexes on startup
        CreateIndexes();
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Product> Products => _database.GetCollection<Product>("products");
    public IMongoCollection<CartItem> CartItems => _database.GetCollection<CartItem>("cartItems");
    public IMongoCollection<WishlistItem> WishlistItems => _database.GetCollection<WishlistItem>("wishlistItems");
    public IMongoCollection<Order> Orders => _database.GetCollection<Order>("orders");
    public IMongoCollection<SellerApplication> SellerApplications => _database.GetCollection<SellerApplication>("sellerApplications");
    public IMongoCollection<SellerProfile> SellerProfiles => _database.GetCollection<SellerProfile>("sellerProfiles");
    public IMongoCollection<Coupon> Coupons => _database.GetCollection<Coupon>("coupons");
    public IMongoCollection<Category> Categories => _database.GetCollection<Category>("categories");
    public IMongoCollection<Review> Reviews => _database.GetCollection<Review>("reviews");
    public IMongoCollection<Brand> Brands => _database.GetCollection<Brand>("brands");
    public IMongoCollection<InventoryLog> InventoryLogs => _database.GetCollection<InventoryLog>("inventoryLogs");

    private void CreateIndexes()
    {
        // Product indexes
        var productIndexes = Products.Indexes;
        productIndexes.CreateOne(new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Combine(
                Builders<Product>.IndexKeys.Text(p => p.Name),
                Builders<Product>.IndexKeys.Text(p => p.Description),
                Builders<Product>.IndexKeys.Text(p => p.Sku)
            ),
            new CreateIndexOptions { Name = "product_text_search" }
        ));
        productIndexes.CreateOne(new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Combine(
                Builders<Product>.IndexKeys.Ascending(p => p.SellerId),
                Builders<Product>.IndexKeys.Ascending(p => p.Status)
            ),
            new CreateIndexOptions { Name = "product_seller_status" }
        ));
        productIndexes.CreateOne(new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Ascending(p => p.CategoryId),
            new CreateIndexOptions { Name = "product_category" }
        ));
        productIndexes.CreateOne(new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Ascending(p => p.BrandId),
            new CreateIndexOptions { Name = "product_brand" }
        ));
        productIndexes.CreateOne(new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Descending(p => p.CreatedAt),
            new CreateIndexOptions { Name = "product_created" }
        ));
        productIndexes.CreateOne(new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Descending(p => p.SoldCount),
            new CreateIndexOptions { Name = "product_sold" }
        ));
        productIndexes.CreateOne(new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Descending(p => p.ViewCount),
            new CreateIndexOptions { Name = "product_views" }
        ));
        productIndexes.CreateOne(new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Ascending(p => p.Slug),
            new CreateIndexOptions { Name = "product_slug", Unique = true }
        ));

        // Order indexes
        Orders.Indexes.CreateOne(new CreateIndexModel<Order>(
            Builders<Order>.IndexKeys.Ascending(o => o.UserId),
            new CreateIndexOptions { Name = "order_user" }
        ));
        Orders.Indexes.CreateOne(new CreateIndexModel<Order>(
            Builders<Order>.IndexKeys.Ascending(o => o.Status),
            new CreateIndexOptions { Name = "order_status" }
        ));

        // Review indexes
        Reviews.Indexes.CreateOne(new CreateIndexModel<Review>(
            Builders<Review>.IndexKeys.Ascending(r => r.ProductId),
            new CreateIndexOptions { Name = "review_product" }
        ));

        // InventoryLog indexes
        InventoryLogs.Indexes.CreateOne(new CreateIndexModel<InventoryLog>(
            Builders<InventoryLog>.IndexKeys.Combine(
                Builders<InventoryLog>.IndexKeys.Ascending(l => l.ProductId),
                Builders<InventoryLog>.IndexKeys.Descending(l => l.CreatedAt)
            ),
            new CreateIndexOptions { Name = "inventory_product_date" }
        ));
    }
}
