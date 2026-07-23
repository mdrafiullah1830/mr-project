using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
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

    [HttpPost("login-by-username")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> LoginByUsername([FromBody] SellerUsernameLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username and password are required." });
        }

        var application = await _mongoDb.SellerApplications
            .Find(a => a.SellerUsername == request.Username.Trim() && a.Status == "approved")
            .FirstOrDefaultAsync();

        if (application == null)
        {
            return Unauthorized(new { message = "Invalid username or account not approved yet." });
        }

        if (application.SellerPasswordHash == null || !VerifyPassword(request.Password, application.SellerPasswordHash))
        {
            return Unauthorized(new { message = "Invalid password." });
        }

        var user = await _mongoDb.Users
            .Find(u => u.Email == application.Email.ToLower().Trim() && u.Role == "seller")
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return Unauthorized(new { message = "Seller account not found." });
        }

        var token = _jwt.GenerateToken(user);

        var profile = await _mongoDb.SellerProfiles
            .Find(p => p.UserId == user.Id)
            .FirstOrDefaultAsync();

        return Ok(new
        {
            token,
            seller = new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email,
                phone = user.Phone,
                role = user.Role,
                shopName = profile?.ShopName ?? application.ShopName,
                sellerUsername = application.SellerUsername
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
}
