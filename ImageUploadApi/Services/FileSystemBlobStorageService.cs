using ImageUploadApi.Services;

namespace ImageUploadApi.Services
{
    public class FileSystemBlobStorageService : IBlobStorageService
    {
        private readonly string _basePath;
        private readonly ILogger<FileSystemBlobStorageService> _logger;

        public FileSystemBlobStorageService(ILogger<FileSystemBlobStorageService> logger, IConfiguration configuration)
        {
            _logger = logger;
            // Use a uploads directory in the project root, or configure via appsettings
            _basePath = configuration["FileStorage:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            
            // Ensure base directory exists
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
                _logger.LogInformation("Created uploads directory at {BasePath}", _basePath);
            }
        }

        public Task EnsureContainerExistsAsync(string containerName)
        {
            var containerPath = Path.Combine(_basePath, containerName);
            if (!Directory.Exists(containerPath))
            {
                Directory.CreateDirectory(containerPath);
                _logger.LogInformation("Created container directory {ContainerName} at {ContainerPath}", containerName, containerPath);
            }
            return Task.CompletedTask;
        }

        public async Task<string> UploadImageAsync(byte[] imageData, string containerName, string fileName, string contentType)
        {
            await EnsureContainerExistsAsync(containerName);
            
            var filePath = Path.Combine(_basePath, containerName, fileName);
            await File.WriteAllBytesAsync(filePath, imageData);
            
            // Store content type in a separate metadata file
            var metadataPath = filePath + ".meta";
            await File.WriteAllTextAsync(metadataPath, contentType);
            
            var url = $"http://localhost:5119/api/images/blob/{containerName}/{fileName}";
            _logger.LogInformation("Stored file {FileName} in container {ContainerName} at {FilePath}", fileName, containerName, filePath);
            
            return url;
        }

        public async Task<string> GetImageUrlAsync(string containerName, string fileName)
        {
            await EnsureContainerExistsAsync(containerName);
            
            var filePath = Path.Combine(_basePath, containerName, fileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File {fileName} not found in container {containerName}");
            }
            
            return $"http://localhost:5119/api/images/blob/{containerName}/{fileName}";
        }

        public Task<bool> DeleteImageAsync(string containerName, string fileName)
        {
            var filePath = Path.Combine(_basePath, containerName, fileName);
            var metadataPath = filePath + ".meta";
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                
                // Also delete metadata file if it exists
                if (File.Exists(metadataPath))
                {
                    File.Delete(metadataPath);
                }
                
                _logger.LogInformation("Deleted file {FileName} from container {ContainerName}", fileName, containerName);
                return Task.FromResult(true);
            }
            
            return Task.FromResult(false);
        }

        public (byte[] data, string contentType)? GetBlob(string containerName, string fileName)
        {
            var filePath = Path.Combine(_basePath, containerName, fileName);
            var metadataPath = filePath + ".meta";
            
            if (File.Exists(filePath))
            {
                var data = File.ReadAllBytes(filePath);
                var contentType = "application/octet-stream"; // Default
                
                // Try to read content type from metadata file
                if (File.Exists(metadataPath))
                {
                    contentType = File.ReadAllText(metadataPath).Trim();
                }
                
                return (data, contentType);
            }
            
            return null;
        }
    }
}