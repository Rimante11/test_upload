using System.ComponentModel.DataAnnotations;

namespace ImageUploadApi.Models
{
    public class Tenant
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Subdomain { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        // Storage container name for this tenant
        [Required]
        [StringLength(63)] // Azure blob container name limit
        public string StorageContainer { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<ImageMetadata> Images { get; set; } = new List<ImageMetadata>();
    }
}