using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AtmChallenge.Application.Interfaces;
using AtmChallenge.Application.Services;
using AtmChallenge.Infrastructure.Persistence;
using AtmChallenge.Infrastructure.Repositories;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Load Configuration
var configuration = builder.Configuration;

// 🔹 Add PostgreSQL Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("PostgreSQLConnection")));

// 🔹 Register Services (Dependency Injection)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<UserRepository>();

// 🔹 Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
        };
    });

// 🔹 Enable Authorization
builder.Services.AddAuthorization();

// 🔹 Register Controllers
builder.Services.AddControllers();

// 🔹 Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 🔹 Run Migrations Automatically on Startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        Console.WriteLine("🔹 Applying database migrations...");
        dbContext.Database.Migrate(); // Apply pending migrations
        Console.WriteLine("✅ Migrations applied successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Migration failed: {ex.Message}");
    }
}

// 🔹 Global Exception Handling Middleware
//app.UseMiddleware<GlobalExceptionMiddleware>();

// 🔹 Enable Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔹 Enable HTTPS
//app.UseHttpsRedirection();

// 🔹 Enable Authentication & Authorization Middleware
app.UseAuthentication();
app.UseAuthorization();

// 🔹 Map Controllers
app.MapControllers();

// 🔹 Run Application
app.Run();