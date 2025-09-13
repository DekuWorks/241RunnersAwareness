using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using _241RunnersAPI.Data;
using _241RunnersAPI.Services;
using _241RunnersAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add Application Insights for comprehensive monitoring
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING") ?? 
                              builder.Configuration["ApplicationInsights:ConnectionString"];
    options.EnableAdaptiveSampling = false; // Disable sampling for better monitoring
    options.EnableQuickPulseMetricStream = true; // Enable live metrics
    options.EnablePerformanceCounterCollectionModule = true; // Enable performance counters
    options.EnableEventCounterCollectionModule = true; // Enable event counters
    options.EnableDependencyTrackingTelemetryModule = true; // Enable dependency tracking
    options.EnableAppServicesHeartbeatTelemetryModule = true; // Enable heartbeat
});

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

// Add performance monitoring service
builder.Services.AddScoped<PerformanceMonitoringService>();

// Add caching service
builder.Services.AddMemoryCache();
builder.Services.AddScoped<CachingService>();

// Add comprehensive health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database", tags: new[] { "ready" })
    .AddCheck("api", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is healthy"), tags: new[] { "ready" })
    .AddCheck("memory", () => 
    {
        var memory = GC.GetTotalMemory(false);
        var isHealthy = memory < 100 * 1024 * 1024; // 100MB threshold
        return isHealthy 
            ? Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy($"Memory usage: {memory / 1024 / 1024}MB")
            : Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Degraded($"High memory usage: {memory / 1024 / 1024}MB");
    }, tags: new[] { "ready" });

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

// Add performance logging middleware
app.Use(async (context, next) =>
{
    var startTime = DateTime.UtcNow;
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Request started: {Method} {Path} at {StartTime}", 
        context.Request.Method, context.Request.Path, startTime);
    
    await next();
    
    var endTime = DateTime.UtcNow;
    var duration = endTime - startTime;
    
    logger.LogInformation("Request completed: {Method} {Path} in {Duration}ms with status {StatusCode}", 
        context.Request.Method, context.Request.Path, duration.TotalMilliseconds, context.Response.StatusCode);
    
    // Log slow requests
    if (duration.TotalMilliseconds > 1000)
    {
        logger.LogWarning("Slow request detected: {Method} {Path} took {Duration}ms", 
            context.Request.Method, context.Request.Path, duration.TotalMilliseconds);
    }
});

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

// Simple test endpoint to verify deployment
app.MapGet("/api/simple-test", () => Results.Ok(new { 
    message = "Simple test working", 
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}));

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

// Comprehensive health check endpoints
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            environment = app.Environment.EnvironmentName,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration.TotalMilliseconds,
                data = entry.Value.Data
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

// Performance monitoring endpoint
app.MapGet("/api/performance", (PerformanceMonitoringService performanceService) =>
{
    // Track memory usage
    performanceService.TrackMemoryUsage();
    
    var memory = GC.GetTotalMemory(false);
    var gen0Collections = GC.CollectionCount(0);
    var gen1Collections = GC.CollectionCount(1);
    var gen2Collections = GC.CollectionCount(2);
    
    return Results.Ok(new
    {
        timestamp = DateTime.UtcNow,
        memory = new
        {
            totalBytes = memory,
            totalMB = Math.Round(memory / 1024.0 / 1024.0, 2),
            gen0Collections = gen0Collections,
            gen1Collections = gen1Collections,
            gen2Collections = gen2Collections
        },
        environment = app.Environment.EnvironmentName,
        uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()
    });
});

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