using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MRShop.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration - reads from appsettings.json AND environment variables
var configuration = builder.Configuration;

// MongoDB
builder.Services.AddSingleton<MongoDbService>();

// JWT
builder.Services.AddSingleton<JwtService>();

// Authentication
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException(
        "JWT secret key not configured. Set JWT_SECRET environment variable.");

var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? configuration["Jwt:Issuer"]
    ?? "MRShopAPI";

var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? configuration["Jwt:Audience"]
    ?? "MRShopClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            policy.WithOrigins(
                    "https://mrshopbangladesh.tech",
                    "https://www.mrshopbangladesh.tech",
                    "http://localhost:3000",
                    "http://localhost:5000",
                    "http://localhost:8080",
                    "http://localhost:8000"
                )
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
            policy.WithOrigins(
                    "https://mrshopbangladesh.tech",
                    "https://www.mrshopbangladesh.tech",
                    "https://mrshop-bd.azurewebsites.net"
                )
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MR Shop API",
        Version = "v1",
        Description = "REST API for MR Shop - Bangladesh's #1 Online Marketplace",
        Contact = new OpenApiContact
        {
            Name = "MR Shop Team",
            Email = "api@mrshopbd.com"
        }
    });

    // JWT Bearer Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Example: eyJhbGciOiJIUzI1NiIs..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Serve static files from parent directory (frontend)
var frontendPath = Path.Combine(Directory.GetCurrentDirectory(), "..");
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(frontendPath),
    RequestPath = ""
});

// Swagger only available in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MR Shop API v1");
        options.RoutePrefix = "swagger";
        options.ConfigObject.AdditionalItems["persistAuthorization"] = true;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// Simple in-memory rate limiting middleware
var requestCounts = new System.Collections.Concurrent.ConcurrentDictionary<string, (int Count, DateTime WindowStart)>();
app.Use(async (context, next) =>
{
    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    var now = DateTime.UtcNow;
    var windowMinutes = 1;

    requestCounts.AddOrUpdate(clientIp,
        _ => (1, now),
        (_, existing) =>
        {
            if ((now - existing.WindowStart).TotalMinutes >= windowMinutes)
                return (1, now);
            return (existing.Count + 1, existing.WindowStart);
        });

    if (requestCounts.TryGetValue(clientIp, out var entry) && entry.Count > 100)
    {
        context.Response.StatusCode = 429;
        await context.Response.WriteAsJsonAsync(new { message = "Too many requests. Please try again later." });
        return;
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "MR Shop API",
    version = "1.0.0",
    timestamp = DateTime.UtcNow
}));

// Root endpoint
app.MapGet("/", () => Results.Ok(new
{
    name = "MR Shop API",
    version = "v1",
    documentation = "/swagger",
    health = "/health",
    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
}));

app.Run();
