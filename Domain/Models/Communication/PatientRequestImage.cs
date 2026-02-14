namespace Domain.Models.Communication;

public class PatientRequestImage : ParentEntity
{
    public string ImagePath { get; set; }
    public int PatientRequestId { get; set; }
}