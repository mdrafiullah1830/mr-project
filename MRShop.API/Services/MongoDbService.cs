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

        CreateIndexes();
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Product> Products => _database.GetCollection<Product>("products");
    public IMongoCollection<CartItem> CartItems => _database.GetCollection<CartItem>("cartItems");
    public IMongoCollection<WishlistItem> WishlistItems => _database.GetCollection<WishlistItem>("wishlistItems");
    public IMongoCollection<Order> Orders => _database.GetCollection<Order>("orders");
    public IMongoCollection<OrderTimeline> OrderTimelines => _database.GetCollection<OrderTimeline>("orderTimelines");
    public IMongoCollection<Address> Addresses => _database.GetCollection<Address>("addresses");
    public IMongoCollection<Invoice> Invoices => _database.GetCollection<Invoice>("invoices");
    public IMongoCollection<SellerApplication> SellerApplications => _database.GetCollection<SellerApplication>("sellerApplications");
    public IMongoCollection<SellerProfile> SellerProfiles => _database.GetCollection<SellerProfile>("sellerProfiles");
    public IMongoCollection<Coupon> Coupons => _database.GetCollection<Coupon>("coupons");
    public IMongoCollection<Category> Categories => _database.GetCollection<Category>("categories");
    public IMongoCollection<Review> Reviews => _database.GetCollection<Review>("reviews");
    public IMongoCollection<Brand> Brands => _database.GetCollection<Brand>("brands");
    public IMongoCollection<InventoryLog> InventoryLogs => _database.GetCollection<InventoryLog>("inventoryLogs");

    private void CreateIndexes()
    {
        var indexes = new (string name, Action create)[]
        {
            ("product_text_search", () => Products.Indexes.CreateOne(new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Combine(
                    Builders<Product>.IndexKeys.Text(p => p.Name),
                    Builders<Product>.IndexKeys.Text(p => p.Description),
                    Builders<Product>.IndexKeys.Text(p => p.Sku)
                ), new CreateIndexOptions { Name = "product_text_search" }))),
            ("product_seller_status", () => Products.Indexes.CreateOne(new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Combine(
                    Builders<Product>.IndexKeys.Ascending(p => p.SellerId),
                    Builders<Product>.IndexKeys.Ascending(p => p.Status)
                ), new CreateIndexOptions { Name = "product_seller_status" }))),
            ("product_category", () => Products.Indexes.CreateOne(new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.CategoryId),
                new CreateIndexOptions { Name = "product_category" }))),
            ("product_brand", () => Products.Indexes.CreateOne(new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.BrandId),
                new CreateIndexOptions { Name = "product_brand" }))),
            ("product_created", () => Products.Indexes.CreateOne(new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Descending(p => p.CreatedAt),
                new CreateIndexOptions { Name = "product_created" }))),
            ("product_sold", () => Products.Indexes.CreateOne(new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Descending(p => p.SoldCount),
                new CreateIndexOptions { Name = "product_sold" }))),
            ("product_views", () => Products.Indexes.CreateOne(new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Descending(p => p.ViewCount),
                new CreateIndexOptions { Name = "product_views" }))),
            ("product_slug", () => Products.Indexes.CreateOne(new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.Slug),
                new CreateIndexOptions { Name = "product_slug", Unique = true, Sparse = true }))),
            ("order_customer", () => Orders.Indexes.CreateOne(new CreateIndexModel<Order>(
                Builders<Order>.IndexKeys.Ascending(o => o.CustomerId),
                new CreateIndexOptions { Name = "order_customer" }))),
            ("order_seller", () => Orders.Indexes.CreateOne(new CreateIndexModel<Order>(
                Builders<Order>.IndexKeys.Ascending(o => o.SellerId),
                new CreateIndexOptions { Name = "order_seller" }))),
            ("order_status", () => Orders.Indexes.CreateOne(new CreateIndexModel<Order>(
                Builders<Order>.IndexKeys.Ascending(o => o.Status),
                new CreateIndexOptions { Name = "order_status" }))),
            ("order_number", () => Orders.Indexes.CreateOne(new CreateIndexModel<Order>(
                Builders<Order>.IndexKeys.Ascending(o => o.OrderNumber),
                new CreateIndexOptions { Name = "order_number", Unique = true }))),
            ("order_created", () => Orders.Indexes.CreateOne(new CreateIndexModel<Order>(
                Builders<Order>.IndexKeys.Descending(o => o.CreatedAt),
                new CreateIndexOptions { Name = "order_created" }))),
            ("order_customer_date", () => Orders.Indexes.CreateOne(new CreateIndexModel<Order>(
                Builders<Order>.IndexKeys.Combine(
                    Builders<Order>.IndexKeys.Ascending(o => o.CustomerId),
                    Builders<Order>.IndexKeys.Descending(o => o.CreatedAt)
                ), new CreateIndexOptions { Name = "order_customer_date" }))),
            ("order_seller_date", () => Orders.Indexes.CreateOne(new CreateIndexModel<Order>(
                Builders<Order>.IndexKeys.Combine(
                    Builders<Order>.IndexKeys.Ascending(o => o.SellerId),
                    Builders<Order>.IndexKeys.Descending(o => o.CreatedAt)
                ), new CreateIndexOptions { Name = "order_seller_date" }))),
            ("timeline_order", () => OrderTimelines.Indexes.CreateOne(new CreateIndexModel<OrderTimeline>(
                Builders<OrderTimeline>.IndexKeys.Ascending(t => t.OrderId),
                new CreateIndexOptions { Name = "timeline_order" }))),
            ("address_user", () => Addresses.Indexes.CreateOne(new CreateIndexModel<Address>(
                Builders<Address>.IndexKeys.Ascending(a => a.UserId),
                new CreateIndexOptions { Name = "address_user" }))),
            ("invoice_order", () => Invoices.Indexes.CreateOne(new CreateIndexModel<Invoice>(
                Builders<Invoice>.IndexKeys.Ascending(i => i.OrderId),
                new CreateIndexOptions { Name = "invoice_order" }))),
            ("invoice_customer", () => Invoices.Indexes.CreateOne(new CreateIndexModel<Invoice>(
                Builders<Invoice>.IndexKeys.Ascending(i => i.CustomerId),
                new CreateIndexOptions { Name = "invoice_customer" }))),
            ("invoice_number", () => Invoices.Indexes.CreateOne(new CreateIndexModel<Invoice>(
                Builders<Invoice>.IndexKeys.Ascending(i => i.InvoiceNumber),
                new CreateIndexOptions { Name = "invoice_number", Unique = true }))),
            ("cart_user", () => CartItems.Indexes.CreateOne(new CreateIndexModel<CartItem>(
                Builders<CartItem>.IndexKeys.Ascending(c => c.UserId),
                new CreateIndexOptions { Name = "cart_user" }))),
            ("review_product", () => Reviews.Indexes.CreateOne(new CreateIndexModel<Review>(
                Builders<Review>.IndexKeys.Ascending(r => r.ProductId),
                new CreateIndexOptions { Name = "review_product" }))),
            ("inventory_product_date", () => InventoryLogs.Indexes.CreateOne(new CreateIndexModel<InventoryLog>(
                Builders<InventoryLog>.IndexKeys.Combine(
                    Builders<InventoryLog>.IndexKeys.Ascending(l => l.ProductId),
                    Builders<InventoryLog>.IndexKeys.Descending(l => l.CreatedAt)
                ), new CreateIndexOptions { Name = "inventory_product_date" }))),
        };

        foreach (var (name, create) in indexes)
        {
            try { create(); }
            catch (Exception ex)
            {
                Console.WriteLine($"[MongoDbService] Index '{name}' warning: {ex.Message}");
            }
        }
    }
}
