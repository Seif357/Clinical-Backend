using Application.Dto;
using Application.Dto.AuthDto;
using Application.DTOs;
using Application.Interfaces;
using Application.Mapper;
using Domain.Models;
using Domain.Models.Auth;
using Infrastructure.DataAccess;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class PatientService(AppDbContext context,
    UserManager<AppUser> userManager,
    IFileStorageService fileStorage) : IPatientService
{
    public async Task<IActionResult> GetPatientDataServiceAsync(string userId)
    {
        var patient = await context.Patients
            .AsNoTracking()
            .Include(p => p.PatientData)
            .FirstOrDefaultAsync(p => p.UserId.ToString() == userId && !p.IsDeleted);

        if (patient is null)
            return new Result { Success = false, Message = "Patient not found" };

        return new Result<PatientDto> { Success = true, Data =patient.ToDto() };
    }

    public async Task<IActionResult> UpdatePatientDataServiceAsync(string userId, UpdatePatientDto dto)
    {
        var patient = await context.Patients
            .Include(p => p.PatientData)
            .FirstOrDefaultAsync(p => p.UserId.ToString() == userId && !p.IsDeleted);

        if (patient is null)
            return new Result { Success = false, Message = "Patient not found" };

        var user = patient.PatientData;
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

        if (dto.Image is not null)
        {
            var oldPath = patient.ImagePath;
            patient.ImagePath = await fileStorage.SaveFileAsync(dto.Image, "profiles");
            if (oldPath is not null)
                await fileStorage.DeleteFileAsync(oldPath);
        }

        if (dto.DateOfBirth.HasValue)
            patient.DateOfBirth = dto.DateOfBirth.Value;

        if (dto.BloodType.HasValue)
            patient.BloodType = dto.BloodType.Value;

        await context.SaveChangesAsync();

        return new Result { Success = true, Message = "Profile updated successfully" };
    }
}