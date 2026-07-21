using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MRShop.API.DTOs;
using MRShop.API.Models;
using MRShop.API.Services;

namespace MRShop.API.Controllers;

[Authorize]
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly MongoDbService _mongoDb;

    private static readonly string[] ValidStatuses = { "pending", "confirmed", "processing", "packed", "shipped", "out_for_delivery", "delivered", "cancelled", "returned", "refunded" };
    private static readonly string[] SellerTransitions = { "confirmed", "processing", "packed", "shipped", "out_for_delivery", "delivered" };
    private static readonly string[] CustomerCancellable = { "pending", "confirmed" };

    public OrdersController(MongoDbService mongoDb)
    {
        _mongoDb = mongoDb;
    }

    // ==================== CUSTOMER ====================

    [HttpGet]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var filter = Builders<Order>.Filter.Eq(o => o.CustomerId, userId);
        if (!string.IsNullOrWhiteSpace(status))
            filter = Builders<Order>.Filter.And(filter, Builders<Order>.Filter.Eq(o => o.Status, status));

        var total = await _mongoDb.Orders.CountDocumentsAsync(filter);
        var orders = await _mongoDb.Orders.Find(filter)
            .SortByDescending(o => o.CreatedAt)
            .Skip((page - 1) * limit)
            .Limit(limit)
            .ToListAsync();

        return Ok(new
        {
            orders = orders.Select(o => MapToOrderResponse(o, includeTimeline: false)),
            totalCount = total,
            page,
            totalPages = (int)Math.Ceiling((double)total / limit)
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(string id)
    {
        var userId = GetUserId();
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (userId == null) return Unauthorized();

        var order = await _mongoDb.Orders.Find(o => o.Id == id).FirstOrDefaultAsync();
        if (order == null) return NotFound(new { message = "Order not found." });

        // Ownership check
        if (role == "customer" && order.CustomerId != userId)
            return Forbid();
        if (role == "seller")
        {
            var isSeller = order.SellerId == userId;
            if (!isSeller) return Forbid();
        }

        var timeline = await _mongoDb.OrderTimelines
            .Find(t => t.OrderId == id)
            .SortByDescending(t => t.UpdatedAt)
            .ToListAsync();

        var resp = MapToOrderResponse(order, includeTimeline: true);
        resp.Timeline = timeline.Select(t => new OrderTimelineResponse
        {
            Status = t.Status,
            UpdatedBy = t.UpdatedBy,
            UpdatedAt = t.UpdatedAt,
            Remarks = t.Remarks
        }).ToList();

        return Ok(resp);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(string id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var order = await _mongoDb.Orders.Find(o => o.Id == id).FirstOrDefaultAsync();
        if (order == null) return NotFound(new { message = "Order not found." });

        if (order.CustomerId != userId)
            return Forbid();

        if (!CustomerCancellable.Contains(order.Status))
            return BadRequest(new { message = $"Cannot cancel order in '{order.Status}' status." });

        // Restore stock
        foreach (var item in order.Items)
        {
            var product = await _mongoDb.Products.Find(p => p.Id == item.ProductId).FirstOrDefaultAsync();
            if (product != null)
            {
                await _mongoDb.Products.UpdateOneAsync(
                    p => p.Id == item.ProductId,
                    Builders<Product>.Update
                        .Inc(p => p.StockQuantity, item.Quantity)
                        .Inc(p => p.ReservedStock, -item.Quantity)
                );

                await _mongoDb.InventoryLogs.InsertOneAsync(new InventoryLog
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    SellerId = item.SellerId,
                    Action = "restored",
                    Quantity = item.Quantity,
                    PreviousStock = product.StockQuantity,
                    NewStock = product.StockQuantity + item.Quantity,
                    Reason = $"Order {order.OrderNumber} cancelled by customer",
                    PerformedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await UpdateOrderStatus(order, "cancelled", userId, "Cancelled by customer");
        return Ok(new { message = "Order cancelled." });
    }

    [HttpPost("{id}/request-return")]
    public async Task<IActionResult> RequestReturn(string id, [FromBody] ReturnRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var order = await _mongoDb.Orders.Find(o => o.Id == id).FirstOrDefaultAsync();
        if (order == null) return NotFound(new { message = "Order not found." });
        if (order.CustomerId != userId) return Forbid();
        if (order.Status != "delivered")
            return BadRequest(new { message = "Can only request return for delivered orders." });

        await UpdateOrderStatus(order, "returned", userId, request?.Reason ?? "Return requested");
        return Ok(new { message = "Return request submitted." });
    }

    [HttpGet("{id}/track")]
    public async Task<IActionResult> TrackOrder(string id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var order = await _mongoDb.Orders.Find(o => o.Id == id).FirstOrDefaultAsync();
        if (order == null) return NotFound(new { message = "Order not found." });

        if (order.CustomerId != userId)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "admin" && order.SellerId != userId)
                return Forbid();
        }

        var timeline = await _mongoDb.OrderTimelines
            .Find(t => t.OrderId == id)
            .SortBy(t => t.UpdatedAt)
            .ToListAsync();

        return Ok(new
        {
            orderNumber = order.OrderNumber,
            status = order.Status,
            trackingNumber = order.TrackingNumber,
            carrier = order.Carrier,
            deliveryMethod = order.DeliveryMethod,
            estimatedDelivery = order.DeliveryMethod == "express" ?
                order.CreatedAt.AddDays(2) : order.CreatedAt.AddDays(5),
            timeline = timeline.Select(t => new
            {
                status = t.Status,
                timestamp = t.UpdatedAt,
                remarks = t.Remarks
            })
        });
    }

    // ==================== SELLER ====================

    [HttpGet("seller")]
    [Authorize(Roles = "seller,admin")]
    public async Task<IActionResult> GetSellerOrders(
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var filter = Builders<Order>.Filter.Eq(o => o.SellerId, userId);

        if (!string.IsNullOrWhiteSpace(status))
            filter = Builders<Order>.Filter.And(filter, Builders<Order>.Filter.Eq(o => o.Status, status));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var escaped = Regex.Escape(search);
            filter = Builders<Order>.Filter.And(filter,
                Builders<Order>.Filter.Or(
                    Builders<Order>.Filter.Regex(o => o.OrderNumber, new BsonRegularExpression(escaped, "i")),
                    Builders<Order>.Filter.Regex(o => o.CustomerName, new BsonRegularExpression(escaped, "i")),
                    Builders<Order>.Filter.Regex(o => o.CustomerPhone, new BsonRegularExpression(escaped, "i"))
                ));
        }

        var total = await _mongoDb.Orders.CountDocumentsAsync(filter);
        var orders = await _mongoDb.Orders.Find(filter)
            .SortByDescending(o => o.CreatedAt)
            .Skip((page - 1) * limit)
            .Limit(limit)
            .ToListAsync();

        return Ok(new
        {
            orders = orders.Select(o => MapToOrderResponse(o, includeTimeline: false)),
            totalCount = total,
            page,
            totalPages = (int)Math.Ceiling((double)total / limit)
        });
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "seller,admin")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateOrderStatusRequest request)
    {
        var userId = GetUserId();
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (userId == null) return Unauthorized();

        if (!ValidStatuses.Contains(request.Status))
            return BadRequest(new { message = "Invalid status." });

        var order = await _mongoDb.Orders.Find(o => o.Id == id).FirstOrDefaultAsync();
        if (order == null) return NotFound(new { message = "Order not found." });

        // Seller can only update own orders and only to allowed transitions
        if (role == "seller")
        {
            if (order.SellerId != userId) return Forbid();
            if (!SellerTransitions.Contains(request.Status))
                return BadRequest(new { message = "Seller cannot set this status." });
        }

        // When delivered: finalize stock (decrease reserved permanently)
        if (request.Status == "delivered")
        {
            foreach (var item in order.Items)
            {
                await _mongoDb.Products.UpdateOneAsync(
                    p => p.Id == item.ProductId,
                    Builders<Product>.Update.Inc(p => p.ReservedStock, -item.Quantity)
                );

                await _mongoDb.Products.UpdateOneAsync(
                    p => p.Id == item.ProductId,
                    Builders<Product>.Update.Inc(p => p.SoldCount, item.Quantity)
                );
            }

            // Update payment status for COD
            if (order.PaymentMethod == "cod")
            {
                await _mongoDb.Orders.UpdateOneAsync(
                    o => o.Id == id,
                    Builders<Order>.Update.Set(o => o.PaymentStatus, "paid")
                );
            }
        }

        // When cancelled: restore stock
        if (request.Status == "cancelled" && order.Status != "cancelled")
        {
            foreach (var item in order.Items)
            {
                var product = await _mongoDb.Products.Find(p => p.Id == item.ProductId).FirstOrDefaultAsync();
                if (product != null)
                {
                    await _mongoDb.Products.UpdateOneAsync(
                        p => p.Id == item.ProductId,
                        Builders<Product>.Update
                            .Inc(p => p.StockQuantity, item.Quantity)
                            .Inc(p => p.ReservedStock, -item.Quantity)
                    );
                }
            }
        }

        // When refunded: restore stock
        if (request.Status == "refunded" && order.Status != "refunded")
        {
            foreach (var item in order.Items)
            {
                var product = await _mongoDb.Products.Find(p => p.Id == item.ProductId).FirstOrDefaultAsync();
                if (product != null)
                {
                    await _mongoDb.Products.UpdateOneAsync(
                        p => p.Id == item.ProductId,
                        Builders<Product>.Update
                            .Inc(p => p.StockQuantity, item.Quantity)
                            .Inc(p => p.ReservedStock, -item.Quantity)
                    );
                }
            }
            await _mongoDb.Orders.UpdateOneAsync(
                o => o.Id == id,
                Builders<Order>.Update.Set(o => o.PaymentStatus, "refunded")
            );
        }

        var update = Builders<Order>.Update
            .Set(o => o.Status, request.Status)
            .Set(o => o.UpdatedAt, DateTime.UtcNow);

        if (request.TrackingNumber != null)
            update = update.Set(o => o.TrackingNumber, request.TrackingNumber);
        if (request.Carrier != null)
            update = update.Set(o => o.Carrier, request.Carrier);

        await _mongoDb.Orders.UpdateOneAsync(o => o.Id == id, update);

        await UpdateOrderStatus(order, request.Status, userId, request.Remarks);

        return Ok(new { message = $"Order status updated to {request.Status}." });
    }

    [HttpPut("{id}/tracking")]
    [Authorize(Roles = "seller,admin")]
    public async Task<IActionResult> UpdateTracking(string id, [FromBody] UpdateOrderStatusRequest request)
    {
        var userId = GetUserId();
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (userId == null) return Unauthorized();

        var order = await _mongoDb.Orders.Find(o => o.Id == id).FirstOrDefaultAsync();
        if (order == null) return NotFound(new { message = "Order not found." });

        if (role == "seller" && order.SellerId != userId) return Forbid();

        var update = Builders<Order>.Update.Set(o => o.UpdatedAt, DateTime.UtcNow);
        if (request.TrackingNumber != null) update = update.Set(o => o.TrackingNumber, request.TrackingNumber);
        if (request.Carrier != null) update = update.Set(o => o.Carrier, request.Carrier);

        await _mongoDb.Orders.UpdateOneAsync(o => o.Id == id, update);
        return Ok(new { message = "Tracking updated." });
    }

    // ==================== ADMIN ====================

    [HttpGet("admin/all")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAllOrders([FromQuery] OrderSearchRequest request)
    {
        var filter = Builders<Order>.Filter.Empty;

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var escaped = Regex.Escape(request.Search);
            filter = Builders<Order>.Filter.And(filter,
                Builders<Order>.Filter.Or(
                    Builders<Order>.Filter.Regex(o => o.OrderNumber, new BsonRegularExpression(escaped, "i")),
                    Builders<Order>.Filter.Regex(o => o.CustomerName, new BsonRegularExpression(escaped, "i")),
                    Builders<Order>.Filter.Regex(o => o.CustomerEmail, new BsonRegularExpression(escaped, "i")),
                    Builders<Order>.Filter.Regex(o => o.CustomerPhone, new BsonRegularExpression(escaped, "i"))
                ));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
            filter = Builders<Order>.Filter.And(filter, Builders<Order>.Filter.Eq(o => o.Status, request.Status));

        if (!string.IsNullOrWhiteSpace(request.PaymentStatus))
            filter = Builders<Order>.Filter.And(filter, Builders<Order>.Filter.Eq(o => o.PaymentStatus, request.PaymentStatus));

        if (!string.IsNullOrWhiteSpace(request.SellerId))
            filter = Builders<Order>.Filter.And(filter, Builders<Order>.Filter.Eq(o => o.SellerId, request.SellerId));

        if (!string.IsNullOrWhiteSpace(request.CustomerId))
            filter = Builders<Order>.Filter.And(filter, Builders<Order>.Filter.Eq(o => o.CustomerId, request.CustomerId));

        if (request.DateFrom.HasValue)
            filter = Builders<Order>.Filter.And(filter, Builders<Order>.Filter.Gte(o => o.CreatedAt, request.DateFrom.Value));

        if (request.DateTo.HasValue)
            filter = Builders<Order>.Filter.And(filter, Builders<Order>.Filter.Lte(o => o.CreatedAt, request.DateTo.Value.AddDays(1)));

        var sortDef = request.Sort switch
        {
            "oldest" => Builders<Order>.Sort.Ascending(o => o.CreatedAt),
            "total_asc" => Builders<Order>.Sort.Ascending(o => o.GrandTotal),
            "total_desc" => Builders<Order>.Sort.Descending(o => o.GrandTotal),
            _ => Builders<Order>.Sort.Descending(o => o.CreatedAt)
        };

        var total = await _mongoDb.Orders.CountDocumentsAsync(filter);
        var orders = await _mongoDb.Orders.Find(filter)
            .Sort(sortDef)
            .Skip((request.Page - 1) * request.Limit)
            .Limit(request.Limit)
            .ToListAsync();

        return Ok(new
        {
            orders = orders.Select(o => MapToOrderResponse(o, includeTimeline: false)),
            totalCount = total,
            page = request.Page,
            totalPages = (int)Math.Ceiling((double)total / request.Limit)
        });
    }

    [HttpPut("admin/{id}/status")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AdminUpdateStatus(string id, [FromBody] UpdateOrderStatusRequest request)
    {
        if (!ValidStatuses.Contains(request.Status))
            return BadRequest(new { message = "Invalid status." });

        var order = await _mongoDb.Orders.Find(o => o.Id == id).FirstOrDefaultAsync();
        if (order == null) return NotFound(new { message = "Order not found." });

        var userId = GetUserId()!;

        if (request.Status == "delivered" && order.Status != "delivered")
        {
            foreach (var item in order.Items)
            {
                await _mongoDb.Products.UpdateOneAsync(
                    p => p.Id == item.ProductId,
                    Builders<Product>.Update
                        .Inc(p => p.ReservedStock, -item.Quantity)
                        .Inc(p => p.SoldCount, item.Quantity)
                );
            }
            if (order.PaymentMethod == "cod")
            {
                await _mongoDb.Orders.UpdateOneAsync(o => o.Id == id,
                    Builders<Order>.Update.Set(o => o.PaymentStatus, "paid"));
            }
        }

        if ((request.Status == "cancelled" || request.Status == "refunded") && order.Status != request.Status)
        {
            foreach (var item in order.Items)
            {
                var product = await _mongoDb.Products.Find(p => p.Id == item.ProductId).FirstOrDefaultAsync();
                if (product != null)
                {
                    await _mongoDb.Products.UpdateOneAsync(
                        p => p.Id == item.ProductId,
                        Builders<Product>.Update
                            .Inc(p => p.StockQuantity, item.Quantity)
                            .Inc(p => p.ReservedStock, -item.Quantity)
                    );
                }
            }
            if (request.Status == "refunded")
            {
                await _mongoDb.Orders.UpdateOneAsync(o => o.Id == id,
                    Builders<Order>.Update.Set(o => o.PaymentStatus, "refunded"));
            }
        }

        var update = Builders<Order>.Update
            .Set(o => o.Status, request.Status)
            .Set(o => o.UpdatedAt, DateTime.UtcNow);
        if (request.TrackingNumber != null) update = update.Set(o => o.TrackingNumber, request.TrackingNumber);
        if (request.Carrier != null) update = update.Set(o => o.Carrier, request.Carrier);

        await _mongoDb.Orders.UpdateOneAsync(o => o.Id == id, update);
        await UpdateOrderStatus(order, request.Status, userId, request.Remarks);

        return Ok(new { message = $"Order status updated to {request.Status}." });
    }

    // ==================== HELPERS ====================

    private async Task UpdateOrderStatus(Order order, string status, string updatedBy, string? remarks)
    {
        await _mongoDb.OrderTimelines.InsertOneAsync(new OrderTimeline
        {
            OrderId = order.Id,
            Status = status,
            UpdatedBy = updatedBy,
            UpdatedAt = DateTime.UtcNow,
            Remarks = remarks ?? $"Status updated to {status}"
        });
    }

    private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    private static OrderResponse MapToOrderResponse(Order o, bool includeTimeline) => new()
    {
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        CustomerId = o.CustomerId,
        CustomerName = o.CustomerName,
        CustomerEmail = o.CustomerEmail,
        CustomerPhone = o.CustomerPhone,
        SellerId = o.SellerId,
        SellerName = o.SellerName,
        Items = o.Items.Select(i => new OrderItemResponse
        {
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            Sku = i.Sku,
            Price = i.Price,
            DiscountPrice = i.DiscountPrice,
            Quantity = i.Quantity,
            Image = i.Image,
            SellerId = i.SellerId
        }).ToList(),
        ShippingAddress = o.ShippingAddress != null ? new OrderAddressResponse
        {
            FullName = o.ShippingAddress.FullName,
            Phone = o.ShippingAddress.Phone,
            Email = o.ShippingAddress.Email,
            Country = o.ShippingAddress.Country,
            Division = o.ShippingAddress.Division,
            District = o.ShippingAddress.District,
            City = o.ShippingAddress.City,
            Area = o.ShippingAddress.Area,
            PostalCode = o.ShippingAddress.PostalCode,
            AddressLine = o.ShippingAddress.AddressLine,
            FullAddress = o.ShippingAddress.ToDisplayString()
        } : null,
        PaymentMethod = o.PaymentMethod,
        PaymentStatus = o.PaymentStatus,
        Status = o.Status,
        DeliveryMethod = o.DeliveryMethod,
        TrackingNumber = o.TrackingNumber,
        Carrier = o.Carrier,
        Subtotal = o.Subtotal,
        Discount = o.Discount,
        CouponCode = o.CouponCode,
        CouponDiscount = o.CouponDiscount,
        ShippingCharge = o.ShippingCharge,
        Tax = o.Tax,
        GrandTotal = o.GrandTotal,
        Notes = o.Notes,
        CreatedAt = o.CreatedAt,
        UpdatedAt = o.UpdatedAt
    };
}

public class ReturnRequest
{
    public string? Reason { get; set; }
}
