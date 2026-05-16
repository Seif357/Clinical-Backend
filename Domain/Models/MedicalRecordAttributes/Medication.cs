namespace Domain.Models.MedicalRecordAttributes;

public class Medication : ReviewableEntry
{
    public string Name { get; set; }
    public string Dosage { get; set; }
    public string Frequency { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int MedicalRecordId { get; set; }
    public MedicalRecord MedicalRecord { get; set; } = null!;
    public MedicationSource Source { get; set; } = MedicationSource.SelfAdded;
    public int? PrescribedByDoctorId { get; set; }
    public int? DoctorRequestId { get; set; }
    public List<string> ReminderTimes { get; set; } = [];
    public List<int> DaysOfWeek { get; set; } = [];
    public string? Notes { get; set; }
    
}