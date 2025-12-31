namespace MRShop.OrderTracking.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a customer order
    /// </summary>
    public class Order
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonProperty("user_id")]
        public int UserId { get; set; }
        
        [JsonProperty("user_name")]
        public string UserName { get; set; } = string.Empty;
        
        [JsonProperty("items")]
        public List<OrderItem> Items { get; set; } = new();
        
        [JsonProperty("total")]
        public decimal Total { get; set; }
        
        [JsonProperty("payment_method")]
        public string PaymentMethod { get; set; } = string.Empty;
        
        [JsonProperty("shipping_address")]
        public ShippingAddress ShippingAddress { get; set; } = new();
        
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;
        
        [JsonProperty("tracking_id")]
        public string TrackingId { get; set; } = string.Empty;
        
        [JsonProperty("timeline")]
        public List<TimelineEntry> Timeline { get; set; } = new();
        
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Represents an item in an order
    /// </summary>
    public class OrderItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty("category")]
        public string Category { get; set; } = string.Empty;
        
        [JsonProperty("quantity")]
        public int Quantity { get; set; }
        
        [JsonProperty("price")]
        public decimal Price { get; set; }
        
        [JsonProperty("image")]
        public string Image { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents shipping address information
    /// </summary>
    public class ShippingAddress
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty("phone")]
        public string Phone { get; set; } = string.Empty;
        
        [JsonProperty("address")]
        public string Address { get; set; } = string.Empty;
        
        [JsonProperty("zip_code")]
        public string ZipCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a timeline entry for order tracking
    /// </summary>
    public class TimelineEntry
    {
        [JsonProperty("stage")]
        public string Stage { get; set; } = string.Empty;
        
        [JsonProperty("date")]
        public DateTime Date { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for creating a new order
    /// </summary>
    public class CreateOrderRequest
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public List<OrderItem> Items { get; set; } = new();
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public ShippingAddress ShippingAddress { get; set; } = new();
    }

    /// <summary>
    /// Request model for updating order status
    /// </summary>
    public class UpdateOrderStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response model for API operations
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// Response model for order list
    /// </summary>
    public class OrderListResponse
    {
        public int Count { get; set; }
        public List<Order> Orders { get; set; } = new();
    }

    /// <summary>
    /// Response model for order timeline
    /// </summary>
    public class OrderTimelineResponse
    {
        public string OrderId { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public string CurrentStage { get; set; } = string.Empty;
        public string TrackingId { get; set; } = string.Empty;
        public List<TimelineEntry> Timeline { get; set; } = new();
    }
}
