using Microsoft.AspNetCore.Mvc;
using MRShop.OrderTracking.Models;
using MRShop.OrderTracking.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRShop.OrderTracking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SellerRegistrationController : ControllerBase
    {
        private readonly ISellerRegistrationService _sellerService;
        private readonly ILogger<SellerRegistrationController> _logger;

        public SellerRegistrationController(
            ISellerRegistrationService sellerService,
            ILogger<SellerRegistrationController> logger)
        {
            _sellerService = sellerService;
            _logger = logger;
        }

        /// <summary>
        /// Submit a new seller registration
        /// </summary>
        /// <param name="request">Seller registration details</param>
        /// <returns>Registration confirmation with ID</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<SellerRegistration>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<IActionResult> RegisterSeller([FromBody] SellerRegistrationRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Error = "Request body is empty"
                    });
                }

                // Get client IP address
                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                
                // Get user agent
                string userAgent = Request.Headers["User-Agent"].ToString() ?? "Unknown";

                // Register seller
                var registration = await _sellerService.RegisterSellerAsync(request, ipAddress, userAgent);

                if (registration == null)
                    return BadRequest(new ApiResponse<object> { Success = false, Message = "Failed to register seller" });

                _logger.LogInformation("Seller registered successfully: {RegistrationId}", registration.Id);

                return CreatedAtAction(
                    nameof(GetRegistration),
                    new { registrationId = registration.Id },
                    new ApiResponse<SellerRegistration>
                    {
                        Success = true,
                        Message = "Seller registration submitted successfully",
                        Data = registration
                    }
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error in seller registration");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Error = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering seller");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = "An error occurred while processing your registration. Please try again."
                });
            }
        }

        /// <summary>
        /// Get a specific registration by ID
        /// </summary>
        [HttpGet("{registrationId}")]
        [ProducesResponseType(typeof(ApiResponse<SellerRegistration>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetRegistration(string registrationId)
        {
            try
            {
                var registration = await _sellerService.GetRegistrationByIdAsync(registrationId);

                if (registration == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Error = "Registration not found"
                    });
                }

                return Ok(new ApiResponse<SellerRegistration>
                {
                    Success = true,
                    Data = registration
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving registration {RegistrationId}", registrationId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get all seller registrations (admin endpoint)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<SellerRegistration>>), 200)]
        public async Task<IActionResult> GetAllRegistrations()
        {
            try
            {
                var registrations = await _sellerService.GetAllRegistrationsAsync();

                return Ok(new ApiResponse<List<SellerRegistration>>
                {
                    Success = true,
                    Message = $"Found {registrations.Count} seller registrations",
                    Data = registrations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all registrations");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get pending seller registrations (admin endpoint)
        /// </summary>
        [HttpGet("pending")]
        [ProducesResponseType(typeof(ApiResponse<List<SellerRegistration>>), 200)]
        public async Task<IActionResult> GetPendingRegistrations()
        {
            try
            {
                var registrations = await _sellerService.GetPendingRegistrationsAsync();

                return Ok(new ApiResponse<List<SellerRegistration>>
                {
                    Success = true,
                    Message = $"Found {registrations.Count} pending seller registrations",
                    Data = registrations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending registrations");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get registration statistics (admin endpoint)
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(ApiResponse<SellerRegistrationStats>), 200)]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _sellerService.GetRegistrationStatsAsync();

                return Ok(new ApiResponse<SellerRegistrationStats>
                {
                    Success = true,
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving registration statistics");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Update registration status (admin endpoint)
        /// </summary>
        [HttpPut("{registrationId}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> UpdateRegistrationStatus(
            string registrationId,
            [FromBody] UpdateStatusRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.Status))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Error = "Status is required"
                    });
                }

                var validStatuses = new[] { "Pending", "Approved", "Rejected" };
                if (!Array.Exists(validStatuses, s => s == request.Status))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Error = "Invalid status. Must be: Pending, Approved, or Rejected"
                    });
                }

                bool updated = await _sellerService.UpdateRegistrationStatusAsync(registrationId, request.Status);

                if (!updated)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Error = "Registration not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Registration status updated to {request.Status}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating registration status");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Request model for updating registration status
    /// </summary>
    public class UpdateStatusRequest
    {
        public string? Status { get; set; }
    }
}
