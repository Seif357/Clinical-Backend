namespace Domain.Models.Auth;

public enum OtpPurpose
{
    EmailVerification = 1,
    PhoneVerification,
    ForgotPassword,
    PasswordChangeConfirmation   // used by the "update password via email" flow
}

/// <summary>
/// A short-lived, single-use code sent to e-mail or SMS.
/// One row per request; the code is hashed before storage.
/// </summary>
public class OtpRecord : ParentEntity
{
    public int UserId { get; set; }
    public OtpPurpose Purpose { get; set; }

    /// <summary>E-mail address OR phone number the code was sent to.</summary>
    public string Target { get; set; } = string.Empty;

    /// <summary>Bcrypt hash of the 6-digit code.</summary>
    public string CodeHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }

    /// <summary>
    /// For PasswordChangeConfirmation: stores the new password hash so the
    /// service can apply it atomically once the OTP is verified.
    /// </summary>
    public string? PendingPasswordHash { get; set; }

    public AppUser User { get; set; } = null!;
}