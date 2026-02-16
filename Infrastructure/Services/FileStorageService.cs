using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _basePath;

    public FileStorageService(ILogger<FileStorageService> logger)
    {
        _logger = logger;
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "histopathology");
        
        // Ensure directory exists
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
            _logger.LogInformation("Created upload directory: {BasePath}", _basePath);
        }
    }

    public async Task<string> SaveFileAsync(IFormFile file, string folder)
    {
        try
        {
            var uploadPath = Path.Combine(_basePath, folder);
            
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            _logger.LogInformation("File saved successfully: {FilePath}, Size: {Size} bytes", 
                filePath, file.Length);

            // Return relative path from wwwroot
            var relativePath = Path.Combine("uploads", "histopathology", folder, uniqueFileName)
                .Replace("\\", "/");
            
            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file: {FileName}", file.FileName);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath);
            
            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
                _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
                return true;
            }

            _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath);
            return await Task.Run(() => File.Exists(fullPath));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<byte[]> ReadFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath);
            
            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("File not found: {FilePath}", filePath);
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            return await File.ReadAllBytesAsync(fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file: {FilePath}", filePath);
            throw;
        }
    }
}
