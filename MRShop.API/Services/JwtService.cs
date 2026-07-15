using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MRShop.API.Models;

namespace MRShop.API.Services;

public class JwtService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiresInMinutes;

    public JwtService(IConfiguration configuration)
    {
        _secretKey = Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException(
                "JWT secret key not configured. " +
                "Set JWT_SECRET environment variable.");

        _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? configuration["Jwt:Issuer"]
            ?? "MRShopAPI";

        _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            ?? configuration["Jwt:Audience"]
            ?? "MRShopClient";

        _expiresInMinutes = int.Parse(
            Environment.GetEnvironmentVariable("JWT_EXPIRES_IN_MINUTES")
            ?? configuration["Jwt:ExpiresInMinutes"]
            ?? "1440"
        );
    }

    public string GenerateToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expiresInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
