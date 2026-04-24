namespace Domain.Models.Auth;

/// <summary>
/// Supports multiple phone numbers per user, one marked primary.
/// OTP-based verification works the same way as e-mail tokens.
/// </summary>
public class UserPhone : ParentEntity
{
    public int UserId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public bool IsVerified { get; set; }
    public string? OtpCode { get; set; }
    public DateTime? OtpExpiresAt { get; set; }
    public DateTime? VerifiedAt { get; set; }

    public AppUser User { get; set; } = null!;
}