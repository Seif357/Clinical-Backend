using Domain.Models.Auth;
 
namespace Application.Interfaces;
 
public interface IOtpService
{
    /// <summary>
    /// Creates a 6-digit OTP, persists it (hashed), and sends it.
    /// For PasswordChangeConfirmation pass pendingPasswordHash to store atomically.
    /// </summary>
    Task<string> IssueAsync(
        int userId,
        string displayName,
        string target,           // email or phone
        OtpPurpose purpose,
        string? pendingPasswordHash = null);
 
    /// <summary>Verifies the plain-text code against the stored hash. Returns the record if valid.</summary>
    Task<OtpRecord?> VerifyAsync(int userId, OtpPurpose purpose, string plainCode);
}