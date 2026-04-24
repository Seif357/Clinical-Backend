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
    AppDbContext context) : IDoctorAdminService
{
    public async Task<PagedResponse<DoctorRegistrationSummaryDto>> GetPendingDoctorsAsync(int page, int pageSize)
    {
        var query = context.Doctors
            .Include(d => d.DoctorData)
            .Where(d => d.ApprovalStatus == DoctorApprovalStatus.Pending && !d.IsDeleted);

        return await query.ToPagedDtoAsync(d => d.ToDto(), page, pageSize);

    }

    public async Task<PagedResponse<DoctorRegistrationSummaryDto>> GetApprovedDoctorsAsync(int page, int pageSize)
    {
        var query = context.Doctors
            .Include(d => d.DoctorData)
            .Where(d => d.ApprovalStatus == DoctorApprovalStatus.Approved && !d.IsDeleted);

        return await query.ToPagedDtoAsync(d => d.ToDto(), page, pageSize);
    }

    public async Task<Result> GetDoctorRegistrationAsync(int doctorUserId)
    {
        var doctor = await context.Doctors
            .Include(d => d.DoctorData)
            .FirstOrDefaultAsync(d => d.UserId == doctorUserId);

        if (doctor is null)
            return new Result { Success = false, Message = "Doctor not found." };

        return new Result { Success = true, Data = doctor.ToDto() };
    }

    public async Task<Result> ApproveDoctorAsync(int adminUserId, ApproveDoctorDto dto)
    {
        var doctor = await context.Doctors
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
        return new Result { Success = true, Message = "Doctor approved. They are now visible to patients." };
    }

    public async Task<Result> RejectDoctorAsync(int adminUserId, RejectDoctorDto dto)
    {
        var doctor = await context.Doctors
            .FirstOrDefaultAsync(d => d.UserId == dto.DoctorUserId && !d.IsDeleted);

        if (doctor is null)
            return new Result { Success = false, Message = "Doctor not found." };

        doctor.ApprovalStatus    = DoctorApprovalStatus.Rejected;
        doctor.ApprovedByAdminId = adminUserId;
        doctor.ApprovedAt        = null;
        doctor.RejectionReason   = dto.Reason;

        await context.SaveChangesAsync();
        return new Result { Success = true, Message = "Doctor registration rejected." };
    }
    
}