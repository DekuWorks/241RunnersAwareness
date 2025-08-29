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
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Initialize database and seed admin users
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var jwtService = scope.ServiceProvider.GetRequiredService<JwtService>();
        
        // Ensure database is created
        context.Database.EnsureCreated();
        
        // Seed admin users if they don't exist
        if (!context.Users.Any())
        {
            var adminUsers = new[]
            {
                new User
                {
                    Email = "dekuworks1@gmail.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("runners1997"),
                    FirstName = "Marcus",
                    LastName = "Brown",
                    Role = "admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Email = "danielcarey9770@yahoo.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("runners0428"),
                    FirstName = "Daniel",
                    LastName = "Carey",
                    Role = "admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };
            
            context.Users.AddRange(adminUsers);
            context.SaveChanges();
            
            Console.WriteLine("Admin users seeded successfully!");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Database initialization failed: {ex.Message}");
}

app.Run();
