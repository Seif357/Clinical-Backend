using Domain.Models.MedicalRecordAttributes;

namespace Application.Dto.MedicalRecord;

public record AllergyDto(
    int Id,
    string Name,
    string Severity,
    string Reaction,
    MedicalEntryStatus Status,
    string? ReviewNote,
    DateTime CreatedAt
);