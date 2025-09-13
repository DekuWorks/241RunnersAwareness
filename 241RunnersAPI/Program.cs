using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using _241RunnersAPI.Data;
using _241RunnersAPI.Services;
using _241RunnersAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? 
                    builder.Configuration["Jwt:Key"] ?? 
                    "your-super-secret-key-that-is-at-least-32-characters-long-for-241-runners";
        
        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? 
                       builder.Configuration["Jwt:Issuer"] ?? 
                       "241RunnersAwareness";
        
        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? 
                         builder.Configuration["Jwt:Audience"] ?? 
                         "241RunnersAwareness";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("https://241runnersawareness.org",
                          "https://www.241runnersawareness.org",
                          "http://localhost:5173",
                          "http://localhost:3000",
                          "http://localhost:8080")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add validation service
builder.Services.AddScoped<ValidationService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable Swagger in production if configured
var swaggerEnabled = builder.Configuration.GetValue<bool>("Swagger:Enabled", false) ||
                     builder.Configuration.GetValue<bool>("Swagger__Enabled", false);
if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Add API info endpoint
app.MapGet("/api", () => Results.Ok(new { 
    message = "241 Runners Awareness API", 
    version = "1.0",
    status = "operational",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName,
    endpoints = new {
        health = "/api/health",
        auth = "/api/auth/*",
        cases = "/api/cases/*",
        users = "/api/users/*",
        admin = "/api/Admin/*",
        runner = "/api/Runner/*",
        swagger = "/swagger"
    }
}));

// Test endpoint to check if controllers are working
app.MapGet("/api/test", () => Results.Ok(new { 
    message = "Controllers are working", 
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}));

// Test endpoint to check database connectivity
app.MapGet("/api/test-db", async (ApplicationDbContext db) => {
    try {
        var userCount = await db.Users.CountAsync();
        return Results.Ok(new { 
            message = "Database is working", 
            userCount = userCount,
            timestamp = DateTime.UtcNow
        });
    } catch (Exception ex) {
        return Results.Ok(new { 
            message = "Database error", 
            error = ex.Message,
            timestamp = DateTime.UtcNow
        });
    }
});

// Test endpoint to check if controllers are accessible
app.MapGet("/api/test-controllers", () => Results.Ok(new { 
    message = "Controllers are accessible", 
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}));

// Test endpoint to check auth without Entity Framework
app.MapPost("/api/test-auth", (HttpContext context) => {
    try {
        var request = context.Request;
        return Results.Ok(new { 
            message = "Auth test endpoint working", 
            method = request.Method,
            timestamp = DateTime.UtcNow
        });
    } catch (Exception ex) {
        return Results.Ok(new { 
            message = "Auth test error", 
            error = ex.Message,
            timestamp = DateTime.UtcNow
        });
    }
});

// Test endpoint to check Entity Framework with simple query
app.MapGet("/api/test-ef", async (ApplicationDbContext db) => {
    try {
        var userCount = await db.Users.CountAsync();
        var firstUser = await db.Users.FirstOrDefaultAsync();
        return Results.Ok(new { 
            message = "EF test working", 
            userCount = userCount,
            firstUserEmail = firstUser?.Email ?? "No users",
            timestamp = DateTime.UtcNow
        });
    } catch (Exception ex) {
        return Results.Ok(new { 
            message = "EF test error", 
            error = ex.Message,
            timestamp = DateTime.UtcNow
        });
    }
});

// Health check endpoints
app.MapGet("/healthz", () => Results.Ok(new { 
    status = "ok", 
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}));

app.MapGet("/readyz", async (ApplicationDbContext db) =>
{
    try
    {
        await db.Database.CanConnectAsync();
        return Results.Ok(new { 
            status = "ok", 
            database = "connected",
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new { 
            status = "error", 
            database = "disconnected",
            error = ex.Message,
            timestamp = DateTime.UtcNow
        }, statusCode: 503);
    }
});

app.MapGet("/api/health", () => Results.Ok(new { 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}));

// Simple test endpoint
app.MapGet("/", () => Results.Ok(new { 
    message = "Hello from 241 Runners API!",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}));

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");
        
        // Initialize database with seed data
        await _241RunnersAPI.Data.DbInitializer.Initialize(db, logger);
        logger.LogInformation("Database initialization completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to initialize database");
        // Don't throw - let the app start even if initialization fails
    }
}

app.Run();