using Newtonsoft.Json;

namespace MRShop.Profile.Models
{
    /// <summary>
    /// User profile data model
    /// </summary>
    public class UserProfile
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonProperty("full_name")]
        public string? FullName { get; set; }

        [JsonProperty("phone_number")]
        public string? PhoneNumber { get; set; }

        [JsonProperty("email_address")]
        public string? EmailAddress { get; set; }

        [JsonProperty("address")]
        public string? Address { get; set; }

        [JsonProperty("date_of_birth")]
        public string? DateOfBirth { get; set; }

        [JsonProperty("gender")]
        public string? Gender { get; set; }

        [JsonProperty("profile_photo_path")]
        public string? ProfilePhotoPath { get; set; }

        [JsonProperty("bio")]
        public string? Bio { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; } = DateTime.UtcNow.ToString("o");

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");
    }

    /// <summary>
    /// Request model for updating user profile
    /// </summary>
    public class UpdateProfileRequest
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonProperty("full_name")]
        public string? FullName { get; set; }

        [JsonProperty("phone_number")]
        public string? PhoneNumber { get; set; }

        [JsonProperty("email_address")]
        public string? EmailAddress { get; set; }

        [JsonProperty("address")]
        public string? Address { get; set; }

        [JsonProperty("date_of_birth")]
        public string? DateOfBirth { get; set; }

        [JsonProperty("gender")]
        public string? Gender { get; set; }

        [JsonProperty("bio")]
        public string? Bio { get; set; }
    }

    /// <summary>
    /// Generic API response wrapper
    /// </summary>
    public class ProfileApiResponse<T>
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("data")]
        public T? Data { get; set; }
    }
}
