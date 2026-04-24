using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.Auth;

namespace Domain.Models;

public class Doctor : BaseUser
{
    [Key] public int UserId { get; set; }

    public DateOnly? LicenseExpirationDate { get; set; }

    [Required] public required string ProfessionalPracticeLicense { get; set; }

    public bool? IsLicenseVerified { get; set; }

    public bool IsLicenseExpired =>
        LicenseExpirationDate.HasValue && LicenseExpirationDate > DateOnly.FromDateTime(DateTime.Now);

    public required string IssuingAuthority { get; set; }

    // ── Doctor approval ──────────────────────────────────────────────────────
    public DoctorApprovalStatus ApprovalStatus { get; set; } = DoctorApprovalStatus.Pending;

    /// <summary>Set by the admin who approved/rejected the registration.</summary>
    public int? ApprovedByAdminId { get; set; }

    public DateTime? ApprovedAt { get; set; }

    /// <summary>Optional rejection reason visible to the doctor.</summary>
    public string? RejectionReason { get; set; }

    [ForeignKey("UserId")] public AppUser DoctorData { get; set; } = null!;
}