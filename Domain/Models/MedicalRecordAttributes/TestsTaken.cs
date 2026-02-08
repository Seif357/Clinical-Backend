namespace Domain.Models.MedicalRecordAttributes
{
    public class TestTaken: ParentEntity
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Result { get; set; }
    }
}