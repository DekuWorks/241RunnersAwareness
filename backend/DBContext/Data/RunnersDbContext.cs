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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Individual Configuration
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

            // EmergencyContact Configuration
            modelBuilder.Entity<EmergencyContact>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Relationship).HasMaxLength(50);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
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
        }
    }
}
