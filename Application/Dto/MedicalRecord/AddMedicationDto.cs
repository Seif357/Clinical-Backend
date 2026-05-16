namespace Application.Dto.MedicalRecord;

public record AddMedicationDto(
    string Name,
    string Dosage,
    string Frequency,
    DateTime StartDate,
    DateTime? EndDate
);