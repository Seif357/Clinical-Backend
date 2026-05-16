namespace Application.Dto.AuthDto;
public record GoogleRegisterDoctorDto(
    string IdToken,

    string ProfessionalPracticeLicense,

    string IssuingAuthority
);