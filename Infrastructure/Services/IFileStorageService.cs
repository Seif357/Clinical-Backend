using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string folder);
    Task<bool> DeleteFileAsync(string filePath);
    Task<bool> FileExistsAsync(string filePath);
    Task<byte[]> ReadFileAsync(string filePath);
}
