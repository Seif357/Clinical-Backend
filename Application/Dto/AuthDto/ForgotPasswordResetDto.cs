namespace Application.Dto.AuthDto;

/// <summary>Step 2 – verify the OTP and set the new password.</summary>
public record ForgotPasswordResetDto(
    string EmailOrPhone,
    string OtpCode,
    string NewPassword,
    string ConfirmNewPassword
);