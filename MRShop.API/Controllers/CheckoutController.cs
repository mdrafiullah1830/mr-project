using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MRShop.API.DTOs;
using MRShop.API.Models;
using MRShop.API.Services;

namespace MRShop.API.Controllers;

[Authorize]
[ApiController]
[Route("api/checkout")]
public class CheckoutController : ControllerBase
{
    private readonly MongoDbService _mongoDb;

    private const decimal STANDARD_SHIPPING = 60;
    private const decimal EXPRESS_SHIPPING = 150;
    private const decimal FREE_SHIPPING_THRESHOLD = 5000;
    private const decimal TAX_RATE = 0.0m;

    public CheckoutController(MongoDbService mongoDb)
    {
        _mongoDb = mongoDb;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetCheckoutSummary(
        [FromQuery] string? addressId,
        [FromQuery] string? deliveryMethod,
        [FromQuery] string? couponCode)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var cartItems = await _mongoDb.CartItems
            .Find(c => c.UserId == userId && !c.SavedForLater)
            .ToListAsync();

        if (cartItems.Count == 0)
            return BadRequest(new { message = "Cart is empty." });

        // Validate stock for each item
        var validItems = new List<CartItemResponse>();
        decimal subtotal = 0;
        decimal discount = 0;

        foreach (var ci in cartItems)
        {
            var product = await _mongoDb.Products.Find(p => p.Id == ci.ProductId).FirstOrDefaultAsync();
            if (product == null || product.Status != "published") continue;

            var effectivePrice = product.DiscountPrice ?? product.Price;
            var originalTotal = product.Price * ci.Quantity;
            var effectiveTotal = effectivePrice * ci.Quantity;
            discount += originalTotal - effectiveTotal;

            validItems.Add(new CartItemResponse
            {
                Id = ci.Id,
                ProductId = ci.ProductId,
                ProductName = product.Name,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                Image = product.ThumbnailImage ?? ci.Image,
                Quantity = ci.Quantity,
                SellerId = product.SellerId,
                StockQuantity = product.StockQuantity,
                SavedForLater = ci.SavedForLater
            });

            subtotal += originalTotal;
        }

        if (validItems.Count == 0)
            return BadRequest(new { message = "No valid products in cart." });

        // Delivery
        var method = deliveryMethod ?? "standard";
        var shippingCharge = subtotal >= FREE_SHIPPING_THRESHOLD ? 0 :
            method == "express" ? EXPRESS_SHIPPING : STANDARD_SHIPPING;

        // Coupon
        decimal couponDiscount = 0;
        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var coupon = await _mongoDb.Coupons
                .Find(c => c.Code == couponCode.ToUpper().Trim() && c.IsActive)
                .FirstOrDefaultAsync();

            if (coupon != null && (!coupon.ExpiresAt.HasValue || coupon.ExpiresAt.Value > DateTime.UtcNow) && coupon.UsageCount < coupon.MaxUsage && subtotal >= coupon.MinOrder)
            {
                couponDiscount = coupon.DiscountType switch
                {
                    "percent" => Math.Round(subtotal * coupon.DiscountValue / 100),
                    "fixed" => coupon.DiscountValue,
                    "shipping" => shippingCharge,
                    _ => 0
                };
                if (coupon.DiscountType != "shipping")
                    couponDiscount = Math.Min(couponDiscount, subtotal);
            }
        }

        var tax = Math.Round(subtotal * TAX_RATE);
        var grandTotal = subtotal - discount - couponDiscount + shippingCharge + tax;

        // Address
        AddressResponse? addressResp = null;
        if (!string.IsNullOrEmpty(addressId))
        {
            var addr = await _mongoDb.Addresses.Find(a => a.Id == addressId && a.UserId == userId).FirstOrDefaultAsync();
            if (addr != null) addressResp = MapAddress(addr);
        }

        return Ok(new CheckoutSummary
        {
            Items = validItems,
            ShippingAddress = addressResp,
            Subtotal = subtotal,
            Discount = discount,
            CouponDiscount = couponDiscount,
            CouponCode = couponCode?.ToUpper().Trim(),
            ShippingCharge = shippingCharge,
            Tax = tax,
            GrandTotal = grandTotal,
            DeliveryMethod = method,
            PaymentMethod = "cod"
        });
    }

    [HttpPost("place")]
    public async Task<IActionResult> PlaceOrder([FromBody] CheckoutRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        // Validate address
        var address = await _mongoDb.Addresses
            .Find(a => a.Id == request.ShippingAddressId && a.UserId == userId)
            .FirstOrDefaultAsync();
        if (address == null)
            return BadRequest(new { message = "Invalid shipping address." });

        // Get cart items
        var cartItems = await _mongoDb.CartItems
            .Find(c => c.UserId == userId && !c.SavedForLater)
            .ToListAsync();
        if (cartItems.Count == 0)
            return BadRequest(new { message = "Cart is empty." });

        // Validate each product and reserve stock
        var orderItems = new List<OrderItem>();
        decimal subtotal = 0;
        decimal discount = 0;
        string sellerId = string.Empty;
        string sellerName = string.Empty;

        foreach (var ci in cartItems)
        {
            var product = await _mongoDb.Products
                .Find(p => p.Id == ci.ProductId && p.Status == "published")
                .FirstOrDefaultAsync();
            if (product == null)
                return BadRequest(new { message = $"Product '{ci.ProductName}' is no longer available." });

            if (product.StockQuantity < ci.Quantity)
                return BadRequest(new { message = $"Insufficient stock for '{product.Name}'. Only {product.StockQuantity} available." });

            var effectivePrice = product.DiscountPrice ?? product.Price;
            var originalTotal = product.Price * ci.Quantity;
            var effectiveTotal = effectivePrice * ci.Quantity;
            discount += originalTotal - effectiveTotal;
            subtotal += originalTotal;

            sellerId = product.SellerId;
            var sellerProfile = await _mongoDb.SellerProfiles.Find(s => s.UserId == product.SellerId).FirstOrDefaultAsync();
            sellerName = sellerProfile?.ShopName ?? "MR Shop Seller";

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Sku = product.Sku,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                Quantity = ci.Quantity,
                Image = product.ThumbnailImage,
                SellerId = product.SellerId
            });

            // Reserve stock: decrease available, increase reserved
            await _mongoDb.Products.UpdateOneAsync(
                p => p.Id == ci.ProductId,
                Builders<Product>.Update
                    .Inc(p => p.StockQuantity, -ci.Quantity)
                    .Inc(p => p.ReservedStock, ci.Quantity)
            );

            // Log inventory
            await _mongoDb.InventoryLogs.InsertOneAsync(new InventoryLog
            {
                ProductId = product.Id,
                ProductName = product.Name,
                SellerId = product.SellerId,
                Action = "reserved",
                Quantity = ci.Quantity,
                PreviousStock = product.StockQuantity,
                NewStock = product.StockQuantity - ci.Quantity,
                Reason = "Order placed - stock reserved",
                PerformedBy = userId,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Delivery
        var shippingCharge = subtotal >= FREE_SHIPPING_THRESHOLD ? 0 :
            request.DeliveryMethod == "express" ? EXPRESS_SHIPPING : STANDARD_SHIPPING;

        // Coupon
        decimal couponDiscount = 0;
        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var coupon = await _mongoDb.Coupons
                .Find(c => c.Code == request.CouponCode.ToUpper().Trim() && c.IsActive)
                .FirstOrDefaultAsync();
            if (coupon != null && (!coupon.ExpiresAt.HasValue || coupon.ExpiresAt.Value > DateTime.UtcNow) && coupon.UsageCount < coupon.MaxUsage && subtotal >= coupon.MinOrder)
            {
                couponDiscount = coupon.DiscountType switch
                {
                    "percent" => Math.Round(subtotal * coupon.DiscountValue / 100),
                    "fixed" => coupon.DiscountValue,
                    "shipping" => shippingCharge,
                    _ => 0
                };
                if (coupon.DiscountType != "shipping")
                    couponDiscount = Math.Min(couponDiscount, subtotal);

                // Increment usage
                await _mongoDb.Coupons.UpdateOneAsync(
                    c => c.Id == coupon.Id,
                    Builders<Coupon>.Update.Inc(c => c.UsageCount, 1)
                );
            }
        }

        var tax = Math.Round(subtotal * TAX_RATE);
        var grandTotal = subtotal - discount - couponDiscount + shippingCharge + tax;

        // Generate order number
        var orderCount = await _mongoDb.Orders.CountDocumentsAsync(_ => true);
        var orderNumber = $"MR-{DateTime.UtcNow:yyyyMMdd}-{(orderCount + 1).ToString("D5")}";

        var user = await _mongoDb.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();

        var order = new Order
        {
            OrderNumber = orderNumber,
            CustomerId = userId,
            CustomerName = user?.Name ?? "Customer",
            CustomerEmail = user?.Email,
            CustomerPhone = user?.Phone,
            SellerId = sellerId,
            SellerName = sellerName,
            Items = orderItems,
            ShippingAddress = new OrderAddress
            {
                FullName = address.FullName,
                Phone = address.Phone,
                Email = address.Email,
                Country = address.Country,
                Division = address.Division,
                District = address.District,
                City = address.City,
                Area = address.Area,
                PostalCode = address.PostalCode,
                AddressLine = address.AddressLine
            },
            PaymentMethod = request.PaymentMethod ?? "cod",
            PaymentStatus = request.PaymentMethod == "cod" ? "pending" : "pending",
            Status = "pending",
            DeliveryMethod = request.DeliveryMethod ?? "standard",
            Subtotal = subtotal,
            Discount = discount,
            CouponCode = request.CouponCode?.ToUpper().Trim(),
            CouponDiscount = couponDiscount,
            ShippingCharge = shippingCharge,
            Tax = tax,
            GrandTotal = grandTotal,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _mongoDb.Orders.InsertOneAsync(order);

        // Create timeline
        await _mongoDb.OrderTimelines.InsertOneAsync(new OrderTimeline
        {
            OrderId = order.Id,
            Status = "pending",
            UpdatedBy = userId,
            UpdatedAt = DateTime.UtcNow,
            Remarks = "Order placed successfully"
        });

        // Clear purchased cart items
        await _mongoDb.CartItems.DeleteManyAsync(c => c.UserId == userId && !c.SavedForLater);

        // Generate invoice
        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{(orderCount + 1).ToString("D5")}";
        var invoice = new Invoice
        {
            OrderId = order.Id,
            InvoiceNumber = invoiceNumber,
            CustomerId = userId,
            CustomerName = user?.Name ?? "Customer",
            CustomerEmail = user?.Email,
            SellerId = sellerId,
            SellerName = sellerName,
            Items = orderItems.Select(i => new InvoiceItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Sku = i.Sku,
                Price = i.Price,
                Quantity = i.Quantity,
                Total = (i.DiscountPrice ?? i.Price) * i.Quantity,
                Image = i.Image
            }).ToList(),
            Subtotal = subtotal,
            Discount = discount,
            CouponDiscount = couponDiscount,
            ShippingCharge = shippingCharge,
            Tax = tax,
            GrandTotal = grandTotal,
            ShippingAddress = $"{address.AddressLine}{(string.IsNullOrEmpty(address.Area) ? "" : ", " + address.Area)}{(string.IsNullOrEmpty(address.City) ? "" : ", " + address.City)}{(string.IsNullOrEmpty(address.District) ? "" : ", " + address.District)}{(string.IsNullOrEmpty(address.Division) ? "" : ", " + address.Division)}{(string.IsNullOrEmpty(address.PostalCode) ? "" : ", " + address.PostalCode)}, {address.Country}",
            PaymentMethod = request.PaymentMethod ?? "cod",
            PaymentStatus = "pending",
            CreatedAt = DateTime.UtcNow
        };
        await _mongoDb.Invoices.InsertOneAsync(invoice);

        return Ok(new { message = "Order placed successfully.", orderId = order.Id, orderNumber, invoiceNumber });
    }

    private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    private static AddressResponse MapAddress(Address a) => new()
    {
        Id = a.Id,
        FullName = a.FullName,
        Phone = a.Phone,
        Email = a.Email,
        Country = a.Country,
        Division = a.Division,
        District = a.District,
        City = a.City,
        Area = a.Area,
        PostalCode = a.PostalCode,
        AddressLine = a.AddressLine,
        AddressType = a.AddressType,
        IsDefault = a.IsDefault
    };
}
