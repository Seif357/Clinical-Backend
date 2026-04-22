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
}