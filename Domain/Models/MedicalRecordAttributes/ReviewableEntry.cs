namespace Domain.Models.MedicalRecordAttributes;

public class ReviewableEntry:ParentEntity
{
    public int SubmittedByUserId { get; set; }
    public MedicalEntryStatus Status { get; set; }
    public int? ReviewedByDoctorId { get; set; }
    public string? ReviewNote { get; set; }
}