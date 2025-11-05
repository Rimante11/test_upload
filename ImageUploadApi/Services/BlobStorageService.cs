using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ImageUploadApi.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadImageAsync(byte[] imageData, string containerName, string fileName, string contentType);
        Task<string> GetImageUrlAsync(string containerName, string fileName);
        Task<bool> DeleteImageAsync(string containerName, string fileName);
        Task EnsureContainerExistsAsync(string containerName);
        (byte[] data, string contentType)? GetBlob(string containerName, string fileName);
    }

    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<BlobStorageService> _logger;

        public BlobStorageService(BlobServiceClient blobServiceClient, ILogger<BlobStorageService> logger)
        {
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        public async Task EnsureContainerExistsAsync(string containerName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
                _logger.LogInformation("Container {ContainerName} ensured to exist", containerName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring container {ContainerName} exists", containerName);
                throw;
            }
        }

        public async Task<string> UploadImageAsync(byte[] imageData, string containerName, string fileName, string contentType)
        {
            try
            {
                await EnsureContainerExistsAsync(containerName);
                
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(fileName);

                using var stream = new MemoryStream(imageData);
                
                var blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType,
                    CacheControl = "public, max-age=31536000" // 1 year cache for images
                };

                await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders
                });

                _logger.LogInformation("Successfully uploaded blob {FileName} to container {ContainerName}", fileName, containerName);
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading blob {FileName} to container {ContainerName}", fileName, containerName);
                throw;
            }
        }

        public async Task<string> GetImageUrlAsync(string containerName, string fileName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(fileName);
                
                var exists = await blobClient.ExistsAsync();
                if (!exists.Value)
                {
                    throw new FileNotFoundException($"Blob {fileName} not found in container {containerName}");
                }
                
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting URL for blob {FileName} in container {ContainerName}", fileName, containerName);
                throw;
            }
        }

        public async Task<bool> DeleteImageAsync(string containerName, string fileName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(fileName);
                
                var result = await blobClient.DeleteIfExistsAsync();
                
                _logger.LogInformation("Deleted blob {FileName} from container {ContainerName}: {Success}", fileName, containerName, result.Value);
                return result.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blob {FileName} from container {ContainerName}", fileName, containerName);
                return false;
            }
        }

        public (byte[] data, string contentType)? GetBlob(string containerName, string fileName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(fileName);
                
                var response = blobClient.DownloadContent();
                var content = response.Value.Content.ToArray();
                var contentType = response.Value.Details.ContentType ?? "application/octet-stream";
                
                return (content, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blob {FileName} from container {ContainerName}", fileName, containerName);
                return null;
            }
        }
    }
}