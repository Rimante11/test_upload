using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ImageUploadApi.Models.DTOs;
using ImageUploadApi.Services;

namespace ImageUploadApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly ILogger<ImagesController> _logger;

        public ImagesController(IImageService imageService, ILogger<ImagesController> logger)
        {
            _imageService = imageService;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<ActionResult<ImageUploadResponseDto>> UploadImage(
            [FromForm] IFormFile file,
            [FromForm] string? description = null,
            [FromForm] string? tags = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file provided");
                }

                var (userId, tenantId) = GetUserAndTenantId();

                var request = new ImageUploadRequestDto
                {
                    Description = description,
                    Tags = tags
                };

                var result = await _imageService.UploadImageAsync(file, userId, tenantId, request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid upload request");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ImageListResponseDto>>> GetImages()
        {
            try
            {
                var (userId, tenantId) = GetUserAndTenantId();
                var images = await _imageService.GetUserImagesAsync(userId, tenantId);
                return Ok(images);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting images");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ImageUploadResponseDto>> GetImage(int id)
        {
            try
            {
                var (userId, tenantId) = GetUserAndTenantId();
                var image = await _imageService.GetImageByIdAsync(id, userId, tenantId);
                
                if (image == null)
                {
                    return NotFound("Image not found");
                }

                return Ok(image);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image {ImageId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteImage(int id)
        {
            try
            {
                var (userId, tenantId) = GetUserAndTenantId();
                var success = await _imageService.DeleteImageAsync(id, userId, tenantId);
                
                if (!success)
                {
                    return NotFound("Image not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image {ImageId}", id);
                return StatusCode(500, "Internal server error");
            }
        }



        private (int userId, int tenantId) GetUserAndTenantId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;

            if (userIdClaim == null || tenantIdClaim == null ||
                !int.TryParse(userIdClaim, out var userId) ||
                !int.TryParse(tenantIdClaim, out var tenantId))
            {
                throw new UnauthorizedAccessException("Invalid user or tenant claims");
            }

            return (userId, tenantId);
        }
    }
}