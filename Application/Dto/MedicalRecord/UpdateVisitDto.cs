namespace Application.Dto.MedicalRecord;

public record UpdateVisitDto(
    DateTime? Date,
    string? DoctorName,
    string? ReasonForVisit,
    string? Diagnosis,
    string? TreatmentPlan
);