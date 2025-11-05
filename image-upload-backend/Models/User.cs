using System.ComponentModel.DataAnnotations;

namespace ImageUploadApi.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        // Foreign key to Tenant
        public int TenantId { get; set; }
        public virtual Tenant Tenant { get; set; } = null!;
        
        // Navigation properties
        public virtual ICollection<ImageMetadata> Images { get; set; } = new List<ImageMetadata>();
    }
}