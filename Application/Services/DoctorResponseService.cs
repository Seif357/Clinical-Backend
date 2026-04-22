using Application.Dto.AuthDto;
using Application.Dto.Communication;
using Application.DTOs;
using Application.Interfaces;
using Domain.Models.Communication;
using Infrastructure.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class DoctorResponseService(
    AppDbContext context,
    INotificationService notificationService) : IDoctorResponseService
{
    public async Task<IActionResult> CreateAsync(string doctorId, CreateDoctorResponseDto dto)
    {
        var patientRequest = await context.PatientRequests
            .FirstOrDefaultAsync(r => r.Id == dto.PatientRequestId && !r.IsDeleted);

        if (patientRequest is null)
            return new Result { Success = false, Message = "Patient request not found" };

        if (patientRequest.DoctorId != doctorId)
            return new Result { Success = false, Message = "This request was not directed at you" };

        var response = new DoctorResponse
        {
            DoctorId = int.Parse(doctorId),
            PatientRequest_Id = dto.PatientRequestId,
            Message = dto.Message,
            AppointmentSchedule = dto.AppointmentSchedule?.Cast<DateTime?>().ToList() ?? []
        };

        await context.DoctorResponses.AddAsync(response);
        await context.SaveChangesAsync();

        await notificationService.NotifyUserAsync(patientRequest.PatientId, "NewDoctorResponse", new
        {
            response.Id,
            response.Message,
            response.AppointmentSchedule,
            DoctorId = doctorId,
            PatientRequestId = dto.PatientRequestId,
            SentAt = response.CreatedAt
        });

        return new Result<DoctorResponse> { Success = true, Data = response, Message = "Response sent successfully" };
    }

    public async Task<IActionResult> UpdateAsync(string doctorId, int responseId, UpdateDoctorResponseDto dto)
    {
        var response = await context.DoctorResponses
            .FirstOrDefaultAsync(r => r.Id == responseId && r.DoctorId == int.Parse(doctorId) && !r.IsDeleted);

        if (response is null)
            return new Result { Success = false, Message = "Response not found" };

        if (!string.IsNullOrEmpty(dto.Message))
            response.Message = dto.Message;

        if (dto.AppointmentSchedule is not null)
            response.AppointmentSchedule = dto.AppointmentSchedule.Cast<DateTime?>().ToList();

        response.UpdatedAt = DateTime.Now;
        await context.SaveChangesAsync();

        return new Result<DoctorResponse> { Success = true, Data = response, Message = "Response updated successfully" };
    }

    public async Task<IActionResult> DeleteAsync(string doctorId, int responseId)
    {
        var response = await context.DoctorResponses
            .FirstOrDefaultAsync(r => r.Id == responseId && r.DoctorId == int.Parse(doctorId) && !r.IsDeleted);

        if (response is null)
            return new Result { Success = false, Message = "Response not found" };

        response.IsDeleted = true;
        response.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync();

        return new Result { Success = true, Message = "Response deleted successfully" };
    }
}