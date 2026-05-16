using Domain.Models.MedicalRecordAttributes;

namespace Application.Dto.MedicalRecord;

public record TestDto(
    int Id,
    string Name,
    DateTime Date,
    string Result,
    MedicalEntryStatus Status,
    string? ReviewNote,
    DateTime CreatedAt
);