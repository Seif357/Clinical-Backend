namespace Domain.Models.Communication;

public class PatientRequest : ParentEntity
{
    public string PatientId { get; set; }
    public string DoctorId { get; set; }
    public string Message { get; set; }
    public string Subject { get; set; }
    public bool IsCompleted { get; set; } = false;
    public RequestImportance Importance { get; set; }
    public ICollection<PatientRequestImage> PatientRequestImages { get; set; }
}