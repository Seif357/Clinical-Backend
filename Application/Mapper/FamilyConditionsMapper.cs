using Application.Dto.MedicalRecord;

namespace Application.Mapper;

public static class FamilyConditionMapper
{
    public static FamilyConditionDto ToDto(this Domain.Models.MedicalRecordAttributes.FamilyCondition e) =>
        new(e.Id, e.Name, e.Relative, e.DiagnosisDate, e.Status, e.ReviewNote, e.CreatedAt);

}