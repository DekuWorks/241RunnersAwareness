using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using _241RunnersAwarenessAPI.Data;
using _241RunnersAwarenessAPI.Services;
using _241RunnersAwarenessAPI.Models;
using BCrypt.Net;

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
        policy.WithOrigins(
                "https://241runnersawareness.org",
                "https://www.241runnersawareness.org",
                "http://localhost:3000",
                "http://localhost:5173",
                "http://localhost:8080"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Helper function to get JWT configuration from multiple sources
        string GetJwtKey() => builder.Configuration["Jwt:Key"] 
            ?? Environment.GetEnvironmentVariable("JWT__Key")
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_JWT__Key")
            ?? "your-super-secret-key-with-at-least-32-characters";
            
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
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger in all environments for API documentation
app.UseSwagger();
app.UseSwaggerUI();

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
            var canConnect = await context.Database.CanConnectAsync();
            Console.WriteLine($"Database connection test: {(canConnect ? "SUCCESS" : "FAILED")}");
            
            if (!canConnect)
            {
                Console.WriteLine("WARNING: Cannot connect to database. App will start but database features will not work.");
                Console.WriteLine("Please check database connection string and user permissions.");
                // Continue without database - don't fail the app
                goto skipDatabaseInit;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WARNING: Database connection test failed: {ex.Message}");
            Console.WriteLine("App will start but database features will not work.");
            goto skipDatabaseInit;
        }
        
        // Apply any pending migrations first
        try
        {
            context.Database.Migrate();
            Console.WriteLine("Database migrations applied successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WARNING: Database migration error: {ex.Message}");
            Console.WriteLine("This usually means the database user lacks permissions to create/modify tables.");
            Console.WriteLine("App will start but database features will not work.");
            // Continue without migrations - don't fail the app
            goto skipDatabaseInit;
        }
        
        // Wait a moment for database to be fully ready
        await Task.Delay(1000);
        
        // Seed admin users if they don't exist
        try
        {
            await adminSeedService.SeedAdminUsersAsync();
            Console.WriteLine("Admin user seeding completed successfully");
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

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AppCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
