namespace Application.Dto.MedicalRecord;

public record UpdateTestDto(
    string? Name,
    DateTime? Date,
    string? Result
);