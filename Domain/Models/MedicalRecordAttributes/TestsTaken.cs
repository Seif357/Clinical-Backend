namespace Domain.Models.MedicalRecordAttributes;

public class TestTaken : ReviewableEntry
{
    public int? MedicalRecordId { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public string Result { get; set; }
}