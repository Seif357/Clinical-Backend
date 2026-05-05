namespace Application.Dto.MedicalRecord;

public record AddAllergyDto(
    string Name,
    string Severity,
    string Reaction
);