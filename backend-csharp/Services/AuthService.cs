using MRShop.Authentication.Models;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace MRShop.Authentication.Services
{
    /// <summary>
    /// Service for managing user authentication
    /// </summary>
    public interface IAuthService
    {
        Task<(bool Success, string Message, User? User)> SignUpAsync(SignUpRequest request);
        Task<(bool Success, string Message, User? User)> SignInAsync(SignInRequest request);
        Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequest request);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByUsernameAsync(string username);
    }

    public class AuthService : IAuthService
    {
        private readonly string _dataFilePath;
        private readonly SemaphoreSlim _fileLock = new(1, 1);
        private readonly ILogger<AuthService> _logger;

        public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
        {
            _logger = logger;
            var dataDir = configuration["DataDirectory"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "data");
            Directory.CreateDirectory(dataDir);
            _dataFilePath = Path.Combine(dataDir, "users.json");

            // Initialize file if it doesn't exist
            if (!File.Exists(_dataFilePath))
            {
                File.WriteAllText(_dataFilePath, "[]");
                _logger.LogInformation("Created users.json file at {Path}", _dataFilePath);
            }
        }

        public async Task<(bool Success, string Message, User? User)> SignUpAsync(SignUpRequest request)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Username))
                return (false, "Username is required", null);

            if (string.IsNullOrWhiteSpace(request.Email))
                return (false, "Email is required", null);

            if (string.IsNullOrWhiteSpace(request.Password))
                return (false, "Password is required", null);

            if (request.Password.Length < 6)
                return (false, "Password must be at least 6 characters", null);

            // Validate email format
            if (!IsValidEmail(request.Email))
                return (false, "Invalid email format", null);

            await _fileLock.WaitAsync();
            try
            {
                var users = await GetAllUsersAsync();

                // Check if username already exists
                if (users.Any(u => u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase)))
                    return (false, "Username already exists", null);

                // Check if email already exists
                if (users.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
                    return (false, "Email already exists", null);

                // Create new user
                var newUser = new User
                {
                    Id = users.Any() ? users.Max(u => u.Id) + 1 : 1,
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = HashPassword(request.Password),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                users.Add(newUser);
                await SaveUsersAsync(users);

                _logger.LogInformation("User {Username} registered successfully", request.Username);

                return (true, "Sign up successful! You can now sign in.", newUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sign up");
                return (false, "An error occurred during sign up", null);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<(bool Success, string Message, User? User)> SignInAsync(SignInRequest request)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.UsernameOrEmail))
                return (false, "Username or email is required", null);

            if (string.IsNullOrWhiteSpace(request.Password))
                return (false, "Password is required", null);

            try
            {
                var users = await GetAllUsersAsync();

                // Find user by username or email
                var user = users.FirstOrDefault(u =>
                    u.Username.Equals(request.UsernameOrEmail, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Equals(request.UsernameOrEmail, StringComparison.OrdinalIgnoreCase));

                if (user == null)
                    return (false, "Invalid username/email or password", null);

                // Verify password
                if (!VerifyPassword(request.Password, user.PasswordHash))
                    return (false, "Invalid username/email or password", null);

                _logger.LogInformation("User {Username} signed in successfully", user.Username);

                return (true, "Sign in successful!", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sign in");
                return (false, "An error occurred during sign in", null);
            }
        }

        public async Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email))
                return (false, "Email is required");

            try
            {
                var user = await GetUserByEmailAsync(request.Email);

                if (user == null)
                    return (true, "If an account exists with that email, you will receive a password reset link.");

                // In a real system, you'd send an email here
                // For now, we'll return a message indicating the process
                _logger.LogInformation("Password reset requested for user {Email}", request.Email);

                return (true, "If an account exists with that email, you will receive a password reset link.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password");
                return (false, "An error occurred during password recovery");
            }
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequest request)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email))
                return (false, "Email is required");

            if (string.IsNullOrWhiteSpace(request.NewPassword))
                return (false, "New password is required");

            if (request.NewPassword.Length < 6)
                return (false, "Password must be at least 6 characters");

            if (request.NewPassword != request.ConfirmPassword)
                return (false, "Passwords do not match");

            await _fileLock.WaitAsync();
            try
            {
                var users = await GetAllUsersAsync();
                var user = users.FirstOrDefault(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));

                if (user == null)
                    return (false, "User not found");

                // Update password
                user.PasswordHash = HashPassword(request.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await SaveUsersAsync(users);

                _logger.LogInformation("Password reset for user {Email}", request.Email);

                return (true, "Password reset successfully! You can now sign in with your new password.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return (false, "An error occurred during password reset");
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var users = await GetAllUsersAsync();
            return users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            var users = await GetAllUsersAsync();
            return users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                var json = await File.ReadAllTextAsync(_dataFilePath);
                return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading users from file");
                return new List<User>();
            }
        }

        private async Task SaveUsersAsync(List<User> users)
        {
            var json = JsonConvert.SerializeObject(users, Formatting.Indented);
            await File.WriteAllTextAsync(_dataFilePath, json, Encoding.UTF8);
        }

        /// <summary>
        /// Hash password using PBKDF2
        /// </summary>
        private string HashPassword(string password)
        {
            var iterations = 10000;
            var salt = new byte[16];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var hash = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                System.Security.Cryptography.HashAlgorithmName.SHA256,
                20
            );

            var hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Verify password against hash
        /// </summary>
        private bool VerifyPassword(string password, string hash)
        {
            try
            {
                var hashBytes = Convert.FromBase64String(hash);
                var salt = new byte[16];
                Array.Copy(hashBytes, 0, salt, 0, 16);

                var hash2 = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    10000,
                    System.Security.Cryptography.HashAlgorithmName.SHA256,
                    20
                );

                for (int i = 0; i < 20; i++)
                {
                    if (hashBytes[i + 16] != hash2[i])
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validate email format
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
