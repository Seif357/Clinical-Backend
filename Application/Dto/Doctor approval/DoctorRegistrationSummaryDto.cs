namespace Application.Dto.Doctor_approval;

public class DoctorRegistrationSummaryDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ProfessionalPracticeLicense { get; set; } = string.Empty;
    public string IssuingAuthority { get; set; } = string.Empty;
    public DateOnly? LicenseExpirationDate { get; set; }
    public string ApprovalStatus { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
}