namespace Domain.Models.Communication;

public class PatientResponse : ParentEntity
{
    public int PatientId { get; set; }
    public int DoctorRequestId { get; set; }
    public string Message { get; set; }
    public string Subject { get; set; }
    public ICollection<PatientResponseImage> PatientResponseImages { get; set; }
}