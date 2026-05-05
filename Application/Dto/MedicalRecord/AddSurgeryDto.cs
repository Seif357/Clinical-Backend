namespace Application.Dto.MedicalRecord;

public record AddSurgeryDto(
    string Name,
    DateTime Date,
    string Outcome
);