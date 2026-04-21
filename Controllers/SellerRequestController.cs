using Microsoft.AspNetCore.Mvc;
using MRShop.SellerRequests;

namespace MRShop.OrderTracking.Controllers
{
    [ApiController]
    [Route("api/sellerrequest")]
    public class SellerRequestController : ControllerBase
    {
        private readonly SellerRequestService _sellerRequestService;

        public SellerRequestController(SellerRequestService sellerRequestService)
        {
            _sellerRequestService = sellerRequestService;
        }

        [HttpGet("admin/notifications")]
        public async Task<IActionResult> GetAdminNotifications()
        {
            var notifications = await _sellerRequestService.GetAdminNotificationsAsync();
            return Ok(new
            {
                success = true,
                message = notifications.Count > 0
                    ? $"Found {notifications.Count} seller request notification(s)"
                    : "No seller request notifications found",
                data = notifications
            });
        }

        [HttpPut("admin/{requestId}/status")]
        public async Task<IActionResult> UpdateStatus(string requestId, [FromBody] SellerRequestStatusUpdate update)
        {
            if (string.IsNullOrWhiteSpace(requestId))
            {
                return BadRequest(new { success = false, message = "Request ID is required" });
            }

            if (update == null || string.IsNullOrWhiteSpace(update.Status))
            {
                return BadRequest(new { success = false, message = "Status is required" });
            }

            var updated = await _sellerRequestService.UpdateRequestStatusAsync(requestId, update.Status, update.Notes);
            if (!updated)
            {
                return NotFound(new { success = false, message = $"Seller request '{requestId}' not found" });
            }

            var request = await _sellerRequestService.GetRequestByIdAsync(requestId);
            return Ok(new
            {
                success = true,
                message = $"Seller request '{requestId}' updated to {update.Status}",
                data = request
            });
        }

        [HttpPost("admin/acknowledge/{requestId}")]
        public async Task<IActionResult> Acknowledge(string requestId)
        {
            var acknowledged = await _sellerRequestService.AcknowledgeAdminNotificationAsync(requestId);
            if (!acknowledged)
            {
                return NotFound(new { success = false, message = $"Seller request '{requestId}' not found" });
            }

            return Ok(new
            {
                success = true,
                message = $"Seller request '{requestId}' acknowledged"
            });
        }
    }
}