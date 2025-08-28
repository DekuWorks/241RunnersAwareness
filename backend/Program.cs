/**
 * ============================================
 * 241 RUNNERS AWARENESS - ASP.NET CORE BACKEND API
 * ============================================
 * 
 * This is the main entry point for the 241 Runners Awareness backend API.
 * It configures the web application, services, middleware, and routing.
 * 
 * Application Features:
 * - RESTful API endpoints for all platform functionality
 * - JWT-based authentication and authorization
 * - Entity Framework Core with SQL Server
 * - Real-time notifications with SignalR
 * - CORS support for cross-origin requests
 * - Swagger API documentation
 * - Multiple service integrations (email, SMS, DNA, etc.)
 * 
 * Architecture:
 * - Clean Architecture with service layer
 * - Repository pattern with Entity Framework
 * - Dependency injection for loose coupling
 * - Middleware pipeline for request processing
 */

// ASP.NET Core framework imports
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Database and Entity Framework imports
using _241RunnersAwareness.BackendAPI.DBContext.Data;
using _241RunnersAwareness.BackendAPI.DBContext.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;

// Authentication and security imports
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Application service imports
using _241RunnersAwareness.BackendAPI.Services;

// SignalR imports
using Microsoft.AspNetCore.SignalR;
using _241RunnersAwareness.BackendAPI.Hubs;

// Performance and monitoring imports
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.RateLimiting;

// Logging and monitoring imports
using Serilog;
using Serilog.Events;
using Sentry;
using Sentry.AspNetCore;

namespace _241RunnersAwareness.BackendAPI
{
    /**
     * Main Program Class
     * 
     * Configures and starts the ASP.NET Core web application.
     * Sets up all services, middleware, and application pipeline.
     */
    public class Program
    {
        /**
         * Application Entry Point
         * 
         * Creates and configures the web application with all necessary
         * services, middleware, and endpoints.
         * 
         * @param args Command line arguments
         */
        public static async Task Main(string[] args)
        {
            // 
            // ============================================
            // ENVIRONMENT VARIABLES LOADING
            // ============================================
            
            // Load environment variables from .env file for development
            var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (File.Exists(envPath))
            {
                foreach (var line in File.ReadAllLines(envPath))
                {
                    if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                    {
                        var parts = line.Split('=', 2);
                        if (parts.Length == 2)
                        {
                            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
                        }
                    }
                }
            }
            
            // Also load from env.local if it exists (fallback)
            var envLocalPath = Path.Combine(Directory.GetCurrentDirectory(), "env.local");
            if (File.Exists(envLocalPath))
            {
                foreach (var line in File.ReadAllLines(envLocalPath))
                {
                    if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                    {
                        var parts = line.Split('=', 2);
                        if (parts.Length == 2)
                        {
                            // Only set if not already set by .env file
                            if (Environment.GetEnvironmentVariable(parts[0].Trim()) == null)
                            {
                                Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
                            }
                        }
                    }
                }
            }
            
            // 
            // ============================================
            // LOGGING CONFIGURATION
            // ============================================
            
            // Configure Serilog for structured logging
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs/app-.txt", 
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    retainedFileCountLimit: 30,
                    fileSizeLimitBytes: 10 * 1024 * 1024) // 10MB
                // TODO: Configure Sentry sink properly
                // .WriteTo.Sentry(o =>
                // {
                //     o.Dsn = "https://your-sentry-dsn@your-sentry-instance.ingest.sentry.io/your-project-id"; // Replace with actual Sentry DSN
                //     o.Environment = "development";
                //     o.Release = "241runners-awareness@1.0.0";
                //     o.TracesSampleRate = 1.0;
                //     o.SendDefaultPii = false;
                // })
                .CreateLogger();

            try
            {
                Log.Information("Starting 241 Runners Awareness API");
                
                // 
                // ============================================
                // APPLICATION BUILDER CONFIGURATION
                // ============================================
                
                // Create web application builder with development environment
                var builder = WebApplication.CreateBuilder(new WebApplicationOptions
                {
                    Args = args,
                    EnvironmentName = Environments.Development // Use Development environment
                });

            // 
            // ============================================
            // CORE SERVICES REGISTRATION
            // ============================================
            
            // Add Serilog to the application
            builder.Host.UseSerilog();
            
            // Add Sentry to the application (only if DSN is configured)
            var sentryDsn = builder.Configuration["Sentry:Dsn"];
            if (!string.IsNullOrEmpty(sentryDsn) && sentryDsn != "https://your-sentry-dsn@your-sentry-instance.ingest.sentry.io/your-project-id")
            {
                builder.WebHost.UseSentry(options =>
                {
                    options.Dsn = sentryDsn;
                    options.Environment = "development";
                    options.Release = "241runners-awareness@1.0.0";
                    options.TracesSampleRate = 1.0;
                    options.SendDefaultPii = false;
                });
            }
            
            // Add MVC controllers for API endpoints
            builder.Services.AddControllers();
            
            // Configure Entity Framework with SQLite for both development and production
            builder.Services.AddDbContext<RunnersDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                
                // Use SQLite for both development and production (quick fix)
                options.UseSqlite(connectionString);
                
                // Enable query tracking only when needed for better performance
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            // 
            // ============================================
            // PERFORMANCE OPTIMIZATIONS
            // ============================================
            
            // Add response compression for better performance
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
                options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
            });

            // Add rate limiting to prevent abuse
            builder.Services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 100, // Default value
                            Window = TimeSpan.FromMinutes(1) // Default value
                        }));
            });

            // Add health checks for monitoring
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<RunnersDbContext>("database")
                .AddCheck("memory", () =>
                {
                    var memoryThreshold = 1024L; // Default value in MB
                    var memoryUsage = GC.GetTotalMemory(false) / 1024 / 1024; // MB
                    return memoryUsage < memoryThreshold ? HealthCheckResult.Healthy() : HealthCheckResult.Degraded();
                });

            // 
            // ============================================
            // CORS CONFIGURATION
            // ============================================
            
            // Configure Cross-Origin Resource Sharing (CORS)
            // Allows frontend applications to access the API
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins(
                            // Production domains
                            "https://241runnersawareness.org",
                            "https://www.241runnersawareness.org",
                            "https://app.241runnersawareness.org",
                            
                            // Development and testing domains
                            "http://localhost:3000",      // React development server
                            "http://localhost:5173",      // Vite development server
                            "https://localhost:5173",     // Vite HTTPS
                            "http://localhost:5000",      // ASP.NET Core default
                            "http://localhost:5001",      // ASP.NET Core HTTPS
                            "https://localhost:5001",     // ASP.NET Core HTTPS
                            "http://localhost:5113",      // Alternative dev port
                            "http://localhost:8080",      // Common dev port
                            "http://localhost:5500",      // Live Server
                            "http://localhost:4000",      // Alternative dev port
                            "http://localhost:3001",      // Alternative React port
                            
                            // IP-based localhost alternatives
                            "http://127.0.0.1:5500",
                            "http://127.0.0.1:8080",
                            "http://127.0.0.1:3000",
                            
                            // File protocol for local development
                            "file://"
                          )
                          .AllowAnyMethod()      // Allow all HTTP methods (GET, POST, PUT, DELETE, etc.)
                          .AllowAnyHeader()      // Allow all request headers
                          .AllowCredentials();   // Allow cookies and authentication headers
                });
            });

            // 
            // ============================================
            // AUTHENTICATION CONFIGURATION
            // ============================================
            
            // Configure JWT (JSON Web Token) authentication
            // Provides secure token-based authentication for API access
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,           // Validate token issuer
                        ValidateAudience = true,         // Validate token audience
                        ValidateLifetime = true,         // Validate token expiration
                        ValidateIssuerSigningKey = true, // Validate token signature
                        
                        // Token issuer and audience (from configuration or defaults)
                        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "241RunnersAwareness",
                        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "241RunnersAwareness",
                        
                        // Signing key for token validation (from configuration or default)
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.ASCII.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? "your-super-secret-key-with-at-least-32-characters")
                        )
                    };
                });

            // Add authorization services for role-based access control
            builder.Services.AddAuthorization();

            // 
            // ============================================
            // APPLICATION SERVICES REGISTRATION
            // ============================================
            
            // Core authentication and communication services
            builder.Services.AddScoped<IAuthService, AuthService>();           // User authentication
            builder.Services.AddScoped<IEmailService, EmailService>();         // Email notifications
            builder.Services.AddScoped<ISmsService, SmsService>();             // SMS notifications
            builder.Services.AddScoped<ICsvExportService, CsvExportService>(); // Data export functionality
            builder.Services.AddScoped<ITwoFactorService, TwoFactorService>(); // 2FA implementation
            builder.Services.AddScoped<IPasswordResetService, PasswordResetService>(); // Password reset functionality
            
            // Advanced platform services
            builder.Services.AddScoped<INotificationService, NotificationService>();           // Notification management
            builder.Services.AddScoped<IRealTimeNotificationService, RealTimeNotificationService>(); // Real-time notifications
            builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();                 // Analytics and reporting
            builder.Services.AddScoped<IImageService, ImageService>();                         // Image processing and storage
            builder.Services.AddScoped<IDNAService, DNAService>();                             // DNA tracking and identification
            builder.Services.AddScoped<SeedDataService, SeedDataService>();                    // Database seeding service
            builder.Services.AddScoped<IDataCleanupService, DataCleanupService>();             // Database cleanup and maintenance
            
            // 
            // ============================================
            // REAL-TIME COMMUNICATION
            // ============================================
            
            // Add SignalR for real-time notifications and updates
            // Enables WebSocket connections for live data updates
            builder.Services.AddSignalR();

            // 
            // ============================================
            // API DOCUMENTATION
            // ============================================
            
            // Add Swagger/OpenAPI documentation services
            // Provides interactive API documentation at /swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // 
            // ============================================
            // APPLICATION BUILD AND CONFIGURATION
            // ============================================
            
            // Build the web application
            var app = builder.Build();

            // 
            // ============================================
            // MIDDLEWARE PIPELINE CONFIGURATION
            // ============================================
            
            // Configure the HTTP request pipeline
            // Middleware order is important for proper request processing
            
            // Enable response compression (must be early in pipeline)
            app.UseResponseCompression();
            
            // Enable rate limiting
            app.UseRateLimiter();
            
            // Enable Swagger documentation in development and production
            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseSwagger();        // Generate OpenAPI specification
                app.UseSwaggerUI();      // Serve Swagger UI interface
            }

            // Redirect HTTP requests to HTTPS for security (disabled in development)
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            // Enable CORS with the configured policy
            app.UseCors("AllowAll");

            // Enable authentication and authorization middleware
            app.UseAuthentication();     // Validate JWT tokens
            app.UseAuthorization();      // Check user permissions

            // 
            // ============================================
            // ENDPOINT MAPPING
            // ============================================
            
            // Map API controllers for REST endpoints
            app.MapControllers();
            
            // Map SignalR hub for real-time notifications
            // Clients can connect to /notificationHub for live updates
            app.MapHub<NotificationHub>("/notificationHub");

            // 
            // ============================================
            // HEALTH CHECK ENDPOINTS
            // ============================================
            
            // Comprehensive health check endpoints for monitoring
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description,
                            duration = e.Value.Duration.ToString()
                        })
                    };
                    await context.Response.WriteAsJsonAsync(response);
                }
            });
            
            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready")
            });
            
            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false
            });

            // 
            // ============================================
            // DATABASE SEEDING
            // ============================================
            
            // Seed the database with initial data (non-blocking)
            try
            {
                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<RunnersDbContext>();
                    await context.Database.EnsureCreatedAsync();
                    
                    // Create main admin user if it doesn't exist
                    if (!context.Users.Any(u => u.Email == "contact@241runnersawareness.org"))
                    {
                        var mainAdmin = new User
                        {
                            UserId = Guid.NewGuid(),
                            Email = "contact@241runnersawareness.org",
                            FirstName = "Main",
                            LastName = "Admin",
                            FullName = "Main Admin",
                            Username = "main_admin",
                            Role = "superadmin",
                            Organization = "241 Runners Awareness",
                            Credentials = "System Administrator",
                            Specialization = "System Management",
                            YearsOfExperience = "5+",
                            EmailVerified = true,
                            PhoneVerified = true,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            LastLoginAt = DateTime.UtcNow
                        };
                        
                        // Hash the password
                        mainAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("runners241@");
                        
                        context.Users.Add(mainAdmin);
                        await context.SaveChangesAsync();
                        
                        Log.Information("Main admin user created successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error seeding database - continuing startup");
            }
            
            // 
            // ============================================
            // APPLICATION STARTUP
            // ============================================
            
            // Start the web application
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
        }
    }
}
