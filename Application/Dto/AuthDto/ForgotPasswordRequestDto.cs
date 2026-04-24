namespace Application.Dto.AuthDto;

/// <summary>Step 1 – request a reset OTP; only email/phone required.</summary>
public record ForgotPasswordRequestDto(string EmailOrPhone);