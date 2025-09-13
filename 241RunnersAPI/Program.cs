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

// Add JWT Authentication - Environment variables take precedence
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? builder.Configuration["Jwt:Key"] ?? "your-super-secret-key-that-is-at-least-32-characters-long-for-241-runners";
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"] ?? "241RunnersAwareness";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"] ?? "241RunnersAwareness";

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
        
        // Configure JWT for SignalR
        options.Events = new JwtBearerEvents
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

// Add CORS - Production domains with proper SignalR support
builder.Services.AddCors(options =>
{
    options.AddPolicy("SpaPolicy", p => p
        .WithOrigins(
            "https://241runnersawareness.org",
            "https://www.241runnersawareness.org", 
            "http://localhost:5173",
            "http://localhost:3000",
            "http://localhost:8080"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()); // Required for SignalR connections
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
app.UseCors("SpaPolicy");   // put this BEFORE auth

// CORS is handled by the policy above

// Configure Swagger for production
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "241 Runners Awareness API v1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    // In production, expose Swagger behind auth
    app.UseSwagger();
    app.MapSwagger().RequireAuthorization();
}

// Enable static files for uploaded images
app.UseStaticFiles();

// Add Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers().RequireCors("SpaPolicy");

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

// Add minimal health endpoint
app.MapGet("/healthz", () => Results.Ok(new { 
    status = "ok", 
    time = DateTime.UtcNow
}));

// Add database health check endpoint
app.MapGet("/healthz/db", async (ApplicationDbContext db) =>
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    try
    {
        await db.Database.ExecuteSqlRawAsync("SELECT 1");
        stopwatch.Stop();
        return Results.Ok(new { 
            status = "ok", 
            db = "connected", 
            latencyMs = stopwatch.ElapsedMilliseconds 
        });
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        return Results.Json(new { 
            status = "error", 
            db = "disconnected", 
            latencyMs = stopwatch.ElapsedMilliseconds,
            error = ex.Message 
        }, statusCode: 503);
    }
}).RequireAuthorization();

// Add a simple health endpoint that doesn't require database
app.MapGet("/api/health", () => new { 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName 
});

// Map SignalR hubs with explicit CORS
app.MapHub<_241RunnersAPI.Hubs.AdminHub>("/hubs/notifications")
    .RequireCors("SpaPolicy");
app.MapHub<_241RunnersAPI.Hubs.UserHub>("/hubs/user")
    .RequireCors("SpaPolicy");

// Migrations on startup (safe pattern with try/catch + logs)
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Starting database migration...");
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migration completed successfully");

        // Seed admin user if it doesn't exist
        var adminEmail = "admin@241runnersawareness.org";
        var adminPassword = Environment.GetEnvironmentVariable("SEED_ADMIN_PWD") ?? "Admin2025!";
        
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
        }
        else
        {
            logger.LogInformation("Admin user already exists: {Email}", adminEmail);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database migration or seeding");
        throw; // Re-throw to prevent app startup if critical
    }
}

app.Run();