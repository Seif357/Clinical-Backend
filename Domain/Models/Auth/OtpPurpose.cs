namespace Application.Dto.AuthDto;

public enum OtpPurpose
{
    EmailVerification = 1,
    PhoneVerification,
    ForgotPassword,
    PasswordChangeConfirmation,
    AccountDeletion
}