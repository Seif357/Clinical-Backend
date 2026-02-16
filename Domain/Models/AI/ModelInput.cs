namespace Domain.Models.AI;

public class ModelInput : ParentEntity
{
    public string HistopathologyImagePath { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int? PatientId { get; set; }
    public string? Notes { get; set; }
    public DateTime UploadedAt => DateTime.Now;
    public string Status { get; set; } = "Uploaded";
    
    public ModelOutput? Output { get; set; }
}