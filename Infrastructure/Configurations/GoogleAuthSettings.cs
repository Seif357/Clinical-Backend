namespace Infrastructure.Configurations;

public class GoogleAuthSettings
{
    public const string SectionName = "GoogleAuth";
    public string ClientId { get; set; } = string.Empty;
    public string AndroidClientId { get; set; } = string.Empty;
}
