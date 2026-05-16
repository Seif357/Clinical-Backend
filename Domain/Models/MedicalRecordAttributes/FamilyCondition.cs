namespace Domain.Models.MedicalRecordAttributes;

public class FamilyCondition : ReviewableEntry
{
    public int? MedicalRecordId { get; set; }
    public string Name { get; set; }
    public string Relative { get; set; }
    public DateTime DiagnosisDate { get; set; }
}