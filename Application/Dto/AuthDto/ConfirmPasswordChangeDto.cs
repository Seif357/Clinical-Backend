namespace Application.Dto.AuthDto;

/// <summary>Step 2 – user submits the OTP + the new password.</summary>
public record ConfirmPasswordChangeDto(
    string OtpCode,
    string NewPassword,
    string ConfirmNewPassword
);