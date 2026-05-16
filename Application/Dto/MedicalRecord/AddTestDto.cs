namespace Application.Dto.MedicalRecord;

public record AddTestDto(
    string Name,
    DateTime Date,
    string Result
);