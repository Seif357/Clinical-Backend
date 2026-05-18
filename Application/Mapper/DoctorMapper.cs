using Application.Dto;
using Domain.Models;

namespace Application.Mapper;

public static class DoctorMapper
{
    public static DoctorDto ToDto(this Doctor d) => new(
        UserId:                      d.UserId,
        UserName:                    d.DoctorData.UserName ?? string.Empty,
        Email:                       d.DoctorData.Email    ?? string.Empty,
        PhoneNumber:                 d.DoctorData.PhoneNumber,
        EmailConfirmed:              d.DoctorData.EmailConfirmed,
        PhoneNumberConfirmed:        d.DoctorData.PhoneNumberConfirmed,
        ImagePath:                   d.ImagePath,
        Gender:                      d.Gender,
        LicenseExpirationDate:       d.LicenseExpirationDate,
        ProfessionalPracticeLicense: d.ProfessionalPracticeLicense,
        IssuingAuthority:            d.IssuingAuthority,
        IsLicenseVerified:           d.IsLicenseVerified,
        IsLicenseExpired:            d.IsLicenseExpired,
        ApprovalStatus:              d.ApprovalStatus,
        RejectionReason:             d.RejectionReason,
        ApprovedAt:                  d.ApprovedAt,
        CreatedAt:                   d.CreatedAt,
        UpdatedAt:                   d.UpdatedAt
    );
}