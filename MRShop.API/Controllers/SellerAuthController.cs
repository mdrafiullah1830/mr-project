using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MRShop.API.DTOs;
using MRShop.API.Models;
using MRShop.API.Services;

namespace MRShop.API.Controllers;

[ApiController]
[Route("api/sellerauth")]
public class SellerAuthController : ControllerBase
{
    private readonly MongoDbService _mongoDb;
    private readonly JwtService _jwt;

    public SellerAuthController(MongoDbService mongoDb, JwtService jwt)
    {
        _mongoDb = mongoDb;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] SellerRegisterRequest request)
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
            Role = "seller",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _mongoDb.Users.InsertOneAsync(user);

        var token = _jwt.GenerateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            User = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role
            }
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
            .Find(u => u.Email == request.Email.ToLower().Trim() && u.Role == "seller")
            .FirstOrDefaultAsync();

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var token = _jwt.GenerateToken(user);

        return Ok(new
        {
            token,
            seller = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role
            }
        });
    }

    [Authorize(Roles = "seller")]
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
            return NotFound(new { message = "Seller not found." });
        }

        return Ok(new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role
        });
    }

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
}

public class SellerRegisterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
}
