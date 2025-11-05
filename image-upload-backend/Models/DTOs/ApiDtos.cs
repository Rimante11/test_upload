namespace ImageUploadApi.Models.DTOs
{
    public class ImageUploadRequestDto
    {
        public string? Description { get; set; }
        public string? Tags { get; set; }
    }
    
    public class ImageUploadResponseDto
    {
        public int Id { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string? Description { get; set; }
        public string? Tags { get; set; }
        public DateTime UploadedAt { get; set; }
        public string OriginalImageUrl { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
    }
    
    public class ImageListResponseDto
    {
        public int Id { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime UploadedAt { get; set; }
        public string ThumbnailUrl { get; set; } = string.Empty;
    }
    
    public class LoginRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string TenantSubdomain { get; set; } = string.Empty;
    }
    
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string TenantName { get; set; } = string.Empty;
    }
}