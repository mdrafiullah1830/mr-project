using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MRShop.OrderTracking.Models;
using Microsoft.Extensions.Logging;

namespace MRShop.OrderTracking.Services
{
    /// <summary>
    /// Interface for seller registration service
    /// </summary>
    public interface ISellerRegistrationService
    {
        Task<SellerRegistration?> RegisterSellerAsync(SellerRegistrationRequest request, string ipAddress, string userAgent);
        Task<SellerRegistration?> GetRegistrationByIdAsync(string registrationId);
        Task<List<SellerRegistration>> GetAllRegistrationsAsync();
        Task<List<SellerRegistration>> GetPendingRegistrationsAsync();
        Task<bool> UpdateRegistrationStatusAsync(string registrationId, string status);
        Task<SellerRegistrationStats> GetRegistrationStatsAsync();
    }

    /// <summary>
    /// Service for managing seller registration and storing data to JSON files
    /// </summary>
    public class SellerRegistrationService : ISellerRegistrationService
    {
        private readonly string _dataDirectory;
        private readonly string _registrationsDirectory;
        private readonly ILogger<SellerRegistrationService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public SellerRegistrationService(ILogger<SellerRegistrationService> logger)
        {
            _logger = logger;
            
            // Set data directory from environment variable or use default
            string baseDataDir = Environment.GetEnvironmentVariable("MR_DATA_DIR") 
                ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "data");
            
            _dataDirectory = Path.GetFullPath(baseDataDir);
            _registrationsDirectory = Path.Combine(_dataDirectory, "seller_registrations");

            // Create directories if they don't exist
            Directory.CreateDirectory(_dataDirectory);
            Directory.CreateDirectory(_registrationsDirectory);

            _logger.LogInformation("Seller Registration Service initialized");
            _logger.LogInformation("Data directory: {DataDirectory}", _dataDirectory);
            _logger.LogInformation("Registrations directory: {RegistrationsDirectory}", _registrationsDirectory);

            // Configure JSON serialization options
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        /// <summary>
        /// Register a new seller and save to JSON file
        /// </summary>
        public async Task<SellerRegistration?> RegisterSellerAsync(
            SellerRegistrationRequest request, 
            string ipAddress, 
            string userAgent)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.FullName))
                    throw new ArgumentException("Full name is required");
                
                if (string.IsNullOrWhiteSpace(request.Email))
                    throw new ArgumentException("Email is required");
                
                if (string.IsNullOrWhiteSpace(request.Phone))
                    throw new ArgumentException("Phone number is required");
                
                if (string.IsNullOrWhiteSpace(request.ShopName))
                    throw new ArgumentException("Shop name is required");
                
                if (string.IsNullOrWhiteSpace(request.PaymentMethod))
                    throw new ArgumentException("Payment method is required");
                
                if (request.Categories == null || request.Categories.Count == 0)
                    throw new ArgumentException("At least one category must be selected");

                if (request.Latitude == 0 || request.Longitude == 0)
                    throw new ArgumentException("Location information is required");

                // Create seller registration object
                var registration = new SellerRegistration
                {
                    FullName = request.FullName,
                    Phone = request.Phone,
                    Email = request.Email,
                    ShopName = request.ShopName,
                    PaymentMethod = request.PaymentMethod,
                    BankName = request.BankName,
                    AccountNumber = request.AccountNumber,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Categories = request.Categories,
                    DocumentType = request.DocumentType,
                    AdditionalInfo = request.AdditionalInfo,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };

                // Save to JSON file
                await SaveRegistrationToFileAsync(registration);

                _logger.LogInformation(
                    "New seller registration created: {RegistrationId} - {ShopName} ({Email})",
                    registration.Id,
                    registration.ShopName,
                    registration.Email
                );

                return registration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering seller: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieve registration by ID
        /// </summary>
        public async Task<SellerRegistration?> GetRegistrationByIdAsync(string registrationId)
        {
            try
            {
                string filePath = Path.Combine(_registrationsDirectory, $"{registrationId}.json");
                
                if (!File.Exists(filePath))
                    return null;

                string jsonContent = await File.ReadAllTextAsync(filePath);
                var registration = JsonSerializer.Deserialize<SellerRegistration>(jsonContent, _jsonOptions);
                
                return registration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving registration {RegistrationId}", registrationId);
                return null;
            }
        }

        /// <summary>
        /// Get all registrations
        /// </summary>
        public async Task<List<SellerRegistration>> GetAllRegistrationsAsync()
        {
            try
            {
                var registrations = new List<SellerRegistration>();
                var files = Directory.GetFiles(_registrationsDirectory, "*.json");

                foreach (var file in files)
                {
                    try
                    {
                        string jsonContent = await File.ReadAllTextAsync(file);
                        var registration = JsonSerializer.Deserialize<SellerRegistration>(jsonContent, _jsonOptions);
                        if (registration != null)
                            registrations.Add(registration);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error reading file: {File}", file);
                    }
                }

                return registrations.OrderByDescending(r => r.SubmittedAt).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all registrations");
                return new List<SellerRegistration>();
            }
        }

        /// <summary>
        /// Get pending registrations only
        /// </summary>
        public async Task<List<SellerRegistration>> GetPendingRegistrationsAsync()
        {
            try
            {
                var allRegistrations = await GetAllRegistrationsAsync();
                return allRegistrations.Where(r => r.Status == "Pending").ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending registrations");
                return new List<SellerRegistration>();
            }
        }

        /// <summary>
        /// Update registration status
        /// </summary>
        public async Task<bool> UpdateRegistrationStatusAsync(string registrationId, string status)
        {
            try
            {
                var registration = await GetRegistrationByIdAsync(registrationId);
                if (registration == null)
                    return false;

                registration.Status = status;
                await SaveRegistrationToFileAsync(registration);

                _logger.LogInformation(
                    "Registration {RegistrationId} status updated to: {Status}",
                    registrationId,
                    status
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating registration status");
                return false;
            }
        }

        /// <summary>
        /// Get statistics about registrations
        /// </summary>
        public async Task<SellerRegistrationStats> GetRegistrationStatsAsync()
        {
            try
            {
                var registrations = await GetAllRegistrationsAsync();
                var stats = new SellerRegistrationStats
                {
                    TotalApplications = registrations.Count,
                    PendingApplications = registrations.Count(r => r.Status == "Pending"),
                    ApprovedApplications = registrations.Count(r => r.Status == "Approved"),
                    RejectedApplications = registrations.Count(r => r.Status == "Rejected")
                };

                // Count by category
                foreach (var reg in registrations)
                {
                    foreach (var category in reg.Categories)
                    {
                        if (stats.CategoryCounts.ContainsKey(category))
                            stats.CategoryCounts[category]++;
                        else
                            stats.CategoryCounts[category] = 1;
                    }
                }

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating registration statistics");
                return new SellerRegistrationStats();
            }
        }

        /// <summary>
        /// Save registration to JSON file
        /// </summary>
        private async Task SaveRegistrationToFileAsync(SellerRegistration registration)
        {
            try
            {
                string filePath = Path.Combine(_registrationsDirectory, $"{registration.Id}.json");
                string jsonContent = JsonSerializer.Serialize(registration, _jsonOptions);
                
                await File.WriteAllTextAsync(filePath, jsonContent);

                _logger.LogInformation("Registration saved to file: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving registration to file");
                throw;
            }
        }

        /// <summary>
        /// Export all registrations to a summary CSV file
        /// </summary>
        public async Task<string> ExportRegistrationsAsync()
        {
            try
            {
                var registrations = await GetAllRegistrationsAsync();
                var csvPath = Path.Combine(_dataDirectory, $"seller_registrations_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

                var csvLines = new List<string>
                {
                    "ID,SubmittedAt,Status,FullName,Email,Phone,ShopName,PaymentMethod,Latitude,Longitude,Categories,IpAddress"
                };

                foreach (var reg in registrations)
                {
                    var categoriesStr = string.Join(";", reg.Categories);
                    var line = $"{reg.Id},{reg.SubmittedAt:yyyy-MM-dd HH:mm:ss},{reg.Status},{reg.FullName},{reg.Email},{reg.Phone},{reg.ShopName},{reg.PaymentMethod},{reg.Latitude},{reg.Longitude},\"{categoriesStr}\",{reg.IpAddress}";
                    csvLines.Add(line);
                }

                await File.WriteAllLinesAsync(csvPath, csvLines);
                _logger.LogInformation("Registrations exported to: {CsvPath}", csvPath);
                
                return csvPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting registrations");
                throw;
            }
        }
    }
}
