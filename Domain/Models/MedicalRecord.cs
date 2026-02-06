namespace Domain.Models
{
    public class MedicalRecord : ParentEntity
    {
        public int patient_id { get; set; }
        public ICollection<Visits> visits { get; set; }
        public ICollection<Surgeries> surgeries { get; set; }
        public ICollection<TestsTaken> tests_taken { get; set; }
        public ICollection<Allergy> allergies { get; set; }
        public ICollection<MedicationTaken> medications { get; set; }
        public ICollection<FamilyHistory> family_medical_history { get; set; }
    }
}