using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MRShop.OrderTracking.Models
{
    /// <summary>
    /// Comprehensive seller request model with audit trail and security features
    /// </summary>
    public class SellerRequest
    {
        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }

        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        [JsonPropertyName("submittedAt")]
        public DateTime SubmittedAt { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Under Review

        [JsonPropertyName("requestVersion")]
        public int RequestVersion { get; set; } = 1;

        // Seller Information
        [JsonPropertyName("fullName")]
        public string? FullName { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("shopName")]
        public string? ShopName { get; set; }

        [JsonPropertyName("paymentMethod")]
        public string? PaymentMethod { get; set; }

        [JsonPropertyName("bankName")]
        public string? BankName { get; set; }

        [JsonPropertyName("accountNumber")]
        public string? AccountNumber { get; set; }

        // Location Data
        [JsonPropertyName("latitude")]
        public decimal Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public decimal Longitude { get; set; }

        // Categories
        [JsonPropertyName("categories")]
        public List<string>? Categories { get; set; } = new List<string>();

        // Document Information
        [JsonPropertyName("documentType")]
        public string? DocumentType { get; set; } // "nid" or "birth"

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

        // Audit Trail
        [JsonPropertyName("ipAddress")]
        public string? IpAddress { get; set; }

        [JsonPropertyName("userAgent")]
        public string? UserAgent { get; set; }

        [JsonPropertyName("browserFingerprint")]
        public string? BrowserFingerprint { get; set; }

        [JsonPropertyName("deviceType")]
        public string? DeviceType { get; set; }

        [JsonPropertyName("submissionMethod")]
        public string? SubmissionMethod { get; set; } = "WebForm";

        // Admin Review Fields
        [JsonPropertyName("adminNotes")]
        public string? AdminNotes { get; set; }

        [JsonPropertyName("reviewedBy")]
        public string? ReviewedBy { get; set; }

        [JsonPropertyName("reviewedAt")]
        public DateTime? ReviewedAt { get; set; }

        [JsonPropertyName("approvalScore")]
        public int? ApprovalScore { get; set; } // 0-100

        [JsonPropertyName("rejectionReason")]
        public string? RejectionReason { get; set; }

        // Storage Information
        [JsonPropertyName("userFolderPath")]
        public string? UserFolderPath { get; set; }

        [JsonPropertyName("requestFilePath")]
        public string? RequestFilePath { get; set; }

        [JsonPropertyName("adminNotificationSent")]
        public bool AdminNotificationSent { get; set; }

        [JsonPropertyName("adminNotificationSentAt")]
        public DateTime? AdminNotificationSentAt { get; set; }

        [JsonPropertyName("lastModified")]
        public DateTime LastModified { get; set; }

        [JsonPropertyName("modifiedCount")]
        public int ModifiedCount { get; set; } = 0;
    }

    /// <summary>
    /// Admin request summary for the admin panel
    /// </summary>
    public class AdminRequestNotification
    {
        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }

        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        [JsonPropertyName("fullName")]
        public string? FullName { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("shopName")]
        public string? ShopName { get; set; }

        [JsonPropertyName("submittedAt")]
        public DateTime SubmittedAt { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("categories")]
        public List<string>? Categories { get; set; }

        [JsonPropertyName("location")]
        public LatLng? Location { get; set; }

        [JsonPropertyName("notificationSentAt")]
        public DateTime NotificationSentAt { get; set; }

        [JsonPropertyName("acknowledged")]
        public bool Acknowledged { get; set; }

        [JsonPropertyName("acknowledgedAt")]
        public DateTime? AcknowledgedAt { get; set; }
    }

    /// <summary>
    /// Location helper
    /// </summary>
    public class LatLng
    {
        [JsonPropertyName("lat")]
        public decimal Lat { get; set; }

        [JsonPropertyName("lng")]
        public decimal Lng { get; set; }
    }

    /// <summary>
    /// Request validation result
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }
}
