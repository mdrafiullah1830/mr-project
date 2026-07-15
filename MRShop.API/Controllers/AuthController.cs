using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public AuthController(MongoDbService mongoDb, JwtService jwt)
    {
        _mongoDb = mongoDb;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Name, email, and password are required." });
        }

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
        if (string.IsNullOrWhiteSpace(request.Credential))
        {
            return BadRequest(new { message = "Google credential is required." });
        }

        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { "407138009600-5qc9upb4bec6iss4n1ujhef5g92mbvso.apps.googleusercontent.com" }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(request.Credential, settings);

            if (payload == null || string.IsNullOrEmpty(payload.Email))
            {
                return BadRequest(new { message = "Invalid Google credential." });
            }

            var user = await _mongoDb.Users
                .Find(u => u.Email == payload.Email.ToLower().Trim())
                .FirstOrDefaultAsync();

            if (user == null)
            {
                user = new User
                {
                    Name = payload.Name ?? payload.GivenName ?? payload.Email.Split('@')[0],
                    Email = payload.Email.ToLower().Trim(),
                    PasswordHash = "",
                    Role = "customer",
                    ProfilePhoto = payload.Picture,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _mongoDb.Users.InsertOneAsync(user);
            }
            else if (string.IsNullOrEmpty(user.ProfilePhoto) && !string.IsNullOrEmpty(payload.Picture))
            {
                await _mongoDb.Users.UpdateOneAsync(
                    u => u.Id == user.Id,
                    Builders<User>.Update.Set(u => u.ProfilePhoto, payload.Picture)
                );
                user.ProfilePhoto = payload.Picture;
            }

            var token = _jwt.GenerateToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                User = MapToUserResponse(user)
            });
        }
        catch (InvalidJwtException ex)
        {
            return Unauthorized(new { message = "Invalid Google token.", detail = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Google authentication failed.", detail = ex.Message });
        }
    }

    // TODO: Replace SHA256 with BCrypt (BCrypt.Net package) for production use.
    // SHA256 is not suitable for password hashing. Use BCrypt.Net.BCrypt.HashPassword() and BCrypt.Net.BCrypt.Verify() instead.
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "MRShop_Salt_2024"));
        return Convert.ToBase64String(bytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
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
}
