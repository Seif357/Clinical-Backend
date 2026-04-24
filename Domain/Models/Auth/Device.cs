namespace Domain.Models.Auth;

/// <summary>
/// Represents a browser/app session device.
/// A user can have many devices; each RefreshToken belongs to exactly one device.
/// </summary>
public class Device : ParentEntity
{
    public int UserId { get; set; }

    /// <summary>e.g. "Chrome 124 / Windows 11"</summary>
    public string UserAgent { get; set; } = string.Empty;

    /// <summary>Client IP at login time.</summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>Friendly label the user can set: "My iPhone", "Work laptop".</summary>
    public string? DeviceName { get; set; }

    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

    public AppUser User { get; set; } = null!;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}