namespace Domain.Models.MedicalRecord
{
    public class Surgeries: ParentEntity
    {
            public string Name { get; set; }
            public DateTime Date { get; set; }
            public string Outcome { get; set; }
    }
}