using Application.Dto.MedicalRecord;

namespace Application.Mapper;

public static class AllergyMapper
{
    public static AllergyDto ToDto(this Domain.Models.MedicalRecordAttributes.Allergy e) =>
        new(e.Id, e.Name, e.Severity, e.Reaction, e.Status, e.ReviewNote, e.CreatedAt);

}