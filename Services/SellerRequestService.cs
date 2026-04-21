using System.Text.Json;
using System.Text.Json.Serialization;

namespace MRShop.SellerRequests
{
    public class SellerRequest
    {
        [JsonPropertyName("requestId")]
        public string RequestId { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("submittedAt")]
        public DateTime SubmittedAt { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "Pending";

        [JsonPropertyName("requestVersion")]
        public int RequestVersion { get; set; } = 1;

        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("shopName")]
        public string ShopName { get; set; } = string.Empty;

        [JsonPropertyName("paymentMethod")]
        public string PaymentMethod { get; set; } = string.Empty;

        [JsonPropertyName("bankName")]
        public string? BankName { get; set; }

        [JsonPropertyName("accountNumber")]
        public string AccountNumber { get; set; } = string.Empty;

        [JsonPropertyName("latitude")]
        public decimal Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public decimal Longitude { get; set; }

        [JsonPropertyName("categories")]
        public List<string> Categories { get; set; } = new();

        [JsonPropertyName("documentType")]
        public string DocumentType { get; set; } = string.Empty;

        [JsonPropertyName("profilePhotoPath")]
        public string? ProfilePhotoPath { get; set; }

        [JsonPropertyName("nidFrontPath")]
        public string? NidFrontPath { get; set; }

        [JsonPropertyName("nidBackPath")]
        public string? NidBackPath { get; set; }

        [JsonPropertyName("birthCertificatePath")]
        public string? BirthCertificatePath { get; set; }

        [JsonPropertyName("additionalInfo")]
        public string? AdditionalInfo { get; set; }

        [JsonPropertyName("ipAddress")]
        public string? IpAddress { get; set; }

        [JsonPropertyName("userAgent")]
        public string? UserAgent { get; set; }

        [JsonPropertyName("browserFingerprint")]
        public string? BrowserFingerprint { get; set; }

        [JsonPropertyName("deviceType")]
        public string? DeviceType { get; set; }

        [JsonPropertyName("submissionMethod")]
        public string SubmissionMethod { get; set; } = "WebForm";

        [JsonPropertyName("adminNotes")]
        public string? AdminNotes { get; set; }

        [JsonPropertyName("reviewedBy")]
        public string? ReviewedBy { get; set; }

        [JsonPropertyName("reviewedAt")]
        public DateTime? ReviewedAt { get; set; }

        [JsonPropertyName("approvalScore")]
        public int? ApprovalScore { get; set; }

        [JsonPropertyName("rejectionReason")]
        public string? RejectionReason { get; set; }

        [JsonPropertyName("userFolderPath")]
        public string? UserFolderPath { get; set; }

        [JsonPropertyName("requestFilePath")]
        public string? RequestFilePath { get; set; }

        [JsonPropertyName("adminNotificationSent")]
        public bool AdminNotificationSent { get; set; }

        [JsonPropertyName("adminNotificationSentAt")]
        public DateTime? AdminNotificationSentAt { get; set; }

        [JsonPropertyName("lastModified")]
        public DateTime? LastModified { get; set; }

        [JsonPropertyName("modifiedCount")]
        public int ModifiedCount { get; set; }
    }

    public class LatLng
    {
        [JsonPropertyName("lat")]
        public decimal Lat { get; set; }

        [JsonPropertyName("lng")]
        public decimal Lng { get; set; }
    }

    public class AdminRequestNotification
    {
        [JsonPropertyName("requestId")]
        public string RequestId { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("shopName")]
        public string ShopName { get; set; } = string.Empty;

        [JsonPropertyName("submittedAt")]
        public DateTime SubmittedAt { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "Pending";

        [JsonPropertyName("categories")]
        public List<string> Categories { get; set; } = new();

        [JsonPropertyName("location")]
        public LatLng Location { get; set; } = new();

        [JsonPropertyName("notificationSentAt")]
        public DateTime NotificationSentAt { get; set; }

        [JsonPropertyName("acknowledged")]
        public bool Acknowledged { get; set; }

        [JsonPropertyName("acknowledgedAt")]
        public DateTime? AcknowledgedAt { get; set; }
    }

    public class SellerRequestStatusUpdate
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }
    }

    public class SellerRequestService
    {
        private static readonly object LockObject = new();
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly string _requestsRootPath;
        private readonly string _usersRootPath;

        public SellerRequestService()
        {
            _requestsRootPath = ResolveSellerRequestRootPath();
            _usersRootPath = Path.Combine(_requestsRootPath, "users");

            Directory.CreateDirectory(_usersRootPath);
            Directory.CreateDirectory(Path.Combine(_requestsRootPath, "admin_pending"));
            Directory.CreateDirectory(Path.Combine(_requestsRootPath, "archive"));
        }

        public Task<List<SellerRequest>> GetAllRequestsAsync()
        {
            lock (LockObject)
            {
                return Task.FromResult(ReadAllRequests());
            }
        }

        public Task<List<AdminRequestNotification>> GetAdminNotificationsAsync()
        {
            lock (LockObject)
            {
                var notifications = ReadAllRequests()
                    .Where(request => NormalizeStatus(request.Status).Equals("Pending", StringComparison.OrdinalIgnoreCase))
                    .Select(ToNotification)
                    .OrderByDescending(notification => notification.SubmittedAt)
                    .ToList();

                return Task.FromResult(notifications);
            }
        }

        public Task<SellerRequest?> GetRequestByIdAsync(string requestId)
        {
            lock (LockObject)
            {
                return Task.FromResult(ReadAllRequests().FirstOrDefault(request => string.Equals(request.RequestId, requestId, StringComparison.OrdinalIgnoreCase)));
            }
        }

        public Task<bool> UpdateRequestStatusAsync(string requestId, string newStatus, string? adminNotes = null)
        {
            lock (LockObject)
            {
                var request = ReadAllRequests().FirstOrDefault(item => string.Equals(item.RequestId, requestId, StringComparison.OrdinalIgnoreCase));
                if (request == null)
                {
                    return Task.FromResult(false);
                }

                request.Status = NormalizeStatus(newStatus);
                request.AdminNotes = adminNotes ?? request.AdminNotes;
                request.ReviewedBy = string.IsNullOrWhiteSpace(request.ReviewedBy) ? "adminrafi" : request.ReviewedBy;
                request.ReviewedAt = DateTime.UtcNow;
                request.LastModified = DateTime.UtcNow;
                request.ModifiedCount += 1;
                request.AdminNotificationSent = true;
                request.AdminNotificationSentAt ??= DateTime.UtcNow;

                if (request.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
                {
                    request.RejectionReason = adminNotes ?? request.RejectionReason ?? "Rejected by admin";
                }

                SaveRequest(request);
                return Task.FromResult(true);
            }
        }

        public Task<bool> AcknowledgeAdminNotificationAsync(string requestId)
        {
            lock (LockObject)
            {
                var request = ReadAllRequests().FirstOrDefault(item => string.Equals(item.RequestId, requestId, StringComparison.OrdinalIgnoreCase));
                if (request == null)
                {
                    return Task.FromResult(false);
                }

                request.AdminNotificationSent = true;
                request.AdminNotificationSentAt ??= DateTime.UtcNow;
                request.LastModified = DateTime.UtcNow;
                request.ModifiedCount += 1;

                SaveRequest(request);
                return Task.FromResult(true);
            }
        }

        private List<SellerRequest> ReadAllRequests()
        {
            if (!Directory.Exists(_usersRootPath))
            {
                return new List<SellerRequest>();
            }

            var requestFiles = Directory.EnumerateFiles(_usersRootPath, "seller_request.json", SearchOption.AllDirectories);
            var requests = new List<SellerRequest>();

            foreach (var filePath in requestFiles)
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    var request = JsonSerializer.Deserialize<SellerRequest>(json, JsonOptions);
                    if (request == null || string.IsNullOrWhiteSpace(request.RequestId))
                    {
                        continue;
                    }

                    request.RequestFilePath = filePath;
                    request.UserFolderPath = Path.GetDirectoryName(filePath);
                    requests.Add(request);
                }
                catch
                {
                    // Ignore malformed files so one bad request does not block the dashboard.
                }
            }

            return requests;
        }

        private void SaveRequest(SellerRequest request)
        {
            var filePath = request.RequestFilePath;
            if (string.IsNullOrWhiteSpace(filePath))
            {
                filePath = ResolveRequestFilePath(request.RequestId, request.UserId);
                request.RequestFilePath = filePath;
            }

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(request, JsonOptions);
            File.WriteAllText(filePath, json);
        }

        private string ResolveRequestFilePath(string requestId, string userId)
        {
            var userFolder = Path.Combine(_usersRootPath, userId);
            Directory.CreateDirectory(userFolder);
            return Path.Combine(userFolder, "seller_request.json");
        }

        private static AdminRequestNotification ToNotification(SellerRequest request)
        {
            return new AdminRequestNotification
            {
                RequestId = request.RequestId,
                UserId = request.UserId,
                FullName = request.FullName,
                Email = request.Email,
                ShopName = request.ShopName,
                SubmittedAt = request.SubmittedAt,
                Status = request.Status,
                Categories = request.Categories ?? new List<string>(),
                Location = new LatLng
                {
                    Lat = request.Latitude,
                    Lng = request.Longitude
                },
                NotificationSentAt = request.AdminNotificationSentAt ?? request.SubmittedAt,
                Acknowledged = request.AdminNotificationSent,
                AcknowledgedAt = request.AdminNotificationSentAt
            };
        }

        private static string NormalizeStatus(string? status)
        {
            var value = string.IsNullOrWhiteSpace(status) ? "Pending" : status.Trim();

            if (value.Contains("reject", StringComparison.OrdinalIgnoreCase))
            {
                return "Rejected";
            }

            if (value.Contains("approve", StringComparison.OrdinalIgnoreCase))
            {
                return "Approved";
            }

            if (value.Contains("review", StringComparison.OrdinalIgnoreCase))
            {
                return "Under Review";
            }

            return value;
        }

        private static string ResolveSellerRequestRootPath()
        {
            var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);

            while (currentDirectory != null)
            {
                var candidate = Path.Combine(currentDirectory.FullName, "data", "seller_requests");
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                currentDirectory = currentDirectory.Parent;
            }

            var fallback = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "data", "seller_requests"));
            Directory.CreateDirectory(fallback);
            return fallback;
        }
    }
}