using Microsoft.EntityFrameworkCore;
using _241RunnersAwareness.BackendAPI.DBContext.Models;

namespace _241RunnersAwareness.BackendAPI.DBContext.Data
{
    public class RunnersDbContext : DbContext
    {
        public RunnersDbContext(DbContextOptions<RunnersDbContext> options) : base(options)
        {
        }

        // Existing Models
        public DbSet<Individual> Individuals { get; set; }
        public DbSet<EmergencyContact> EmergencyContacts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<DNAReport> DNAReports { get; set; }

        // New E-commerce Models
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Partnership> Partnerships { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<CaseImage> CaseImages { get; set; }
        public DbSet<CaseDocument> CaseDocuments { get; set; }

        // New Case Management Models
        public DbSet<Case> Cases { get; set; }
        public DbSet<CaseUpdate> CaseUpdates { get; set; }
        public DbSet<CaseUpdateMedia> CaseUpdateMedia { get; set; }

        // New Runner Profile Models
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Activity> Activities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Performance Optimization: Add indexes for frequently queried fields
            modelBuilder.Entity<Individual>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MiddleName).HasMaxLength(100);
                entity.Property(e => e.Gender).HasMaxLength(10);
                entity.Property(e => e.LastKnownAddress).HasMaxLength(200);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(50);
                entity.Property(e => e.ZipCode).HasMaxLength(10);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Height).HasMaxLength(50);
                entity.Property(e => e.Weight).HasMaxLength(50);
                entity.Property(e => e.HairColor).HasMaxLength(50);
                entity.Property(e => e.EyeColor).HasMaxLength(50);
                entity.Property(e => e.DistinguishingFeatures).HasMaxLength(200);
                entity.Property(e => e.SpecialNeeds).HasMaxLength(500);
                entity.Property(e => e.MedicalConditions).HasMaxLength(200);
                entity.Property(e => e.Medications).HasMaxLength(200);
                entity.Property(e => e.Allergies).HasMaxLength(200);
                entity.Property(e => e.DNASample).HasMaxLength(500);
                entity.Property(e => e.DNASampleType).HasMaxLength(100);
                entity.Property(e => e.DNALabReference).HasMaxLength(100);
                entity.Property(e => e.DNASequence).HasMaxLength(500);
                entity.Property(e => e.FingerprintData).HasMaxLength(200);
                entity.Property(e => e.DentalRecords).HasMaxLength(200);
                entity.Property(e => e.MedicalRecords).HasMaxLength(200);
                entity.Property(e => e.SocialSecurityNumber).HasMaxLength(100);
                entity.Property(e => e.DriverLicenseNumber).HasMaxLength(100);
                entity.Property(e => e.PassportNumber).HasMaxLength(100);
                entity.Property(e => e.CaseStatus).HasMaxLength(50);
                entity.Property(e => e.LastSeenLocation).HasMaxLength(200);
                entity.Property(e => e.Circumstances).HasMaxLength(500);
                entity.Property(e => e.NAMUSCaseNumber).HasMaxLength(100);
                entity.Property(e => e.LocalCaseNumber).HasMaxLength(100);
                entity.Property(e => e.InvestigatingAgency).HasMaxLength(100);
                entity.Property(e => e.InvestigatorName).HasMaxLength(100);
                entity.Property(e => e.InvestigatorPhone).HasMaxLength(20);
                entity.Property(e => e.MediaReferences).HasMaxLength(500);
                entity.Property(e => e.SocialMediaPosts).HasMaxLength(500);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);

                // Performance: Add indexes for frequently queried fields
                entity.HasIndex(e => e.CaseStatus).HasDatabaseName("IX_Individuals_CaseStatus");
                entity.HasIndex(e => e.State).HasDatabaseName("IX_Individuals_State");
                entity.HasIndex(e => e.City).HasDatabaseName("IX_Individuals_City");
                entity.HasIndex(e => e.LastSeenDate).HasDatabaseName("IX_Individuals_LastSeenDate");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_Individuals_CreatedAt");
                entity.HasIndex(e => new { e.FirstName, e.LastName }).HasDatabaseName("IX_Individuals_Name");
                entity.HasIndex(e => e.NAMUSCaseNumber).HasDatabaseName("IX_Individuals_NAMUSCaseNumber");
                entity.HasIndex(e => e.LocalCaseNumber).HasDatabaseName("IX_Individuals_LocalCaseNumber");

                // Relationships
                entity.HasMany(e => e.EmergencyContacts)
                    .WithOne(e => e.Individual)
                    .HasForeignKey(e => e.IndividualId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Images)
                    .WithOne(e => e.Individual)
                    .HasForeignKey(e => e.IndividualId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Documents)
                    .WithOne(e => e.Individual)
                    .HasForeignKey(e => e.IndividualId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.Property(e => e.EmailVerificationToken).HasMaxLength(100);
                entity.Property(e => e.PhoneVerificationCode).HasMaxLength(10);
                entity.Property(e => e.PasswordResetToken).HasMaxLength(100);
                entity.Property(e => e.RefreshToken).HasMaxLength(500);
                entity.Property(e => e.TwoFactorSecret).HasMaxLength(100);
                entity.Property(e => e.TwoFactorBackupCodes).HasMaxLength(1000);
                
                // Role-specific fields
                entity.Property(e => e.RelationshipToRunner).HasMaxLength(100);
                entity.Property(e => e.LicenseNumber).HasMaxLength(100);
                entity.Property(e => e.Organization).HasMaxLength(200);
                entity.Property(e => e.Credentials).HasMaxLength(200);
                entity.Property(e => e.Specialization).HasMaxLength(200);
                entity.Property(e => e.YearsOfExperience).HasMaxLength(50);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(50);
                entity.Property(e => e.ZipCode).HasMaxLength(20);
                entity.Property(e => e.EmergencyContactName).HasMaxLength(100);
                entity.Property(e => e.EmergencyContactPhone).HasMaxLength(20);
                entity.Property(e => e.EmergencyContactRelationship).HasMaxLength(100);

                // Performance: Add indexes for frequently queried fields
                entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("IX_Users_Email");
                entity.HasIndex(e => e.Username).IsUnique().HasDatabaseName("IX_Users_Username");
                entity.HasIndex(e => e.Role).HasDatabaseName("IX_Users_Role");
                entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_Users_IsActive");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_Users_CreatedAt");

                // Relationships
                entity.HasOne(e => e.Individual)
                    .WithMany()
                    .HasForeignKey(e => e.IndividualId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // EmergencyContact Configuration
            modelBuilder.Entity<EmergencyContact>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Relationship).HasMaxLength(50);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(200);
            });

            // Product Configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.LongDescription).HasMaxLength(1000);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SalePrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CompareAtPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SKU).HasMaxLength(50);
                entity.Property(e => e.Barcode).HasMaxLength(50);
                entity.Property(e => e.Brand).HasMaxLength(100);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Subcategory).HasMaxLength(100);
                entity.Property(e => e.Collection).HasMaxLength(100);
                entity.Property(e => e.Gender).HasMaxLength(50);
                entity.Property(e => e.Size).HasMaxLength(100);
                entity.Property(e => e.Color).HasMaxLength(100);
                entity.Property(e => e.ImageUrls).HasMaxLength(500);
                entity.Property(e => e.Tags).HasMaxLength(500);
                entity.Property(e => e.Weight).HasColumnType("decimal(18,2)");
                entity.Property(e => e.WeightUnit).HasMaxLength(50);
                entity.Property(e => e.Length).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Width).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Height).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DimensionUnit).HasMaxLength(50);
                entity.Property(e => e.AwarenessType).HasMaxLength(100);
                entity.Property(e => e.MissionStatement).HasMaxLength(500);
                entity.Property(e => e.PartnerOrganization).HasMaxLength(200);
                entity.Property(e => e.DonationPercentage).HasColumnType("decimal(5,2)");
                entity.Property(e => e.DonationAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MetaTitle).HasMaxLength(200);
                entity.Property(e => e.MetaDescription).HasMaxLength(500);
                entity.Property(e => e.Slug).HasMaxLength(200);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);

                // Relationships
                entity.HasMany(e => e.Variants)
                    .WithOne(e => e.Product)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Images)
                    .WithOne(e => e.Product)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Reviews)
                    .WithOne(e => e.Product)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ProductVariant Configuration
            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Size).HasMaxLength(100);
                entity.Property(e => e.Color).HasMaxLength(100);
                entity.Property(e => e.Style).HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SalePrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SKU).HasMaxLength(50);
            });

            // ProductImage Configuration
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(500);
                entity.Property(e => e.AltText).HasMaxLength(100);
                entity.Property(e => e.ImageType).HasMaxLength(100);
            });

            // ProductReview Configuration
            modelBuilder.Entity<ProductReview>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Comment).HasMaxLength(500);
            });

            // Order Configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CustomerEmail).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CustomerPhone).HasMaxLength(20);
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Tax).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Shipping).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DonationAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.TrackingNumber).HasMaxLength(100);
                entity.Property(e => e.ShippingAddress).HasMaxLength(500);
                entity.Property(e => e.ShippingCity).HasMaxLength(100);
                entity.Property(e => e.ShippingState).HasMaxLength(50);
                entity.Property(e => e.ShippingZipCode).HasMaxLength(20);
                entity.Property(e => e.ShippingCountry).HasMaxLength(100);

                // Relationships
                entity.HasMany(e => e.Items)
                    .WithOne(e => e.Order)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // OrderItem Configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.VariantName).HasMaxLength(100);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DonationAmount).HasColumnType("decimal(18,2)");

                // Relationships
                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Variant)
                    .WithMany()
                    .HasForeignKey(e => e.ProductVariantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Partnership Configuration
            modelBuilder.Entity<Partnership>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PartnerName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PartnerWebsite).HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.PartnershipType).HasMaxLength(100);
                entity.Property(e => e.LogoUrl).HasMaxLength(500);
                entity.Property(e => e.Terms).HasMaxLength(1000);
            });

            // Campaign Configuration
            modelBuilder.Entity<Campaign>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.CampaignType).HasMaxLength(100);
                entity.Property(e => e.GoalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CurrentAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
            });

            // CaseImage Configuration
            modelBuilder.Entity<CaseImage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ImageType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.UploadedBy).HasMaxLength(100);
            });

            // CaseDocument Configuration
            modelBuilder.Entity<CaseDocument>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DocumentUrl).IsRequired().HasMaxLength(200);
                entity.Property(e => e.DocumentType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.UploadedBy).HasMaxLength(100);
            });

            // Case Configuration
            modelBuilder.Entity<Case>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CaseNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PublicSlug).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.LastSeenLocation).HasMaxLength(500);
                entity.Property(e => e.Circumstances).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Priority).HasMaxLength(20);
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.RiskLevel).HasMaxLength(50);
                entity.Property(e => e.ResolutionNotes).HasMaxLength(500);
                entity.Property(e => e.LawEnforcementCaseNumber).HasMaxLength(100);
                entity.Property(e => e.InvestigatingAgency).HasMaxLength(200);
                entity.Property(e => e.InvestigatorName).HasMaxLength(100);
                entity.Property(e => e.InvestigatorContact).HasMaxLength(100);
                entity.Property(e => e.Tags).HasMaxLength(500);
                entity.Property(e => e.SocialMediaHandles).HasMaxLength(500);
                entity.Property(e => e.MediaContacts).HasMaxLength(1000);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);

                // Performance: Add indexes for frequently queried fields
                entity.HasIndex(e => e.CaseNumber).IsUnique().HasDatabaseName("IX_Cases_CaseNumber");
                entity.HasIndex(e => e.PublicSlug).IsUnique().HasDatabaseName("IX_Cases_PublicSlug");
                entity.HasIndex(e => e.OwnerUserId).HasDatabaseName("IX_Cases_OwnerUserId");
                entity.HasIndex(e => e.Status).HasDatabaseName("IX_Cases_Status");
                entity.HasIndex(e => e.IsPublic).HasDatabaseName("IX_Cases_IsPublic");
                entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_Cases_IsActive");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_Cases_CreatedAt");
                entity.HasIndex(e => e.LastUpdatedAt).HasDatabaseName("IX_Cases_LastUpdatedAt");

                // Relationships
                entity.HasOne(e => e.Individual)
                    .WithMany()
                    .HasForeignKey(e => e.IndividualId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.OwnerUser)
                    .WithMany()
                    .HasForeignKey(e => e.OwnerUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Updates)
                    .WithOne(e => e.Case)
                    .HasForeignKey(e => e.CaseId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Note: Images, Documents, and EmergencyContacts are managed through the Individual entity
                // The Case entity references the Individual but doesn't directly manage these collections
            });

            // CaseUpdate Configuration
            modelBuilder.Entity<CaseUpdate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UpdateType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.Location).HasMaxLength(500);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);

                // Performance: Add indexes for frequently queried fields
                entity.HasIndex(e => e.CaseId).HasDatabaseName("IX_CaseUpdates_CaseId");
                entity.HasIndex(e => e.CreatedByUserId).HasDatabaseName("IX_CaseUpdates_CreatedByUserId");
                entity.HasIndex(e => e.UpdateType).HasDatabaseName("IX_CaseUpdates_UpdateType");
                entity.HasIndex(e => e.IsPublic).HasDatabaseName("IX_CaseUpdates_IsPublic");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_CaseUpdates_CreatedAt");

                // Relationships
                entity.HasOne(e => e.Case)
                    .WithMany(e => e.Updates)
                    .HasForeignKey(e => e.CaseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Media)
                    .WithOne(e => e.CaseUpdate)
                    .HasForeignKey(e => e.CaseUpdateId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // CaseUpdateMedia Configuration
            modelBuilder.Entity<CaseUpdateMedia>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MediaType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.MediaUrl).IsRequired().HasMaxLength(500);
                entity.Property(e => e.OriginalFilename).HasMaxLength(200);
                entity.Property(e => e.MimeType).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.UploadedBy).HasMaxLength(100);

                // Performance: Add indexes for frequently queried fields
                entity.HasIndex(e => e.CaseUpdateId).HasDatabaseName("IX_CaseUpdateMedia_CaseUpdateId");
                entity.HasIndex(e => e.MediaType).HasDatabaseName("IX_CaseUpdateMedia_MediaType");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_CaseUpdateMedia_CreatedAt");

                // Relationships
                entity.HasOne(e => e.CaseUpdate)
                    .WithMany(e => e.Media)
                    .HasForeignKey(e => e.CaseUpdateId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // DNAReport Configuration
            modelBuilder.Entity<DNAReport>(entity =>
            {
                entity.HasKey(e => e.ReportId);
                entity.Property(e => e.ReportTitle).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Location).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50);
                
                // DNA Sample fields
                entity.Property(e => e.DNASampleDescription).HasMaxLength(500);
                entity.Property(e => e.DNASampleType).HasMaxLength(100);
                entity.Property(e => e.DNASampleLocation).HasMaxLength(100);
                entity.Property(e => e.DNALabReference).HasMaxLength(100);
                entity.Property(e => e.DNASequence).HasMaxLength(50);
                
                // Additional details
                entity.Property(e => e.WeatherConditions).HasMaxLength(100);
                entity.Property(e => e.ClothingDescription).HasMaxLength(100);
                entity.Property(e => e.PhysicalDescription).HasMaxLength(100);
                entity.Property(e => e.BehaviorDescription).HasMaxLength(100);
                
                // Contact information
                entity.Property(e => e.WitnessName).HasMaxLength(100);
                entity.Property(e => e.WitnessPhone).HasMaxLength(20);
                entity.Property(e => e.WitnessEmail).HasMaxLength(100);
                
                // Resolution
                entity.Property(e => e.ResolutionNotes).HasMaxLength(500);
                entity.Property(e => e.ResolvedBy).HasMaxLength(100);

                // Performance: Add indexes for frequently queried fields
                entity.HasIndex(e => e.Status).HasDatabaseName("IX_DNAReports_Status");
                entity.HasIndex(e => e.ReportDate).HasDatabaseName("IX_DNAReports_ReportDate");
                entity.HasIndex(e => e.IndividualId).HasDatabaseName("IX_DNAReports_IndividualId");
                entity.HasIndex(e => e.ReporterUserId).HasDatabaseName("IX_DNAReports_ReporterUserId");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_DNAReports_CreatedAt");

                // Relationships
                entity.HasOne(e => e.Reporter)
                    .WithMany()
                    .HasForeignKey(e => e.ReporterUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Individual)
                    .WithMany()
                    .HasForeignKey(e => e.IndividualId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Photo Configuration
            modelBuilder.Entity<Photo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Caption).HasMaxLength(200);
                entity.Property(e => e.ImageType).HasMaxLength(100);
                entity.Property(e => e.UploadedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.FileName).HasMaxLength(100);
                entity.Property(e => e.ContentType).HasMaxLength(50);

                // Performance: Add indexes for frequently queried fields
                entity.HasIndex(e => e.IndividualId).HasDatabaseName("IX_Photos_IndividualId");
                entity.HasIndex(e => e.IsPrimary).HasDatabaseName("IX_Photos_IsPrimary");
                entity.HasIndex(e => e.UploadedAt).HasDatabaseName("IX_Photos_UploadedAt");

                // Relationships
                entity.HasOne(e => e.Individual)
                    .WithMany(e => e.Photos)
                    .HasForeignKey(e => e.IndividualId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Activity Configuration
            modelBuilder.Entity<Activity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ActivityType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.Metadata).HasMaxLength(500);

                // Performance: Add indexes for frequently queried fields
                entity.HasIndex(e => e.IndividualId).HasDatabaseName("IX_Activities_IndividualId");
                entity.HasIndex(e => e.ActivityType).HasDatabaseName("IX_Activities_ActivityType");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_Activities_CreatedAt");
                entity.HasIndex(e => e.RelatedCaseId).HasDatabaseName("IX_Activities_RelatedCaseId");
                entity.HasIndex(e => e.RelatedPhotoId).HasDatabaseName("IX_Activities_RelatedPhotoId");

                // Relationships
                entity.HasOne(e => e.Individual)
                    .WithMany(e => e.Activities)
                    .HasForeignKey(e => e.IndividualId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.RelatedCase)
                    .WithMany()
                    .HasForeignKey(e => e.RelatedCaseId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.RelatedPhoto)
                    .WithMany()
                    .HasForeignKey(e => e.RelatedPhotoId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
