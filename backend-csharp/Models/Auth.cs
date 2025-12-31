namespace MRShop.Authentication.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a user in the system
    /// </summary>
    public class User
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonProperty("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [JsonProperty("role")]
        public string Role { get; set; } = "user"; // 'user' or 'admin'

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Request model for user sign up
    /// </summary>
    public class SignUpRequest
    {
        [JsonProperty("username")]
        public string Username { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonProperty("password")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for user sign in
    /// </summary>
    public class SignInRequest
    {
        [JsonProperty("username_or_email")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [JsonProperty("password")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for password reset
    /// </summary>
    public class ForgotPasswordRequest
    {
        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for resetting password with new password
    /// </summary>
    public class ResetPasswordRequest
    {
        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonProperty("new_password")]
        public string NewPassword { get; set; } = string.Empty;

        [JsonProperty("confirm_password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response model for API operations
    /// </summary>
    public class ApiResponse<T>
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("data")]
        public T? Data { get; set; }

        [JsonProperty("error")]
        public string? Error { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }
    }

    /// <summary>
    /// Response data for successful sign in
    /// </summary>
    public class SignInResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonProperty("role")]
        public string Role { get; set; } = "user";

        [JsonProperty("sign_in_time")]
        public DateTime SignInTime { get; set; }
    }

    /// <summary>
    /// Response data for successful sign up
    /// </summary>
    public class SignUpResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonProperty("role")]
        public string Role { get; set; } = "user";

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
