namespace Application.Dto.MedicalRecord;

public record UpdateSurgeryDto(
    string? Name,
    DateTime? Date,
    string? Outcome
);