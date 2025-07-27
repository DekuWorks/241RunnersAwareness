using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using _241RunnersAwareness.BackendAPI.DBContext.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using _241RunnersAwareness.BackendAPI.Services;

namespace _241RunnersAwareness.BackendAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = args,
                EnvironmentName = Environments.Development // Use Development environment
            });

            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddDbContext<RunnersDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins(
                            "https://241runnersawareness.org",
                            "https://www.241runnersawareness.org",
                            "https://app.241runnersawareness.org",
                            "http://localhost:3000",
                            "http://localhost:5173",
                            "https://localhost:5173",
                            "http://localhost:5000",
                            "http://localhost:5001",
                            "https://localhost:5001",
                            "http://localhost:5113"
                          )
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            // Add JWT Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "241RunnersAwareness",
                        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "241RunnersAwareness",
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.ASCII.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? "your-super-secret-key-with-at-least-32-characters")
                        )
                    };
                });

            // Add Authorization
            builder.Services.AddAuthorization();

            // Register services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<ISmsService, SmsService>();
            builder.Services.AddScoped<ICsvExportService, CsvExportService>();
            builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();
            
            // Register new services for real-time notifications and analytics
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IRealTimeNotificationService, RealTimeNotificationService>();
            builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
            builder.Services.AddScoped<IImageService, ImageService>();
            builder.Services.AddScoped<IDNAService, DNAService>();
            
            // Add SignalR for real-time notifications
            builder.Services.AddSignalR();

            // 🚀 ADD Swagger service here
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Use CORS
            app.UseCors("AllowAll");

            // Use Authentication and Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            
            // Map SignalR hub for real-time notifications
            app.MapHub<NotificationHub>("/notificationHub");

            // Add a simple health check endpoint
            app.MapGet("/health", () => "Backend is running!");

            app.Run();
        }
    }
}
