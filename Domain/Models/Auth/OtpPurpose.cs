namespace Application.Dto.AuthDto;

public enum OtpPurpose
{
    EmailVerification = 1,
    PhoneVerification,
    ForgotPassword,
    PasswordChangeConfirmation   // used by the "update password via email" flow
}