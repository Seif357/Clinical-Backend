using Domain.Models.MedicalRecordAttributes;

namespace Application.Dto.MedicalRecord;

public record FamilyConditionDto(
    int Id,
    string Name,
    string Relative,
    DateTime DiagnosisDate,
    MedicalEntryStatus Status,
    string? ReviewNote,
    DateTime CreatedAt
);