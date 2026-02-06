namespace Domain.Models
{
    public class FamilyMedicalHistory: ParentEntity
    {
        public string Relative { get; set; }
        public string Condition { get; set; }
        public DateTime DiagnosisDate { get; set; }
    }
}