using MRShop.OrderTracking.Services;
using MRShop.Authentication.Services;
using MRShop.Profile.Services;
using MRShop.Admin.Services;
using MRShop.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MR Shop - Order Tracking API",
        Version = "v1",
        Description = "REST API for order tracking and management",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "MR Shop",
            Email = "support@mrshop.com"
        }
    });

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register services
builder.Services.AddSingleton<IOrderService, OrderService>();
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IProfileService, ProfileService>();
builder.Services.AddSingleton<IAdminService, AdminService>();
builder.Services.AddSingleton<ISearchService, SearchService>();
builder.Services.AddSingleton<ISellerRegistrationService, SellerRegistrationService>();
builder.Services.AddSingleton<ISellerRequestService, SellerRequestService>();

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MR Shop Order Tracking API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at app root
    });
}

app.UseHttpsRedirection();

// Serve static files (for uploaded profile images)
app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Log startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🚀 MR Shop API started");
logger.LogInformation("📍 Swagger UI: http://localhost:5010");
logger.LogInformation("📍 Order Tracking: http://localhost:5010/api/orders");
logger.LogInformation("📍 Authentication: http://localhost:5010/api/auth");
logger.LogInformation("👤 User Profiles: http://localhost:5010/api/profile");
logger.LogInformation("⚙️  Admin Panel: http://localhost:5010/api/admin");
logger.LogInformation("🔍 Search: http://localhost:5010/api/search");
logger.LogInformation("👨‍💼 Seller Registration: http://localhost:5010/api/sellerregistration");
logger.LogInformation("� Seller Requests: http://localhost:5010/api/sellerrequests");
logger.LogInformation("�📦 Data directory: {DataDir}", builder.Configuration["DataDirectory"] ?? "../data");

app.Run();
