using MRShop.Search;
using Microsoft.EntityFrameworkCore;
using MRShop.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddScoped<SearchService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("*")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<MRShopDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowLocalhost");
app.UseAuthorization();
app.MapControllers();

var port = 5010;
app.Urls.Add($"http://localhost:{port}");

Console.WriteLine($"🚀 MR Shop API running on http://localhost:{port}");
Console.WriteLine($"📚 Swagger UI available at http://localhost:{port}/swagger");

app.Run();
