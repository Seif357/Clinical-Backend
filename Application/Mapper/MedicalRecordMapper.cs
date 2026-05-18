using Application.Dto.MedicalRecord;

namespace Application.Mapper;

public static class MedicalRecordMapper
{
    public static MedicalRecordDto ToDto(this Domain.Models.MedicalRecord r) => new(
        Id:               r.Id,
        PatientId:        r.PatientId,
        Allergies:        r.Allergies.Where(e => !e.IsDeleted).Select(e => e.ToDto()),
        Visits:           r.Visits.Where(e => !e.IsDeleted).Select(e => e.ToDto()),
        Surgeries:        r.Surgeries.Where(e => !e.IsDeleted).Select(e => e.ToDto()),
        Tests:            r.TestsTaken.Where(e => !e.IsDeleted).Select(e => e.ToDto()),
        Medications:      r.Medications.Where(e => !e.IsDeleted).Select(e => e.ToDto()),
        FamilyConditions: r.FamilyConditions.Where(e => !e.IsDeleted).Select(e => e.ToDto())
    );
}