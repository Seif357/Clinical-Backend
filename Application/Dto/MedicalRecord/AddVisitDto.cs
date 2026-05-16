namespace Application.Dto.MedicalRecord;

public record AddVisitDto(
    DateTime Date,
    string DoctorName,
    string ReasonForVisit,
    string Diagnosis,
    string TreatmentPlan
);