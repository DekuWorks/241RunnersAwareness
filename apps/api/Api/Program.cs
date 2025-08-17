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
using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

// Authentication and security imports
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Application service imports
using Api.Services;

// Performance and monitoring imports
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.RateLimiting;

// Swagger and SignalR imports
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.SignalR;

namespace Api
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
            var builder = WebApplication.CreateBuilder(args);

            // 
            // ============================================
            // CORE SERVICES REGISTRATION
            // ============================================
            
            // Add MVC controllers for API endpoints
            builder.Services.AddControllers();
            
            // Configure CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // 
            // ============================================
            // APPLICATION SERVICES
            // ============================================
            
            // Register application services for dependency injection
            builder.Services.AddScoped<TokenService>();

            // 
            // ============================================
            // APPLICATION BUILDING
            // ============================================
            
            // Build the web application
            var app = builder.Build();

            // 
            // ============================================
            // MIDDLEWARE PIPELINE CONFIGURATION
            // ============================================
            
            // Configure the HTTP request pipeline
            // Middleware order is important for proper request processing
            
            // Enable CORS with the configured policy
            app.UseCors("AllowAll");

            // Map API controllers for REST endpoints
            app.MapControllers();

            // Add a simple test endpoint
            app.MapGet("/test", () => new { message = "API is working!", timestamp = DateTime.UtcNow });

            // 
            // ============================================
            // APPLICATION STARTUP
            // ============================================
            
            // Start the web application
            app.Run();
        }
    }
}
