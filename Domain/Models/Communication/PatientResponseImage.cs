namespace Domain.Models.Communication;

public class PatientResponseImage : ParentEntity
{
    public string ImagePath { get; set; }
    public int PatientResponseId { get; set; }
}