namespace Infrastructure.Services;

public interface IImageUrlHelper
{
    /// <summary>
    /// Converts a stored relative path (e.g. "uploads/histopathology/profiles/guid.jpg")
    /// to a fully-qualified URL the frontend can use directly (e.g. "https://server/uploads/...").
    /// Returns null when <paramref name="relativePath"/> is null or empty.
    /// </summary>
    string? Resolve(string? relativePath);
}