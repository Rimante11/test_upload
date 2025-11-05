using Microsoft.AspNetCore.Mvc;
using ImageUploadApi.Services;

namespace ImageUploadApi.Controllers
{
    [ApiController]
    [Route("api/images/blob")]
    public class BlobController : ControllerBase
    {
        private readonly IBlobStorageService _blobService;

        public BlobController(IBlobStorageService blobService)
        {
            _blobService = blobService;
        }

        [HttpGet("{containerName}/{fileName}")]
        public ActionResult GetBlob(string containerName, string fileName)
        {
            var blob = _blobService.GetBlob(containerName, fileName);
            
            if (blob == null)
            {
                return NotFound();
            }

            return File(blob.Value.data, blob.Value.contentType);
        }
    }
}