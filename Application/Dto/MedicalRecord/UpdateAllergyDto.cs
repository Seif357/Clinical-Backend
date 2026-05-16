namespace Application.Dto.MedicalRecord;

public record UpdateAllergyDto(
    string? Name,
    string? Severity,
    string? Reaction
);