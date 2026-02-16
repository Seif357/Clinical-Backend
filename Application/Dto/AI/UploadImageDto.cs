using Microsoft.AspNetCore.Http;

namespace Application.Dto.AI;

public class UploadImageDto
{
    public IFormFile Image { get; set; } = null!;
    public int? PatientId { get; set; }
    public string? Notes { get; set; }
}
