namespace Domain.Models.MedicalRecordAttributes;

public class Visit : ParentEntity
{
    public DateTime Date { get; set; }
    public string DoctorName { get; set; }
    public string ReasonForVisit { get; set; }
    public string Diagnosis { get; set; }
    public string Treatment_Plan { get; set; }
}