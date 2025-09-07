using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using _241RunnersAwarenessAPI.Data;
using _241RunnersAwarenessAPI.Services;
using _241RunnersAwarenessAPI.Models;
using _241RunnersAwarenessAPI.Hubs;
using BCrypt.Net;
using DotNetEnv;
using Microsoft.Extensions.FileProviders;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Try to get connection string from multiple sources
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? Environment.GetEnvironmentVariable("DefaultConnection")
        ?? "Data Source=app.db";
    
    options.UseSqlite(connectionString);
});

// Add JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY") ?? "your-super-secret-key-that-is-at-least-32-characters-long";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "241RunnersAwareness";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "241RunnersAwareness";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero // Remove default 5 minute clock skew
        };
        
        // Configure JWT for SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/admin-hub"))
                {
                    context.Token = accessToken;
                }
                
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Add SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
});

// Add CORS - P0 Fix: Proper CORS configuration with named policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCors", policy =>
    {
        policy.WithOrigins(
                "https://241runnersawareness.org",
                "https://www.241runnersawareness.org"
            )
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
            .WithHeaders("Authorization", "Content-Type", "X-CSRF-Token", "X-Client")
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromSeconds(86400)); // 24 hours
    });
});

// Add services
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AdminSeedService>();
builder.Services.AddScoped<ImageUploadService>();
builder.Services.AddScoped<DatabaseCleanupService>();
builder.Services.AddScoped<RealtimeNotificationService>();

// Add rate limiting
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use HTTPS redirection
app.UseHttpsRedirection();

// P0 Fix: Use CORS BEFORE authentication/authorization
app.UseCors("AppCors");

// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Add request logging
app.Use(async (context, next) =>
{
    var requestId = Guid.NewGuid().ToString();
    context.Items["RequestId"] = requestId;
    
    Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {context.Request.Method} {context.Request.Path} - RequestId: {requestId}");
    
    await next();
});

// Serve static files
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
    RequestPath = "/uploads"
});

// Map controllers
app.MapControllers();

// Map SignalR hubs
app.MapHub<AdminHub>("/admin-hub");

// P0 Fix: Fast health check endpoint (no DB, in-process only)
app.MapGet("/healthz", () => new { 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Version = "2.0.0"
}).WithName("HealthCheck");

// P0 Fix: Readiness check (can check DB lightly)
app.MapGet("/readyz", async (ApplicationDbContext context) => {
    try
    {
        // Light DB check with timeout
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var userCount = await context.Users.CountAsync(cts.Token);
        return Results.Ok(new { 
            Status = "Ready", 
            Timestamp = DateTime.UtcNow,
            Database = "Connected",
            UserCount = userCount
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new { 
            Status = "Not Ready", 
            Timestamp = DateTime.UtcNow,
            Error = ex.Message
        });
    }
}).WithName("ReadinessCheck");

// Legacy health endpoint for backward compatibility
app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow });

// CORS test endpoint
app.MapGet("/api/cors-test", () => new { Message = "CORS is working", Timestamp = DateTime.UtcNow })
    .WithName("CorsTest")
    .WithOpenApi();

// Data version endpoint for polling fallback
app.MapGet("/api/data-version", () => new { 
    version = DateTime.UtcNow.Ticks.ToString(),
    timestamp = DateTime.UtcNow
}).WithName("DataVersion");

app.Run();
