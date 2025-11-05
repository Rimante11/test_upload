namespace ImageUploadApi.Services
{
    public class InMemoryBlobStorageService : IBlobStorageService
    {
        private readonly Dictionary<string, Dictionary<string, byte[]>> _containers = new();
        private readonly Dictionary<string, Dictionary<string, string>> _contentTypes = new();
        private readonly ILogger<InMemoryBlobStorageService> _logger;

        public InMemoryBlobStorageService(ILogger<InMemoryBlobStorageService> logger)
        {
            _logger = logger;
        }

        public Task EnsureContainerExistsAsync(string containerName)
        {
            if (!_containers.ContainsKey(containerName))
            {
                _containers[containerName] = new Dictionary<string, byte[]>();
                _contentTypes[containerName] = new Dictionary<string, string>();
                _logger.LogInformation("Created in-memory container {ContainerName}", containerName);
            }
            return Task.CompletedTask;
        }

        public async Task<string> UploadImageAsync(byte[] imageData, string containerName, string fileName, string contentType)
        {
            await EnsureContainerExistsAsync(containerName);
            
            _containers[containerName][fileName] = imageData;
            _contentTypes[containerName][fileName] = contentType;
            
            // Return a mock URL that we'll serve from our API
            var url = $"http://localhost:5119/api/images/blob/{containerName}/{fileName}";
            _logger.LogInformation("Stored blob {FileName} in container {ContainerName}", fileName, containerName);
            
            return url;
        }

        public async Task<string> GetImageUrlAsync(string containerName, string fileName)
        {
            await EnsureContainerExistsAsync(containerName);
            
            if (!_containers[containerName].ContainsKey(fileName))
            {
                throw new FileNotFoundException($"Blob {fileName} not found in container {containerName}");
            }
            
            return $"http://localhost:5119/api/images/blob/{containerName}/{fileName}";
        }

        public Task<bool> DeleteImageAsync(string containerName, string fileName)
        {
            if (_containers.ContainsKey(containerName) && _containers[containerName].ContainsKey(fileName))
            {
                _containers[containerName].Remove(fileName);
                _contentTypes[containerName].Remove(fileName);
                _logger.LogInformation("Deleted blob {FileName} from container {ContainerName}", fileName, containerName);
                return Task.FromResult(true);
            }
            
            return Task.FromResult(false);
        }

        public (byte[] data, string contentType)? GetBlob(string containerName, string fileName)
        {
            if (_containers.ContainsKey(containerName) && 
                _containers[containerName].ContainsKey(fileName))
            {
                var data = _containers[containerName][fileName];
                var contentType = _contentTypes[containerName][fileName];
                return (data, contentType);
            }
            
            return null;
        }
    }
}