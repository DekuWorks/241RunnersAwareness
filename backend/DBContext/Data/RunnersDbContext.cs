using Microsoft.EntityFrameworkCore;
using _241RunnersAwareness.BackendAPI.Models;

namespace _241RunnersAwareness.BackendAPI.Data
{
    public class RunnersDbContext : DbContext
    {
        public RunnersDbContext(DbContextOptions<RunnersDbContext> options) : base(options)
        {
        }

        // These match your table names
        public DbSet<Individual> Individuals { get; set; }
        public DbSet<EmergencyContact> EmergencyContacts { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).HasMaxLength(50);
                
                // Create unique index on email
                entity.HasIndex(e => e.Email).IsUnique();
                
                // Create unique index on phone number
                entity.HasIndex(e => e.PhoneNumber).IsUnique();
            });

            // Configure Individual entity to have optional relationship with User
            modelBuilder.Entity<Individual>(entity =>
            {
                entity.HasOne<User>()
                    .WithOne(u => u.Individual)
                    .HasForeignKey<User>(u => u.IndividualId)
                    .IsRequired(false);
            });
        }
    }
    // IF _contex was null, this is where it would blow up(meaning it didn't register it or the it isn't recieving it properly)
}
