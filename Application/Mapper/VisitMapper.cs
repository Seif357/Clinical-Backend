using Application.Dto.MedicalRecord;

namespace Application.Mapper;

public static class VisitMapper
{
    public static VisitDto ToDto(this Domain.Models.MedicalRecordAttributes.Visit e) =>
        new(e.Id, e.Date, e.DoctorName, e.ReasonForVisit, e.Diagnosis, e.Treatment_Plan, e.Status, e.ReviewNote, e.CreatedAt);
}