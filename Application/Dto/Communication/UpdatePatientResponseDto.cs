using Microsoft.AspNetCore.Http;

namespace Application.Dto.Communication;

public record UpdatePatientResponseDto(
    string? Subject,
    string? Message,
    List<IFormFile>? NewImages,
    List<int>? ImageIdsToRemove
);
