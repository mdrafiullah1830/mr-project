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
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly MongoDbService _mongoDb;

    public InvoicesController(MongoDbService mongoDb)
    {
        _mongoDb = mongoDb;
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetInvoiceByOrder(string orderId)
    {
        var userId = GetUserId();
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (userId == null) return Unauthorized();

        // Filter invoice by user ownership upfront
        var invoice = role == "admin"
            ? await _mongoDb.Invoices.Find(i => i.OrderId == orderId).FirstOrDefaultAsync()
            : await _mongoDb.Invoices.Find(i => i.OrderId == orderId && (i.CustomerId == userId || i.SellerId == userId)).FirstOrDefaultAsync();

        if (invoice == null) return NotFound(new { message = "Invoice not found." });

        var order = await _mongoDb.Orders.Find(o => o.Id == orderId).FirstOrDefaultAsync();

        return Ok(new InvoiceResponse
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            OrderId = invoice.OrderId,
            OrderNumber = order?.OrderNumber ?? "",
            CustomerName = invoice.CustomerName,
            CustomerEmail = invoice.CustomerEmail,
            SellerName = invoice.SellerName,
            Items = invoice.Items.Select(i => new InvoiceItemResponse
            {
                ProductName = i.ProductName,
                Sku = i.Sku,
                Price = i.Price,
                Quantity = i.Quantity,
                Total = i.Total,
                Image = i.Image
            }).ToList(),
            Subtotal = invoice.Subtotal,
            Discount = invoice.Discount,
            CouponDiscount = invoice.CouponDiscount,
            ShippingCharge = invoice.ShippingCharge,
            Tax = invoice.Tax,
            GrandTotal = invoice.GrandTotal,
            ShippingAddress = invoice.ShippingAddress,
            PaymentMethod = invoice.PaymentMethod,
            PaymentStatus = invoice.PaymentStatus,
            CreatedAt = invoice.CreatedAt
        });
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyInvoices([FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 50);

        var filter = Builders<Invoice>.Filter.Eq(i => i.CustomerId, userId);
        var total = await _mongoDb.Invoices.CountDocumentsAsync(filter);
        var invoices = await _mongoDb.Invoices.Find(filter)
            .SortByDescending(i => i.CreatedAt)
            .Skip((page - 1) * limit)
            .Limit(limit)
            .ToListAsync();

        return Ok(new
        {
            invoices = invoices.Select(i => new
            {
                id = i.Id,
                invoiceNumber = i.InvoiceNumber,
                orderId = i.OrderId,
                grandTotal = i.GrandTotal,
                paymentStatus = i.PaymentStatus,
                createdAt = i.CreatedAt
            }),
            totalCount = total,
            page,
            totalPages = (int)Math.Ceiling((double)total / limit)
        });
    }

    private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}
