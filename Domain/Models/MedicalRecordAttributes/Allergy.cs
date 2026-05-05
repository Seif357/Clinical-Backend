namespace Domain.Models.MedicalRecordAttributes;

public class Allergy : ReviewableEntry
{
    public int? MedicalRecordId { get; set; }
    public string Name { get; set; }
    public string Severity { get; set; }
    public string Reaction { get; set; }
}