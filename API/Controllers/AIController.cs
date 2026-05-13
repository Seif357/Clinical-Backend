using Application.Dto.AI;
using Application.Interfaces;
using FluentValidation;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AIController : ControllerBase
{
    private readonly IImageProcessingService _imageProcessingService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IValidator<UploadImageDto> _uploadValidator;
    private readonly IAiService _aiService;
    private readonly ILogger<AIController> _logger;

    public AIController(
        IImageProcessingService imageProcessingService,
        IFileStorageService fileStorageService,
        IValidator<UploadImageDto> uploadValidator,
        IAiService aiService,
        ILogger<AIController> logger)
    {
        _imageProcessingService = imageProcessingService;
        _fileStorageService = fileStorageService;
        _uploadValidator = uploadValidator;
        _aiService = aiService;
        _logger = logger;
    }


    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [ProducesResponseType(typeof(ImageUploadResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadImage([FromForm] UploadImageDto dto)
    {
        try
        {
            var validationResult = await _uploadValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Image upload validation failed: {Errors}", 
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return BadRequest(new { Errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var result = await _imageProcessingService.UploadImageAsync(dto);
            
            _logger.LogInformation("Image uploaded successfully with ID {ImageId}");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during image upload");
            return StatusCode(500, new { Message = "An error occurred while uploading the image" });
        }
    }


    [HttpGet("image/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImageById(int id)
    {
        try
        {
            var image = await _imageProcessingService.GetImageByIdAsync(id);
            
            if (image == null)
            {
                _logger.LogWarning("Image with ID {ImageId} not found", id);
                return NotFound(new { Message = $"Image with ID {id} not found" });
            }

            return Ok(image);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving image with ID {ImageId}", id);
            return StatusCode(500, new { Message = "An error occurred while retrieving the image" });
        }
    }

    /// Get all images for a specific patient
    [HttpGet("patient/{PatientId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetImagesByPatientId(int patientId)
    {
        try
        {
            var images = await _imageProcessingService.GetImagesByPatientIdAsync(patientId);
            return Ok(images);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving images for patient {PatientId}", patientId);
            return StatusCode(500, new { Message = "An error occurred while retrieving images" });
        }
    }

    
    /// Delete an image by ID
    [HttpDelete("image/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(int id)
    {
        try
        {
            var success = await _imageProcessingService.DeleteImageAsync(id);
            
            if (!success)
            {
                _logger.LogWarning("Image with ID {ImageId} not found for deletion", id);
                return NotFound(new { Message = $"Image with ID {id} not found" });
            }

            _logger.LogInformation("Image with ID {ImageId} deleted successfully", id);
            return Ok(new { Message = "Image deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image with ID {ImageId}", id);
            return StatusCode(500, new { Message = "An error occurred while deleting the image" });
        }
    }
    
    [HttpPost("analyze/{imageId:int}")]
    [ProducesResponseType(typeof(AiAnalysisResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> AnalyzeImage(int imageId)
    {
        _logger.LogInformation("Analyse request received for image {ImageId}", imageId);

        var result = await _aiService.AnalyzeImageAsync(imageId);

        if (!result.Success)
        {
            // Distinguish between "not found" and "model unreachable"
            if (result.Message!.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { result.Message });

            return StatusCode(StatusCodes.Status502BadGateway, new { result.Message });
        }

        return Ok(result);
    }

    [HttpGet("image/{id:int}/file")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadImageFile(int id)
    {
        try
        {
            var image = await _imageProcessingService.GetImageByIdAsync(id);
            
            if (image == null)
            {
                _logger.LogWarning("Image with ID {ImageId} not found for download", id);
                return NotFound(new { Message = $"Image with ID {id} not found" });
            }

            var fileBytes = await _fileStorageService.ReadFileAsync(image.HistopathologyImagePath);
            var contentType = GetContentType(image.OriginalFileName);

            _logger.LogInformation("Image file with ID {ImageId} downloaded", id);
            return File(fileBytes, contentType, image.OriginalFileName);
        }
        catch (FileNotFoundException)
        {
            _logger.LogError("Image file not found on disk for ID {ImageId}", id);
            return NotFound(new { Message = "Image file not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading image file with ID {ImageId}", id);
            return StatusCode(500, new { Message = "An error occurred while downloading the image" });
        }
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".tiff" or ".tif" => "image/tiff",
            _ => "application/octet-stream"
        };
    }
}