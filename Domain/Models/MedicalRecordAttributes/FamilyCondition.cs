namespace Domain.Models.MedicalRecordAttributes
{
    public class FamilyCondition: ParentEntity
    {
        public string Name { get; set; }
        public string Relative { get; set; }
        public DateTime DiagnosisDate { get; set; }
    }
}