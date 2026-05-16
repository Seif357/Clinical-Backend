namespace Application.Dto.MedicalRecord;

public record UpdateMedicationDto(
    string? Name,
    string? Dosage,
    string? Frequency,
    DateTime? StartDate,
    DateTime? EndDate,
    List<string>? ReminderTimes,
    List<int>?   DaysOfWeek,
    string? Notes
);