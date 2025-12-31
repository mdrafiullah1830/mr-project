using MRShop.Profile.Models;
using Newtonsoft.Json;

namespace MRShop.Profile.Services
{
    public interface IProfileService
    {
        Task<UserProfile?> GetProfileAsync(string userId);
        Task<(bool success, string message, UserProfile? profile)> CreateProfileAsync(UserProfile profile);
        Task<(bool success, string message, UserProfile? profile)> UpdateProfileAsync(string userId, UpdateProfileRequest request);
        Task<(bool success, string message)> UpdateProfilePhotoAsync(string userId, string photoPath);
        Task<bool> ProfileExistsAsync(string userId);
    }

    public class ProfileService : IProfileService
    {
        private readonly string _dataDirectory;
        private readonly string _profilesFilePath;
        private readonly SemaphoreSlim _fileLock = new(1, 1);
        private readonly ILogger<ProfileService> _logger;

        public ProfileService(IConfiguration configuration, ILogger<ProfileService> logger)
        {
            _dataDirectory = configuration["DataDirectory"] ?? "/Users/mdrafiullah/Desktop/mr project /data";
            _profilesFilePath = Path.Combine(_dataDirectory, "user_profiles.json");
            _logger = logger;

            // Ensure data directory exists
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
                _logger.LogInformation("Created data directory: {Directory}", _dataDirectory);
            }

            // Initialize profiles file if it doesn't exist
            if (!File.Exists(_profilesFilePath))
            {
                File.WriteAllText(_profilesFilePath, "[]");
                _logger.LogInformation("Created user_profiles.json file");
            }
        }

        public async Task<UserProfile?> GetProfileAsync(string userId)
        {
            await _fileLock.WaitAsync();
            try
            {
                var profiles = await ReadProfilesAsync();
                return profiles.FirstOrDefault(p => p.UserId == userId);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<(bool success, string message, UserProfile? profile)> CreateProfileAsync(UserProfile profile)
        {
            await _fileLock.WaitAsync();
            try
            {
                var profiles = await ReadProfilesAsync();

                // Check if profile already exists
                if (profiles.Any(p => p.UserId == profile.UserId))
                {
                    return (false, "Profile already exists for this user", null);
                }

                profile.CreatedAt = DateTime.UtcNow.ToString("o");
                profile.UpdatedAt = DateTime.UtcNow.ToString("o");

                profiles.Add(profile);
                await WriteProfilesAsync(profiles);

                _logger.LogInformation("Created profile for user {UserId}", profile.UserId);
                return (true, "Profile created successfully", profile);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<(bool success, string message, UserProfile? profile)> UpdateProfileAsync(string userId, UpdateProfileRequest request)
        {
            await _fileLock.WaitAsync();
            try
            {
                var profiles = await ReadProfilesAsync();
                var profile = profiles.FirstOrDefault(p => p.UserId == userId);

                if (profile == null)
                {
                    // Create new profile if it doesn't exist
                    profile = new UserProfile
                    {
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow.ToString("o")
                    };
                    profiles.Add(profile);
                }

                // Update fields (only update if provided)
                if (!string.IsNullOrEmpty(request.FullName))
                    profile.FullName = request.FullName;
                
                if (!string.IsNullOrEmpty(request.PhoneNumber))
                    profile.PhoneNumber = request.PhoneNumber;
                
                if (!string.IsNullOrEmpty(request.EmailAddress))
                    profile.EmailAddress = request.EmailAddress;
                
                if (!string.IsNullOrEmpty(request.Address))
                    profile.Address = request.Address;
                
                if (!string.IsNullOrEmpty(request.DateOfBirth))
                    profile.DateOfBirth = request.DateOfBirth;
                
                if (!string.IsNullOrEmpty(request.Gender))
                    profile.Gender = request.Gender;
                
                if (!string.IsNullOrEmpty(request.Bio))
                    profile.Bio = request.Bio;

                profile.UpdatedAt = DateTime.UtcNow.ToString("o");

                await WriteProfilesAsync(profiles);

                _logger.LogInformation("Updated profile for user {UserId}", userId);
                return (true, "Profile updated successfully", profile);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<(bool success, string message)> UpdateProfilePhotoAsync(string userId, string photoPath)
        {
            await _fileLock.WaitAsync();
            try
            {
                var profiles = await ReadProfilesAsync();
                var profile = profiles.FirstOrDefault(p => p.UserId == userId);

                if (profile == null)
                {
                    return (false, "Profile not found");
                }

                profile.ProfilePhotoPath = photoPath;
                profile.UpdatedAt = DateTime.UtcNow.ToString("o");

                await WriteProfilesAsync(profiles);

                _logger.LogInformation("Updated profile photo for user {UserId}", userId);
                return (true, "Profile photo updated successfully");
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<bool> ProfileExistsAsync(string userId)
        {
            await _fileLock.WaitAsync();
            try
            {
                var profiles = await ReadProfilesAsync();
                return profiles.Any(p => p.UserId == userId);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private async Task<List<UserProfile>> ReadProfilesAsync()
        {
            try
            {
                var json = await File.ReadAllTextAsync(_profilesFilePath);
                return JsonConvert.DeserializeObject<List<UserProfile>>(json) ?? new List<UserProfile>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading profiles from {FilePath}", _profilesFilePath);
                return new List<UserProfile>();
            }
        }

        private async Task WriteProfilesAsync(List<UserProfile> profiles)
        {
            try
            {
                var json = JsonConvert.SerializeObject(profiles, Formatting.Indented);
                await File.WriteAllTextAsync(_profilesFilePath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing profiles to {FilePath}", _profilesFilePath);
                throw;
            }
        }
    }
}
