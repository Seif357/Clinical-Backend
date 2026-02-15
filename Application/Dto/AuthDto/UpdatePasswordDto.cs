namespace Application.Dto.AuthDto;

public record UpdatePasswordDto(
    string Password,
    string NewPassword,
    string ConfirmNewPassword
);