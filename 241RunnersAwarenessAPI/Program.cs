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
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
                "https://dekuworks.github.io",
                "https://dekuworks.github.io/241RunnersAwareness"
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
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "your-super-secret-key-with-at-least-32-characters")),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "241RunnersAwareness",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "241RunnersAwareness",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Add Authorization
builder.Services.AddAuthorization();

// Register AdminSeedService
builder.Services.AddScoped<AdminSeedService>();

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
        
        // Apply any pending migrations first
        try
        {
            context.Database.Migrate();
            Console.WriteLine("Database migrations applied successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database migration error: {ex.Message}");
            // Fallback to EnsureCreated if migrations fail
            try
            {
                context.Database.EnsureCreated();
                Console.WriteLine("Database initialized with EnsureCreated");
            }
            catch (Exception ex2)
            {
                Console.WriteLine($"Database initialization error: {ex2.Message}");
                throw; // Re-throw if we can't initialize the database
            }
        }
        
        // Wait a moment for database to be fully ready
        await Task.Delay(1000);
        
        // Seed admin users if they don't exist
        await adminSeedService.SeedAdminUsersAsync();
        
        Console.WriteLine("Database initialization and admin user seeding completed successfully");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Database initialization failed: {ex.Message}");
    // Log the full exception for debugging
    Console.WriteLine($"Full exception: {ex}");
}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AppCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
