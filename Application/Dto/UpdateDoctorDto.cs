using Microsoft.AspNetCore.Http;

namespace Application.Dto;

public record UpdateDoctorDto(
    IFormFile? Image,
    string? UserName,
    string? Email,
    string? PhoneNumber,
    string? ProfessionalPracticeLicense,
    string? IssuingAuthority,
    DateOnly? LicenseExpirationDate
);