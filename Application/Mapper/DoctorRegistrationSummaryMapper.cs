using Application.Dto;
using Application.Dto.AuthDto;
using Application.Dto.Doctor_approval;
using Application.DTOs;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Mapper;
public static class DoctorRegistrationSummaryMapper
{

    public static DoctorRegistrationSummaryDto ToDto(this Doctor d)
    {
        return new DoctorRegistrationSummaryDto()
        {

            UserId = d.UserId,
            UserName = d.DoctorData.UserName ?? string.Empty,
            Email = d.DoctorData.Email ?? string.Empty,
            ProfessionalPracticeLicense = d.ProfessionalPracticeLicense,
            IssuingAuthority = d.IssuingAuthority,
            LicenseExpirationDate = d.LicenseExpirationDate,
            ApprovalStatus = d.ApprovalStatus.ToString(),
            RejectionReason = d.RejectionReason,
            RegisteredAt = d.CreatedAt,
            ApprovedAt = d.ApprovedAt
        };
    }
}