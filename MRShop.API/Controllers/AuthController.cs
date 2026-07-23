using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MongoDB.Driver;
using MRShop.API.DTOs;
using MRShop.API.Models;
using MRShop.API.Services;

namespace MRShop.API.Controllers;

[ApiController]
[Route("api/customerauth")]
public class AuthController : ControllerBase
{
    private readonly MongoDbService _mongoDb;
    private readonly JwtService _jwt;
    private readonly ILogger<AuthController> _logger;

    public AuthController(MongoDbService mongoDb, JwtService jwt, ILogger<AuthController> logger)
    {
        _mongoDb = mongoDb;
        _jwt = jwt;
        _logger = logger;
    }

    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Name, email, and password are required." });
        }

        if (request.Password.Length < 8)
            return BadRequest(new { message = "Password must be at least 8 characters long." });

        if (!System.Text.RegularExpressions.Regex.IsMatch(request.Password, @"[A-Z]"))
            return BadRequest(new { message = "Password must contain at least one uppercase letter." });

        if (!System.Text.RegularExpressions.Regex.IsMatch(request.Password, @"[a-z]"))
            return BadRequest(new { message = "Password must contain at least one lowercase letter." });

        if (!System.Text.RegularExpressions.Regex.IsMatch(request.Password, @"[0-9]"))
            return BadRequest(new { message = "Password must contain at least one digit." });

        var existingUser = await _mongoDb.Users
            .Find(u => u.Email == request.Email.ToLower().Trim())
            .FirstOrDefaultAsync();

        if (existingUser != null)
        {
            return Conflict(new { message = "Email already registered." });
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = request.Email.ToLower().Trim(),
            PasswordHash = HashPassword(request.Password),
            Phone = request.Phone,
            Address = request.Address,
            Role = "customer",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _mongoDb.Users.InsertOneAsync(user);

        var token = _jwt.GenerateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            User = MapToUserResponse(user)
        });
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and password are required." });
        }

        var user = await _mongoDb.Users
            .Find(u => u.Email == request.Email.ToLower().Trim())
            .FirstOrDefaultAsync();

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var token = _jwt.GenerateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            User = MapToUserResponse(user)
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Not authenticated." });
        }

        var user = await _mongoDb.Users
            .Find(u => u.Id == userId)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(MapToUserResponse(user));
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Not authenticated." });
        }

        var update = Builders<User>.Update
            .Set(u => u.UpdatedAt, DateTime.UtcNow);

        if (!string.IsNullOrWhiteSpace(request.Name))
            update = update.Set(u => u.Name, request.Name.Trim());

        if (request.Phone != null)
            update = update.Set(u => u.Phone, request.Phone);

        if (request.Address != null)
            update = update.Set(u => u.Address, request.Address);

        if (request.ProfilePhoto != null)
            update = update.Set(u => u.ProfilePhoto, request.ProfilePhoto);

        await _mongoDb.Users.UpdateOneAsync(
            u => u.Id == userId,
            update
        );

        var user = await _mongoDb.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(MapToUserResponse(user));
    }

    [Authorize]
    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Not authenticated." });
        }

        var user = await _mongoDb.Users
            .Find(u => u.Id == userId)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest(new { message = "Current password is incorrect." });
        }

        await _mongoDb.Users.UpdateOneAsync(
            u => u.Id == userId,
            Builders<User>.Update
                .Set(u => u.PasswordHash, HashPassword(request.NewPassword))
                .Set(u => u.UpdatedAt, DateTime.UtcNow)
        );

        return Ok(new { message = "Password updated successfully." });
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        _logger.LogInformation("[GoogleAuth] Google login request received. Credential present: {HasCredential}", !string.IsNullOrWhiteSpace(request.Credential));

        if (string.IsNullOrWhiteSpace(request.Credential))
        {
            _logger.LogWarning("[GoogleAuth] Google credential is empty or null.");
            return BadRequest(new { message = "Google credential is required." });
        }

        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { "407138009600-5qc9upb4bec6iss4n1ujhef5g92mbvso.apps.googleusercontent.com" }
            };

            _logger.LogInformation("[GoogleAuth] Validating Google token with client ID: {ClientId}", settings.Audience.First());

            var payload = await GoogleJsonWebSignature.ValidateAsync(request.Credential, settings);

            if (payload == null || string.IsNullOrEmpty(payload.Email))
            {
                _logger.LogWarning("[GoogleAuth] Invalid Google credential - payload is null or email is empty.");
                return BadRequest(new { message = "Invalid Google credential." });
            }

            var email = payload.Email.ToLower().Trim();
            _logger.LogInformation("[GoogleAuth] Google token valid. Email: {Email}, Name: {Name}", email, payload.Name);

            // Admin email whitelist - ONLY this email gets admin role
            const string adminEmail = "mrshop.bd.18@gmail.com";
            string role = email == adminEmail ? "admin" : "customer";

            var user = await _mongoDb.Users
                .Find(u => u.Email == email)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                _logger.LogInformation("[GoogleAuth] Creating new user for email: {Email}", email);
                user = new User
                {
                    Name = payload.Name ?? payload.GivenName ?? payload.Email.Split('@')[0],
                    Email = email,
                    PasswordHash = "",
                    Role = role,
                    ProfilePhoto = payload.Picture,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _mongoDb.Users.InsertOneAsync(user);
                _logger.LogInformation("[GoogleAuth] New user created. ID: {UserId}", user.Id);
            }
            else
            {
                _logger.LogInformation("[GoogleAuth] Existing user found. ID: {UserId}, Email: {Email}", user.Id, user.Email);
                // If existing user, update admin role if email matches
                if (user.Role != role)
                {
                    await _mongoDb.Users.UpdateOneAsync(
                        u => u.Id == user.Id,
                        Builders<User>.Update.Set(u => u.Role, role)
                    );
                    user.Role = role;
                }

                if (string.IsNullOrEmpty(user.ProfilePhoto) && !string.IsNullOrEmpty(payload.Picture))
                {
                    await _mongoDb.Users.UpdateOneAsync(
                        u => u.Id == user.Id,
                        Builders<User>.Update.Set(u => u.ProfilePhoto, payload.Picture)
                    );
                    user.ProfilePhoto = payload.Picture;
                }
            }

            var token = _jwt.GenerateToken(user);
            _logger.LogInformation("[GoogleAuth] JWT token generated for user {UserId}. Token length: {TokenLength}", user.Id, token.Length);

            return Ok(new AuthResponse
            {
                Token = token,
                User = MapToUserResponse(user)
            });
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogError(ex, "[GoogleAuth] Invalid JWT token from Google. Detail: {Detail}", ex.Message);
            return Unauthorized(new { message = "Invalid Google token." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GoogleAuth] Google authentication failed. Type: {ExceptionType}, Message: {Message}", ex.GetType().Name, ex.Message);
            return StatusCode(500, new { message = "Google authentication failed.", detail = ex.Message, type = ex.GetType().Name });
        }
    }

    [HttpPost("facebook")]
    public async Task<IActionResult> FacebookLogin([FromBody] FacebookLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AccessToken))
        {
            return BadRequest(new { message = "Facebook access token is required." });
        }

        try
        {
            using var httpClient = new HttpClient();
            var appTokenResponse = await httpClient.GetAsync(
                $"https://graph.facebook.com/v21.0/oauth/access_token?client_id=1687875558936104&client_secret=b01738d366b986a06302f2a91c391b83&grant_type=client_credentials");

            if (!appTokenResponse.IsSuccessStatusCode)
            {
                return StatusCode(500, new { message = "Failed to verify Facebook token." });
            }

            var appTokenData = await appTokenResponse.Content.ReadFromJsonAsync<FacebookTokenResponse>();
            var debugResponse = await httpClient.GetAsync(
                $"https://graph.facebook.com/v21.0/debug_token?input_token={request.AccessToken}&access_token={appTokenData?.AccessToken}");

            if (!debugResponse.IsSuccessStatusCode)
            {
                return Unauthorized(new { message = "Invalid Facebook token." });
            }

            var debugData = await debugResponse.Content.ReadFromJsonAsync<FacebookDebugResponse>();
            if (debugData?.Data == null || !debugData.Data.IsValid)
            {
                return Unauthorized(new { message = "Invalid or expired Facebook token." });
            }

            var userInfoResponse = await httpClient.GetAsync(
                $"https://graph.facebook.com/v21.0/me?fields=id,email,first_name,last_name,picture.type(large)&access_token={request.AccessToken}");

            if (!userInfoResponse.IsSuccessStatusCode)
            {
                return BadRequest(new { message = "Failed to get Facebook user info." });
            }

            var fbUser = await userInfoResponse.Content.ReadFromJsonAsync<FacebookUserInfo>();
            if (fbUser == null || string.IsNullOrEmpty(fbUser.Email))
            {
                return BadRequest(new { message = "Facebook account does not have an email associated." });
            }

            var user = await _mongoDb.Users
                .Find(u => u.Email == fbUser.Email.ToLower().Trim())
                .FirstOrDefaultAsync();

            if (user == null)
            {
                user = new User
                {
                    Name = $"{fbUser.FirstName} {fbUser.LastName}".Trim(),
                    Email = fbUser.Email.ToLower().Trim(),
                    PasswordHash = "",
                    Role = "customer",
                    ProfilePhoto = fbUser.Picture?.Data?.Url,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _mongoDb.Users.InsertOneAsync(user);
            }
            else if (string.IsNullOrEmpty(user.ProfilePhoto) && fbUser.Picture?.Data?.Url != null)
            {
                await _mongoDb.Users.UpdateOneAsync(
                    u => u.Id == user.Id,
                    Builders<User>.Update.Set(u => u.ProfilePhoto, fbUser.Picture.Data.Url)
                );
                user.ProfilePhoto = fbUser.Picture.Data.Url;
            }

            var token = _jwt.GenerateToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                User = MapToUserResponse(user)
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Facebook authentication failed." });
        }
    }

    private static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations: 100_000,
            HashAlgorithmName.SHA256,
            outputLength: 32);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split('.', 2);
        if (parts.Length != 2) return false;
        var salt = Convert.FromBase64String(parts[0]);
        var hash = Convert.FromBase64String(parts[1]);
        var computed = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations: 100_000,
            HashAlgorithmName.SHA256,
            outputLength: 32);
        return CryptographicOperations.FixedTimeEquals(computed, hash);
    }

    private static UserResponse MapToUserResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            Role = user.Role,
            ProfilePhoto = user.ProfilePhoto
        };
    }

    private class FacebookTokenResponse
    {
        public string? AccessToken { get; set; }
        public string? TokenType { get; set; }
        public int ExpiresIn { get; set; }
    }

    private class FacebookDebugResponse
    {
        public FacebookDebugData? Data { get; set; }
    }

    private class FacebookDebugData
    {
        public string? AppId { get; set; }
        public bool IsValid { get; set; }
        public long ExpiresAt { get; set; }
    }

    private class FacebookUserInfo
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("first_name")]
        public string? FirstName { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("last_name")]
        public string? LastName { get; set; }
        public FacebookPicture? Picture { get; set; }
    }

    private class FacebookPicture
    {
        public FacebookPictureData? Data { get; set; }
    }

    private class FacebookPictureData
    {
        public string? Url { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
