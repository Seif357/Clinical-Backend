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

public class DoctorRequestService(
    AppDbContext context,
    IFileStorageService fileStorage,
    INotificationService notificationService) : IDoctorRequestService
{
    // GET / — lightweight summary list, no images, no responses
    public async Task<IActionResult> GetAllSummaryAsync(string doctorId)
    {
        var summaries = await context.DoctorRequests
            .AsNoTracking()
            .Where(r => r.DoctorId == doctorId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new DoctorRequestSummaryDto
            (
                r.Id,
                r.Subject,
                r.Message.Length > 100
                    ? r.Message.Substring(0, 100) + "..."
                    : r.Message,
                r.Importance,
                r.RequestType,
                r.PatientId,
                context.PatientResponses
                    .Count(pr => pr.DoctorRequestId == r.Id && !pr.IsDeleted),
                r.CreatedAt
            ))
            .ToListAsync();

        return new Result<List<DoctorRequestSummaryDto>> { Success = true, Data = summaries };
    }

    // GET /{id} — full detail with images + embedded PatientResponses + their images
    public async Task<IActionResult> GetByIdAsync(string doctorId, int requestId)
    {
        var request = await context.DoctorRequests
            .AsNoTracking()
            .Include(r => r.DoctorReqestImages)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.DoctorId == doctorId && !r.IsDeleted);

        if (request is null)
            return new Result { Success = false, Message = "Request not found" };

        var responses = await context.PatientResponses
            .AsNoTracking()
            .Include(pr => pr.PatientResponseImages)
            .Where(pr => pr.DoctorRequestId == requestId && !pr.IsDeleted)
            .OrderBy(pr => pr.CreatedAt)
            .ToListAsync();

        return new Result<object>
        {
            Success = true,
            Data = new { Request = request, Responses = responses }
        };
    }

    public async Task<IActionResult> CreateAsync(string doctorId, CreateDoctorRequestDto dto)
    {
        var patientExists = await context.Patients
            .AnyAsync(p => p.UserId.ToString() == dto.PatientId && !p.IsDeleted);

        if (!patientExists)
            return new Result { Success = false, Message = "Patient not found" };

        var request = new DoctorRequest
        {
            DoctorId = doctorId,
            PatientId = dto.PatientId,
            Subject = dto.Subject,
            Message = dto.Message,
            RequestType = dto.RequestType,
            Importance = dto.Importance,
            DoctorReqestImages = []
        };

        if (dto.Images is { Count: > 0 })
        {
            foreach (var file in dto.Images)
            {
                var path = await fileStorage.SaveFileAsync(file, "doctor-requests");
                request.DoctorReqestImages.Add(new DoctorReqestImage { ImagePath = path });
            }
        }

        await context.DoctorRequests.AddAsync(request);
        await context.SaveChangesAsync();

        await notificationService.NotifyUserAsync(dto.PatientId, "NewDoctorRequest", new
        {
            request.Id,
            request.Subject,
            request.Importance,
            request.RequestType,
            DoctorId = doctorId,
            SentAt = request.CreatedAt
        });

        return new Result<DoctorRequest> { Success = true, Data = request, Message = "Request sent successfully" };
    }

    public async Task<IActionResult> UpdateAsync(string doctorId, int requestId, UpdateDoctorRequestDto dto)
    {
        var request = await context.DoctorRequests
            .Include(r => r.DoctorReqestImages)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.DoctorId == doctorId && !r.IsDeleted);

        if (request is null)
            return new Result { Success = false, Message = "Request not found" };

        if (!string.IsNullOrEmpty(dto.Subject))
            request.Subject = dto.Subject;

        if (!string.IsNullOrEmpty(dto.Message))
            request.Message = dto.Message;

        if (dto.Importance.HasValue)
            request.Importance = dto.Importance.Value;

        if (dto.RequestType.HasValue)
            request.RequestType = dto.RequestType.Value;

        if (dto.ImageIdsToRemove is { Count: > 0 })
        {
            var toRemove = request.DoctorReqestImages
                .Where(img => dto.ImageIdsToRemove.Contains(img.Id))
                .ToList();

            foreach (var img in toRemove)
            {
                await fileStorage.DeleteFileAsync(img.ImagePath);
                request.DoctorReqestImages.Remove(img);
            }
        }

        if (dto.NewImages is { Count: > 0 })
        {
            foreach (var file in dto.NewImages)
            {
                var path = await fileStorage.SaveFileAsync(file, "doctor-requests");
                request.DoctorReqestImages.Add(new DoctorReqestImage { ImagePath = path });
            }
        }

        request.UpdatedAt = DateTime.Now;
        await context.SaveChangesAsync();

        return new Result<DoctorRequest> { Success = true, Data = request, Message = "Request updated successfully" };
    }

    public async Task<IActionResult> DeleteAsync(string doctorId, int requestId)
    {
        var request = await context.DoctorRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.DoctorId == doctorId && !r.IsDeleted);

        if (request is null)
            return new Result { Success = false, Message = "Request not found" };

        request.IsDeleted = true;
        request.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync();

        return new Result { Success = true, Message = "Request deleted successfully" };
    }
}