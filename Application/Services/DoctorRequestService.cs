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
    INotificationService notificationService,
    IMedicationService medicationService,
    IImageUrlHelper imageUrlHelper) : IDoctorRequestService
{
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
                context.DoctorResponses.Count(dr => dr.PatientRequest_Id == r.Id && !dr.IsDeleted)+context.PatientResponses.Count(pt=>pt.DoctorRequestId == r.Id && !pt.IsDeleted),
                r.IsCompleted ? "Completed" : "Active",
                r.CreatedAt
            ))
            .ToListAsync();

        return new Result<List<DoctorRequestSummaryDto>> { Success = true, Data = summaries };
    }

    public async Task<IActionResult> GetByIdAsync(string requesterId, int requestId)
    {
        var request = await context.DoctorRequests
            .AsNoTracking()
            .Include(r => r.DoctorReqestImages)
            .FirstOrDefaultAsync(r => r.Id == requestId && (r.DoctorId == requesterId||r.PatientId==requesterId) && !r.IsDeleted);

        if (request is null)
            return new Result { Success = false, Message = "Request not found" };

        foreach (var img in request.DoctorReqestImages)
            img.ImagePath = imageUrlHelper.Resolve(img.ImagePath)!;

        // Doctor follow-ups on their own request (no images — DoctorResponse has none)
        var doctorFollowUps = await context.DoctorResponses
            .AsNoTracking()
            .Where(dr => dr.PatientRequest_Id == requestId && !dr.IsDeleted)
            .Select(dr => new UnifiedResponseDto
            {
                Id = dr.Id,
                Message = dr.Message,
                CreatedAt = dr.CreatedAt,
                SenderType = "Doctor",
                AppointmentSchedule = dr.AppointmentSchedule
            })
            .ToListAsync();

        // Patient replies to this doctor request
        var patientResponses = await context.PatientResponses
            .AsNoTracking()
            .Where(pr => pr.DoctorRequestId == requestId && !pr.IsDeleted)
            .Select(pr => new UnifiedResponseDto
            {
                Id = pr.Id,
                Message = pr.Message,
                CreatedAt = pr.CreatedAt,
                SenderType = "Patient",
                Subject = pr.Subject,
                Images = pr.PatientResponseImages
                    .Select(img => img.ImagePath)
                    .ToList()
            })
            .ToListAsync();

        // Resolve stored paths to full URLs — must happen after projection, not inside Select
        foreach (var resp in patientResponses.Where(r => r.Images != null))
            resp.Images = resp.Images!.Select(path => imageUrlHelper.Resolve(path)!).ToList();

        var responses = doctorFollowUps
            .Concat(patientResponses)
            .OrderBy(r => r.CreatedAt)
            .ToList();

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
        if (dto.RequestType == RequestType.Prescription && dto.Prescription is not null)
        {
            await medicationService.PrescribeAsync(
                patientUserId: int.Parse(dto.PatientId),
                doctorUserId:  int.Parse(doctorId),
                doctorRequestId: request.Id,
                dto: dto.Prescription);
            await context.SaveChangesAsync();
        }
        await notificationService.NotifyUserAsync(dto.PatientId, "NewDoctorRequest", new
        {
            request.Id,
            request.Subject,
            request.Importance,
            request.RequestType,
            DoctorId = doctorId,
            SentAt = request.CreatedAt
        });

        // Resolve stored paths to full URLs before returning
        foreach (var img in request.DoctorReqestImages)
            img.ImagePath = imageUrlHelper.Resolve(img.ImagePath)!;

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

        // Resolve stored paths to full URLs before returning
        foreach (var img in request.DoctorReqestImages)
            img.ImagePath = imageUrlHelper.Resolve(img.ImagePath)!;

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

    public async Task<IActionResult> MarkCompleteAsync(string doctorId, int requestId)
    {
        var request = await context.DoctorRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.DoctorId == doctorId && !r.IsDeleted);

        if (request is null)
            return new Result { Success = false, Message = "Request not found" };

        if (request.IsCompleted)
            return new Result { Success = false, Message = "Request is already marked as completed" };

        request.IsCompleted = true;
        request.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return new Result { Success = true, Message = "Request marked as completed" };
    }
    // Patient-scoped: all doctor requests directed at this patient
    public async Task<IActionResult> GetIncomingForPatientAsync(string patientId)
    {
        var summaries = await context.DoctorRequests
            .AsNoTracking()
            .Where(r => r.PatientId == patientId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new DoctorRequestSummaryDto(
                r.Id,
                r.Subject,
                r.Message.Length > 100 ? r.Message.Substring(0, 100) + "..." : r.Message,
                r.Importance,
                r.RequestType,
                r.PatientId,
                context.DoctorResponses.Count(dr => dr.PatientRequest_Id == r.Id && !dr.IsDeleted)+context.PatientResponses.Count(pt=>pt.DoctorRequestId == r.Id && !pt.IsDeleted),
                r.IsCompleted ? "Completed" : "Active",
                r.CreatedAt
            ))
            .ToListAsync();

        return new Result<List<DoctorRequestSummaryDto>> { Success = true, Data = summaries };
    }
}