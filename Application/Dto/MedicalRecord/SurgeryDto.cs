using Domain.Models.MedicalRecordAttributes;

namespace Application.Dto.MedicalRecord;

public record SurgeryDto(
    int Id,
    string Name,
    DateTime Date,
    string Outcome,
    MedicalEntryStatus Status,
    string? ReviewNote,
    DateTime CreatedAt
);