namespace Domain.Models.MedicalRecordAttributes;

public class PrescribedMedication : ParentEntity
{
    public string Name { get; set; }
    public string Dosage { get; set; }
    public string Frequency { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}