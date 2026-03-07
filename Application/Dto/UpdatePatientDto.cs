namespace Application.Dto;

public record UpdatePatientDto(
    string? ImagePath,
    string? UserName,
    string? Email,
    string? PhoneNumber
);