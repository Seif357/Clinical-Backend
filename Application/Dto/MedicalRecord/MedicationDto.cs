using Domain.Models.MedicalRecordAttributes;

namespace Application.Dto.MedicalRecord;

public record MedicationDto(
    int Id,
    string Name,
    string Dosage,
    string Frequency,
    DateTime StartDate,
    DateTime? EndDate,
    MedicalEntryStatus Status,
    string? ReviewNote,
    DateTime CreatedAt,
    List<string> ReminderTimes,
    List<int> DaysOfWeek,
    string? Notes,
    MedicationSource Source,
    int? PrescribedByDoctorId,
    int? DoctorRequestId,
    DateTime UpdatedAt
);