using Domain.Models;
using Domain.Models.Auth;

namespace Application.Dto;

public record DoctorDto(
    int UserId,
    string UserName,
    string Email,
    string? PhoneNumber,
    bool EmailConfirmed,
    bool PhoneNumberConfirmed,
    string? ImagePath,
    Gender Gender,
    DateOnly? LicenseExpirationDate,
    string ProfessionalPracticeLicense,
    string IssuingAuthority,
    bool? IsLicenseVerified,
    bool IsLicenseExpired,
    DoctorApprovalStatus ApprovalStatus,
    string? RejectionReason,
    DateTime? ApprovedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
);