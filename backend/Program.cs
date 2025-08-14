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
using Microsoft.EntityFrameworkCore;

// Authentication and security imports
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Application service imports
using _241RunnersAwareness.BackendAPI.Services;

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
            
            // Add MVC controllers for API endpoints
            builder.Services.AddControllers();
            
            // Configure Entity Framework with SQL Server
            // Uses connection string from configuration (appsettings.json)
            builder.Services.AddDbContext<RunnersDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
            
            // Advanced platform services
            builder.Services.AddScoped<INotificationService, NotificationService>();           // Notification management
            builder.Services.AddScoped<IRealTimeNotificationService, RealTimeNotificationService>(); // Real-time notifications
            builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();                 // Analytics and reporting
            builder.Services.AddScoped<IImageService, ImageService>();                         // Image processing and storage
            builder.Services.AddScoped<IDNAService, DNAService>();                             // DNA tracking and identification
            builder.Services.AddScoped<SeedDataService, SeedDataService>();                    // Database seeding service
            
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
            
            // Enable Swagger documentation in development and production
            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseSwagger();        // Generate OpenAPI specification
                app.UseSwaggerUI();      // Serve Swagger UI interface
            }

            // Redirect HTTP requests to HTTPS for security
            app.UseHttpsRedirection();

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
            // HEALTH CHECK ENDPOINT
            // ============================================
            
            // Simple health check endpoint for monitoring
            // Returns "Backend is running!" to verify API availability
            app.MapGet("/health", () => "Backend is running!");

            // 
            // ============================================
            // DATABASE SEEDING
            // ============================================
            
            // Seed the database with initial data
            using (var scope = app.Services.CreateScope())
            {
                var seedService = scope.ServiceProvider.GetRequiredService<SeedDataService>();
                await seedService.SeedDataAsync();
            }

            // 
            // ============================================
            // APPLICATION STARTUP
            // ============================================
            
            // Start the web application
            app.Run();
        }
    }
}
