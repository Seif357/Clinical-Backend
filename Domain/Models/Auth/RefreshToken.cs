using System.ComponentModel.DataAnnotations;
namespace Domain.Models.Auth;
public class RefreshToken
{
    public Guid Id { get; set; }
public string Token { get; set; } = string.Empty;
public string UserId { get; set; } = string.Empty;
public DateTime CreatedAt { get; set; }
public DateTime ExpiresAt { get; set; }
public DateTime? RevokedAt { get; set; }
public string? ReplacedByToken { get; set; }
public string? ReasonRevoked { get; set; }
public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
public bool IsRevoked => RevokedAt != null;
public bool IsActive => !IsRevoked && !IsExpired;
public AppUser User { get; set; } = null!;
}