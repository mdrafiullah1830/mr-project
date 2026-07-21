using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MRShop.API.Models;
using MRShop.API.Services;

namespace MRShop.API.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly MongoDbService _mongoDb;

    public AdminController(MongoDbService mongoDb)
    {
        _mongoDb = mongoDb;
    }

    // ==================== DASHBOARD ====================

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var users = await _mongoDb.Users.Find(_ => true).ToListAsync();
        var sellers = await _mongoDb.Users.Find(u => u.Role == "seller").ToListAsync();
        var applications = await _mongoDb.SellerApplications.Find(_ => true).ToListAsync();
        var products = await _mongoDb.Products.Find(_ => true).ToListAsync();
        var orders = await _mongoDb.Orders.Find(_ => true).ToListAsync();

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var todayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);

        var totalRevenue = orders.Where(o => o.Status == "delivered").Sum(o => o.TotalAmount);
        var monthlyRevenue = orders.Where(o => o.Status == "delivered" && o.CreatedAt >= monthStart).Sum(o => o.TotalAmount);
        var todayRevenue = orders.Where(o => o.Status == "delivered" && o.CreatedAt >= todayStart).Sum(o => o.TotalAmount);

        var recentOrders = orders.OrderByDescending(o => o.CreatedAt).Take(10).Select(o => new
        {
            id = o.Id,
            userId = o.UserId,
            totalAmount = o.TotalAmount,
            status = o.Status,
            createdAt = o.CreatedAt,
            items = o.Items.Count,
            paymentMethod = o.PaymentMethod
        });

        // Top products by sales
        var productSales = new Dictionary<string, (string Name, int Qty, decimal Revenue)>();
        foreach (var order in orders.Where(o => o.Status == "delivered"))
        {
            foreach (var item in order.Items)
            {
                if (productSales.ContainsKey(item.ProductId))
                {
                    var existing = productSales[item.ProductId];
                    productSales[item.ProductId] = (existing.Name, existing.Qty + item.Quantity, existing.Revenue + (item.Price * item.Quantity));
                }
                else
                {
                    productSales[item.ProductId] = (item.ProductName, item.Quantity, item.Price * item.Quantity);
                }
            }
        }
        var topProducts = productSales.Values.OrderByDescending(p => p.Revenue).Take(5).Select(p => new
        {
            name = p.Name,
            totalSold = p.Qty,
            revenue = p.Revenue
        });

        // Top sellers
        var sellerSales = new Dictionary<string, (string Name, decimal Revenue)>();
        foreach (var order in orders.Where(o => o.Status == "delivered"))
        {
            foreach (var item in order.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product?.SellerId != null)
                {
                    var sellerName = sellers.FirstOrDefault(s => s.Id == product.SellerId)?.Name ?? "Unknown";
                    if (sellerSales.ContainsKey(product.SellerId))
                    {
                        sellerSales[product.SellerId] = (sellerName, sellerSales[product.SellerId].Revenue + (item.Price * item.Quantity));
                    }
                    else
                    {
                        sellerSales[product.SellerId] = (sellerName, item.Price * item.Quantity);
                    }
                }
            }
        }
        var topSellers = sellerSales.Values.OrderByDescending(s => s.Revenue).Take(5).Select(s => new
        {
            name = s.Name,
            revenue = s.Revenue
        });

        return Ok(new
        {
            // User stats
            totalUsers = users.Count(u => u.Role == "customer"),
            totalSellers = sellers.Count,
            pendingApplications = applications.Count(a => a.Status == "pending"),
            approvedApplications = applications.Count(a => a.Status == "approved"),
            rejectedApplications = applications.Count(a => a.Status == "rejected"),

            // Product stats
            totalProducts = products.Count,
            activeProducts = products.Count(p => p.IsActive && p.Stock > 0),
            outOfStockProducts = products.Count(p => p.Stock <= 0),

            // Order stats
            totalOrders = orders.Count,
            pendingOrders = orders.Count(o => o.Status == "pending"),
            confirmedOrders = orders.Count(o => o.Status == "confirmed"),
            shippedOrders = orders.Count(o => o.Status == "shipped"),
            deliveredOrders = orders.Count(o => o.Status == "delivered"),
            cancelledOrders = orders.Count(o => o.Status == "cancelled"),

            // Revenue
            totalRevenue,
            monthlyRevenue,
            todayRevenue,

            // Lists
            recentOrders,
            topProducts,
            topSellers,
            newUsersThisMonth = users.Count(u => u.CreatedAt >= monthStart)
        });
    }

    // ==================== USER MANAGEMENT ====================

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] string? search)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Role, "customer");
        if (!string.IsNullOrWhiteSpace(search))
        {
            var escaped = System.Text.RegularExpressions.Regex.Escape(search);
            var searchFilter = Builders<User>.Filter.Or(
                Builders<User>.Filter.Regex(u => u.Name, new MongoDB.Bson.BsonRegularExpression(escaped, "i")),
                Builders<User>.Filter.Regex(u => u.Email, new MongoDB.Bson.BsonRegularExpression(escaped, "i"))
            );
            filter = Builders<User>.Filter.And(filter, searchFilter);
        }

        var users = await _mongoDb.Users
            .Find(filter)
            .SortByDescending(u => u.CreatedAt)
            .ToListAsync();

        // Get order counts for each user
        var orders = await _mongoDb.Orders.Find(_ => true).ToListAsync();
        var userOrderCounts = orders.GroupBy(o => o.UserId).ToDictionary(g => g.Key, g => g.Count());

        return Ok(users.Select(u => new
        {
            id = u.Id,
            name = u.Name,
            email = u.Email,
            phone = u.Phone,
            role = u.Role,
            orderCount = userOrderCounts.GetValueOrDefault(u.Id, 0),
            createdAt = u.CreatedAt,
            isActive = u.Role == "customer"
        }));
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        var user = await _mongoDb.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
        if (user == null) return NotFound(new { message = "User not found." });

        var orders = await _mongoDb.Orders
            .Find(o => o.UserId == id)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(new
        {
            id = user.Id,
            name = user.Name,
            email = user.Email,
            phone = user.Phone,
            address = user.Address,
            role = user.Role,
            createdAt = user.CreatedAt,
            orders = orders.Select(o => new
            {
                id = o.Id,
                totalAmount = o.TotalAmount,
                status = o.Status,
                createdAt = o.CreatedAt,
                items = o.Items.Count
            })
        });
    }

    [HttpPut("users/{id}/suspend")]
    public async Task<IActionResult> SuspendUser(string id)
    {
        var user = await _mongoDb.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
        if (user == null) return NotFound(new { message = "User not found." });
        if (user.Role == "admin") return BadRequest(new { message = "Cannot suspend admin users." });

        await _mongoDb.Users.UpdateOneAsync(
            u => u.Id == id,
            Builders<User>.Update.Set(u => u.UpdatedAt, DateTime.UtcNow)
        );

        return Ok(new { message = "User suspended." });
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _mongoDb.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
        if (user == null) return NotFound(new { message = "User not found." });
        if (user.Role == "admin") return BadRequest(new { message = "Cannot delete admin users." });

        await _mongoDb.Users.DeleteOneAsync(u => u.Id == id);
        return Ok(new { message = "User deleted." });
    }

    // ==================== SELLER MANAGEMENT ====================

    [HttpGet("sellers")]
    public async Task<IActionResult> GetSellers([FromQuery] string? status)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Role, "seller");
        if (!string.IsNullOrWhiteSpace(status))
        {
            // Filter by application status
            var apps = await _mongoDb.SellerApplications
                .Find(a => a.Status == status)
                .ToListAsync();
            var emails = apps.Select(a => a.Email).ToHashSet();
            filter = Builders<User>.Filter.And(filter,
                Builders<User>.Filter.In(u => u.Email, emails));
        }

        var sellers = await _mongoDb.Users.Find(filter).ToListAsync();
        var profiles = await _mongoDb.SellerProfiles.Find(_ => true).ToListAsync();
        var applications = await _mongoDb.SellerApplications.Find(_ => true).ToListAsync();

        return Ok(sellers.Select(s =>
        {
            var profile = profiles.FirstOrDefault(p => p.UserId == s.Id);
            var app = applications.FirstOrDefault(a => a.Email == s.Email);
            return new
            {
                id = s.Id,
                name = s.Name,
                email = s.Email,
                phone = s.Phone,
                shopName = profile?.ShopName ?? app?.ShopName ?? "N/A",
                categories = profile?.Categories ?? new List<string>(),
                totalSales = profile?.TotalSales ?? 0,
                totalProducts = profile?.TotalProducts ?? 0,
                isActive = profile?.IsActive ?? true,
                createdAt = s.CreatedAt
            };
        }));
    }

    [HttpGet("sellers/applications")]
    public async Task<IActionResult> GetSellerApplications([FromQuery] string? status)
    {
        var filter = Builders<SellerApplication>.Filter.Empty;
        if (!string.IsNullOrWhiteSpace(status))
        {
            filter = Builders<SellerApplication>.Filter.Eq(a => a.Status, status);
        }

        var applications = await _mongoDb.SellerApplications
            .Find(filter)
            .SortByDescending(a => a.CreatedAt)
            .ToListAsync();

        return Ok(applications);
    }

    [HttpPut("sellers/{id}/suspend")]
    public async Task<IActionResult> SuspendSeller(string id)
    {
        var user = await _mongoDb.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
        if (user == null) return NotFound(new { message = "Seller not found." });

        await _mongoDb.Users.UpdateOneAsync(
            u => u.Id == id,
            Builders<User>.Update.Set(u => u.Role, "customer")
        );

        await _mongoDb.SellerProfiles.UpdateOneAsync(
            p => p.UserId == id,
            Builders<SellerProfile>.Update.Set(p => p.IsActive, false)
        );

        return Ok(new { message = "Seller suspended." });
    }

    // ==================== PRODUCT MANAGEMENT ====================

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? category,
        [FromQuery] string? search,
        [FromQuery] bool? inStock)
    {
        var filter = Builders<Product>.Filter.Empty;

        if (!string.IsNullOrWhiteSpace(category))
        {
            filter = Builders<Product>.Filter.And(filter,
                Builders<Product>.Filter.Eq(p => p.Category, category));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var escaped = System.Text.RegularExpressions.Regex.Escape(search);
            filter = Builders<Product>.Filter.And(filter,
                Builders<Product>.Filter.Regex(p => p.Name, new MongoDB.Bson.BsonRegularExpression(escaped, "i")));
        }

        if (inStock.HasValue)
        {
            if (inStock.Value)
                filter = Builders<Product>.Filter.And(filter, Builders<Product>.Filter.Gt(p => p.Stock, 0));
            else
                filter = Builders<Product>.Filter.And(filter, Builders<Product>.Filter.Lte(p => p.Stock, 0));
        }

        var products = await _mongoDb.Products
            .Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(products.Select(p => new
        {
            id = p.Id,
            name = p.Name,
            description = p.Description,
            price = p.Price,
            originalPrice = p.OriginalPrice,
            category = p.Category,
            stock = p.Stock,
            image = p.Image,
            images = p.Images,
            rating = p.Rating,
            reviewCount = p.ReviewCount,
            sellerId = p.SellerId,
            isActive = p.IsActive,
            createdAt = p.CreatedAt
        }));
    }

    [HttpPut("products/{id}/toggle-visibility")]
    public async Task<IActionResult> ToggleProductVisibility(string id)
    {
        var product = await _mongoDb.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
        if (product == null) return NotFound(new { message = "Product not found." });

        await _mongoDb.Products.UpdateOneAsync(
            p => p.Id == id,
            Builders<Product>.Update
                .Set(p => p.IsActive, !product.IsActive)
                .Set(p => p.UpdatedAt, DateTime.UtcNow)
        );

        return Ok(new { message = $"Product {(!product.IsActive ? "activated" : "hidden")}.", isActive = !product.IsActive });
    }

    // ==================== ORDER MANAGEMENT ====================

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders(
        [FromQuery] string? status,
        [FromQuery] string? search)
    {
        var filter = Builders<Order>.Filter.Empty;

        if (!string.IsNullOrWhiteSpace(status))
        {
            filter = Builders<Order>.Filter.And(filter,
                Builders<Order>.Filter.Eq(o => o.Status, status));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var escaped = System.Text.RegularExpressions.Regex.Escape(search);
            filter = Builders<Order>.Filter.And(filter,
                Builders<Order>.Filter.Or(
                    Builders<Order>.Filter.Regex(o => o.Id, new MongoDB.Bson.BsonRegularExpression(escaped, "i")),
                    Builders<Order>.Filter.Regex(o => o.ShippingAddress, new MongoDB.Bson.BsonRegularExpression(escaped, "i"))
                ));
        }

        var orders = await _mongoDb.Orders
            .Find(filter)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync();

        // Get user names
        var userIds = orders.Select(o => o.UserId).Distinct().ToList();
        var users = await _mongoDb.Users.Find(u => userIds.Contains(u.Id)).ToListAsync();
        var userMap = users.ToDictionary(u => u.Id, u => u.Name);

        return Ok(orders.Select(o => new
        {
            id = o.Id,
            userId = o.UserId,
            customerName = userMap.GetValueOrDefault(o.UserId, "Customer"),
            items = o.Items.Select(i => new { i.ProductId, i.ProductName, i.Price, i.Quantity, i.Image }),
            itemCount = o.Items.Count,
            totalAmount = o.TotalAmount,
            shippingAddress = o.ShippingAddress,
            paymentMethod = o.PaymentMethod,
            status = o.Status,
            createdAt = o.CreatedAt
        }));
    }

    [HttpPut("orders/{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] UpdateAdminOrderStatusRequest request)
    {
        var validStatuses = new[] { "pending", "confirmed", "shipped", "delivered", "cancelled" };
        if (!validStatuses.Contains(request.Status))
        {
            return BadRequest(new { message = "Invalid status." });
        }

        var order = await _mongoDb.Orders.Find(o => o.Id == id).FirstOrDefaultAsync();
        if (order == null) return NotFound(new { message = "Order not found." });

        await _mongoDb.Orders.UpdateOneAsync(
            o => o.Id == id,
            Builders<Order>.Update
                .Set(o => o.Status, request.Status)
                .Set(o => o.UpdatedAt, DateTime.UtcNow)
        );

        return Ok(new { message = $"Order status updated to {request.Status}." });
    }

    // ==================== CATEGORY MANAGEMENT ====================

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _mongoDb.Categories
            .Find(_ => true)
            .SortBy(c => c.SortOrder)
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Category name is required." });

        var slug = request.Name.ToLower().Replace(" ", "-").Replace("&", "and");
        var existing = await _mongoDb.Categories.Find(c => c.Slug == slug).FirstOrDefaultAsync();
        if (existing != null) return Conflict(new { message = "Category already exists." });

        var category = new Category
        {
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description,
            Image = request.Image,
            IsActive = true,
            SortOrder = request.SortOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _mongoDb.Categories.InsertOneAsync(category);
        return Ok(new { message = "Category created.", category });
    }

    [HttpPut("categories/{id}")]
    public async Task<IActionResult> UpdateCategory(string id, [FromBody] UpdateCategoryRequest request)
    {
        var existing = await _mongoDb.Categories.Find(c => c.Id == id).FirstOrDefaultAsync();
        if (existing == null) return NotFound(new { message = "Category not found." });

        var update = Builders<Category>.Update.Set(c => c.UpdatedAt, DateTime.UtcNow);
        if (request.Name != null) update = update.Set(c => c.Name, request.Name);
        if (request.Description != null) update = update.Set(c => c.Description, request.Description);
        if (request.Image != null) update = update.Set(c => c.Image, request.Image);
        if (request.IsActive.HasValue) update = update.Set(c => c.IsActive, request.IsActive.Value);
        if (request.SortOrder.HasValue) update = update.Set(c => c.SortOrder, request.SortOrder.Value);

        await _mongoDb.Categories.UpdateOneAsync(c => c.Id == id, update);
        return Ok(new { message = "Category updated." });
    }

    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(string id)
    {
        var result = await _mongoDb.Categories.DeleteOneAsync(c => c.Id == id);
        if (result.DeletedCount == 0) return NotFound(new { message = "Category not found." });
        return Ok(new { message = "Category deleted." });
    }

    // ==================== REVIEW MANAGEMENT ====================

    [HttpGet("reviews")]
    public async Task<IActionResult> GetReviews([FromQuery] string? productId)
    {
        var filter = Builders<Review>.Filter.Empty;
        if (!string.IsNullOrWhiteSpace(productId))
        {
            filter = Builders<Review>.Filter.Eq(r => r.ProductId, productId);
        }

        var reviews = await _mongoDb.Reviews
            .Find(filter)
            .SortByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(reviews);
    }

    [HttpDelete("reviews/{id}")]
    public async Task<IActionResult> DeleteReview(string id)
    {
        var result = await _mongoDb.Reviews.DeleteOneAsync(r => r.Id == id);
        if (result.DeletedCount == 0) return NotFound(new { message = "Review not found." });
        return Ok(new { message = "Review deleted." });
    }

    [HttpPut("reviews/{id}/toggle-visibility")]
    public async Task<IActionResult> ToggleReviewVisibility(string id)
    {
        var review = await _mongoDb.Reviews.Find(r => r.Id == id).FirstOrDefaultAsync();
        if (review == null) return NotFound(new { message = "Review not found." });

        await _mongoDb.Reviews.UpdateOneAsync(
            r => r.Id == id,
            Builders<Review>.Update.Set(r => r.IsVisible, !review.IsVisible)
        );

        return Ok(new { message = $"Review {(!review.IsVisible ? "shown" : "hidden")}.", isVisible = !review.IsVisible });
    }

    // ==================== ANALYTICS ====================

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics()
    {
        var users = await _mongoDb.Users.Find(_ => true).ToListAsync();
        var orders = await _mongoDb.Orders.Find(_ => true).ToListAsync();
        var products = await _mongoDb.Products.Find(_ => true).ToListAsync();

        var now = DateTime.UtcNow;

        // Monthly revenue for last 6 months
        var monthlyRevenue = new List<object>();
        for (int i = 5; i >= 0; i--)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-i);
            var monthEnd = monthStart.AddMonths(1);
            var revenue = orders
                .Where(o => o.Status == "delivered" && o.CreatedAt >= monthStart && o.CreatedAt < monthEnd)
                .Sum(o => o.TotalAmount);
            monthlyRevenue.Add(new { month = monthStart.ToString("MMM yyyy"), revenue });
        }

        // Orders by status
        var ordersByStatus = new
        {
            pending = orders.Count(o => o.Status == "pending"),
            confirmed = orders.Count(o => o.Status == "confirmed"),
            shipped = orders.Count(o => o.Status == "shipped"),
            delivered = orders.Count(o => o.Status == "delivered"),
            cancelled = orders.Count(o => o.Status == "cancelled")
        };

        // New users per month (last 6 months)
        var newUsers = new List<object>();
        for (int i = 5; i >= 0; i--)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-i);
            var monthEnd = monthStart.AddMonths(1);
            var count = users.Count(u => u.CreatedAt >= monthStart && u.CreatedAt < monthEnd);
            newUsers.Add(new { month = monthStart.ToString("MMM yyyy"), count });
        }

        // Top categories by product count
        var categoryStats = products
            .GroupBy(p => p.Category)
            .Select(g => new { category = g.Key, count = g.Count(), revenue = orders.Where(o => o.Items.Any(i => products.Any(p => p.Id == i.ProductId && p.Category == g.Key) && o.Status == "delivered")).Sum(o => o.TotalAmount) })
            .OrderByDescending(c => c.count)
            .Take(6)
            .ToList();

        return Ok(new
        {
            monthlyRevenue,
            ordersByStatus,
            newUsers,
            categoryStats,
            totalProducts = products.Count,
            totalOrders = orders.Count,
            totalUsers = users.Count(u => u.Role == "customer")
        });
    }
}

public class UpdateAdminOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Image { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateCategoryRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}
