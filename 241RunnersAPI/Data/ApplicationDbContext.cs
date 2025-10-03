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
        public DbSet<Runner> Runners { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<TopicSubscription> TopicSubscriptions { get; set; }
        public DbSet<Notification> Notifications { get; set; }

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

            // Runner configuration
            modelBuilder.Entity<Runner>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Indices for performance
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Status);
                
                // Foreign key relationship
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                // Required fields
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.DateOfBirth).IsRequired();
                entity.Property(e => e.Gender).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Missing");
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
                
                // String length constraints
                entity.Property(e => e.PhysicalDescription).HasMaxLength(500);
                entity.Property(e => e.MedicalConditions).HasMaxLength(1000);
                entity.Property(e => e.Medications).HasMaxLength(1000);
                entity.Property(e => e.Allergies).HasMaxLength(1000);
                entity.Property(e => e.EmergencyInstructions).HasMaxLength(500);
                entity.Property(e => e.PreferredRunningLocations).HasMaxLength(200);
                entity.Property(e => e.TypicalRunningTimes).HasMaxLength(200);
                entity.Property(e => e.ExperienceLevel).HasMaxLength(100);
                entity.Property(e => e.SpecialNeeds).HasMaxLength(500);
                entity.Property(e => e.AdditionalNotes).HasMaxLength(1000);
                entity.Property(e => e.LastKnownLocation).HasMaxLength(50);
                entity.Property(e => e.PreferredContactMethod).HasMaxLength(50);
                entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);
                entity.Property(e => e.AdditionalImageUrls).HasMaxLength(1000);
                entity.Property(e => e.VerifiedBy).HasMaxLength(255);
                
                // Default values
                entity.Property(e => e.IsProfileComplete).HasDefaultValue(false);
                entity.Property(e => e.IsVerified).HasDefaultValue(false);
            });

            // Case configuration
            modelBuilder.Entity<Case>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Foreign key relationships
                entity.HasOne(e => e.Runner)
                    .WithMany()
                    .HasForeignKey(e => e.RunnerId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.ReportedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.ReportedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Required fields
                entity.Property(e => e.RunnerId).IsRequired();
                entity.Property(e => e.ReportedByUserId).IsRequired();
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.LastSeenDate).IsRequired();
                entity.Property(e => e.LastSeenLocation).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Priority).IsRequired().HasMaxLength(20);
                entity.Property(e => e.IsPublic).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
                
                // String length constraints
                entity.Property(e => e.LastSeenTime).HasMaxLength(100);
                entity.Property(e => e.LastSeenCircumstances).HasMaxLength(1000);
                entity.Property(e => e.ClothingDescription).HasMaxLength(200);
                entity.Property(e => e.PhysicalCondition).HasMaxLength(200);
                entity.Property(e => e.MentalState).HasMaxLength(200);
                entity.Property(e => e.AdditionalInformation).HasMaxLength(1000);
                entity.Property(e => e.ResolutionNotes).HasMaxLength(1000);
                entity.Property(e => e.ResolvedBy).HasMaxLength(200);
                entity.Property(e => e.ContactPersonName).HasMaxLength(100);
                entity.Property(e => e.ContactPersonPhone).HasMaxLength(20);
                entity.Property(e => e.ContactPersonEmail).HasMaxLength(255);
                entity.Property(e => e.CaseImageUrls).HasMaxLength(1000);
                entity.Property(e => e.DocumentUrls).HasMaxLength(1000);
                entity.Property(e => e.LastSeenLatitude).HasPrecision(18, 6);
                entity.Property(e => e.LastSeenLongitude).HasPrecision(18, 6);
                entity.Property(e => e.EmergencyContactName).HasMaxLength(100);
                entity.Property(e => e.EmergencyContactPhone).HasMaxLength(20);
                entity.Property(e => e.EmergencyContactRelationship).HasMaxLength(100);
                entity.Property(e => e.VerifiedBy).HasMaxLength(255);
                entity.Property(e => e.ApprovedBy).HasMaxLength(255);
                
                // Default values
                entity.Property(e => e.IsVerified).HasDefaultValue(false);
                entity.Property(e => e.IsApproved).HasDefaultValue(false);
                entity.Property(e => e.ViewCount).HasDefaultValue(0);
                entity.Property(e => e.ShareCount).HasDefaultValue(0);
                entity.Property(e => e.TipCount).HasDefaultValue(0);
            });

            // Device configuration
            modelBuilder.Entity<Device>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Foreign key relationship
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                // Unique constraint for user + platform combination
                entity.HasIndex(e => new { e.UserId, e.Platform }).IsUnique();
                
                // Required fields
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Platform).IsRequired().HasMaxLength(10);
                entity.Property(e => e.FcmToken).IsRequired().HasMaxLength(500);
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.LastSeenAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
                
                // String length constraints
                entity.Property(e => e.AppVersion).HasMaxLength(20);
                entity.Property(e => e.TopicsJson).HasMaxLength(2000);
                entity.Property(e => e.DeviceModel).HasMaxLength(100);
                entity.Property(e => e.OsVersion).HasMaxLength(50);
                entity.Property(e => e.AppBuildNumber).HasMaxLength(100);
            });

            // TopicSubscription configuration
            modelBuilder.Entity<TopicSubscription>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Foreign key relationship
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                // Unique constraint for user + topic combination
                entity.HasIndex(e => new { e.UserId, e.Topic }).IsUnique();
                
                // Required fields
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Topic).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsSubscribed).IsRequired().HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
                
                // String length constraints
                entity.Property(e => e.SubscriptionReason).HasMaxLength(200);
                
                // Default values
                entity.Property(e => e.NotificationCount).HasDefaultValue(0);
            });

            // Notification configuration
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Foreign key relationship
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                // Required fields
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Body).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IsSent).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.Priority).IsRequired().HasMaxLength(100).HasDefaultValue("normal");
                
                // String length constraints
                entity.Property(e => e.Topic).HasMaxLength(100);
                entity.Property(e => e.DataJson).HasMaxLength(2000);
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);
                
                // Default values
                entity.Property(e => e.IsDelivered).HasDefaultValue(false);
                entity.Property(e => e.IsOpened).HasDefaultValue(false);
                entity.Property(e => e.RetryCount).HasDefaultValue(0);
                
                // Indexes for performance
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.Topic);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.IsSent);
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
