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
            EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
            ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true
        };
        settings.AllowInsecureTls = true;

        var client = new MongoClient(settings);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Product> Products => _database.GetCollection<Product>("products");
    public IMongoCollection<CartItem> CartItems => _database.GetCollection<CartItem>("cartItems");
    public IMongoCollection<WishlistItem> WishlistItems => _database.GetCollection<WishlistItem>("wishlistItems");
    public IMongoCollection<Order> Orders => _database.GetCollection<Order>("orders");
}
