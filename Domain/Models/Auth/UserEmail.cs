namespace Domain.Models.Auth;

/// <summary>
/// Supports multiple e-mail addresses per user.
/// The canonical primary email is still stored in AspNetUsers.Email for
/// Identity compatibility; this table tracks all extras + verification state.
/// </summary>
public class UserEmail : ParentEntity
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public bool IsVerified { get; set; }
    public string? VerificationToken { get; set; }
    public DateTime? VerificationTokenExpiresAt { get; set; }
    public DateTime? VerifiedAt { get; set; }

    public AppUser User { get; set; } = null!;
}