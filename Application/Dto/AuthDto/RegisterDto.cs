namespace Application.Dto.AuthDto;

public record RegisterDto(
    string Username,
    string Email,
    string PhoneNumber,
    string Password,
    string ConfirmPassword,
    bool IsDoctor,
    string? ProfessionalPracticeLicense,
    string? IssuingAuthority
);