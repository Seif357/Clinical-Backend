using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models
{
    public class Patient
    {
        public int user_id { get; set; }
        public DateTime DateOfBirth { get; set; }
        public BloodType blood_type { get; set; }
        public MedicalRecord medicalRecord { get; set; }
    }
}
