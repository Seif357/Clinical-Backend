using Application.Dto.MedicalRecord;
using Domain.Models.MedicalRecordAttributes;

namespace Application.Mapper;

public static class VisitsMapper
{
    public static TestDto ToDto(this TestTaken e) =>
        new(e.Id, e.Name, e.Date, e.Result, e.Status, e.ReviewNote, e.CreatedAt);
}