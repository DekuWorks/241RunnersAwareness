using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Diagnostics;
using _241RunnersAPI.Data;
using _241RunnersAPI.Services;
using _241RunnersAPI.Models;
using _241RunnersAPI.Hubs;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.IISIntegration;

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
builder.Services.AddControllers(options =>
{
    // Add request size limits
    options.MaxModelBindingCollectionSize = 100; // Maximum 100 items in collections
    options.MaxModelBindingRecursionDepth = 10; // Maximum recursion depth
});

// Add API versioning support for mobile app compatibility
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Configure request size limits
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "241 Runners Awareness API", 
        Version = "v1",
        Description = "API for the 241 Runners Awareness missing persons case management system",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "241 Runners Awareness",
            Email = "support@241runnersawareness.org"
        }
    });
    
    // Include XML comments for better documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    
    // Add JWT Bearer authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Add SignalR
builder.Services.AddSignalR();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (connectionString == "UseAzureAppSettings")
    {
        // Get connection string from environment variable (Azure App Settings)
        connectionString = Environment.GetEnvironmentVariable("DefaultConnection");
    }
    options.UseSqlServer(connectionString);
});

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

// Add CORS with enhanced security for both web and mobile
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("https://241runnersawareness.org",
                          "https://www.241runnersawareness.org",
                          "http://localhost:5173",
                          "http://localhost:3000",
                          "http://localhost:8080",
                          // Add mobile app origins (Expo development and production)
                          "exp://localhost:19000", // Expo development
                          "exp://192.168.*:*", // Local network for mobile development
                          "exp://10.*:*", // Local network for mobile development
                          "exp://172.*:*", // Local network for mobile development
                          // React Native and mobile app schemes
                          "rn://localhost:*", // React Native development
                          "capacitor://localhost", // Capacitor apps
                          "ionic://localhost", // Ionic apps
                          // Production mobile app identifiers
                          "com.241runnersawareness.app", // iOS bundle identifier
                          "org.241runnersawareness.app") // Android package name
              .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH")
              .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "X-ClientId", "X-Client", 
                          "Accept", "Origin", "Access-Control-Request-Method", 
                          "Access-Control-Request-Headers", "User-Agent", "X-Version", "X-Platform")
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
    
    // Add a more permissive policy for mobile apps during development
    options.AddPolicy("MobileDevelopment", policy =>
    {
        policy.AllowAnyOrigin()
              .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH")
              .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "X-ClientId", "X-Client", 
                          "Accept", "Origin", "Access-Control-Request-Method", 
                          "Access-Control-Request-Headers", "User-Agent", "X-Version", "X-Platform")
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
    
    // Add specific policy for production mobile apps
    options.AddPolicy("MobileProduction", policy =>
    {
        policy.WithOrigins("https://241runnersawareness.org",
                          "https://www.241runnersawareness.org")
              .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH")
              .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "X-ClientId", "X-Client", 
                          "Accept", "Origin", "Access-Control-Request-Method", 
                          "Access-Control-Request-Headers", "User-Agent", "X-Version", "X-Platform")
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

// Add rate limiting services
builder.Services.AddMemoryCache(); // Required for rate limiting
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// Add validation service
builder.Services.AddScoped<ValidationService>();

// Add input sanitization service
builder.Services.AddScoped<InputSanitizationService>();

// Add coordinate validation service
builder.Services.AddScoped<CoordinateValidationService>();

// Add IP validation service
builder.Services.AddScoped<IpValidationService>();

// Add content security service
builder.Services.AddScoped<ContentSecurityService>();

// Add database query validation service
builder.Services.AddScoped<DatabaseQueryValidationService>();

// Add performance monitoring service
builder.Services.AddScoped<PerformanceMonitoringService>();

// Add caching service
builder.Services.AddScoped<CachingService>();

// Add push notification and real-time services
builder.Services.AddScoped<ITopicService, TopicService>();
builder.Services.AddScoped<IFirebaseNotificationService, FirebaseNotificationService>();
builder.Services.AddScoped<ISignalRService, SignalRService>();

// Add response compression with enhanced options
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    
    // Configure compression levels for better performance
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    
    // Add MIME types that should be compressed
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "application/javascript", "text/css", "text/html", "text/json", "text/plain" });
});

// Configure compression levels
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.SmallestSize;
});

// Add distributed memory cache for better performance
builder.Services.AddMemoryCache(options =>
{
    // Remove size limit to avoid conflicts with rate limiting
    options.TrackStatistics = true;
});

// Add response caching
builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 1024 * 1024; // 1MB max response size
    options.UseCaseSensitivePaths = false;
});

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

// Add IP validation middleware
app.Use(async (context, next) =>
{
    var ipValidationService = context.RequestServices.GetRequiredService<IpValidationService>();
    
    // Skip IP validation for health checks and static files
    var path = context.Request.Path.Value?.ToLowerInvariant();
    if (path != null && (path.StartsWith("/health") || path.StartsWith("/api/health") || path.StartsWith("/uploads")))
    {
        await next();
        return;
    }
    
    // Validate request IP, origin, and user agent
    if (!ipValidationService.ValidateRequest(context))
    {
        context.Response.StatusCode = 403;
        await context.Response.WriteAsync("Access denied");
        return;
    }
    
    await next();
});

// Add database query validation middleware
app.Use(async (context, next) =>
{
    var queryValidationService = context.RequestServices.GetRequiredService<DatabaseQueryValidationService>();
    
    // Skip query validation for health checks and static files
    var path = context.Request.Path.Value?.ToLowerInvariant();
    if (path != null && (path.StartsWith("/health") || path.StartsWith("/api/health") || path.StartsWith("/uploads")))
    {
        await next();
        return;
    }
    
    // Validate query parameters in request body
    if (context.Request.HasFormContentType || context.Request.ContentType?.Contains("application/json") == true)
    {
        try
        {
            context.Request.EnableBuffering();
                            var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
                            context.Request.Body.Position = 0;
                            
                            if (!string.IsNullOrWhiteSpace(body))
                            {
                                var queryResult = queryValidationService.ValidateDynamicQuery(body);
                                if (!queryResult.IsValid)
                                {
                                    context.Response.StatusCode = 400;
                                    await context.Response.WriteAsync("Invalid request parameters");
                                    return;
                                }
                            }
        }
        catch (Exception ex)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Error in database query validation middleware");
            // Continue processing on error to avoid breaking the application
        }
    }
    
    await next();
});

// Add security headers middleware
app.Use(async (context, next) =>
{
    // Security headers
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    
    // Content Security Policy
    var csp = "default-src 'self'; " +
              "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
              "style-src 'self' 'unsafe-inline'; " +
              "img-src 'self' data: https:; " +
              "font-src 'self'; " +
              "connect-src 'self' https:; " +
              "frame-ancestors 'none'; " +
              "base-uri 'self'; " +
              "form-action 'self'";
    
    context.Response.Headers.Add("Content-Security-Policy", csp);
    
    await next();
});

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

app.UseResponseCompression();
app.UseResponseCaching();
app.UseHttpsRedirection();

// Add rate limiting middleware
app.UseIpRateLimiting();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map SignalR hubs
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<AlertsHub>("/hubs/alerts");
app.MapHub<UserHub>("/hubs/user");
app.MapHub<AdminHub>("/hubs/admin");

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
        swagger = "/swagger",
        signalrStats = "/api/signalr/stats",
        signalrBroadcast = "/api/signalr/broadcast/*"
    },
    signalrHubs = new {
        userHub = "/hubs/user",
        alertsHub = "/hubs/alerts",
        notificationsHub = "/hubs/notifications",
        adminHub = "/hubs/admin"
    }
}));

// Test endpoint to check if controllers are working
app.MapGet("/api/test", () => Results.Ok(new { 
    message = "Controllers are working", 
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}));

// SignalR connection statistics endpoint
app.MapGet("/api/signalr/stats", async (ISignalRService signalRService) => {
    try
    {
        var stats = await signalRService.GetConnectionStatsAsync();
        return Results.Ok(new { 
            success = true, 
            data = stats,
            timestamp = DateTime.UtcNow 
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Failed to get SignalR stats: {ex.Message}");
    }
});

// SignalR broadcast endpoints for admin functionality
app.MapPost("/api/signalr/broadcast/all", async (ISignalRService signalRService, HttpContext context) => {
    try
    {
        var request = await context.Request.ReadFromJsonAsync<dynamic>();
        var result = await signalRService.SendToAllAsync("broadcast", request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Failed to broadcast message: {ex.Message}");
    }
});

app.MapPost("/api/signalr/broadcast/admins", async (ISignalRService signalRService, HttpContext context) => {
    try
    {
        var request = await context.Request.ReadFromJsonAsync<dynamic>();
        var result = await signalRService.SendToAdminsAsync("admin_broadcast", request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Failed to broadcast to admins: {ex.Message}");
    }
});

app.MapPost("/api/signalr/send/user/{userId}", async (ISignalRService signalRService, int userId, HttpContext context) => {
    try
    {
        var request = await context.Request.ReadFromJsonAsync<dynamic>();
        var result = await signalRService.SendToUserAsync(userId, "user_message", request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Failed to send message to user: {ex.Message}");
    }
});

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

// Debug endpoint to check database state
app.MapGet("/debug/users", async (ApplicationDbContext db) => {
    try {
        var users = await db.Users.Select(u => new { 
            u.Email, 
            u.Role, 
            u.IsActive, 
            u.CreatedAt 
        }).ToListAsync();
        return Results.Ok(new { 
            userCount = users.Count,
            users = users
        });
    } catch (Exception ex) {
        return Results.Ok(new { 
            error = ex.Message,
            stackTrace = ex.StackTrace
        });
    }
});

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

// Database initialization - made more resilient
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Test database connection first
        logger.LogInformation("Testing database connection...");
        var canConnect = await db.Database.CanConnectAsync();
        if (!canConnect)
        {
            logger.LogWarning("Database connection failed, but continuing with application startup");
            logger.LogWarning("API will be available but database operations may fail");
        }
        else
        {
            logger.LogInformation("Database connection successful");
            
            // Apply migrations
            try
            {
                await db.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully");
            }
            catch (Exception migrationEx)
            {
                logger.LogError(migrationEx, "Database migration failed: {Message}", migrationEx.Message);
                logger.LogWarning("Continuing without migrations - database may be in inconsistent state");
            }
            
            // Initialize database with seed data
            try
            {
                await _241RunnersAPI.Data.DbInitializer.Initialize(db, logger);
                logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception initEx)
            {
                logger.LogError(initEx, "Database initialization failed: {Message}", initEx.Message);
                logger.LogWarning("Continuing without seed data initialization");
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Critical error during database setup: {Message}", ex.Message);
        logger.LogWarning("API will start without database initialization - some features may not work");
        // Don't throw - let the app start even if initialization fails
    }
}

app.Run();