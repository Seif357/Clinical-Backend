namespace Application.Dto.MedicalRecord;

public record UpdateFamilyConditionDto(
    string? Name,
    string? Relative,
    DateTime? DiagnosisDate
);