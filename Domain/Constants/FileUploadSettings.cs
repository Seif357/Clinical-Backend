namespace Domain.Constants;

public static class FileUploadSettings
{
    public const int MaxFileSizeMB = 10;
    public const long MaxFileSizeBytes = MaxFileSizeMB * 1024 * 1024;
    
    public static readonly string[] AllowedExtensions = 
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".tiff",
        ".tif"
    };
    
    public const string DefaultStoragePath = "wwwroot/uploads/histopathology";
}
