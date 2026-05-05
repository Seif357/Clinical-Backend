namespace Domain.Models.MedicalRecordAttributes;

public class Surgery : ReviewableEntry
{
    public int? MedicalRecordId { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public string Outcome { get; set; }
}