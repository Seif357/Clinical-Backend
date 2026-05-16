namespace Application.Dto.MedicalRecord;

public record AddFamilyConditionDto(
    string Name,
    string Relative,
    DateTime DiagnosisDate
);