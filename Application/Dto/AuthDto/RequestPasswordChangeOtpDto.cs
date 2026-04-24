namespace Application.Dto.AuthDto;

/// <summary>Step 1 – user requests a password-change OTP to their primary email.</summary>
public record RequestPasswordChangeOtpDto(
    string CurrentPassword   // proves they are logged in and know their password
);