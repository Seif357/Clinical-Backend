
using Domain.Models.MedicalRecordAttributes;

namespace Domain.Models
{
    public class MedicalRecord : ParentEntity
    {
        public int PatientId { get; set; }
        public ICollection<Visit> Visits { get; set; }
        public ICollection<Surgery> Surgeries { get; set; }
        public ICollection<TestTaken> TestsTaken { get; set; }
        public ICollection<Allergy> Allergies { get; set; }
        public ICollection<MedicationTaken> MedicationsTaken { get; set; }
        public ICollection<FamilyCondition> FamilyConditions { get; set; }
    }
}