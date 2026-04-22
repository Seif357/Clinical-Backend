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

public class PatientRequestService(
    AppDbContext context,
    IFileStorageService fileStorage,
    INotificationService notificationService) : IPatientRequestService
{
    // GET / — lightweight summary list, no images, no responses
    public async Task<IActionResult> GetAllSummaryAsync(string patientId)
    {
        var summaries = await context.PatientRequests
            .AsNoTracking()
            .Where(r => r.PatientId == patientId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new PatientRequestSummaryDto(
                r.Id,
                r.Subject,
                r.Message.Length > 100 ? r.Message.Substring(0, 100) + "..." : r.Message,
                r.Importance,
                r.DoctorId,
                context.DoctorResponses.Count(dr => dr.PatientRequest_Id == r.Id && !dr.IsDeleted),
                r.CreatedAt
            ))
            .ToListAsync();

        return new Result<List<PatientRequestSummaryDto>> { Success = true, Data = summaries };
    }

    // GET /{id} — full detail with images + embedded DoctorResponses
    public async Task<IActionResult> GetByIdAsync(string patientId, int requestId)
    {
        var request = await context.PatientRequests
            .AsNoTracking()
            .Include(r => r.PatientRequestImages)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.PatientId == patientId && !r.IsDeleted);

        if (request is null)
            return new Result { Success = false, Message = "Request not found" };

        var responses = await context.DoctorResponses
            .AsNoTracking()
            .Where(dr => dr.PatientRequest_Id == requestId && !dr.IsDeleted)
            .OrderBy(dr => dr.CreatedAt)
            .ToListAsync();

        return new Result<object>
        {
            Success = true,
            Data = new { Request = request, Responses = responses }
        };
    }

    public async Task<IActionResult> CreateAsync(string patientId, CreatePatientRequestDto dto)
    {
        var doctorExists = await context.Doctors
            .AnyAsync(d => d.UserId.ToString() == dto.DoctorId && !d.IsDeleted);

        if (!doctorExists)
            return new Result { Success = false, Message = "Doctor not found" };

        var request = new PatientRequest
        {
            PatientId = patientId,
            DoctorId = dto.DoctorId,
            Subject = dto.Subject,
            Message = dto.Message,
            Importance = dto.Importance,
            AppointmentRequestedDates = dto.AppointmentRequestedDates ?? [],
            PatientRequestImages = []
        };

        if (dto.Images is { Count: > 0 })
        {
            foreach (var file in dto.Images)
            {
                var path = await fileStorage.SaveFileAsync(file, "patient-requests");
                request.PatientRequestImages.Add(new PatientRequestImage { ImagePath = path });
            }
        }

        await context.PatientRequests.AddAsync(request);
        await context.SaveChangesAsync();

        await notificationService.NotifyUserAsync(dto.DoctorId, "NewPatientRequest", new
        {
            request.Id,
            request.Subject,
            request.Importance,
            PatientId = patientId,
            SentAt = request.CreatedAt
        });

        return new Result<PatientRequest> { Success = true, Data = request, Message = "Request sent successfully" };
    }

    public async Task<IActionResult> UpdateAsync(string patientId, int requestId, UpdatePatientRequestDto dto)
    {
        var request = await context.PatientRequests
            .Include(r => r.PatientRequestImages)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.PatientId == patientId && !r.IsDeleted);

        if (request is null)
            return new Result { Success = false, Message = "Request not found" };

        if (!string.IsNullOrEmpty(dto.Subject))
            request.Subject = dto.Subject;

        if (!string.IsNullOrEmpty(dto.Message))
            request.Message = dto.Message;

        if (dto.Importance.HasValue)
            request.Importance = dto.Importance.Value;

        if (dto.AppointmentRequestedDates is not null)
            request.AppointmentRequestedDates = dto.AppointmentRequestedDates;

        if (dto.ImageIdsToRemove is { Count: > 0 })
        {
            var toRemove = request.PatientRequestImages
                .Where(img => dto.ImageIdsToRemove.Contains(img.Id))
                .ToList();

            foreach (var img in toRemove)
            {
                await fileStorage.DeleteFileAsync(img.ImagePath);
                request.PatientRequestImages.Remove(img);
            }
        }

        if (dto.NewImages is { Count: > 0 })
        {
            foreach (var file in dto.NewImages)
            {
                var path = await fileStorage.SaveFileAsync(file, "patient-requests");
                request.PatientRequestImages.Add(new PatientRequestImage { ImagePath = path });
            }
        }

        request.UpdatedAt = DateTime.Now;
        await context.SaveChangesAsync();

        return new Result<PatientRequest> { Success = true, Data = request, Message = "Request updated successfully" };
    }

    public async Task<IActionResult> DeleteAsync(string patientId, int requestId)
    {
        var request = await context.PatientRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.PatientId == patientId && !r.IsDeleted);

        if (request is null)
            return new Result { Success = false, Message = "Request not found" };

        request.IsDeleted = true;
        request.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync();

        return new Result { Success = true, Message = "Request deleted successfully" };
    }
}