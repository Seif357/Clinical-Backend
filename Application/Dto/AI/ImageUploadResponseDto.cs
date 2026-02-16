namespace Application.Dto.AI;

public class ImageUploadResponseDto
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
