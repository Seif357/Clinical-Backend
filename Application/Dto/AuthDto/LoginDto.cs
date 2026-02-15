namespace Application.Dto.AuthDto;

public record LoginDto(
    string UsernameOrEmail,
    string Password
);