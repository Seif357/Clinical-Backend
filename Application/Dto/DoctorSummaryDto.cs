namespace Application.Dto;

public class DoctorSummaryDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public string ProfessionalPracticeLicense { get; set; } = string.Empty;
    public string IssuingAuthority { get; set; } = string.Empty;
    public DateOnly? LicenseExpirationDate { get; set; }
    public bool? IsLicenseVerified { get; set; }
    public string ApprovalStatus { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}