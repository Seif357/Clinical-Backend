using Application.Dto;
using Application.Dto.AuthDto;
using Application.Dto.Doctor_approval;
using Application.DTOs;
using Application.Interfaces;
using Application.Mapper;
using Domain.Models;
using Infrastructure.DataAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class DoctorAdminService(
    AppDbContext context,
    IEmailService emailService) : IDoctorAdminService
{
    public async Task<PagedResponse<DoctorRegistrationSummaryDto>> GetPendingDoctorsAsync(int page, int pageSize)
    {
        var query = context.Doctors
            .Include(d => d.DoctorData)
            .Where(d => d.ApprovalStatus == DoctorApprovalStatus.Pending && !d.IsDeleted);

        return await query.ToPagedDtoAsync(d => d.ToSummaryDtoDto(), page, pageSize);

    }

    public async Task<PagedResponse<DoctorRegistrationSummaryDto>> GetApprovedDoctorsAsync(int page, int pageSize)
    {
        var query = context.Doctors
            .Include(d => d.DoctorData)
            .Where(d => d.ApprovalStatus == DoctorApprovalStatus.Approved && !d.IsDeleted);

        return await query.ToPagedDtoAsync(d => d.ToSummaryDtoDto(), page, pageSize);
    }

    public async Task<Result> GetDoctorRegistrationAsync(int doctorUserId)
    {
        var doctor = await context.Doctors
            .Include(d => d.DoctorData)
            .FirstOrDefaultAsync(d => d.UserId == doctorUserId);

        if (doctor is null)
            return new Result { Success = false, Message = "Doctor not found." };

        return new Result { Success = true, Data = doctor.ToSummaryDtoDto() };
    }

    public async Task<Result> ApproveDoctorAsync(int adminUserId, ApproveDoctorDto dto)
    {
        var doctor = await context.Doctors
            .Include(d => d.DoctorData)
            .FirstOrDefaultAsync(d => d.UserId == dto.DoctorUserId && !d.IsDeleted);

        if (doctor is null)
            return new Result { Success = false, Message = "Doctor not found." };

        if (doctor.ApprovalStatus == DoctorApprovalStatus.Approved)
            return new Result { Success = false, Message = "Doctor is already approved." };

        doctor.ApprovalStatus    = DoctorApprovalStatus.Approved;
        doctor.ApprovedByAdminId = adminUserId;
        doctor.ApprovedAt        = DateTime.UtcNow;
        doctor.RejectionReason   = null;

        await context.SaveChangesAsync();

        // Send approval email (fire-and-forget – don't fail the request if email fails)
        var email    = doctor.DoctorData.Email;
        var name     = doctor.DoctorData.UserName ?? "Doctor";
        if (!string.IsNullOrWhiteSpace(email))
        {
            var html = $"""
                <div style="font-family:sans-serif;max-width:520px;margin:auto">
                  <h2 style="color:#22c55e">🎉 Registration Approved – Clinical</h2>
                  <p>Hi <strong>{name}</strong>,</p>
                  <p>We're pleased to let you know that your doctor registration on <strong>Clinical</strong> has been <strong>approved</strong>.</p>
                  <p>You are now visible to patients and can start accepting appointments through the platform.</p>
                  <hr style="border:none;border-top:1px solid #e5e7eb;margin:24px 0"/>
                  <p style="color:#6b7280;font-size:13px">If you have any questions, please contact our support team.</p>
                </div>
                """;
            _ = emailService.SendAsync(email, "Your Clinical registration has been approved ✅", html);
        }

        return new Result { Success = true, Message = "Doctor approved. They are now visible to patients." };
    }

    public async Task<Result> RejectDoctorAsync(int adminUserId, RejectDoctorDto dto)
    {
        var doctor = await context.Doctors
            .Include(d => d.DoctorData)
            .FirstOrDefaultAsync(d => d.UserId == dto.DoctorUserId && !d.IsDeleted);

        if (doctor is null)
            return new Result { Success = false, Message = "Doctor not found." };

        doctor.ApprovalStatus    = DoctorApprovalStatus.Rejected;
        doctor.ApprovedByAdminId = adminUserId;
        doctor.ApprovedAt        = null;
        doctor.RejectionReason   = dto.Reason;

        await context.SaveChangesAsync();

        // Send rejection email
        var email = doctor.DoctorData.Email;
        var name  = doctor.DoctorData.UserName ?? "Doctor";
        if (!string.IsNullOrWhiteSpace(email))
        {
            var reasonHtml = string.IsNullOrWhiteSpace(dto.Reason)
                ? string.Empty
                : $"""<p><strong>Reason:</strong> {dto.Reason}</p>""";

            var html = $"""
                <div style="font-family:sans-serif;max-width:520px;margin:auto">
                  <h2 style="color:#ef4444">Registration Not Approved – Clinical</h2>
                  <p>Hi <strong>{name}</strong>,</p>
                  <p>Unfortunately, your doctor registration on <strong>Clinical</strong> has <strong>not been approved</strong> at this time.</p>
                  {reasonHtml}
                  <p>If you believe this decision is incorrect, or if you'd like to re-apply after addressing the reason above, please submit a new registration with the required corrections.</p>
                  <hr style="border:none;border-top:1px solid #e5e7eb;margin:24px 0"/>
                  <p style="color:#6b7280;font-size:13px">If you have any questions, please reach out to our support team.</p>
                </div>
                """;
            _ = emailService.SendAsync(email, "Update on your Clinical registration", html);
        }

        return new Result { Success = true, Message = "Doctor registration rejected." };
    }
}