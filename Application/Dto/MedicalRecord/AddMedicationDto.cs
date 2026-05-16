namespace Application.Dto.MedicalRecord;

public record AddMedicationDto(
    string Name,
    string Dosage,
    string Frequency,
    DateTime StartDate,
    DateTime? EndDate,
    List<string>? ReminderTimes,
    List<int>?   DaysOfWeek,
    string? Notes
);