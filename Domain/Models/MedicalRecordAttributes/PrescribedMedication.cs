namespace Domain.Models.MedicalRecordAttributes;

public class PrescribedMedication : ReviewableEntry
{
    public string Name { get; set; }
    public string Dosage { get; set; }
    public string Frequency { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int MedicalRecordId { get; set; }
}