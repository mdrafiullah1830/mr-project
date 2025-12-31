using Microsoft.AspNetCore.Mvc;
using MRShop.OrderTracking.Models;
using MRShop.OrderTracking.Services;

namespace MRShop.OrderTracking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Get all orders for a specific user
        /// </summary>
        /// <param name="userId">User ID (default: 1)</param>
        /// <returns>List of orders</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<OrderListResponse>), 200)]
        public async Task<IActionResult> GetOrders([FromQuery] int userId = 1)
        {
            try
            {
                var orders = await _orderService.GetUserOrdersAsync(userId);

                return Ok(new ApiResponse<OrderListResponse>
                {
                    Success = true,
                    Data = new OrderListResponse
                    {
                        Count = orders.Count,
                        Orders = orders
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders for user {UserId}", userId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get detailed information for a specific order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Order details</returns>
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(ApiResponse<Order>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetOrderDetails(string orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);

                if (order == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Error = "Order not found"
                    });
                }

                return Ok(new ApiResponse<Order>
                {
                    Success = true,
                    Data = order
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {OrderId}", orderId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get the delivery timeline for an order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Order timeline</returns>
        [HttpGet("{orderId}/timeline")]
        [ProducesResponseType(typeof(ApiResponse<OrderTimelineResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetOrderTimeline(string orderId)
        {
            try
            {
                var timeline = await _orderService.GetOrderTimelineAsync(orderId);

                if (timeline == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Error = "Order not found"
                    });
                }

                return Ok(new ApiResponse<OrderTimelineResponse>
                {
                    Success = true,
                    Data = timeline
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting timeline for order {OrderId}", orderId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Create a new order (for checkout flow integration)
        /// </summary>
        /// <param name="request">Order creation request</param>
        /// <returns>Created order details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Order>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                if (request.Items == null || !request.Items.Any())
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Error = "Order must contain at least one item"
                    });
                }

                if (string.IsNullOrEmpty(request.PaymentMethod))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Error = "Payment method is required"
                    });
                }

                var order = await _orderService.CreateOrderAsync(request);

                return CreatedAtAction(
                    nameof(GetOrderDetails),
                    new { orderId = order.Id },
                    new ApiResponse<object>
                    {
                        Success = true,
                        Data = new
                        {
                            order.Id,
                            OrderId = order.Id,
                            order.TrackingId,
                            Message = "Order created successfully"
                        }
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Update order status (admin only)
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="request">Status update request</param>
        /// <returns>Updated order</returns>
        [HttpPut("{orderId}/status")]
        [ProducesResponseType(typeof(ApiResponse<Order>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateOrderStatus(
            string orderId,
            [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Status))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Error = "Status is required"
                    });
                }

                var order = await _orderService.UpdateOrderStatusAsync(orderId, request.Status);

                if (order == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Error = "Order not found"
                    });
                }

                return Ok(new ApiResponse<Order>
                {
                    Success = true,
                    Message = $"Order status updated to {request.Status}",
                    Data = order
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId} status", orderId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }
    }
}
