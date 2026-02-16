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

            // Save file to storage
            var folder = dto.PatientId?.ToString() ?? "unassigned";
            var filePath = await _fileStorageService.SaveFileAsync(dto.Image, folder);

            // Create database record
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
                Message = "Image uploaded successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image for patient {PatientId}", dto.PatientId);
            return new Result
            {
                Success = false,
                Message = "Failed to upload image"
            };
        }
    }

    public async Task<ModelInput?> GetImageByIdAsync(int id)
    {
        try
        {
            return await _context.Set<ModelInput>()
                .Include(m => m.Output)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving image with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ModelInput>> GetImagesByPatientIdAsync(int patientId)
    {
        try
        {
            return await _context.Set<ModelInput>()
                .Include(m => m.Output)
                .Where(m => m.PatientId == patientId)
                .OrderByDescending(m => m.UploadedAt)
                .ToListAsync();
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

            // Delete file from storage
            await _fileStorageService.DeleteFileAsync(modelInput.HistopathologyImagePath);

            // Delete database record
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
}
