using Microsoft.EntityFrameworkCore;
using ImageUploadApi.Data;
using ImageUploadApi.Models;
using ImageUploadApi.Models.DTOs;

namespace ImageUploadApi.Services
{
    public interface IImageService
    {
        Task<ImageUploadResponseDto> UploadImageAsync(IFormFile file, int userId, int tenantId, ImageUploadRequestDto request);
        Task<IEnumerable<ImageListResponseDto>> GetUserImagesAsync(int userId, int tenantId);
        Task<ImageUploadResponseDto?> GetImageByIdAsync(int imageId, int userId, int tenantId);
        Task<bool> DeleteImageAsync(int imageId, int userId, int tenantId);
    }

    public class ImageService : IImageService
    {
        private readonly ImageUploadDbContext _context;
        private readonly IImageProcessingService _imageProcessingService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ITenantService _tenantService;
        private readonly ILogger<ImageService> _logger;

        public ImageService(
            ImageUploadDbContext context,
            IImageProcessingService imageProcessingService,
            IBlobStorageService blobStorageService,
            ITenantService tenantService,
            ILogger<ImageService> logger)
        {
            _context = context;
            _imageProcessingService = imageProcessingService;
            _blobStorageService = blobStorageService;
            _tenantService = tenantService;
            _logger = logger;
        }

        public async Task<ImageUploadResponseDto> UploadImageAsync(IFormFile file, int userId, int tenantId, ImageUploadRequestDto request)
        {
            if (!_imageProcessingService.IsValidImageFormat(file.ContentType))
            {
                throw new ArgumentException($"Unsupported image format: {file.ContentType}");
            }

            if (file.Length > 10 * 1024 * 1024) // 10MB limit
            {
                throw new ArgumentException("File size exceeds 10MB limit");
            }

            try
            {
                var containerName = await _tenantService.GetTenantStorageContainerAsync(tenantId);
                
                // Process the image
                using var imageStream = file.OpenReadStream();
                var processedImage = await _imageProcessingService.ProcessImageAsync(imageStream);
                
                // Generate unique file names
                var uniqueId = Guid.NewGuid().ToString();
                var extension = _imageProcessingService.GetFileExtension(file.ContentType);
                var originalFileName = $"original_{uniqueId}{extension}";
                var thumbnailFileName = $"thumb_{uniqueId}{extension}";
                
                // Upload to blob storage
                var originalUrl = await _blobStorageService.UploadImageAsync(
                    processedImage.originalBytes, containerName, originalFileName, "image/jpeg");
                    
                var thumbnailUrl = await _blobStorageService.UploadImageAsync(
                    processedImage.thumbnailBytes, containerName, thumbnailFileName, "image/jpeg");
                
                // Save metadata to database
                var imageMetadata = new ImageMetadata
                {
                    OriginalFileName = file.FileName,
                    StorageFileName = originalFileName,
                    ThumbnailStorageFileName = thumbnailFileName,
                    ContentType = file.ContentType,
                    FileSizeBytes = file.Length,
                    Width = processedImage.width,
                    Height = processedImage.height,
                    ThumbnailWidth = processedImage.thumbWidth,
                    ThumbnailHeight = processedImage.thumbHeight,
                    Description = request.Description,
                    Tags = request.Tags,
                    UserId = userId,
                    TenantId = tenantId,
                    OriginalImageUrl = originalUrl,
                    ThumbnailUrl = thumbnailUrl
                };

                _context.Images.Add(imageMetadata);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully uploaded image {ImageId} for user {UserId} in tenant {TenantId}", 
                    imageMetadata.Id, userId, tenantId);

                return new ImageUploadResponseDto
                {
                    Id = imageMetadata.Id,
                    OriginalFileName = imageMetadata.OriginalFileName,
                    ContentType = imageMetadata.ContentType,
                    FileSizeBytes = imageMetadata.FileSizeBytes,
                    Width = imageMetadata.Width,
                    Height = imageMetadata.Height,
                    Description = imageMetadata.Description,
                    Tags = imageMetadata.Tags,
                    UploadedAt = imageMetadata.UploadedAt,
                    OriginalImageUrl = originalUrl,
                    ThumbnailUrl = thumbnailUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image for user {UserId} in tenant {TenantId}", userId, tenantId);
                throw;
            }
        }

        public async Task<IEnumerable<ImageListResponseDto>> GetUserImagesAsync(int userId, int tenantId)
        {
            try
            {
                var images = await _context.Images
                    .Where(i => i.UserId == userId && i.TenantId == tenantId)
                    .OrderByDescending(i => i.UploadedAt)
                    .Select(i => new ImageListResponseDto
                    {
                        Id = i.Id,
                        OriginalFileName = i.OriginalFileName,
                        Description = i.Description,
                        UploadedAt = i.UploadedAt,
                        ThumbnailUrl = i.ThumbnailUrl ?? ""
                    })
                    .ToListAsync();

                // Update URLs if they're missing (for backwards compatibility)
                var containerName = await _tenantService.GetTenantStorageContainerAsync(tenantId);
                foreach (var image in images.Where(i => string.IsNullOrEmpty(i.ThumbnailUrl)))
                {
                    var metadata = await _context.Images.FindAsync(image.Id);
                    if (metadata != null)
                    {
                        image.ThumbnailUrl = await _blobStorageService.GetImageUrlAsync(containerName, metadata.ThumbnailStorageFileName);
                    }
                }

                return images;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting images for user {UserId} in tenant {TenantId}", userId, tenantId);
                throw;
            }
        }

        public async Task<ImageUploadResponseDto?> GetImageByIdAsync(int imageId, int userId, int tenantId)
        {
            try
            {
                var image = await _context.Images
                    .FirstOrDefaultAsync(i => i.Id == imageId && i.UserId == userId && i.TenantId == tenantId);

                if (image == null)
                    return null;

                var containerName = await _tenantService.GetTenantStorageContainerAsync(tenantId);
                
                // Update URLs if they're missing
                if (string.IsNullOrEmpty(image.OriginalImageUrl) || string.IsNullOrEmpty(image.ThumbnailUrl))
                {
                    image.OriginalImageUrl = await _blobStorageService.GetImageUrlAsync(containerName, image.StorageFileName);
                    image.ThumbnailUrl = await _blobStorageService.GetImageUrlAsync(containerName, image.ThumbnailStorageFileName);
                    await _context.SaveChangesAsync();
                }

                return new ImageUploadResponseDto
                {
                    Id = image.Id,
                    OriginalFileName = image.OriginalFileName,
                    ContentType = image.ContentType,
                    FileSizeBytes = image.FileSizeBytes,
                    Width = image.Width,
                    Height = image.Height,
                    Description = image.Description,
                    Tags = image.Tags,
                    UploadedAt = image.UploadedAt,
                    OriginalImageUrl = image.OriginalImageUrl,
                    ThumbnailUrl = image.ThumbnailUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image {ImageId} for user {UserId} in tenant {TenantId}", imageId, userId, tenantId);
                throw;
            }
        }

        public async Task<bool> DeleteImageAsync(int imageId, int userId, int tenantId)
        {
            try
            {
                var image = await _context.Images
                    .FirstOrDefaultAsync(i => i.Id == imageId && i.UserId == userId && i.TenantId == tenantId);

                if (image == null)
                    return false;

                // Soft delete in database
                image.IsDeleted = true;
                await _context.SaveChangesAsync();

                // Optionally delete from blob storage (commented out for data retention)
                // var containerName = await _tenantService.GetTenantStorageContainerAsync(tenantId);
                // await _blobStorageService.DeleteImageAsync(containerName, image.StorageFileName);
                // await _blobStorageService.DeleteImageAsync(containerName, image.ThumbnailStorageFileName);

                _logger.LogInformation("Successfully deleted image {ImageId} for user {UserId} in tenant {TenantId}", imageId, userId, tenantId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image {ImageId} for user {UserId} in tenant {TenantId}", imageId, userId, tenantId);
                throw;
            }
        }
    }
}