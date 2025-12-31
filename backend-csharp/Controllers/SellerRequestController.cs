using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MRShop.OrderTracking.Models;
using MRShop.Services;

namespace MRShop.Controllers
{
    /// <summary>
    /// Production-level Seller Request Controller
    /// Handles seller registration with secure storage, audit trail, and admin notifications
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SellerRequestController : ControllerBase
    {
        private readonly ISellerRequestService _sellerRequestService;
        private readonly ILogger<SellerRequestController> _logger;

        public SellerRequestController(
            ISellerRequestService sellerRequestService,
            ILogger<SellerRequestController> logger)
        {
            _sellerRequestService = sellerRequestService;
            _logger = logger;
        }

        /// <summary>
        /// Submit seller registration request
        /// Creates user folder, saves JSON, generates admin notification
        /// </summary>
        /// <param name="request">Seller registration data</param>
        /// <returns>API response with request details and ID</returns>
        [HttpPost("submit")]
        [ProducesResponseType(typeof(ApiResponse<SellerRequest>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SubmitRequest([FromBody] SellerRegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Error = "Invalid request data",
                    Message = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))
                });
            }

            try
            {
                var ipAddress = GetClientIpAddress();
                var userAgent = Request.Headers["User-Agent"].ToString();

                var result = await _sellerRequestService.SubmitSellerRequestAsync(
                    request,
                    ipAddress,
                    userAgent);

                if (result.Success && result.Data != null)
                {
                    _logger.LogInformation($"📝 New seller request: {result.Data.RequestId} from {result.Data.Email}");
                    return CreatedAtAction(nameof(GetRequest), new { id = result.Data.RequestId }, result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error submitting seller request: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to submit request",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get seller request by ID
        /// </summary>
        /// <param name="id">Request ID</param>
        /// <returns>Request details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<SellerRequest>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRequest(string id)
        {
            try
            {
                var request = await _sellerRequestService.GetRequestByIdAsync(id);
                if (request == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Request not found"
                    });
                }

                return Ok(new ApiResponse<SellerRequest>
                {
                    Success = true,
                    Message = "Request retrieved successfully",
                    Data = request
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error retrieving request: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get all pending requests (for admin)
        /// </summary>
        /// <returns>List of pending requests</returns>
        [HttpGet("status/pending")]
        [ProducesResponseType(typeof(ApiResponse<List<SellerRequest>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPendingRequests()
        {
            try
            {
                var requests = await _sellerRequestService.GetAllPendingRequestsAsync();
                return Ok(new ApiResponse<List<SellerRequest>>
                {
                    Success = true,
                    Message = $"Found {requests.Count} pending requests",
                    Data = requests
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error retrieving pending requests: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get requests by status
        /// </summary>
        /// <param name="status">Status filter (Pending, Approved, Rejected, Under Review)</param>
        /// <returns>Filtered requests</returns>
        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(ApiResponse<List<SellerRequest>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRequestsByStatus(string status)
        {
            try
            {
                var requests = await _sellerRequestService.GetRequestsByStatusAsync(status);
                return Ok(new ApiResponse<List<SellerRequest>>
                {
                    Success = true,
                    Message = $"Found {requests.Count} requests with status: {status}",
                    Data = requests
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error retrieving requests by status: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Update request status (admin action)
        /// </summary>
        /// <param name="id">Request ID</param>
        /// <param name="statusUpdate">New status and optional notes</param>
        /// <returns>Updated request</returns>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] StatusUpdateRequest statusUpdate)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(statusUpdate.Status))
                    return BadRequest(new ApiResponse<object> { Success = false, Message = "Status is required" });

                var result = await _sellerRequestService.UpdateRequestStatusAsync(
                    id,
                    statusUpdate.Status,
                    statusUpdate.AdminNotes);

                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Request not found"
                    });
                }

                var updatedRequest = await _sellerRequestService.GetRequestByIdAsync(id);

                _logger.LogInformation($"✅ Request {id} status updated to: {statusUpdate.Status}");

                return Ok(new ApiResponse<SellerRequest>
                {
                    Success = true,
                    Message = "Status updated successfully",
                    Data = updatedRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error updating request status: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get admin notifications for unacknowledged requests
        /// </summary>
        /// <returns>List of pending notifications</returns>
        [HttpGet("admin/notifications")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminRequestNotification>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAdminNotifications()
        {
            try
            {
                var notifications = await _sellerRequestService.GetAdminNotificationsAsync();
                return Ok(new ApiResponse<List<AdminRequestNotification>>
                {
                    Success = true,
                    Message = $"Found {notifications.Count} unacknowledged notifications",
                    Data = notifications
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error retrieving admin notifications: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Acknowledge admin notification
        /// </summary>
        /// <param name="id">Request ID</param>
        /// <returns>Acknowledgment status</returns>
        [HttpPost("{id}/acknowledge")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AcknowledgeNotification(string id)
        {
            try
            {
                var result = await _sellerRequestService.AcknowledgeAdminNotificationAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Notification not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Notification acknowledged"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error acknowledging notification: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get statistics
        /// </summary>
        /// <returns>Request statistics</returns>
        [HttpGet("admin/statistics")]
        [ProducesResponseType(typeof(ApiResponse<RequestStats>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var stats = await _sellerRequestService.GetRequestStatisticsAsync();
                return Ok(new ApiResponse<RequestStats>
                {
                    Success = true,
                    Message = "Statistics retrieved successfully",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error retrieving statistics: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Export request data
        /// </summary>
        /// <param name="id">Request ID</param>
        /// <param name="format">Export format (default: json)</param>
        /// <returns>Export status</returns>
        [HttpGet("{id}/export")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportRequest(string id, [FromQuery] string format = "json")
        {
            try
            {
                var result = await _sellerRequestService.ExportRequestAsync(id, format);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Request not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Request exported successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error exporting request: {ex.Message}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        // ============================================
        // HELPER METHODS
        // ============================================

        private string GetClientIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
            }
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }

    /// <summary>
    /// Status update request model
    /// </summary>
    public class StatusUpdateRequest
    {
        public string? Status { get; set; }
        public string? AdminNotes { get; set; }
    }
}
