using Domain.Models.MedicalRecordAttributes;

namespace Application.Dto.MedicalRecord;

public record VisitDto(
    int Id,
    DateTime Date,
    string DoctorName,
    string ReasonForVisit,
    string Diagnosis,
    string TreatmentPlan,
    MedicalEntryStatus Status,
    string? ReviewNote,
    DateTime CreatedAt
);