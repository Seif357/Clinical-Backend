using Domain.Models.MedicalRecordAttributes;

namespace Application.Dto.MedicalRecord;

public record PrescribeMedicationDto(
    string Name,
    string Dosage,
    string Frequency,
    DateTime StartDate,
    DateTime? EndDate,
    List<string>? ReminderTimes,
    List<int>?   DaysOfWeek,
    string? Notes
);