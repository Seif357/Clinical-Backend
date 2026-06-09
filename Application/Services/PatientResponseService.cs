using Application.Dto.AuthDto;
using Application.Dto.Communication;
using Application.DTOs;
using Application.Interfaces;
using Domain.Models.Communication;
using Infrastructure.DataAccess;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class PatientResponseService(
    AppDbContext context,
    IFileStorageService fileStorage,
    INotificationService notificationService) : IPatientResponseService
{
    public async Task<IActionResult> CreateAsync(string patientId, CreatePatientResponseDto dto)
    {
        var doctorRequest = await context.DoctorRequests
            .FirstOrDefaultAsync(r => r.Id == dto.DoctorRequestId && !r.IsDeleted);
        var patientRequest = await context.PatientRequests
            .FirstOrDefaultAsync(r => r.Id == dto.DoctorRequestId && !r.IsDeleted);

        if (doctorRequest is null && patientRequest is null)
            return new Result { Success = false, Message = "Request not found" };

        // Resolve the owning patient and target doctor from whichever request was found.
        // Using null-coalescing here avoids NullReferenceException when only one is non-null.
        var requestPatientId = doctorRequest?.PatientId ?? patientRequest?.PatientId;
        var requestDoctorId  = doctorRequest?.DoctorId  ?? patientRequest?.DoctorId;

        if (requestPatientId != patientId)
            return new Result { Success = false, Message = "This request was not directed at you" };

        var response = new PatientResponse
        {
            PatientId = int.Parse(patientId),
            DoctorRequestId = dto.DoctorRequestId,
            Subject = dto.Subject,
            Message = dto.Message,
            PatientResponseImages = []
        };

        if (dto.Images is { Count: > 0 })
        {
            foreach (var file in dto.Images)
            {
                var path = await fileStorage.SaveFileAsync(file, "patient-responses");
                response.PatientResponseImages.Add(new PatientResponseImage { ImagePath = path });
            }
        }

        await context.PatientResponses.AddAsync(response);
        await context.SaveChangesAsync();

        await notificationService.NotifyUserAsync(requestDoctorId!, "NewPatientResponse", new
        {
            response.Id,
            response.Subject,
            PatientId = patientId,
            DoctorRequestId = dto.DoctorRequestId,
            SentAt = response.CreatedAt
        });

        return new Result<PatientResponse> { Success = true, Data = response, Message = "Response sent successfully" };
    }

    public async Task<IActionResult> UpdateAsync(string patientId, int responseId, UpdatePatientResponseDto dto)
    {
        var response = await context.PatientResponses
            .Include(r => r.PatientResponseImages)
            .FirstOrDefaultAsync(r => r.Id == responseId && r.PatientId == int.Parse(patientId) && !r.IsDeleted);

        if (response is null)
            return new Result { Success = false, Message = "Response not found" };

        if (!string.IsNullOrEmpty(dto.Subject))
            response.Subject = dto.Subject;

        if (!string.IsNullOrEmpty(dto.Message))
            response.Message = dto.Message;

        if (dto.ImageIdsToRemove is { Count: > 0 })
        {
            var toRemove = response.PatientResponseImages
                .Where(img => dto.ImageIdsToRemove.Contains(img.Id))
                .ToList();

            foreach (var img in toRemove)
            {
                await fileStorage.DeleteFileAsync(img.ImagePath);
                response.PatientResponseImages.Remove(img);
            }
        }

        if (dto.NewImages is { Count: > 0 })
        {
            foreach (var file in dto.NewImages)
            {
                var path = await fileStorage.SaveFileAsync(file, "patient-responses");
                response.PatientResponseImages.Add(new PatientResponseImage { ImagePath = path });
            }
        }

        response.UpdatedAt = DateTime.Now;
        await context.SaveChangesAsync();

        return new Result<PatientResponse> { Success = true, Data = response, Message = "Response updated successfully" };
    }

    public async Task<IActionResult> DeleteAsync(string patientId, int responseId)
    {
        var response = await context.PatientResponses
            .FirstOrDefaultAsync(r => r.Id == responseId && r.PatientId == int.Parse(patientId) && !r.IsDeleted);

        if (response is null)
            return new Result { Success = false, Message = "Response not found" };

        response.IsDeleted = true;
        response.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync();

        return new Result { Success = true, Message = "Response deleted successfully" };
    }
}