using Microsoft.AspNetCore.Identity;

namespace Domain.Models.Auth;

public class AppUser : IdentityUser<int>
{
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; }
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}