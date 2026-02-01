namespace Infrastructure.Configuration;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string ValidIssuer { get; set; } = string.Empty;
    public string ValidAudience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int AdminExpiryMinutes { get; set; }
    public int UserExpiryMinutes { get; set; }
    public int RefreshTokenExpiryHours { get; set; }
}
