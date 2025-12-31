using System;
using System.Collections.Generic;

namespace MRShop.OrderTracking.Models
{
    /// <summary>
    /// Represents a seller registration application
    /// </summary>
    public class SellerRegistration
    {
        /// <summary>
        /// Unique identifier for this registration
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Registration timestamp
        /// </summary>
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Current status of the application
        /// </summary>
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        // ============================================
        // PERSONAL INFORMATION
        // ============================================
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }

        // ============================================
        // BUSINESS INFORMATION
        // ============================================
        public string? ShopName { get; set; }
        public string? PaymentMethod { get; set; } // bkash, nagad, rocket, bank
        public string? BankName { get; set; } // Only if bank transfer selected
        public string? AccountNumber { get; set; }

        // ============================================
        // LOCATION INFORMATION
        // ============================================
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        // ============================================
        // SELLER CATEGORIES
        // ============================================
        public List<string> Categories { get; set; } = new List<string>();

        // ============================================
        // FILE UPLOADS (Base64 encoded or file paths)
        // ============================================
        public string? ProfilePhotoPath { get; set; }
        public string? DocumentType { get; set; } // nid or birth
        public string? NIDFrontPath { get; set; }
        public string? NIDBackPath { get; set; }
        public string? BirthCertificatePath { get; set; }

        // ============================================
        // ADDITIONAL INFORMATION
        // ============================================
        public string? AdditionalInfo { get; set; }

        // ============================================
        // METADATA
        // ============================================
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    /// <summary>
    /// Request model for seller registration from frontend
    /// </summary>
    public class SellerRegistrationRequest
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? ShopName { get; set; }
        public string? PaymentMethod { get; set; }
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
        public string? DocumentType { get; set; }
        public string? AdditionalInfo { get; set; }
    }

    /// <summary>
    /// Response model for seller registration
    /// </summary>
    public class SellerRegistrationResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? RegistrationId { get; set; }
        public SellerRegistration? Data { get; set; }
    }

    /// <summary>
    /// Statistics about seller registrations
    /// </summary>
    public class SellerRegistrationStats
    {
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int RejectedApplications { get; set; }
        public Dictionary<string, int> CategoryCounts { get; set; } = new Dictionary<string, int>();
    }
}
