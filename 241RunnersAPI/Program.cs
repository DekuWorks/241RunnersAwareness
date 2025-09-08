using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;
using _241RunnersAPI.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

// Add services to the container.
builder.Services.AddControllers();

// Add SignalR for real-time communication
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
});

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Swagger/OpenAPI documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "241 Runners Awareness API",
        Version = "v1",
        Description = "API for managing missing person cases and runner safety with comprehensive input validation",
        Contact = new OpenApiContact
        {
            Name = "241 Runners Awareness",
            Email = "support@241runnersawareness.org"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License"
        }
    });

    // Configure JWT authentication for Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY") ?? "your-super-secret-key-that-is-at-least-32-characters-long-for-241-runners";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "241RunnersAwareness";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "241RunnersAwareness";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
        ClockSkew = TimeSpan.Zero
    };
    
    // Configure JWT for SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/adminHub"))
            {
                context.Token = accessToken;
            }
            
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCors", policy =>
    {
        policy.WithOrigins(
                "https://241runnersawareness.org",
                "https://www.241runnersawareness.org",
                "http://localhost:5000", // For local development
                "https://localhost:5001" // For local HTTPS development
            )
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
            .WithHeaders("Authorization", "Content-Type", "X-CSRF-Token", "X-Client")
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromSeconds(86400)); // 24 hours
    });

    // Add a more permissive policy for Swagger UI
    options.AddPolicy("SwaggerCors", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Log startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting 241 Runners Awareness API v4.0.0");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("Application started at: {StartTime}", DateTime.UtcNow);

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

// Add error handling middleware
app.UseExceptionHandler("/error");

app.UseRouting(); // Must be before UseCors, UseAuthentication, UseAuthorization

app.UseCors("AppCors"); // Apply the main CORS policy

app.UseAuthentication();
app.UseAuthorization();

// Enable Swagger in all environments for API documentation
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.RoutePrefix = "swagger";
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "241Runners API v1");
    c.DocumentTitle = "241 Runners Awareness API";
    c.DefaultModelsExpandDepth(-1); // Hide models section by default
    c.DisplayRequestDuration();
    c.EnableDeepLinking();
    c.EnableFilter();
    c.ShowExtensions();
    c.EnableValidator();
});

app.MapControllers();

// Map SignalR hubs
app.MapHub<AdminHub>("/adminHub");

// Health check endpoints that don't require database connectivity
app.MapGet("/healthz", () => Results.Ok(new
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Version = "4.0.0",
    Features = new[] { "Input Validation", "JWT Authentication", "Role-based Access", "Comprehensive User Management" }
})).WithName("HealthCheck");

app.MapGet("/readyz", () => Results.Ok(new { 
    Status = "Ready", 
    Timestamp = DateTime.UtcNow,
    Database = "Available on demand",
    Features = new[] { "Input Validation", "JWT Authentication", "Role-based Access", "Comprehensive User Management" }
})).WithName("ReadinessCheck");

// Legacy health endpoint
app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow });

// API health endpoint for frontend monitoring
app.MapGet("/api/auth/health", () => Results.Ok(new { status = "Healthy" }));

// CORS test endpoint
app.MapGet("/cors-test", () => Results.Ok(new { Message = "CORS is working!", Timestamp = DateTime.UtcNow }))
    .WithName("CorsTest");

// Error endpoint
app.MapGet("/error", () => Results.Problem(
    detail: "An error occurred while processing your request",
    statusCode: 500,
    title: "Internal Server Error"
)).WithName("ErrorEndpoint");

// API Info endpoint
app.MapGet("/api/info", () => Results.Ok(new
{
    Name = "241 Runners Awareness API",
    Version = "4.0.0",
    Description = "API for managing missing person cases and runner safety with comprehensive input validation",
    Features = new[] {
        "Comprehensive Input Validation",
        "JWT Authentication",
        "Role-based Access Control",
        "User Management",
        "Profile Management",
        "Password Security",
        "Account Lockout Protection",
        "Email Verification",
        "Phone Verification",
        "Emergency Contact Management"
    },
    Endpoints = new[] {
        "POST /api/auth/register - Register new user",
        "POST /api/auth/login - User login",
        "GET /api/auth/me - Get current user",
        "PUT /api/auth/profile - Update user profile",
        "POST /api/auth/change-password - Change password",
        "POST /api/auth/logout - Logout user"
    },
    Timestamp = DateTime.UtcNow
}));

// Test endpoint that doesn't require database
app.MapGet("/api/test", () => Results.Ok(new { 
    Message = "241 Runners Awareness API is working!",
    Timestamp = DateTime.UtcNow,
    Version = "4.0.0",
    Status = "Operational"
})).WithName("TestEndpoint");

// Initialize database with seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DbInitializer.Initialize(context, logger);
}

// Log startup completion
logger.LogInformation("241 Runners Awareness API startup completed successfully");
logger.LogInformation("API is ready to accept requests");

app.Run();