using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using _241RunnersAPI.Data;
using _241RunnersAPI.Services;
using _241RunnersAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Enable detailed startup logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Optional: Enhanced logging with Serilog (uncomment if needed)
// builder.Host.UseSerilog((ctx, cfg) => cfg
//   .ReadFrom.Configuration(ctx.Configuration)
//   .Enrich.FromLogContext()
//   .WriteTo.Console());

// Add services to the container.
builder.Services.AddControllers();

// Add SignalR for real-time communication
builder.Services.AddSignalR();

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add validation service
builder.Services.AddScoped<ValidationService>();

// JWT wiring (prevent 401/403 surprises)
builder.Services
  .AddAuthentication("Bearer")
  .AddJwtBearer(o =>
  {
    o.RequireHttpsMetadata = true;
    
    // Fallback to environment variables if config is not set
    var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? builder.Configuration["Jwt:Key"] ?? "your-super-secret-key-that-is-at-least-32-characters-long-for-241-runners";
    var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"] ?? "241RunnersAwareness";
    var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"] ?? "241RunnersAwareness";
    
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
        
        // Configure JWT for SignalR
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                
                // If the request is for the SignalR hub
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    // Read the token out of the query string
                    context.Token = accessToken;
                }
                // Also check Authorization header for SignalR connections
                else if (path.StartsWithSegments("/hubs"))
                {
                    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                    {
                        context.Token = authHeader.Substring("Bearer ".Length).Trim();
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database");

// CORS policy (allow exact sites only)
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("web", p => p
        .WithOrigins("https://241runnersawareness.org",
                     "https://www.241runnersawareness.org",
                     "http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "241 Runners Awareness API", 
        Version = "v1",
        Description = "API for 241 Runners Awareness missing persons platform"
    });
    
    // Add JWT authentication to Swagger
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

var app = builder.Build();

// Configure port for Azure App Service (Linux)
// For Linux App Service, use the PORT environment variable
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
if (Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") != null)
{
    // Running on Azure App Service
    app.Urls.Add($"http://0.0.0.0:{port}");
}

// Configure the HTTP request pipeline.

// Force HTTPS redirect in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors("web");   // put this BEFORE auth

// Security headers
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    ctx.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    ctx.Response.Headers.TryAdd("Referrer-Policy", "no-referrer");
    await next();
});

// Rate limiting for auth endpoints
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/api/auth/login"))
    {
        // Simple in-memory rate limiting (for production, use Redis)
        var clientIp = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var key = $"rate_limit_{clientIp}";
        
        // Check if we have a rate limit entry (simplified - in production use proper rate limiting)
        // For now, we'll just log the attempt
        var logger = ctx.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Login attempt from IP: {ClientIp}", clientIp);
        
        // In production, implement proper rate limiting with Redis or similar
        // await rateLimiter.AssertWithinLimitAsync(ctx.Connection.RemoteIpAddress);
    }
    await next();
});

// CORS is handled by the policy above

// Swagger visibility - enable based on configuration
var swaggerEnabled = builder.Configuration.GetValue<bool>("Swagger:Enabled", false) ||
                     builder.Configuration.GetValue<bool>("Swagger__Enabled", false);
if (app.Environment.IsDevelopment() || swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Optional gated: app.MapSwagger().RequireAuthorization(new AuthorizeAttribute{ Roles="Admin" });

// Enable static files for uploaded images
app.UseStaticFiles();

// Add Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers().RequireCors("web");

// Map health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/api/auth/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString().ToLower(),
            timestamp = DateTime.UtcNow,
            environment = app.Environment.EnvironmentName
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

// Robust health endpoints - liveness vs readiness
// /healthz must not touch DB (stays green while DB is booting)
app.MapGet("/healthz", () => Results.Ok(new { 
    status = "ok", 
    time = DateTime.UtcNow
}));

// /readyz hits DB for readiness
app.MapGet("/readyz", async (ApplicationDbContext db) =>
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    try
    {
        await db.Database.CanConnectAsync();
        sw.Stop();
        return Results.Ok(new { 
            status = "ok", 
            db = "connected", 
            latencyMs = sw.ElapsedMilliseconds 
        });
    }
    catch (Exception ex)
    {
        sw.Stop();
        return Results.Json(new { 
            status = "error", 
            db = "disconnected", 
            latencyMs = sw.ElapsedMilliseconds,
            error = ex.Message 
        }, statusCode: 503);
    }
});

// Add a simple health endpoint that doesn't require database
app.MapGet("/api/health", () => new { 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName 
});

// Map SignalR hubs with explicit CORS
app.MapHub<_241RunnersAPI.Hubs.AdminHub>("/hubs/notifications")
    .RequireCors("web");
app.MapHub<_241RunnersAPI.Hubs.UserHub>("/hubs/user")
    .RequireCors("web");

// EF Core migrations run safely on startup
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Starting database migration...");
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        logger.LogInformation("EF migrations applied.");

        // Seed admin user if it doesn't exist (only when SEED_ADMIN_PWD is set)
        var adminEmail = "admin@241runnersawareness.org";
        var adminPassword = Environment.GetEnvironmentVariable("SEED_ADMIN_PWD");
        
        if (!string.IsNullOrWhiteSpace(adminPassword))
        {
            var existingAdmin = await db.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            if (existingAdmin == null)
            {
                logger.LogInformation("Seeding admin user...");
                var adminUser = new User
                {
                    Email = adminEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                    FirstName = "System",
                    LastName = "Administrator",
                    Role = "admin",
                    IsActive = true,
                    IsEmailVerified = true,
                    IsPhoneVerified = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailVerifiedAt = DateTime.UtcNow,
                    PhoneVerifiedAt = DateTime.UtcNow,
                    Organization = "241 Runners Awareness",
                    Title = "System Administrator"
                };
                
                db.Users.Add(adminUser);
                await db.SaveChangesAsync();
                logger.LogInformation("Admin user seeded successfully: {Email}", adminEmail);
                logger.LogWarning("⚠️ IMPORTANT: Remove SEED_ADMIN_PWD from Azure App Service Configuration after first admin login!");
            }
            else
            {
                logger.LogInformation("Admin user already exists: {Email}", adminEmail);
                logger.LogWarning("⚠️ Consider removing SEED_ADMIN_PWD from Azure App Service Configuration for security");
            }
        }
        else
        {
            logger.LogInformation("SEED_ADMIN_PWD not set, skipping admin user seeding");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to apply EF migrations on startup.");
        // Do not throw; keep app up for /healthz visibility.
    }
}

app.Run();