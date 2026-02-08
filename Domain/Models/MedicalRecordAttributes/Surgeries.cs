namespace Domain.Models.MedicalRecordAttributes
{
    public class Surgery: ParentEntity
    {
            public string Name { get; set; }
            public DateTime Date { get; set; }
            public string Outcome { get; set; }
    }
}