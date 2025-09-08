using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Models;

namespace _241RunnersAPI.Data
{
    /// <summary>
    /// Entity Framework database context for the 241 Runners API
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Unique constraints
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.EmailVerificationToken).IsUnique().HasFilter("[EmailVerificationToken] IS NOT NULL");
                entity.HasIndex(e => e.PasswordResetToken).IsUnique().HasFilter("[PasswordResetToken] IS NOT NULL");
                
                // Required fields
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);
                
                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(255);
                
                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasMaxLength(50);
                
                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);
                
                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");
                
                entity.Property(e => e.IsEmailVerified)
                    .HasDefaultValue(false);
                
                entity.Property(e => e.IsPhoneVerified)
                    .HasDefaultValue(false);
                
                entity.Property(e => e.FailedLoginAttempts)
                    .HasDefaultValue(0);
                
                // String length constraints
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(50);
                entity.Property(e => e.ZipCode).HasMaxLength(20);
                entity.Property(e => e.Organization).HasMaxLength(200);
                entity.Property(e => e.Title).HasMaxLength(100);
                entity.Property(e => e.Credentials).HasMaxLength(200);
                entity.Property(e => e.Specialization).HasMaxLength(200);
                entity.Property(e => e.YearsOfExperience).HasMaxLength(50);
                entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);
                entity.Property(e => e.AdditionalImageUrls).HasMaxLength(1000);
                entity.Property(e => e.DocumentUrls).HasMaxLength(1000);
                entity.Property(e => e.EmergencyContactName).HasMaxLength(100);
                entity.Property(e => e.EmergencyContactPhone).HasMaxLength(20);
                entity.Property(e => e.EmergencyContactRelationship).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(2000);
                entity.Property(e => e.EmailVerificationToken).HasMaxLength(255);
                entity.Property(e => e.PasswordResetToken).HasMaxLength(255);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        /// <summary>
        /// Seed initial data for the database
        /// </summary>
        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed admin users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Email = "admin@241runnersawareness.org",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!@#"),
                    FirstName = "System",
                    LastName = "Administrator",
                    Role = "admin",
                    IsActive = true,
                    IsEmailVerified = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailVerifiedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = 2,
                    Email = "support@241runnersawareness.org",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Support123!@#"),
                    FirstName = "Support",
                    LastName = "Team",
                    Role = "admin",
                    IsActive = true,
                    IsEmailVerified = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailVerifiedAt = DateTime.UtcNow
                }
            );
        }
    }
}
