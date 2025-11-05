using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace ImageUploadApi.Services
{
    public interface IImageProcessingService
    {
        Task<(byte[] originalBytes, byte[] thumbnailBytes, int width, int height, int thumbWidth, int thumbHeight)> ProcessImageAsync(
            Stream imageStream, 
            int maxThumbnailSize = 200);
        bool IsValidImageFormat(string contentType);
        string GetFileExtension(string contentType);
    }

    public class ImageProcessingService : IImageProcessingService
    {
        private readonly string[] _allowedFormats = { "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp" };
        
        public bool IsValidImageFormat(string contentType)
        {
            return _allowedFormats.Contains(contentType.ToLower());
        }

        public string GetFileExtension(string contentType)
        {
            return contentType.ToLower() switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/bmp" => ".bmp",
                "image/webp" => ".webp",
                _ => ".jpg"
            };
        }

        public async Task<(byte[] originalBytes, byte[] thumbnailBytes, int width, int height, int thumbWidth, int thumbHeight)> ProcessImageAsync(
            Stream imageStream, 
            int maxThumbnailSize = 200)
        {
            using var image = await Image.LoadAsync(imageStream);
            
            var originalWidth = image.Width;
            var originalHeight = image.Height;
            
            // Convert original to JPEG for storage consistency
            using var originalMs = new MemoryStream();
            await image.SaveAsJpegAsync(originalMs, new JpegEncoder { Quality = 90 });
            var originalBytes = originalMs.ToArray();
            
            // Create thumbnail
            var thumbnailSize = CalculateThumbnailSize(originalWidth, originalHeight, maxThumbnailSize);
            
            using var thumbnail = image.Clone(x => x.Resize(thumbnailSize.width, thumbnailSize.height));
            using var thumbnailMs = new MemoryStream();
            await thumbnail.SaveAsJpegAsync(thumbnailMs, new JpegEncoder { Quality = 85 });
            var thumbnailBytes = thumbnailMs.ToArray();
            
            return (originalBytes, thumbnailBytes, originalWidth, originalHeight, thumbnailSize.width, thumbnailSize.height);
        }
        
        private (int width, int height) CalculateThumbnailSize(int originalWidth, int originalHeight, int maxSize)
        {
            if (originalWidth <= maxSize && originalHeight <= maxSize)
                return (originalWidth, originalHeight);
                
            double ratio = (double)originalWidth / originalHeight;
            
            if (originalWidth > originalHeight)
            {
                return (maxSize, (int)(maxSize / ratio));
            }
            else
            {
                return ((int)(maxSize * ratio), maxSize);
            }
        }
    }
}