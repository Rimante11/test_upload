using System.ComponentModel.DataAnnotations;

namespace ImageUploadApi.Models
{
    public class ImageMetadata
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string StorageFileName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string ThumbnailStorageFileName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string ContentType { get; set; } = string.Empty;
        
        public long FileSizeBytes { get; set; }
        
        public int Width { get; set; }
        
        public int Height { get; set; }
        
        public int ThumbnailWidth { get; set; }
        
        public int ThumbnailHeight { get; set; }
        
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(1000)]
        public string? Tags { get; set; }
        
        public bool IsDeleted { get; set; } = false;
        
        // Foreign keys
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
        
        public int TenantId { get; set; }
        public virtual Tenant Tenant { get; set; } = null!;
        
        // Computed properties for URLs (will be set by service layer)
        public string? OriginalImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
}