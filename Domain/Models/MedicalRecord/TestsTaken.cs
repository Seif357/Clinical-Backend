namespace Domain.Models.MedicalRecord
{
    public class TestsTaken: ParentEntity
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Result { get; set; }
    }
}