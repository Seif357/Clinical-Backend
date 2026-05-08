using Application.Dto.MedicalRecord;

namespace Application.Mapper;

public static class SurgeryMapper
{
    public static SurgeryDto ToDto(this Domain.Models.MedicalRecordAttributes.Surgery e) =>
        new(e.Id, e.Name, e.Date, e.Outcome, e.Status, e.ReviewNote, e.CreatedAt);
}