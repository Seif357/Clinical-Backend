using Application.Dto.AI;
using Application.DTOs;
using Application.Interfaces;
using Domain.Models.AI;
using Infrastructure.DataAccess;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ImageProcessingService : IImageProcessingService
{
    private readonly AppDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<ImageProcessingService> _logger;

    public ImageProcessingService(
        AppDbContext context,
        IFileStorageService fileStorageService,
        ILogger<ImageProcessingService> logger)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<Result> UploadImageAsync(UploadImageDto dto)
    {
        try
        {
            _logger.LogInformation("Starting image upload for patient {PatientId}", dto.PatientId);

            var folder = dto.PatientId?.ToString() ?? "unassigned";
            var filePath = await _fileStorageService.SaveFileAsync(dto.Image, folder);

            var modelInput = new ModelInput
            {
                HistopathologyImagePath = filePath,
                OriginalFileName = dto.Image.FileName,
                FileSizeBytes = dto.Image.Length,
                PatientId = dto.PatientId,
                Notes = dto.Notes,
                Status = "Uploaded"
            };
            await _context.Set<ModelInput>().AddAsync(modelInput);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Image uploaded successfully with ID {Id}", modelInput.Id);

            return new Result
            {
                Success = true,
                Message = "Image uploaded successfully",
                Data = new ImageUploadResponseDto
                {
                    Id            = modelInput.Id,
                    FilePath      = modelInput.HistopathologyImagePath,
                    FileName      = modelInput.OriginalFileName,
                    FileSizeBytes = modelInput.FileSizeBytes,
                    UploadedAt    = modelInput.UploadedAt,
                    Status        = modelInput.Status
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image for patient {PatientId}", dto.PatientId);
            return new Result { Success = false, Message = "Failed to upload image" };
        }
    }

    public async Task<ModelInputDto?> GetImageByIdAsync(int id)
    {
        try
        {
            var entity = await _context.Set<ModelInput>()
                .Include(m => m.Output)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            return entity is null ? null : ToDto(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving image with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ModelInputDto>> GetImagesByPatientIdAsync(int patientId)
    {
        try
        {
            var entities = await _context.Set<ModelInput>()
                .Include(m => m.Output)
                .Where(m => m.PatientId == patientId && !m.IsDeleted)
                .OrderByDescending(m => m.UploadedAt)
                .ToListAsync();

            return entities.Select(ToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving images for patient {PatientId}", patientId);
            throw;
        }
    }

    public async Task<bool> DeleteImageAsync(int id)
    {
        try
        {
            var modelInput = await _context.Set<ModelInput>().FindAsync(id);

            if (modelInput == null)
            {
                _logger.LogWarning("Image with ID {Id} not found for deletion", id);
                return false;
            }

            await _fileStorageService.DeleteFileAsync(modelInput.HistopathologyImagePath);
            _context.Set<ModelInput>().Remove(modelInput);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Image with ID {Id} deleted successfully", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image with ID {Id}", id);
            return false;
        }
    }

    // ── Mapper ────────────────────────────────────────────────────────────────

    private static ModelInputDto ToDto(ModelInput m) => new(
        Id:                      m.Id,
        HistopathologyImagePath: m.HistopathologyImagePath,
        OriginalFileName:        m.OriginalFileName,
        FileSizeBytes:           m.FileSizeBytes,
        PatientId:               m.PatientId,
        Notes:                   m.Notes,
        UploadedAt:              m.UploadedAt,
        Status:                  m.Status,
        Output: m.Output is null ? null : new ModelOutputDto(
            ModelInputId:   m.Output.ModelInputId,
            Classification: m.Output.Classification,
            Confidence:     m.Output.Confidence,
            ProcessedAt:    m.Output.ProcessedAt
        )
    );
}