using Domain.Models.Schedule;

namespace Application.Dto.Schedule;

public record ScheduleSlotDto(
    int Id,
    DateTime StartTime,
    DateTime EndTime,
    AppointmentStatus Status,
    int? PatientId,
    string? PatientName,
    string? PatientNotes,
    string? DoctorNotes,
    string? CancellationReason,
    DateTime? BookedAt
);