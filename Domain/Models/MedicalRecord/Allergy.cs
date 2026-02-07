namespace Domain.Models.MedicalRecord
{
    public class Allergy: ParentEntity
    {
        public string Name { get; set; }
        public string Severity { get; set; }
        public string Reaction { get; set; }   
    }
}