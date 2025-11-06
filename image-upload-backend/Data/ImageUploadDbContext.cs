using Microsoft.EntityFrameworkCore;
using ImageUploadApi.Models;

namespace ImageUploadApi.Data
{
    public class ImageUploadDbContext : DbContext
    {
        public ImageUploadDbContext(DbContextOptions<ImageUploadDbContext> options) : base(options)
        {
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ImageMetadata> Images { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tenant configuration
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Subdomain).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Subdomain).IsRequired().HasMaxLength(100);
                entity.Property(e => e.StorageContainer).IsRequired().HasMaxLength(63);
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.Email, e.TenantId }).IsUnique();
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.PasswordHash).IsRequired();
                
                entity.HasOne(e => e.Tenant)
                      .WithMany(t => t.Users)
                      .HasForeignKey(e => e.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ImageMetadata configuration
            modelBuilder.Entity<ImageMetadata>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.UploadedAt);
                
                entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.StorageFileName).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ThumbnailStorageFileName).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ContentType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Tags).HasMaxLength(1000);
                
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Images)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Tenant)
                      .WithMany(t => t.Images)
                      .HasForeignKey(e => e.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}