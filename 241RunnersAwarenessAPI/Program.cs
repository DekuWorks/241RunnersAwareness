using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using _241RunnersAwarenessAPI.Data;
using _241RunnersAwarenessAPI.Services;
using _241RunnersAwarenessAPI.Models;
using BCrypt.Net;
using DotNetEnv;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Try to get connection string from multiple sources
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
        ?? Environment.GetEnvironmentVariable("ASPNETCORE_ConnectionStrings__DefaultConnection");
    
    // Log configuration sources for debugging
    Console.WriteLine($"Configuration sources checked:");
    Console.WriteLine($"  - Configuration: {builder.Configuration.GetConnectionString("DefaultConnection") != null}");
    Console.WriteLine($"  - Env ConnectionStrings__DefaultConnection: {Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") != null}");
    Console.WriteLine($"  - Env ASPNETCORE_ConnectionStrings__DefaultConnection: {Environment.GetEnvironmentVariable("ASPNETCORE_ConnectionStrings__DefaultConnection") != null}");
    Console.WriteLine($"  - Final connection string found: {!string.IsNullOrEmpty(connectionString)}");
    
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Database connection string not found in configuration or environment variables.");
    }
    
    options.UseSqlServer(connectionString);
});

// Add JWT Service
builder.Services.AddScoped<JwtService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCors", policy =>
    {
        policy.WithOrigins("https://241runnersawareness.org", "https://www.241runnersawareness.org", "https://dekuworks.github.io", "https://dekuworks.github.io/241RunnersAwareness")
              .AllowAnyMethod()
              .AllowCredentials()
              .WithHeaders("Content-Type", "Authorization", "X-Client", "WWW-Authenticate", "Set-Cookie");
    });
});

// Add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        string GetJwtKey() => builder.Configuration["Jwt:Key"] 
            ?? Environment.GetEnvironmentVariable("JWT__Key")
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_JWT__Key")
            ?? "241RunnersAwareness2025!SecureJWTKeyForProductionUse-32CharsMin";
            
        string GetJwtIssuer() => builder.Configuration["Jwt:Issuer"] 
            ?? Environment.GetEnvironmentVariable("JWT__Issuer")
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_JWT__Issuer")
            ?? "241RunnersAwareness";
            
        string GetJwtAudience() => builder.Configuration["Jwt:Audience"] 
            ?? Environment.GetEnvironmentVariable("JWT__Audience")
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_JWT__Audience")
            ?? "241RunnersAwareness";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtKey())),
            ValidateIssuer = true,
            ValidIssuer = GetJwtIssuer(),
            ValidateAudience = true,
            ValidAudience = GetJwtAudience(),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Add Authorization
builder.Services.AddAuthorization();

// Register AdminSeedService
builder.Services.AddScoped<AdminSeedService>();

// Register ImageUploadService
// Register DatabaseCleanupService
builder.Services.AddScoped<DatabaseCleanupService>();builder.Services.AddScoped<IImageUploadService, ImageUploadService>();

// Register HttpClient for NamUs service
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger in all environments for API documentation
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "241 Runners Awareness API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "241 Runners Awareness API";
    c.DisplayRequestDuration();
    c.EnableDeepLinking();
    c.EnableFilter();
    c.ShowExtensions();
    c.EnableValidator();
});

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AppCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Initialize database, apply migrations, and seed admin users
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var adminSeedService = scope.ServiceProvider.GetRequiredService<AdminSeedService>();
        
        Console.WriteLine("Starting database initialization...");
        
        // Test database connection first
        try
        {
            context.Database.EnsureCreated();
            Console.WriteLine("Database connection successful");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database connection failed: {ex.Message}");
            goto skipDatabaseInit;
        }
        
        // Apply migrations
        try
        {
            context.Database.Migrate();
            Console.WriteLine("Database migrations applied successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Migration failed: {ex.Message}");
            goto skipDatabaseInit;
        }
        
        // Seed admin users
        try
        {
            await adminSeedService.SeedAdminUsersAsync();
            Console.WriteLine("Admin users seeded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WARNING: Admin user seeding failed: {ex.Message}");
            Console.WriteLine("Admin users may not be available.");
        }
        
        Console.WriteLine("Database initialization completed successfully");
        goto databaseInitComplete;
        
        skipDatabaseInit:
        Console.WriteLine("Skipping database initialization due to connection/permission issues.");
        Console.WriteLine("The app will start but database-dependent features will not work.");
        Console.WriteLine("Please check:");
        Console.WriteLine("  1. Database connection string is correct");
        Console.WriteLine("  2. Database user 'sqladmin' has db_owner permissions");
        Console.WriteLine("  3. Database server firewall allows Azure App Service connections");
        
        databaseInitComplete:
        Console.WriteLine("Database initialization process completed.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"WARNING: Database initialization failed: {ex.Message}");
    Console.WriteLine("App will continue to start but database features will not work.");
    // Don't throw - let the app start even if database fails
}

app.Run();
