namespace Application.Dto.MedicalRecord;

public record PendingEntriesDto(
    IEnumerable<AllergyDto> Allergies,
    IEnumerable<VisitDto> Visits,
    IEnumerable<SurgeryDto> Surgeries,
    IEnumerable<TestDto> Tests,
    IEnumerable<MedicationDto> Medications,
    IEnumerable<FamilyConditionDto> FamilyConditions
);