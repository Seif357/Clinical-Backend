namespace Application.Dto.MedicalRecord;

public record ReviewEntryDto(
    bool Approve,
    string? Note
);