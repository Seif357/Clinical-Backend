namespace Application.Dto;

public record UpdateDoctorDto(
    string? ImagePath,
    string? UserName,
    string? Email,
    string? PhoneNumber,
    string? ProfessionalPracticeLicense,
    string? IssuingAuthority,
    DateOnly? LicenseExpirationDate
);