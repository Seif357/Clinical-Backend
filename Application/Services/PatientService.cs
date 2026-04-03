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

public class PatientService(AppDbContext context,
    UserManager<AppUser> userManager) : IPatientService
{
    public async Task<IActionResult> GetPatientDataServiceAsync(string userId)
    {
        var patient = await context.Patients
            .AsNoTracking()
            .Include(p => p.PatientData)
            .Include(p => p.MedicalRecord)
            .ThenInclude(m => m.Allergies)
            .Include(p => p.MedicalRecord)
            .ThenInclude(m => m.Visits)
            .Include(p => p.MedicalRecord)
            .ThenInclude(m => m.Surgeries)
            .Include(p => p.MedicalRecord)
            .ThenInclude(m => m.TestsTaken)
            .Include(p => p.MedicalRecord)
            .ThenInclude(m => m.PrescribedMedications)
            .Include(p => p.MedicalRecord)
            .ThenInclude(m => m.FamilyConditions)
            .FirstOrDefaultAsync(p => p.UserId.ToString() == userId && !p.IsDeleted);

        if (patient is null)
            return new Result { Success = false, Message = "Patient not found" };

        return new Result<Patient> { Success = true, Data = patient };
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

        if (dto.ImagePath is not null)
            patient.ImagePath = dto.ImagePath;

        await context.SaveChangesAsync();

        return new Result { Success = true, Message = "Profile updated successfully" };
    }
}