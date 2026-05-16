namespace Application.Dto.AuthDto;
public record GoogleLoginDto(
    string IdToken,
    bool IsDoctor
);
