using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public class ImageUrlHelper(IHttpContextAccessor httpContextAccessor) : IImageUrlHelper
{
    public string? Resolve(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return null;

        // Already an absolute URL – return as-is
        if (relativePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            relativePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return relativePath;

        var req = httpContextAccessor.HttpContext?.Request;
        if (req is null)
            return relativePath; // fallback – return stored path unchanged

        // Normalise: strip any leading slash so we don't double up
        var path = relativePath.TrimStart('/');

        return $"{req.Scheme}://{req.Host}/{path}";
    }
}