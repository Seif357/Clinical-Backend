using Application.Dto;
using Application.Dto.AuthDto;
using Application.DTOs;
using Application.Interfaces;
using Domain.Models;
using Domain.Models.Auth;
using Infrastructure.DataAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class DoctorService(AppDbContext context,
    UserManager<AppUser> userManager) : IDoctorService
{
    public async Task<IActionResult> GetDoctorDataServiceAsync(string userId)
    {
        var doctor = await context.Doctors
            .AsNoTracking()
            .Include(d => d.DoctorData)
            .FirstOrDefaultAsync(d => d.UserId.ToString() == userId && !d.IsDeleted);

        if (doctor is null)
            return new Result { Success = false, Message = "Doctor not found" };

        return new Result<Doctor> { Success = true, Data = doctor };
    }

    public async Task<IActionResult> UpdateDoctorDataServiceAsync(string userId, UpdateDoctorDto dto)
    {
        var doctor = await context.Doctors
            .Include(d => d.DoctorData)
            .FirstOrDefaultAsync(d => d.UserId.ToString() == userId && !d.IsDeleted);

        if (doctor is null)
            return new Result { Success = false, Message = "Doctor not found" };

        var user = doctor.DoctorData;
        var userChanged = false;

        if (!string.IsNullOrEmpty(dto.UserName) && dto.UserName != user.UserName)
        {
            user.UserName = dto.UserName;
            userChanged = true;
        }

        if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
        {
            user.Email = dto.Email;
            userChanged = true;
        }

        if (!string.IsNullOrEmpty(dto.PhoneNumber) && dto.PhoneNumber != user.PhoneNumber)
        {
            user.PhoneNumber = dto.PhoneNumber;
            userChanged = true;
        }

        if (userChanged)
        {
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return new Result
                {
                    Success = false,
                    Message = $"Failed to update profile: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}"
                };
        }

        if (dto.ImagePath is not null)
            doctor.ImagePath = dto.ImagePath;

        if (!string.IsNullOrEmpty(dto.ProfessionalPracticeLicense))
            doctor.ProfessionalPracticeLicense = dto.ProfessionalPracticeLicense;

        if (!string.IsNullOrEmpty(dto.IssuingAuthority))
            doctor.IssuingAuthority = dto.IssuingAuthority;

        if (dto.LicenseExpirationDate.HasValue)
            doctor.LicenseExpirationDate = dto.LicenseExpirationDate;

        await context.SaveChangesAsync();

        return new Result { Success = true, Message = "Doctor profile updated successfully" };
    }
    
    public async Task<Result> SearchDoctorsAsync(DoctorSearchQuery q)
    {
        var query = context.Doctors
            .Include(d => d.DoctorData)
            .Where(d => !d.IsDeleted && d.ApprovalStatus == DoctorApprovalStatus.Approved)
            .AsQueryable();
 
        // ── Search ────────────────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim().ToLower();
            query = query.Where(d =>
                d.DoctorData.UserName!.ToLower().Contains(term) ||
                d.DoctorData.Email!.ToLower().Contains(term)    ||
                d.IssuingAuthority.ToLower().Contains(term));
        }
 
        // ── Filters ───────────────────────────────────────────────────────────
        if (q.IsLicenseVerified.HasValue)
            query = query.Where(d => d.IsLicenseVerified == q.IsLicenseVerified.Value);
 
        if (q.HasSchedule.HasValue)
        {
            var doctorIdsWithSchedule = context.Schedules
                .Where(s => !s.IsDeleted)
                .Select(s => s.DoctorId);
 
            query = q.HasSchedule.Value
                ? query.Where(d => doctorIdsWithSchedule.Contains(d.UserId))
                : query.Where(d => !doctorIdsWithSchedule.Contains(d.UserId));
        }
 
        // ── Sort ──────────────────────────────────────────────────────────────
        query = (q.SortBy?.ToLower(), q.Descending) switch
        {
            ("name",         false) => query.OrderBy(d => d.DoctorData.UserName),
            ("name",         true)  => query.OrderByDescending(d => d.DoctorData.UserName),
            ("registeredat", true)  => query.OrderByDescending(d => d.CreatedAt),
            _                       => query.OrderBy(d => d.CreatedAt)
        };
 
        // ── Paginate ──────────────────────────────────────────────────────────
        var pageSize = Math.Clamp(q.PageSize, 1, 100);
        var page     = Math.Max(q.Page, 1);
        var total    = await query.CountAsync();
 
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DoctorSummaryDto
            {
                UserId                      = d.UserId,
                UserName                    = d.DoctorData.UserName ?? string.Empty,
                Email                       = d.DoctorData.Email    ?? string.Empty,
                ImagePath                   = d.ImagePath,
                ProfessionalPracticeLicense = d.ProfessionalPracticeLicense,
                IssuingAuthority            = d.IssuingAuthority,
                LicenseExpirationDate       = d.LicenseExpirationDate,
                IsLicenseVerified           = d.IsLicenseVerified,
                ApprovalStatus              = d.ApprovalStatus.ToString(),
                RegisteredAt                = d.CreatedAt
            })
            .ToListAsync();
 
        return new Result
        {
            Success = true,
            Data    = new PagedResponse<DoctorSummaryDto>
            {
                Total    = total,
                Page     = page,
                PageSize = pageSize,
                Items    = items
            }
        };
    }
}