using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MRShop.OrderTracking.Models;

namespace MRShop.Services
{
    /// <summary>
    /// Production-level Seller Request Service with thread-safe operations,
    /// secure folder structures, and real-time admin notifications
    /// </summary>
    public interface ISellerRequestService
    {
        Task<ApiResponse<SellerRequest>> SubmitSellerRequestAsync(
            SellerRegistrationRequest request,
            string ipAddress,
            string userAgent,
            string? userId = null);

        Task<SellerRequest?> GetRequestByIdAsync(string requestId);
        Task<SellerRequest?> GetRequestByUserIdAsync(string userId);
        Task<List<SellerRequest>> GetAllPendingRequestsAsync();
        Task<List<SellerRequest>> GetRequestsByStatusAsync(string status);
        Task<bool> UpdateRequestStatusAsync(string requestId, string newStatus, string? adminNotes = null);
        Task<List<AdminRequestNotification>> GetAdminNotificationsAsync();
        Task<bool> AcknowledgeAdminNotificationAsync(string requestId);
        Task<RequestStats> GetRequestStatisticsAsync();
        Task<bool> ExportRequestAsync(string requestId, string exportFormat = "json");
    }

    public class SellerRequestService : ISellerRequestService
    {
        private readonly ILogger<SellerRequestService> _logger;
        private readonly string _userRequestsBasePath;
        private readonly string _adminRequestsPath;
        private readonly string _archivePath;
        private static readonly object _lockObject = new object();
        private const int MAX_RETRIES = 3;
        private const int RETRY_DELAY_MS = 100;

        public SellerRequestService(ILogger<SellerRequestService> logger)
        {
            _logger = logger;
            _userRequestsBasePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "data", "seller_requests", "users");
            _adminRequestsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "data", "seller_requests", "admin_pending");
            _archivePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "data", "seller_requests", "archive");

            InitializeFolderStructure();
        }

        /// <summary>
        /// Initialize required folder structure for secure storage
        /// </summary>
        private void InitializeFolderStructure()
        {
            try
            {
                Directory.CreateDirectory(_userRequestsBasePath);
                Directory.CreateDirectory(_adminRequestsPath);
                Directory.CreateDirectory(_archivePath);
                
                if (_logger != null)
                {
                    _logger.LogInformation("✅ Seller Request folder structure initialized");
                }
            }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    _logger.LogError($"❌ Failed to initialize folder structure: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Submit seller registration request with full audit trail
        /// </summary>
        public async Task<ApiResponse<SellerRequest>> SubmitSellerRequestAsync(
            SellerRegistrationRequest request,
            string ipAddress,
            string userAgent,
            string? userId = null)
        {
            try
            {
                // Validate request
                var validationResult = ValidateSellerRequest(request);
                if (!validationResult.IsValid)
                {
                    return new ApiResponse<SellerRequest>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Error = string.Join(", ", validationResult.Errors)
                    };
                }

                // Generate unique IDs and paths
                userId = userId ?? GenerateUserId();
                var requestId = GenerateUniqueRequestId();
                var userFolderPath = Path.Combine(_userRequestsBasePath, userId);
                var requestFilePath = Path.Combine(userFolderPath, "seller_request.json");

                // Create user-specific folder (thread-safe)
                lock (_lockObject)
                {
                    if (!Directory.Exists(userFolderPath))
                    {
                        Directory.CreateDirectory(userFolderPath);
                        _logger.LogInformation($"📁 Created user folder: {userFolderPath}");
                    }
                }

                // Create seller request object with complete audit trail
                var sellerRequest = new SellerRequest
                {
                    RequestId = requestId,
                    UserId = userId,
                    SubmittedAt = DateTime.UtcNow,
                    Status = "Pending",
                    FullName = request.FullName,
                    Phone = request.Phone,
                    Email = request.Email,
                    ShopName = request.ShopName,
                    PaymentMethod = request.PaymentMethod,
                    BankName = request.BankName,
                    AccountNumber = request.AccountNumber,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Categories = request.Categories ?? new List<string>(),
                    DocumentType = request.DocumentType,
                    AdditionalInfo = request.AdditionalInfo,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    UserFolderPath = userFolderPath,
                    RequestFilePath = requestFilePath,
                    LastModified = DateTime.UtcNow,
                    ModifiedCount = 0
                };

                // Save request to user's folder (thread-safe with retry logic)
                await SaveRequestToFileAsync(sellerRequest, requestFilePath);

                // Create admin notification in AdminRequests folder
                await CreateAdminNotificationAsync(sellerRequest);

                _logger.LogInformation($"✅ Seller request submitted: {requestId} by user: {userId}");

                return new ApiResponse<SellerRequest>
                {
                    Success = true,
                    Message = "Seller registration submitted successfully",
                    Data = sellerRequest
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error submitting seller request: {ex.Message}");
                return new ApiResponse<SellerRequest>
                {
                    Success = false,
                    Message = "Failed to submit registration",
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// Save request to file with thread-safe locking and retry logic
        /// </summary>
        private async Task SaveRequestToFileAsync(SellerRequest request, string filePath)
        {
            int retryCount = 0;
            while (retryCount < MAX_RETRIES)
            {
                try
                {
                    lock (_lockObject)
                    {
                        var options = new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            PropertyNameCaseInsensitive = false
                        };

                        var json = JsonSerializer.Serialize(request, options);
                        File.WriteAllText(filePath, json);
                    }

                    _logger.LogInformation($"💾 Seller request saved to: {filePath}");
                    return;
                }
                catch (IOException ex) when (retryCount < MAX_RETRIES - 1)
                {
                    retryCount++;
                    _logger.LogWarning($"⚠️ Retry {retryCount}/{MAX_RETRIES} - File locked: {ex.Message}");
                    await Task.Delay(RETRY_DELAY_MS);
                }
            }

            throw new IOException($"Failed to save request after {MAX_RETRIES} retries");
        }

        /// <summary>
        /// Create admin notification entry for pending review
        /// </summary>
                private async Task CreateAdminNotificationAsync(SellerRequest request)
        {
            try
            {
                var notification = new AdminRequestNotification
                {
                    RequestId = request.RequestId,
                    UserId = request.UserId,
                    FullName = request.FullName,
                    Email = request.Email,
                    ShopName = request.ShopName,
                    SubmittedAt = request.SubmittedAt,
                    Status = "Pending",
                    Categories = request.Categories,
                    Location = new LatLng { Lat = request.Latitude, Lng = request.Longitude },
                    NotificationSentAt = DateTime.UtcNow,
                    Acknowledged = false
                };

                var notificationPath = Path.Combine(_adminRequestsPath, $"{request.RequestId}_notification.json");
                
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(notification, options);
                
                // Use async file write instead of synchronous write inside lock
                await File.WriteAllTextAsync(notificationPath, json);

                _logger.LogInformation($"🔔 Admin notification created: {notificationPath}");
                // Update request with notification flag
                request.AdminNotificationSent = true;
                request.AdminNotificationSentAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Failed to create admin notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieve request by ID
        /// </summary>
        public async Task<SellerRequest?> GetRequestByIdAsync(string requestId)
        {
            try
            {
                var files = Directory.GetFiles(_userRequestsBasePath, "seller_request.json", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    lock (_lockObject)
                    {
                        var json = File.ReadAllText(file);
                        var request = JsonSerializer.Deserialize<SellerRequest>(json);
                        if (request?.RequestId == requestId)
                        {
                            return request;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error retrieving request {requestId}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Retrieve all requests by user ID
        /// </summary>
        public async Task<SellerRequest?> GetRequestByUserIdAsync(string userId)
        {
            try
            {
                var userFolderPath = Path.Combine(_userRequestsBasePath, userId);
                var requestFile = Path.Combine(userFolderPath, "seller_request.json");

                if (!File.Exists(requestFile))
                    return null;

                lock (_lockObject)
                {
                    var json = File.ReadAllText(requestFile);
                    return JsonSerializer.Deserialize<SellerRequest>(json);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error retrieving request for user {userId}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get all pending requests
        /// </summary>
        public async Task<List<SellerRequest>> GetAllPendingRequestsAsync()
        {
            return await GetRequestsByStatusAsync("Pending");
        }

        /// <summary>
        /// Get requests filtered by status
        /// </summary>
        public async Task<List<SellerRequest>> GetRequestsByStatusAsync(string status)
        {
            var requests = new List<SellerRequest>();
            try
            {
                var files = Directory.GetFiles(_userRequestsBasePath, "seller_request.json", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    lock (_lockObject)
                    {
                        var json = File.ReadAllText(file);
                        var request = JsonSerializer.Deserialize<SellerRequest>(json);
                        if (request?.Status == status)
                        {
                            requests.Add(request);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error retrieving requests: {ex.Message}");
            }

            return requests.OrderByDescending(r => r.SubmittedAt).ToList();
        }

        /// <summary>
        /// Update request status
        /// </summary>
        public async Task<bool> UpdateRequestStatusAsync(string requestId, string newStatus, string? adminNotes = null)
        {
            try
            {
                var request = await GetRequestByIdAsync(requestId);
                if (request == null)
                    return false;

                request.Status = newStatus;
                request.AdminNotes = adminNotes;
                request.ReviewedAt = DateTime.UtcNow;
                request.LastModified = DateTime.UtcNow;
                request.ModifiedCount++;

                if (!string.IsNullOrWhiteSpace(request.RequestFilePath))
                {
                    await SaveRequestToFileAsync(request, request.RequestFilePath);
                }
                _logger.LogInformation($"✅ Request {requestId} status updated to: {newStatus}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error updating request status: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get admin notifications
        /// </summary>
        public async Task<List<AdminRequestNotification>> GetAdminNotificationsAsync()
        {
            var notifications = new List<AdminRequestNotification>();
            try
            {
                var files = Directory.GetFiles(_adminRequestsPath, "*_notification.json");
                foreach (var file in files)
                {
                    lock (_lockObject)
                    {
                        var json = File.ReadAllText(file);
                        var notification = JsonSerializer.Deserialize<AdminRequestNotification>(json);
                        if (notification != null && !notification.Acknowledged)
                        {
                            notifications.Add(notification);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error retrieving admin notifications: {ex.Message}");
            }

            return notifications.OrderByDescending(n => n.NotificationSentAt).ToList();
        }

        /// <summary>
        /// Acknowledge admin notification
        /// </summary>
        public async Task<bool> AcknowledgeAdminNotificationAsync(string requestId)
        {
            try
            {
                var notificationPath = Path.Combine(_adminRequestsPath, $"{requestId}_notification.json");
                if (!File.Exists(notificationPath))
                    return false;

                lock (_lockObject)
                {
                    var json = File.ReadAllText(notificationPath);
                    var notification = JsonSerializer.Deserialize<AdminRequestNotification>(json);
                    if (notification != null)
                    {
                        notification.Acknowledged = true;
                        notification.AcknowledgedAt = DateTime.UtcNow;

                        var options = new JsonSerializerOptions { WriteIndented = true };
                        var updatedJson = JsonSerializer.Serialize(notification, options);
                        File.WriteAllText(notificationPath, updatedJson);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error acknowledging notification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get request statistics
        /// </summary>
        public async Task<RequestStats> GetRequestStatisticsAsync()
        {
            try
            {
                var allRequests = new List<SellerRequest>();
                var files = Directory.GetFiles(_userRequestsBasePath, "seller_request.json", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    lock (_lockObject)
                    {
                        var json = File.ReadAllText(file);
                        var request = JsonSerializer.Deserialize<SellerRequest>(json);
                        if (request != null)
                        {
                            allRequests.Add(request);
                        }
                    }
                }

                return new RequestStats
                {
                    TotalRequests = allRequests.Count,
                    PendingRequests = allRequests.Count(r => r.Status == "Pending"),
                    ApprovedRequests = allRequests.Count(r => r.Status == "Approved"),
                    RejectedRequests = allRequests.Count(r => r.Status == "Rejected"),
                    UnderReviewRequests = allRequests.Count(r => r.Status == "Under Review"),
                    AverageDaysToReview = CalculateAverageDaysToReview(allRequests)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error calculating statistics: {ex.Message}");
                return new RequestStats();
            }
        }

        /// <summary>
        /// Export request data
        /// </summary>
        public async Task<bool> ExportRequestAsync(string requestId, string exportFormat = "json")
        {
            try
            {
                var request = await GetRequestByIdAsync(requestId);
                if (request == null)
                    return false;

                var exportPath = Path.Combine(_archivePath, $"{requestId}_export.{exportFormat}");

                lock (_lockObject)
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    var json = JsonSerializer.Serialize(request, options);
                    File.WriteAllText(exportPath, json);
                }

                _logger.LogInformation($"✅ Request exported to: {exportPath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error exporting request: {ex.Message}");
                return false;
            }
        }

        // ============================================
        // HELPER METHODS
        // ============================================

        private string GenerateUserId()
        {
            return $"USR_{DateTime.UtcNow.Ticks}_{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        private string GenerateUniqueRequestId()
        {
            return $"REQ_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString().Substring(0, 12).ToUpper()}";
        }

        private ValidationResult ValidateSellerRequest(SellerRegistrationRequest request)
        {
            var result = new ValidationResult { IsValid = true };

            if (string.IsNullOrWhiteSpace(request.FullName))
                result.Errors.Add("Full name is required");

            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
                result.Errors.Add("Valid email is required");

            if (string.IsNullOrWhiteSpace(request.ShopName))
                result.Errors.Add("Shop name is required");

            if (request.Categories == null || request.Categories.Count == 0)
                result.Errors.Add("At least one category is required");

            if (request.Latitude == 0 || request.Longitude == 0)
                result.Warnings.Add("Location data is missing");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        private double CalculateAverageDaysToReview(List<SellerRequest> requests)
        {
            var reviewed = requests.Where(r => r.ReviewedAt.HasValue).ToList();
            if (reviewed.Count == 0) return 0;

            var totalDays = reviewed.Sum(r => (r.ReviewedAt?.Subtract(r.SubmittedAt).TotalDays) ?? 0);
            return totalDays / reviewed.Count;
        }
    }

    /// <summary>
    /// Statistics model
    /// </summary>
    public class RequestStats
    {
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int RejectedRequests { get; set; }
        public int UnderReviewRequests { get; set; }
        public double AverageDaysToReview { get; set; }
    }
}
