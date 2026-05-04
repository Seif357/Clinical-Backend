using Application.Dto.Schedule;
using Application.DTOs;

namespace Application.Interfaces;

public interface IScheduleService
{
    Task<Result> GenerateSlotsAsync(int doctorUserId, GenerateSlotsDto dto);
    Task<Result> CreateSlotAsync(int doctorUserId, CreateSlotDto dto);
    Task<Result> DeleteSlotAsync(int doctorUserId, int slotId);
    Task<Result> CompleteSlotAsync(int doctorUserId, CompleteSlotDto dto);
    Task<Result> GetMyScheduleAsync(int doctorUserId);
    Task<Result> GetDailyScheduleAsync(int doctorUserId, DateOnly date);
    Task<Result> GetWeeklyScheduleAsync(int doctorUserId, DateOnly weekStart);
    Task<Result> GetAvailableSlotsAsync(int doctorUserId);
    Task<Result> BookSlotAsync(int patientUserId, BookSlotDto dto);
    Task<Result> RescheduleAsync(int patientUserId, RescheduleDto dto);
    Task<Result> CancelSlotAsync(int callerUserId, CancelSlotDto dto);
    Task<Result> GetMyAppointmentsAsync(int patientUserId);
 
}