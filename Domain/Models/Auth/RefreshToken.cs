using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Auth;

public class RefreshToken
{
    [Key]
    public string Token { get; set; } = Guid.NewGuid().ToString();
    public int UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? RevokeReason { get; set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

}