using MRShop.OrderTracking.Models;
using Newtonsoft.Json;
using System.Text;

namespace MRShop.OrderTracking.Services
{
    /// <summary>
    /// Service for managing orders using JSON file storage
    /// </summary>
    public interface IOrderService
    {
        Task<List<Order>> GetAllOrdersAsync();
        Task<List<Order>> GetUserOrdersAsync(int userId);
        Task<Order?> GetOrderByIdAsync(string orderId);
        Task<Order> CreateOrderAsync(CreateOrderRequest request);
        Task<Order?> UpdateOrderStatusAsync(string orderId, string status);
        Task<OrderTimelineResponse?> GetOrderTimelineAsync(string orderId);
    }

    public class OrderService : IOrderService
    {
        private readonly string _dataFilePath;
        private readonly SemaphoreSlim _fileLock = new(1, 1);
        private readonly ILogger<OrderService> _logger;

        public OrderService(IConfiguration configuration, ILogger<OrderService> logger)
        {
            _logger = logger;
            var dataDir = configuration["DataDirectory"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "data");
            Directory.CreateDirectory(dataDir);
            _dataFilePath = Path.Combine(dataDir, "orders.json");
            
            // Initialize file if it doesn't exist
            if (!File.Exists(_dataFilePath))
            {
                File.WriteAllText(_dataFilePath, "[]");
                _logger.LogInformation("Created orders.json file at {Path}", _dataFilePath);
            }
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                var json = await File.ReadAllTextAsync(_dataFilePath);
                var orders = JsonConvert.DeserializeObject<List<Order>>(json) ?? new List<Order>();
                return orders.OrderByDescending(o => o.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading orders from file");
                return new List<Order>();
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<List<Order>> GetUserOrdersAsync(int userId)
        {
            var allOrders = await GetAllOrdersAsync();
            return allOrders.Where(o => o.UserId == userId).ToList();
        }

        public async Task<Order?> GetOrderByIdAsync(string orderId)
        {
            var orders = await GetAllOrdersAsync();
            return orders.FirstOrDefault(o => o.Id == orderId);
        }

        public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
        {
            await _fileLock.WaitAsync();
            try
            {
                var orders = await GetAllOrdersAsync();
                
                // Generate order ID
                var orderCount = orders.Count + 1;
                var orderId = $"MR-2025-{orderCount:D4}";
                
                // Generate tracking ID
                var trackingId = $"DHC-{DateTime.Now:yyyyMMddHHmmss}";

                var newOrder = new Order
                {
                    Id = orderId,
                    UserId = request.UserId,
                    UserName = request.UserName,
                    Items = request.Items,
                    Total = request.Total,
                    PaymentMethod = request.PaymentMethod,
                    ShippingAddress = request.ShippingAddress,
                    Status = "confirmed",
                    TrackingId = trackingId,
                    Timeline = new List<TimelineEntry>
                    {
                        new TimelineEntry
                        {
                            Stage = "confirmed",
                            Date = DateTime.UtcNow,
                            Description = "Order confirmed"
                        }
                    },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                orders.Add(newOrder);
                await SaveOrdersAsync(orders);

                _logger.LogInformation("Created new order {OrderId} for user {UserId}", orderId, request.UserId);
                return newOrder;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<Order?> UpdateOrderStatusAsync(string orderId, string status)
        {
            await _fileLock.WaitAsync();
            try
            {
                var orders = await GetAllOrdersAsync();
                var order = orders.FirstOrDefault(o => o.Id == orderId);

                if (order == null)
                {
                    return null;
                }

                // Update status
                order.Status = status;
                order.UpdatedAt = DateTime.UtcNow;

                // Add timeline entry
                var timelineDescriptions = new Dictionary<string, string>
                {
                    { "confirmed", "Order confirmed" },
                    { "packed", "Order packed and ready for shipment" },
                    { "shipped", "Order shipped from warehouse" },
                    { "out-for-delivery", "Out for delivery" },
                    { "delivered", "Order delivered successfully" },
                    { "cancelled", "Order cancelled" }
                };

                order.Timeline.Add(new TimelineEntry
                {
                    Stage = status,
                    Date = DateTime.UtcNow,
                    Description = timelineDescriptions.GetValueOrDefault(status, $"Status updated to {status}")
                });

                await SaveOrdersAsync(orders);

                _logger.LogInformation("Updated order {OrderId} status to {Status}", orderId, status);
                return order;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<OrderTimelineResponse?> GetOrderTimelineAsync(string orderId)
        {
            var order = await GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return null;
            }

            var statusMap = new Dictionary<string, string>
            {
                { "pending", "confirmed" },
                { "confirmed", "confirmed" },
                { "packed", "packed" },
                { "shipped", "shipped" },
                { "out-for-delivery", "out-for-delivery" },
                { "delivered", "delivered" },
                { "cancelled", "cancelled" }
            };

            var currentStage = statusMap.GetValueOrDefault(order.Status, "confirmed");

            return new OrderTimelineResponse
            {
                OrderId = order.Id,
                CurrentStatus = order.Status,
                CurrentStage = currentStage,
                TrackingId = order.TrackingId,
                Timeline = order.Timeline
            };
        }

        private async Task SaveOrdersAsync(List<Order> orders)
        {
            var json = JsonConvert.SerializeObject(orders, Formatting.Indented);
            await File.WriteAllTextAsync(_dataFilePath, json, Encoding.UTF8);
        }
    }
}
