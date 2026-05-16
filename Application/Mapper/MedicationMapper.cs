using Application.Dto.MedicalRecord;

namespace Application.Mapper;

public static class MedicationMapper
{
    public static MedicationDto ToDto(this Domain.Models.MedicalRecordAttributes.Medication e) =>
        new(e.Id, e.Name, e.Dosage, e.Frequency, e.StartDate, e.EndDate, e.Status, e.ReviewNote,
            e.CreatedAt, e.ReminderTimes, e.DaysOfWeek, e.Notes, e.Source,
            e.PrescribedByDoctorId, e.DoctorRequestId, e.UpdatedAt);

}