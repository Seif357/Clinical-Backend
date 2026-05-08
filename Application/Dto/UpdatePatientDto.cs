using Microsoft.AspNetCore.Http;

namespace Application.Dto;

public record UpdatePatientDto(
    IFormFile? Image,
    string? UserName,
    string? Email,
    string? PhoneNumber,
    bool IsDeleted
);