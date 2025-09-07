using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using _241RunnersAwarenessAPI.Data;
using _241RunnersAwarenessAPI.Services;
using _241RunnersAwarenessAPI.Models;
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
        ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
        ?? Environment.GetEnvironmentVariable("ASPNETCORE_ConnectionStrings__DefaultConnection");
    
    // Log configuration sources for debugging
    Console.WriteLine($"Configuration sources checked:");
    Console.WriteLine($"  - Configuration: {builder.Configuration.GetConnectionString("DefaultConnection") != null}");
    Console.WriteLine($"  - Env ConnectionStrings__DefaultConnection: {Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") != null}");
    Console.WriteLine($"  - Env ASPNETCORE_ConnectionStrings__DefaultConnection: {Environment.GetEnvironmentVariable("ASPNETCORE_ConnectionStrings__DefaultConnection") != null}");
    Console.WriteLine($"  - Final connection string found: {!string.IsNullOrEmpty(connectionString)}");
    
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Database connection string not found in configuration or environment variables.");
    }
    
    options.UseSqlServer(connectionString);
});

// Add JWT Service
builder.Services.AddScoped<JwtService>();

// Add CORS with improved configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCors", policy =>
    {
        policy.WithOrigins(
                "https://241runnersawareness.org", 
                "https://www.241runnersawareness.org", 
                "https://dekuworks.github.io", 
                "https://dekuworks.github.io/241RunnersAwareness",
                "http://localhost:3000",
                "http://localhost:8080",
                "http://127.0.0.1:3000",
                "http://127.0.0.1:8080"
              )
              .AllowAnyMethod()
              .AllowCredentials()
              .AllowAnyHeader()
              .SetPreflightMaxAge(TimeSpan.FromSeconds(86400)); // 24 hours
    });
    
    // Add a more permissive policy for development
    options.AddPolicy("DevelopmentCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        string GetJwtKey() => builder.Configuration["Jwt:Key"] 
            ?? Environment.GetEnvironmentVariable("JWT__Key")
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_JWT__Key")
            ?? "241RunnersAwareness2025!SecureJWTKeyForProductionUse-32CharsMin";
            
        string GetJwtIssuer() => builder.Configuration["Jwt:Issuer"] 
            ?? Environment.GetEnvironmentVariable("JWT__Issuer")
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_JWT__Issuer")
            ?? "241RunnersAwareness";
            
        string GetJwtAudience() => builder.Configuration["Jwt:Audience"] 
            ?? Environment.GetEnvironmentVariable("JWT__Audience")
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_JWT__Audience")
            ?? "241RunnersAwareness";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtKey())),
            ValidateIssuer = true,
            ValidIssuer = GetJwtIssuer(),
            ValidateAudience = true,
            ValidAudience = GetJwtAudience(),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Add Authorization
builder.Services.AddAuthorization();

// Register AdminSeedService
builder.Services.AddScoped<AdminSeedService>();

// Register ImageUploadService
// Register DatabaseCleanupService
builder.Services.AddScoped<DatabaseCleanupService>();builder.Services.AddScoped<IImageUploadService, ImageUploadService>();

// Register HttpClient for NamUs service
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger in all environments for API documentation
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "241 Runners Awareness API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "241 Runners Awareness API";
    c.DisplayRequestDuration();
    c.EnableDeepLinking();
    c.EnableFilter();
    c.ShowExtensions();
    c.EnableValidator();
});

app.UseHttpsRedirection();

// Use CORS - MUST be before UseAuthentication and UseAuthorization
app.UseCors("AppCors");

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Add a simple health check endpoint
app.MapGet("/api/health", () => new { status = "healthy", timestamp = DateTime.UtcNow });

// Add a simple CORS test endpoint
app.MapGet("/api/cors-test", () => new { message = "CORS is working!", timestamp = DateTime.UtcNow })
   .WithName("CorsTest")
   .WithOpenApi();

// Add a specific CORS test endpoint for your domain
app.MapGet("/api/cors-test-domain", () => new { 
    message = "CORS is working for 241runnersawareness.org!", 
    timestamp = DateTime.UtcNow,
    allowedOrigins = new[] { 
        "https://241runnersawareness.org", 
        "https://www.241runnersawareness.org" 
    }
})
.WithName("CorsTestDomain")
.WithOpenApi();

// Seed admin users on startup
using (var scope = app.Services.CreateScope())
{
    var adminSeedService = scope.ServiceProvider.GetRequiredService<AdminSeedService>();
    await adminSeedService.SeedAdminUsersAsync();
}

// Configure static files
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
    RequestPath = "/uploads"
});

// Add request logging middleware
app.Use(async (context, next) =>
{
    Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {context.Request.Method} {context.Request.Path} from {context.Request.Headers["Origin"]}");
    await next();
});

app.Run();
